using System;
using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;

namespace iTeffa.Fractions
{
    class MatsWar : Script
    {
        private static readonly API api = new API();
        public static bool isWar = false;
        public static int matsLeft = 15000;
        private static Marker warMarker = null;
        private static readonly Vector3 warPosition = new Vector3(33.33279, -2669.874, 5.008363);
        private static Blip warblip;
        private static string startWarTimer = null;

        private static readonly Nlogs Log = new Nlogs("MatsWar");

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(warPosition, 6, 2, 0);
                col.OnEntityEnterColShape += onEntityEnterColShape;
                col.OnEntityExitColShape += onEntityExitColShape;

                warblip = NAPI.Blip.CreateBlip(478, warPosition, 0.75F, 40, Main.StringToU16("Война за материалы"), 255, 0, true, 0, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
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
            startWarTimer = Timers.StartOnce(600000, () => startWar());
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
            catch (Exception e) { Log.Write($"EndMatsWar: " + e.Message, Nlogs.Type.Error); }
        }

        public static void interact(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            var fracid = Main.Players[player].FractionID;
            if (!((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13)))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете сделать это", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в машине", 3000);
                return;
            }
            if (!player.Vehicle.HasData("CANMATS"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"На этой машине нельзя перевозить маты", 3000);
                return;
            }
            if (player.HasData("loadMatsTimer"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже загружаете материалы в машину", 3000);
                return;
            }
            var count = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
            if (count >= Stocks.maxMats[player.Vehicle.DisplayName])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В машине максимальное кол-во материала", 3000);
                return;
            }
            player.SetData("loadMatsTimer", Timers.StartOnce(20000, () => Fractions.Realm.Army.loadMaterialsTimer(player)));
            player.Vehicle.SetData("loaderMats", player);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Загрузка материалов началась (20 секунд)", 3000);
            Plugins.Trigger.ClientEvent(player, "showLoader", "Загрузка материалов", 1);
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
            catch (Exception ex) { Log.Write("onEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
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
                        Timers.Stop(player.GetData<string>("loadMatsTimer"));
                        NAPI.Data.ResetEntityData(entity.Vehicle, "loaderMats");
                        player.ResetData("loadMatsTimer");
                        Plugins.Trigger.ClientEvent(player, "hideLoader");
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                    }
                }
            }
            catch (Exception ex) { Log.Write("onEntityExitColShape: " + ex.Message, Nlogs.Type.Error); }
        }
    }
}
