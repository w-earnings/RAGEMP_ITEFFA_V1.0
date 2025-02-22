﻿using iTeffa.Globals;
using iTeffa.Settings;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTeffa.Working
{
    class Collector : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Collector");
        private static readonly int checkpointPayment = 7;

        private static readonly Vector3 TakeMoneyPos = new Vector3(915.9069, -1265.255, 24.50912);

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(TakeMoneyPos, 1, 3, 0);
                col.OnEntityEnterColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 45);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                };
                col.OnEntityExitColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                };
                NAPI.Marker.CreateMarker(1, TakeMoneyPos - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E to take money bags"), TakeMoneyPos + new Vector3(0, 0, 0.3), 30f, 0.4f, 0, new Color(255, 255, 255), true, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void collectorCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 7);
                NAPI.Data.SetEntityData(veh, "TYPE", "COLLECTOR");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
            }
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
                NAPI.Data.SetEntityData(veh, "WORK", 7);
                NAPI.Data.SetEntityData(veh, "TYPE", "COLLECTOR");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                Globals.VehicleStreaming.SetEngineState(veh, false);
                Globals.VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("respawnCar: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "COLLECTOR" || player.VehicleSeat != 0) return;
                if (Main.Players[player].WorkID == 7)
                {
                    if (player.HasData("WORKOBJECT"))
                    {
                        BasicSync.DetachObject(player);
                        player.ResetData("WORKOBJECT");
                    }
                    if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (Main.Players[player].Money >= 100) Plugins.Trigger.ClientEvent(player, "openDialog", "COLLECTOR_RENT", "Вы действительно хотите начать работу инкассатором и арендовать транспорт за $100?");
                            else {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас не хватает " + (100 - Main.Players[player].Money) + "$ на аренду автобуса", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                    else
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Эта машина занята", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        else NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете инкассатором. Устроиться можно в мэрии", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "COLLECTOR" &&
                Main.Players[player].WorkID == 7 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    if (!player.HasData("WORKOBJECT") && player.GetData<int>("COLLECTOR_BAGS") > 0)
                    {
                        BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_money_bag_01"), 18905, new Vector3(0.55, 0.02, 0), new Vector3(0, -90, 0));
                        player.SetData("WORKOBJECT", true);
                    }

                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Если Вы не сядете в транспорт через 3 минуты, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, Plugins.Logs.Type.Error); }
        }
        
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 7 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    respawnCar(vehicle);

                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы закончили рабочий день", 3000);
                    NAPI.Data.SetEntityData(player, "PAYMENT", 0);

                    NAPI.Data.SetEntityData(player, "ON_WORK", false);
                    NAPI.Data.SetEntityData(player, "WORK", null);
                    Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 16, 0);
                    Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                    Customization.ApplyCharacter(player);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                    }
                }
                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == 7 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    respawnCar(vehicle);
                }
                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Plugins.Logs.Type.Error); }
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
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 180)
                    {
                        respawnCar(vehicle);

                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы закончили рабочий день", 3000);
                        NAPI.Data.SetEntityData(player, "PAYMENT", 0);

                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteCheckpoint", 16, 0);
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteWorkBlip");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Customization.ApplyCharacter(player);

                        if (player.HasData("WORKOBJECT"))
                        {
                            BasicSync.DetachObject(player);
                            player.ResetData("WORKOBJECT");
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle_Collector:\n" + e.ToString(), Plugins.Logs.Type.Error);
                }
            });
        }

        public static void rentCar(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle.GetData<string>("TYPE") != "COLLECTOR") return;

            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы начали работу инкассатором. Развезите деньги по банкоматам.", 3000);
            Modules.Wallet.Change(player, -100);
            Loggings.Money($"player({Main.Players[player].UUID})", $"server", 100, $"collectorRent");
            var vehicle = player.Vehicle;
            NAPI.Data.SetEntityData(player, "WORK", vehicle);
            player.SetData("ON_WORK", true);
            Globals.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
            player.SetData("COLLECTOR_BAGS", 15);
            player.SetData("W_LASTPOS", player.Position);
            player.SetData("W_LASTTIME", DateTime.Now);

            var x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1); ;
            while (x == 36 || Finance.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1);
            player.SetData("WORKCHECK", x);
            if (Main.Players[player].Gender)
            {
                Customization.SetHat(player, 63, 9);
                player.SetClothes(11, 132, 0);
                player.SetClothes(4, 33, 0);
                player.SetClothes(6, 24, 0);
                player.SetClothes(9, 1, 1);
                player.SetClothes(8, 129, 0);
                player.SetClothes(3, Customization.CorrectTorso[true][132], 0);
            }
            else
            {
                Customization.SetHat(player, 63, 9);
                player.SetClothes(11, 129, 0);
                player.SetClothes(4, 32, 0);
                player.SetClothes(6, 24, 0);
                player.SetClothes(9, 6, 1);
                player.SetClothes(8, 159, 0);
                player.SetClothes(3, Customization.CorrectTorso[false][129], 0);
            }
            Plugins.Trigger.ClientEvent(player, "createCheckpoint", 16, 29, Finance.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
            Plugins.Trigger.ClientEvent(player, "createWaypoint", Finance.ATM.ATMs[x].X, Finance.ATM.ATMs[x].Y);
            Plugins.Trigger.ClientEvent(player, "createWorkBlip", Finance.ATM.ATMs[x]);
        }

        public static void CollectorTakeMoney(Player player)
        {
            if (player.IsInVehicle || Main.Players[player].WorkID != 7 || !player.GetData<bool>("ON_WORK")) return;
            if (player.GetData<int>("COLLECTOR_BAGS") != 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас ещё остались мешки с деньгами ({player.GetData<int>("COLLECTOR_BAGS")}шт)", 3000);
                return;
            }
            else
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы взяли новые мешки с деньгами.", 3000);
                player.SetData("COLLECTOR_BAGS", 15);

                var x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1);
                while (x == 36 || Finance.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                    x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1);

                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);
                player.SetData("WORKCHECK", x);
                Plugins.Trigger.ClientEvent(player, "createCheckpoint", 16, 29, Finance.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
                Plugins.Trigger.ClientEvent(player, "createWaypoint", Finance.ATM.ATMs[x].X, Finance.ATM.ATMs[x].Y);
                Plugins.Trigger.ClientEvent(player, "createWorkBlip", Finance.ATM.ATMs[x]);
            }
        }
        public static void CollectorEnterATM(Player player, ColShape shape)
        {
            try
            {
                if (player.IsInVehicle || Main.Players[player].WorkID != 7 || !player.GetData<bool>("ON_WORK") 
                    || player.GetData<int>("COLLECTOR_BAGS") == 0 || player.GetData<int>("WORKCHECK") != shape.GetData<int>("NUMBER")) return;
                player.SetData("COLLECTOR_BAGS", player.GetData<int>("COLLECTOR_BAGS") - 1);

                var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / 100);
                var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Банкомат ещё полон. Попробуйте позже", 3000);
                    return;
                }

                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);
                Modules.Wallet.Change(player, payment);
                Loggings.Money($"server", $"player({Main.Players[player].UUID})", payment, $"collectorCheck");

                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }

                if (player.GetData<int>("COLLECTOR_BAGS") == 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "Возвращайтесь на базу, чтобы взять новые мешки с деньгами", 3000);
                    Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", TakeMoneyPos.X, TakeMoneyPos.Y);
                    Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 16);
                    return;
                }
                else
                {
                    var x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1); ;
                    while (x == 36 || x == player.GetData<int>("WORKCHECK") || Finance.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                        x = WorkManager.rnd.Next(0, Finance.ATM.ATMs.Count - 1);
                    player.SetData("WORKCHECK", x);
                    Plugins.Trigger.ClientEvent(player, "createCheckpoint", 16, 29, Finance.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", Finance.ATM.ATMs[x].X, Finance.ATM.ATMs[x].Y);
                    Plugins.Trigger.ClientEvent(player, "createWorkBlip", Finance.ATM.ATMs[x]);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Направляйтесь к следующему банкомату.", 3000);
                }
            } catch { }
        }
    }
}
