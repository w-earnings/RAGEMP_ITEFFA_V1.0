using GTANetworkAPI;
using System.Collections.Generic;
using iTeffa.Interface;
using System;
using iTeffa.Globals;
using iTeffa.Settings;

namespace iTeffa.Working
{
    class AutoMechanic : Script
    {
        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void mechanicCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
            }
        }
        private static readonly Nlogs Log = new Nlogs("Mechanic");

        private static readonly int mechanicRentCost = 100;
        private static readonly Dictionary<Player, ColShape> orderCols = new Dictionary<Player, ColShape>();

        public static void mechanicRepair(Player player, Player target, int price)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работает автомехаником", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в рабочем транспорте", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок должен находиться в транспортном средстве", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
                return;
            }
            if (price < 50 || price > 300)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы можете установить цену от 50$ до 300$", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно денег", 3000);
                return;
            }
            
            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", price);
            Trigger.ClientEvent(target, "openDialog", "REPAIR_CAR", $"Игрок ({player.Value}) предложил отремонтировать Ваш транспорт за ${price}");
            
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили игроку ({target.Value}) отремонтировать транспорт за {price}$", 3000);
        }

        public static void mechanicRent(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle.GetData<string>("TYPE") != "MECHANIC") return;

            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы арендовали рабочий транспорт. Ожидайте заказ", 3000);
            Finance.Wallet.Change(player, -mechanicRentCost);
            Loggings.Money($"player({Main.Players[player].UUID})", $"server", mechanicRentCost, $"mechanicRent");
            var vehicle = player.Vehicle;
            NAPI.Data.SetEntityData(player, "WORK", vehicle);
            Globals.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(player, "ON_WORK", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
        }

        public static void mechanicPay(Player player)
        {
            if (!player.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в транспортном средстве", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            if (Main.Players[player].Money < price)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас недостаточно средств", 3000);
                return;
            }

            VehicleManager.RepairCar(player.Vehicle);
            var driver = NAPI.Data.GetEntityData(player, "MECHANIC");
            Finance.Wallet.Change(player, -price);
            Finance.Wallet.Change(driver, price);
            Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price, $"mechanicRepair");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы оплатили ремонт Вашего транспортного средства", 3000);
            Plugins.Notice.Send(driver, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) оплатил ремонт", 3000);
            Commands.Controller.RPChat("me", driver, $"починил автомобиль");

            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }

        private static void order_onEntityExit(ColShape shape, Player player)
        {
            if (shape.GetData<Player>("MECHANIC_CLIENT") != player) return;

            if (player.HasData("MECHANIC_DRIVER"))
            {
                Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                driver.ResetData("MECHANIC_CLIENT");
                player.ResetData("MECHANIC_DRIVER");
                player.SetData("IS_CALL_MECHANIC", false);
                Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Клиент отменил заказ", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы покинули место вызова автомеханика", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[player]);
                    orderCols.Remove(player);
                }
                catch { }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void Event_onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "MECHANIC") return;
                if (seatid == 0)
                {
                    if (!Main.Players[player].Licenses[1])
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет лицензии категории B", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 8)
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (vehicle.GetData<Player>("DRIVER") != null)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Этот рабочий транспорт уже занят", 3000);
                                return;
                            }
                            if (Main.Players[player].Money >= mechanicRentCost)
                            {
                                Trigger.ClientEvent(player, "openDialog", "MECHANIC_RENT", $"Арендовать рабочий транспорт за ${mechanicRentCost}?");
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас не хватает " + (mechanicRentCost - Main.Players[player].Money) + "$ на аренду рабочего транспорта", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle) NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже работаете", 3000);
                    }
                    else
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете автомехаником. Устроиться можно в мэрии", 3000);
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
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("RespawnCar: " + e.Message, Nlogs.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Клиент отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
                if ((Main.Players[player].WorkID == 8 && NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "WORK") != null))
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnCar(vehicle);
                    if (player.HasData("MECHANIC_CLIENT"))
                    {
                        Player client = player.GetData<Player>("MECHANIC_CLIENT");
                        client.ResetData("MECHANIC_DRIVER");
                        client.SetData("IS_CALL_MECHANIC", false);
                        Plugins.Notice.Send(client, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                        try
                        {
                            NAPI.ColShape.DeleteColShape(orderCols[client]);
                            orderCols.Remove(client);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" &&
                Main.Players[player].WorkID == 8 &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, Nlogs.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try {
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
                        if (player.HasData("MECHANIC_CLIENT"))
                        {
                            Player client = player.GetData<Player>("MECHANIC_CLIENT");
                            client.ResetData("MECHANIC_DRIVER");
                            client.SetData("IS_CALL_MECHANIC", false);
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                            player.ResetData("MECHANIC_CLIENT");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[client]);
                                orderCols.Remove(client);
                            }
                            catch { }
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle:\n" + e.ToString(), Nlogs.Type.Error);
                }
            });
        }

        public static void acceptMechanic(Player player, Player target)
        {
            if (Main.Players[player].WorkID == 8 && player.GetData<bool>("ON_WORK"))
            {
                if (player.HasData("MECHANIC_CLIENT"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже взяли заказ", 3000);
                    return;
                }
                if (NAPI.Data.GetEntityData(target, "IS_CALL_MECHANIC") && !target.HasData("MECHANIC_DRIVER"))
                {
                    Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) принял Ваш вызов. Оставайтесь на мест", 3000);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы приняли вызов игрока ({target.Value})", 3000);
                    Trigger.ClientEvent(player, "createWaypoint", NAPI.Entity.GetEntityPosition(target).X, NAPI.Entity.GetEntityPosition(target).Y);

                    target.SetData("MECHANIC_DRIVER", player);
                    player.SetData("MECHANIC_CLIENT", target);

                    orderCols.Add(target, NAPI.ColShape.CreateCylinderColShape(target.Position, 10F, 10F, 0));
                    orderCols[target].SetData("MECHANIC_CLIENT", target);
                    orderCols[target].OnEntityExitColShape += order_onEntityExit;
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не вызывал автомеханика", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете автомехаником в данный момент", 3000);
        }

        public static void cancelMechanic(Player player)
        {
            if (player.HasData("MECHANIC_CLIENT"))
            {
                Player client = player.GetData<Player>("MECHANIC_CLIENT");
                client.ResetData("MECHANIC_DRIVER");
                client.SetData("IS_CALL_MECHANIC", false);
                player.ResetData("MECHANIC_CLIENT");
                Plugins.Notice.Send(client, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Автомеханик покинул рабочее место, сделайте новый заказ", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отменили выезд к клиенту", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[client]);
                    orderCols.Remove(client);
                }
                catch { }
                return;
            }
            if (NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", false);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отменили вызов автомеханика", 3000);
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    player.ResetData("MECHANIC_DRIVER");
                    Plugins.Notice.Send(driver, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Клиент отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не вызывали автомеханика.", 3000);
        }

        public static void callMechanic(Player player)
        {
            if (!NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                List<Player> players = NAPI.Pools.GetAllPlayers();
                var i = 0;
                foreach (var p in players)
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID == 8 && NAPI.Data.GetEntityData(p, "ON_WORK"))
                    {
                        i++;
                        NAPI.Chat.SendChatMessageToPlayer(p, $"~g~[ДИСПЕТЧЕР]: ~w~Игрок ({player.Value}) вызвал автомеханика ~y~({player.Position.DistanceTo(p.Position)}м)~w~. Напишите ~y~/ma ~b~[ID]~w~, чтобы принять вызов");
                    }
                }
                if (i > 0)
                {
                    NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", true);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Ожидайте принятия вызова. В Вашем районе сейчас {i} автомехаников. Для отмены вызова используйте /cmechanic", 3000);
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В Вашем районе сейчас нет автомехаников. Попробуйте в другой раз", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже вызвали автомеханика. Для отмены напишите /cmechanic", 3000);
        }

        public static void buyFuel(Player player, int fuel)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK") || !player.IsInVehicle || player.GetData<Vehicle>("WORK") != player.Vehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны работать автомехаником и находиться в рабочей машине", 3000);
                return;
            }
            if (player.GetData<int>("BIZ_ID") == -1 || BusinessManager.BizList[player.GetData<int>("BIZ_ID")].Type != 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться на заправке", 3000);
                return;
            }
            if (fuel <= 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введите корректные данные", 3000);
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            if (Main.Players[player].Money < biz.Products[0].Price * fuel)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств", 3000);
                return;
            }
            if (player.Vehicle.GetSharedData<int>("FUELTANK") + fuel > 1000)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Бак с бензином полон", 3000);
                return;
            }
            if (!BusinessManager.takeProd(biz.ID, fuel, biz.Products[0].Name, biz.Products[0].Price * fuel))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно топлива на заправке", 3000);
                return;
            }
            Finance.Wallet.Change(player, -biz.Products[0].Price * fuel);
            Loggings.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price * fuel, $"mechanicBuyFuel");
            player.Vehicle.SetSharedData("FUELTANK", player.Vehicle.GetSharedData<int>("FUELTANK") + fuel);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы пополнили бак в вашей рабочей машине до {player.Vehicle.GetSharedData<int>("FUELTANK")}л", 3000);
        }

        public static void mechanicFuel(Player player, Player target, int fuel, int pricePerLitr)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работает автомехаником", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в рабочем транспорте", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок должен находиться в транспортном средстве", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
                return;
            }
            if (fuel < 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете продать меньше литра", 3000);
                return;
            }
            if (pricePerLitr < 2 || pricePerLitr > 10)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы можете установить цену от 2$ до 10$ за литр", 3000);
                return;
            }
            if (Main.Players[target].Money < pricePerLitr * fuel)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно денег", 3000);
                return;
            }
            
            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", pricePerLitr);
            target.SetData("MECHANIC_FEUL", fuel);
            Trigger.ClientEvent(target, "openDialog", "FUEL_CAR", $"Игрок ({player.Value}) предложил заправить Ваш транспорт на {fuel}л за ${fuel * pricePerLitr}");
            
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили игроку ({target.Value}) заправить транспорт на {fuel}л за {fuel * pricePerLitr}$.", 3000);
        }

        public static void mechanicPayFuel(Player player)
        {
            if (!player.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в транспортном средстве", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            var fuel = NAPI.Data.GetEntityData(player, "MECHANIC_FEUL");
            if (Main.Players[player].Money < price * fuel)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас недостаточно средств", 3000);
                return;
            }

            Player driver = NAPI.Data.GetEntityData(player, "MECHANIC");

            if (!driver.IsInVehicle || !driver.Vehicle.HasData("TYPE") || driver.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Механик должен находиться в транспортном средстве", 3000);
                return;
            }

            if (driver.Vehicle.GetSharedData<object>("FUELTANK") < fuel)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У механика недостаточно топлива, чтобы заправить Вас", 3000);
                return;
            }

            Finance.Wallet.Change(player, -price * fuel);
            Finance.Wallet.Change(driver, price * fuel);
            Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price * fuel, $"mechanicFuel");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы оплатили ремонт заправку транспортного средства", 3000);
            Plugins.Notice.Send(driver, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) оплатил заправку транспорта", 3000);
            Commands.Controller.RPChat("me", driver, $"заправил транспортное средство");

            var carFuel = (player.Vehicle.GetSharedData<object>("PETROL") + fuel > player.Vehicle.GetSharedData<object>("MAXPETROL")) ? player.Vehicle.GetSharedData<object>("MAXPETROL") : player.Vehicle.GetSharedData<object>("PETROL") + fuel;
            player.Vehicle.SetSharedData("PETROL", carFuel);
            driver.Vehicle.SetSharedData("FUELTANK", driver.Vehicle.GetSharedData<object>("FUELTANK") - fuel);
            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }
    }
}
