using GTANetworkAPI;
using System.Collections.Generic;
using iTeffa.Interface;
using System;
using iTeffa.Globals;
using iTeffa.Settings;

namespace iTeffa.Working
{
    class Taxi : Script
    {
        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void taxiCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 3);
                NAPI.Data.SetEntityData(veh, "TYPE", "TAXI");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
            }
        }
        private static readonly Nlogs Log = new Nlogs("Taxi");

        private static readonly int taxiRentCost = 100;
        private static readonly Dictionary<Player, ColShape> orderCols = new Dictionary<Player, ColShape>();

        public static void taxiRent(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) && player.VehicleSeat == 0 && player.Vehicle.GetData<string>("TYPE") == "TAXI")
            {
                if (player.Vehicle.GetData<Player>("DRIVER") == null)
                {
                    var vehicle = player.Vehicle;
                    NAPI.Data.SetEntityData(player, "WORK", vehicle);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);

                    NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
                    vehicle.SetData("ON_WORK", true);

                    if (!Finance.Wallet.Change(player, taxiRentCost))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно денег", 3000);
                        return;
                    }
                    Loggings.Money($"player({Main.Players[player].UUID})", $"server", taxiRentCost, $"taxiRent");
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы арендовали такси. Чтобы предложить игроку оплатить проезд, напишите /tprice [ID] [Цена]", 3000);
                    Globals.VehicleStreaming.SetEngineState(vehicle, false);
                }
            }
        }

        public static void taxiPay(Player player)
        {
            var seller = player.GetData<Player>("TAXI_SELLER");
            var price = player.GetData<int>("TAXI_PAY");
            
            if (!Main.Players.ContainsKey(seller)) return;

            if(!Finance.Wallet.Change(player, -price))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно денег", 3000);
                return;
            }
            Finance.Wallet.Change(seller, price);
            Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", taxiRentCost, $"taxiPay");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы оплатили проезд", 3000);
            Plugins.Notice.Send(seller, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок " + player.Name.Replace('_', ' ') + " оплатил проезд", 3000);
        }

        private static void order_onEntityExit(ColShape shape, Player player)
        {
            try
            {
                if (shape.GetData<Player>("PASSAGER") != player) return;

                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    player.ResetData("TAXI_DRIVER");
                    player.SetData("IS_CALL_TAXI", false);
                    Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Пассажир отменил заказ", 3000);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы покинули место вызова такси", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Log.Write("order_onEntityExit: " + ex.Message, Nlogs.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TAXI") return;
                if (seatid == 0)
                {
                    if (!Main.Players[player].Licenses[1])
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет лицензии категории B", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 3)
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (vehicle.GetData<Player>("DRIVER") != null)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Это такси уже занят", 3000);
                                return;
                            }
                            if (Main.Players[player].Money >= taxiRentCost)
                            {
                                Plugins.Trigger.ClientEvent(player, "openDialog", "TAXI_RENT", $"Арендовать такси за ${taxiRentCost}?");
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас не хватает " + (taxiRentCost - Main.Players[player].Money) + "$ на аренду такси", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle) NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже работаете", 3000);
                    }
                    else
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете в такси. Устроиться можно в мэрии", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
                else
                {
                    if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != null)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, "Если Вы хотите передать свой маршрут водителю, то поставьте метку на карте и нажмите Z.", 5000);
                        var driver = NAPI.Data.GetEntityData(vehicle, "DRIVER");
                        if (driver.HasData("PASSAGER") && driver.GetData("PASSAGER") == player)
                        {
                            driver.ResetData("PASSAGER");
                            player.SetData("IS_CALL_TAXI", false);
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[player]);
                                orderCols.Remove(player);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В такси сейчас нет водителя", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, Nlogs.Type.Error); }
        }

        public static void respawnCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");

                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 3);
                NAPI.Data.SetEntityData(veh, "TYPE", "TAXI");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write($"respawnCar: " + e.Message, Nlogs.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Пассажир отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
                if (Main.Players[player].WorkID == 3 && NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    NAPI.Task.Run(() => { try { respawnCar(vehicle); } catch { } });
                    if (player.HasData("PASSAGER"))
                    {
                        Player passager = player.GetData<Player>("PASSAGER");
                        passager.ResetData("TAXI_DRIVER");
                        passager.SetData("IS_CALL_TAXI", false);
                        Plugins.Notice.Send(passager, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                        NAPI.Task.Run(() => {
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[passager]);
                                orderCols.Remove(passager);
                            }
                            catch { }
                        });
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" &&
                Main.Players[player].WorkID == 3 &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, Nlogs.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы закончили рабочий день", 3000);
                        respawnCar(vehicle);
                        player.SetData<bool>("ON_WORK", false);
                        player.SetData<Vehicle>("WORK", null);
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        if (player.HasData("PASSAGER"))
                        {
                            Player passager = player.GetData<Player>("PASSAGER");
                            passager.ResetData("TAXI_DRIVER");
                            passager.SetData("IS_CALL_TAXI", false);
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                            player.ResetData("PASSAGER");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[passager]);
                                orderCols.Remove(passager);
                            }
                            catch { }
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);
                }
                catch (Exception e) { Log.Write("taxi_exitVehicleTimer: " + e.Message); }
            });
        }

        public static void offerTaxiPay(Player player, Player target, int price)
        {
            if (Main.Players[player].WorkID == 3)
            {
                if (NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    if (!target.IsInVehicle || player.Position.DistanceTo(target.Position) > 2) return;
                    if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle != player.GetData<Player>("WORK") || player.Vehicle != target.Vehicle) return;
                    var vehicle = player.Vehicle;
                    if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TAXI")
                    {
                        if (price > 200 || price < 20)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете установить цену выше 200$ или ниже 20$", 3000);
                            return;
                        }
                        if (Main.Players[target].Money < price)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно средств", 3000);
                            return;
                        }

                        Plugins.Trigger.ClientEvent(target, "openDialog", "TAXI_PAY", $"Оплатить проезд за ${price}?");
                        target.SetData("TAXI_SELLER", player);
                        target.SetData("TAXI_PAY", price);

                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили игроку ({target.Value}) оплатить поездку за {price}$", 3000);
                    }
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работает в данный момент", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете таксистом", 3000);
        }

        public static void acceptTaxi(Player player, Player target)
        {
            if (Main.Players[player].WorkID == 3 && NAPI.Data.GetEntityData(player, "WORK") != null)
            {
                if (player.HasData("PASSAGER"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже взяли заказ", 3000);
                    return;
                }
                if (NAPI.Data.GetEntityData(target, "IS_CALL_TAXI") && !target.HasData("TAXI_DRIVER"))
                {
                    Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Таксист ({player.Value}) принял Ваш вызов. Оставайтесь на мест", 3000);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы приняли вызов игрока ({target.Value})", 3000);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", NAPI.Entity.GetEntityPosition(target).X, NAPI.Entity.GetEntityPosition(target).Y);

                    target.SetData("TAXI_DRIVER", player);
                    player.SetData("PASSAGER", target);

                    orderCols.Add(target, NAPI.ColShape.CreateCylinderColShape(target.Position, 10F, 10F, 0));
                    orderCols[target].SetData("PASSAGER", target);
                    orderCols[target].OnEntityExitColShape += order_onEntityExit;
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не вызывал такси или его уже приняли", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете таксистом в данный момент", 3000);
        }

        public static void cancelTaxi(Player player)
        {
            if (player.HasData("PASSAGER"))
            {
                Player passager = player.GetData<Player>("PASSAGER");
                passager.ResetData("TAXI_DRIVER");
                passager.SetData("IS_CALL_TAXI", false);
                player.ResetData("PASSAGER");
                Plugins.Notice.Send(passager, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отменили выезд к клиенту", 3000);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[passager]);
                        orderCols.Remove(passager);
                    }
                    catch { }
                });

                return;
            }
            if (NAPI.Data.GetEntityData(player, "IS_CALL_TAXI"))
            {
                NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", false);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отменили вызов такси", 3000);
                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    player.ResetData("TAXI_DRIVER");
                    Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Пассажир отменил заказ", 3000);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.ColShape.DeleteColShape(orderCols[player]);
                            orderCols.Remove(player);
                        }
                        catch { }
                    });
                }
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не вызывали такси.", 3000);
        }

        public static void callTaxi(Player player)
        {
            if (!NAPI.Data.GetEntityData(player, "IS_CALL_TAXI"))
            {
                List<Player> players = NAPI.Pools.GetAllPlayers();
                var i = 0;
                foreach (var p in players)
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID == 3 && NAPI.Data.GetEntityData(p, "WORK") != null)
                    {
                        i++;
                        NAPI.Chat.SendChatMessageToPlayer(p, $"~g~[ДИСПЕТЧЕР]: ~w~Игрок ({player.Value}) вызвал такси ~y~({player.Position.DistanceTo(p.Position)}м)~w~. Напишите ~y~/ta ~b~[ID]~w~, чтобы принять вызов");
                    }
                }
                if (i > 0)
                {
                    NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", true);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Ожидайте принятия вызова. В Вашем районе сейчас {i} таксистов. Для отмены вызова используйте /ctaxi", 3000);
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В Вашем районе сейчас нет таксистов. Попробуйте в другой раз", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже Вызвали такси. Для отмены напишите /ctaxi", 3000);
        }
    }
}
