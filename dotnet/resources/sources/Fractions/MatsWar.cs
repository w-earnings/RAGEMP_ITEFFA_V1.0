﻿using System;
using GTANetworkAPI;
using iTeffa.Kernel;
using iTeffa.Settings;

namespace iTeffa.Fractions
{
    class MatsWar : Script
    {
        private static API api = new API();
        public static bool isWar = false;
        public static int matsLeft = 15000;
        private static Marker warMarker = null;
        private static Vector3 warPosition = new Vector3(33.33279, -2669.874, 5.008363);
        private static Blip warblip;
        private static string startWarTimer = null;

        private static nLog Log = new nLog("MatsWar");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(warPosition, 6, 2, 0);
                col.OnEntityEnterColShape += onEntityEnterColShape;
                col.OnEntityExitColShape += onEntityExitColShape;

                warblip = NAPI.Blip.CreateBlip(478, warPosition, 1, 40, Main.StringToU16("Война за материалы"), 255, 0, true, 0, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void startMatWarTimer()
        {
            Manager.sendFractionMessage(1, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(2, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(3, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(4, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(5, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(10, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(11, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(12, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            Manager.sendFractionMessage(13, "Через 10 минут в порт Los-Santos прибудет корабль с материалами.");
            //startWarTimer = Main.StartT(600000, 99999999, (o) => startWar(), "STARTMATWAR_TIMER");
            startWarTimer = Timers.StartOnce(600000, () => startWar()); //600000
        }

        public static void startWar()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (isWar) return;
                    matsLeft = 15000;
                    warMarker = NAPI.Marker.CreateMarker(1, warPosition - new Vector3(0, 0, 5), new Vector3(), new Vector3(), 6f, new Color(155, 0, 0, 255));
                    isWar = true;
                    Manager.sendFractionMessage(1, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(2, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(3, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(4, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(5, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(10, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(11, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(12, "Корабль с материалами прибыл в порт Los-Santos.");
                    Manager.sendFractionMessage(13, "Корабль с материалами прибыл в порт Los-Santos.");
                    warblip.Color = 49;
                    //Main.StopT(startWarTimer, "timer_11");
                }
                catch { }
            });
        }

        public static void endWar()
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    NAPI.Entity.DeleteEntity(warMarker);
                    isWar = false;
                    Manager.sendFractionMessage(1, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(2, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(3, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(4, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(5, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(10, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(11, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(12, "Корабль ушел из порта Los-Santos.");
                    Manager.sendFractionMessage(13, "Корабль ушел из порта Los-Santos.");
                    warblip.Color = 49;
                });
            }
            catch (Exception e) { Log.Write($"EndMatsWar: " + e.Message, nLog.Type.Error); }
        }

        public static void interact(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            var fracid = Main.Players[player].FractionID;
            if (!((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13)))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                return;
            }
            if (!player.Vehicle.HasData("CANMATS"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На этой машине нельзя перевозить маты", 3000);
                return;
            }
            if (player.HasData("loadMatsTimer"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже загружаете материалы в машину", 3000);
                return;
            }
            var count = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
            if (count >= Fractions.Stocks.maxMats[player.Vehicle.DisplayName])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                return;
            }
            //player.SetData("loadMatsTimer", Main.StartT(20000, 99999999, (o) => Fractions.Army.loadMaterialsTimer(player), "GMLOADMATS_TIMER"));
            player.SetData("loadMatsTimer", Timers.StartOnce(20000, () => Fractions.Army.loadMaterialsTimer(player)));
            player.Vehicle.SetData("loaderMats", player);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Загрузка материалов началась (20 секунд)", 3000);
            Trigger.ClientEvent(player, "showLoader", "Загрузка материалов", 1);
            player.SetData("vehicleMats", player.Vehicle);
            player.SetData("whereLoad", "WAR");
            return;
        }

        private void onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                if (!isWar) return;
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Player) return;
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 37);
            }
            catch (Exception ex) { Log.Write("onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Player)
                {
                    NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    if (entity.IsInVehicle && NAPI.Data.HasEntityData(entity.Vehicle, "loaderMats"))
                    {
                        Player player = NAPI.Data.GetEntityData(entity.Vehicle, "loaderMats");
                        //Main.StopT(player.GetData("loadMatsTimer"), "timer_12");
                        Timers.Stop(player.GetData<string>("loadMatsTimer"));
                        NAPI.Data.ResetEntityData(entity.Vehicle, "loaderMats");
                        player.ResetData("loadMatsTimer");
                        Trigger.ClientEvent(player, "hideLoader");
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                    }
                }
            }
            catch (Exception ex) { Log.Write("onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
    }
}