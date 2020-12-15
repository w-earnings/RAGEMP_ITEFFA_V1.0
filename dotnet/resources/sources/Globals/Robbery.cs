using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Timers;
using iTeffa.Finance;
using iTeffa.Fractions;
using iTeffa.Interface;
using iTeffa.Settings;
using iTeffa.Globals.Character;

using iTeffa.Plugins;
using iTeffa.Models;
using iTeffa.Globals;

namespace iTeffa.Globals
{
    public class SafeMain : Script
    {
        // config, use meta.xml instead
        public static int SafeRespawnTime = 10800;
        public static int SafeMinLoot = 150;
        public static int SafeMaxLoot = 500;
        public static string SafeDir = "Safes";
        public static int MaxMoneyInBag = 100000;
        public static DateTime NextRobbery = new DateTime();
        public static int NowRobberyID = -1;
        // main safe door
        public static bool isCracking = false;
        private static TextLabel label;
        private static int secondsLeft = 0;
        public static bool isOpen = false;
        public static bool canBeClosed = true;
        private static GTANetworkAPI.Object safeDrill;
        private static string timer = null;

        // other variables

        public static List<Safe> Safes = new List<Safe>();
        public static Random SafeRNG = new Random();

        public static List<Vector3> moneyFlowPoints = new List<Vector3>()
        {
            new Vector3(1395.184, 3613.144, 34.9892),
            new Vector3(166.6278, 2229.249, 90.87845),
            new Vector3(2887.687, 4387.17, 50.85578),
            new Vector3(2192.614, 5596.246, 53.89177),
            new Vector3(-215.4299, 6445.921, 31.43351),
        };
        private static List<string> moneyFlowers = new List<string>()
        {
            "Sergey Mavrodi",
            "Jonny Evreyski",
            "Vladimr Nitup",
            "Ryder Smokejohnson",
            "Ostap Bender",
        };

        public object LogCat { get; private set; }
        private static nLog Log = new nLog("SafeCracker");

        #region Methods
        public static bool IsIDInUse(int ID)
        {
            return (Safes.FirstOrDefault(s => s.ID == ID) != null);
        }

        public static Vector3 XYInFrontOfPoint(Vector3 pos, float angle, float distance)
        {
            angle *= (float)Math.PI / 180;
            pos.X += (distance * (float)Math.Sin(-angle));
            pos.Y += (distance * (float)Math.Cos(-angle));
            return pos;
        }
        #endregion

        #region Safe Methods
        public static void CreateSafe(int i, Vector3 position, float rotation, int minamount, int maxamount, string address)
        {

            // create entity
            Safe new_safe = new Safe(i, position, rotation, minamount, maxamount, address);
            Safes.Add(new_safe);
            var string_pos = JsonConvert.SerializeObject(position);
            Connect.Query($"INSERT INTO safes (minamount, maxamount, pos, address, rotation, idkey) VALUES ({minamount}, {maxamount}, '{string_pos}', '{address}', {rotation})");
            new_safe.Create();
        }

        public static void RemoveSafe(int ID)
        {
            // verify safe
            Safe safe = Safes.FirstOrDefault(s => s.ID == ID);
            if (safe == null) return;

            // destroy entity
            safe.Destroy(true);
            Safes.Remove(safe);

            Connect.Query($"DELETE FROM safes WHERE id={ID}");
        }
        #endregion

        #region Events
        public static void startSafeDoorCracking(Player player)
        {
            if (!player.HasData("HEIST_DRILL"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет дрели для взлома", 3000);
                return;
            }
            if (isCracking)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Взлом уже начат", 3000);
                return;
            }
            if (isOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Дверь в хранилище уже открыта", 3000);
                return;
            }
            nInventory.Remove(player, ItemType.BagWithDrill, 1);
            player.SetClothes(5, 0, 0);
            player.ResetData("HEIST_DRILL");
            isCracking = true;
            safeDrill = NAPI.Object.CreateObject(-443429795, new Vector3(253.9534, 225.2, 102.22), new Vector3(0, 0, -18), 255, 0);
            label = NAPI.TextLabel.CreateTextLabel("~r~8:00", new Vector3(253.9534, 225.2, 102.22), 4F, 0.3F, 0, new Color(255, 255, 255));
            secondsLeft = 480;  
            timer = Timers.StartTask("DoorCracking", 1000, () => NAPI.Task.Run(() => updateDoorCracking()));
            canBeClosed = false;
            Manager.sendFractionMessage(6, "Кто-то пытается взломать дверь в хранилище мэрии.");
            Manager.sendFractionMessage(7, "Кто-то пытается взломать дверь в хранилище мэрии.");
            Manager.sendFractionMessage(9, "Кто-то пытается взломать дверь в хранилище мэрии.");
            Manager.sendFractionMessage(14, "Кто-то пытается взломать дверь в хранилище мэрии.");
        }

        public static void lockCrack(Player player, string name)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player == null) return;
                    Doormanager.SetDoorLocked(player.GetData<int>("DOOR"), false, 0);
                    Trigger.ClientEvent(player, "hideLoader");
                    player.ResetData("LOCK_TIMER");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно взломали дверь", 3000);
                }
                catch { }
            });
        }

        private static void updateDoorCracking()
        {
            secondsLeft--;
            if (secondsLeft == 0)
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        NAPI.Entity.DeleteEntity(label);
                        NAPI.Entity.DeleteEntity(safeDrill);
                    }
                    catch { }
                });
                isCracking = false;
                Timers.StartOnce("bankTimer", 600000, () =>
                {
                    canBeClosed = true;
                });
                Doormanager.SetDoorLocked(2, true, 0.5f);
                Timers.Stop(timer);
                return;
            }
            int minutes = secondsLeft / 60;
            int seconds = 00;
            seconds = secondsLeft % 60;
            label.Text = $"~r~{minutes}:{seconds}";

        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if ((player.HasData("HAND_MONEY") || player.HasData("HEIST_DRILL")) && player.VehicleSeat == -1 && vehicle.Class != 8)
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете сесть в машину с сумками", 3000);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.ResourceStart)]
        public void SafeCracker_Init()
        {
            try
            {
                for (int b = 0; b < moneyFlowPoints.Count; b++)
                {
                    var flowShape = NAPI.ColShape.CreateCylinderColShape(moneyFlowPoints[b], 1.5F, 2, 0);
                    flowShape.OnEntityEnterColShape += narkosale_onEntityEnterColShape;
                    flowShape.OnEntityExitColShape += narkosale_onEntityExitColShape;

                    var flowBlip = NAPI.Blip.CreateBlip(440, moneyFlowPoints[b], 0.75F, 58, Main.StringToU16("Черный рынок"));
                    NAPI.Entity.SetEntityDimension(flowBlip, 0);
                    flowBlip.ShortRange = true;

                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"Нажмите Е\n~b~{moneyFlowers[b]}"), moneyFlowPoints[b] + new Vector3(0, 0, 1.125), 5F, 0.8F, 0, new Color(255, 255, 255));
                }

                var result = Connect.QueryRead($"SELECT * FROM safes");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                int i = 0;
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 safePos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    float safeRot = Convert.ToSingle(Row["rotation"]);

                    Safe safe = new Safe(i, safePos, safeRot, Convert.ToInt32(Row["minamount"]), Convert.ToInt32(Row["maxamount"]), Row["address"].ToString());
                    Safes.Add(safe);
                    safe.Create();
                    i++;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"SAFEMAIN_INIT\":\n" + e.ToString(), nLog.Type.Error);
            }

        }

        public void narkosale_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 14);
            }
            catch (Exception e) { Log.Write("narkosale_onEntityEnterColShape: " + e.ToString(), nLog.Type.Error); }
        }

        public void narkosale_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("narkosale_onEntityExitColShape: " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("dialPress")]
        public static void openSafe(Player player, params object[] arguments)
        {
            try
            {
                if (!player.HasData("temp_SafeID")) return;

                Safe safe = Safes.FirstOrDefault(s => s.ID == player.GetData<int>("temp_SafeID"));
                if (safe == null) return;

                if (!player.HasData("CURRENT_STAGE"))
                {
                    Trigger.ClientEvent(player, "dial", "close");
                    return;
                }

                if (!(bool)arguments[0])
                {
                    Trigger.ClientEvent(player, "dial", "close");
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Неправильный пароль", 2000);
                    nInventory.Remove(player, ItemType.Lockpick, 1);
                    safe.Occupier = null;
                }
                else
                {
                    int stage = player.GetData<int>("CURRENT_STAGE");
                    if (stage == 2)
                    {
                        safe.SafeLoot = SafeRNG.Next(safe.MinAmount * Main.oldconfig.PaydayMultiplier, safe.MaxAmount * Main.oldconfig.PaydayMultiplier);
                        safe.SetDoorOpen(true);
                        safe.Occupier = null;
                        nInventory.Remove(player, ItemType.Lockpick, 1);
                        Trigger.ClientEvent(player, "dial", "close");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно взломали сейф", 2000);
                    }
                    else
                    {
                        stage++;
                        player.SetData("CURRENT_STAGE", stage);
                        Trigger.ClientEvent(player, "dial", "open", safe.LockAngles[stage], true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы подобрали {stage} из 3 паролей", 2000);
                    }
                }
            }
            catch (Exception e) { Log.Write("dialPressed: " + e.Message, nLog.Type.Error); }
        }

        public static void interactSafe(Player player)
        {
            if (!player.HasData("temp_SafeID")) return;

            Safe safe = Safes.FirstOrDefault(s => s.ID == player.GetData<int>("temp_SafeID"));
            if (safe == null) return;

            if (safe.IsOpen)
            {
                safe.Loot(player);
            }
            else
            {
                if (!player.HasSharedData("IS_MASK") || !player.GetData<bool>("IS_MASK"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Взлом возможен только в маске", 3000);
                    return;
                }

                if (safe.Occupier != null && NAPI.Player.GetPlayerFromHandle(safe.Occupier) != null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Этот сейф уже взламывают", 3000);
                    return;
                }
                if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] != 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Доступно только для банд", 3000);
                    return;
                }
                if (DateTime.Now.Hour < 13 || DateTime.Now.Hour > 22)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Возможно открыть только с 13:00 до 23:00", 3000);
                    return;
                }

                var lockpick = nInventory.Find(Main.Players[player].UUID, ItemType.Lockpick);
                var count = (lockpick == null) ? 0 : lockpick.Count;
                if (count == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет отмычки", 3000);
                    return;
                }
                if (safe.Address != "Мэрия")
                {
                    if (DateTime.Now < NextRobbery && NowRobberyID != safe.ID)
                    {
                        DateTime g = new DateTime((NextRobbery - DateTime.Now).Ticks);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Попробуйте через {g.Minute}:{g.Second}", 3000);
                        return;
                    }
                }

                var nearestPlayers = Main.GetPlayersInRadiusOfPosition(player.Position, 7);
                var gangsters = 0;
                foreach (var p in nearestPlayers)
                {
                    if (p == null || !Main.Players.ContainsKey(p) || player == p) continue;
                    if (Fractions.Manager.FractionTypes[Main.Players[p].FractionID] == 1) gangsters++;
                }

                if (gangsters == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"С Вами должен быть как минимум ещё один тру гангстер", 3000);
                }

                safe.Occupier = player;
                player.SetData("CURRENT_STAGE", 0);
                Trigger.ClientEvent(player, "dial", "open", safe.LockAngles[0]);
                Notify.Send(player, NotifyType.Warning, NotifyPosition.TopCenter, $"С минуты на минуту сюда прибудут копы", 3000);
                Manager.sendFractionMessage(7, $"Сейф по адресу {safe.Address} пытаются взломать");
                Manager.sendFractionMessage(9, $"Сейф по адресу {safe.Address} пытаются взломать");

                if (safe.Address != "Мэрия")
                {
                    if (NowRobberyID != safe.ID) NextRobbery = DateTime.Now.AddMinutes(15);
                    NowRobberyID = safe.ID;
                }

                if (DateTime.Now >= safe.BlipSet)
                {
                    safe.Blip = NAPI.Blip.CreateBlip(0, safe.Position, 1, 59, "Ограбление", 0, 0, true, 0, 0);
                    safe.Blip.Transparency = 0;
                    foreach (var p in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (Main.Players[p].FractionID != 7 && Main.Players[p].FractionID != 9) continue;

                        Trigger.ClientEvent(p, "changeBlipAlpha", safe.Blip, 255);
                        Trigger.ClientEvent(p, "createWaypoint", safe.Position.X, safe.Position.Y);
                    }
                    safe.BlipSet = DateTime.Now.AddMinutes(15);
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (safe.Blip != null) safe.Blip.Delete();
                        }
                        catch { }
                    }, 900000);
                }

                if (player.HasSharedData("IS_MASK") && !player.GetSharedData<bool>("IS_MASK"))
                {
                    var wantedLevel = new WantedLevel(4, "Полиция", DateTime.Now, "Ограбление сейфа");
                    Police.setPlayerWantedLevel(player, wantedLevel);
                }
            }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 14:
                    OpenMoneyFlowMenu(player);
                    NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
                    break;
            }
        }

        public static void dropMoneyBag(Player player)
        {
            if (Main.Players[player].InsideHouseID != -1 || Main.Players[player].InsideGarageID != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете сделать это, находясь в доме/гараже", 3000);
                return;
            }

            var pos = NAPI.Entity.GetEntityPosition(player);

            var money = -1;
            foreach (var item in nInventory.Items[Main.Players[player].UUID])
            {
                if (item.Type != ItemType.BagWithMoney) continue;
                money = Convert.ToInt32(item.Data);
                nInventory.Remove(player, item);
                break;
            }
            if (money == -1) return;

            player.SetClothes(5, 0, 0);
            var money_bag = NAPI.Object.CreateObject(-711724000, player.Position + new Vector3(0, 0, -1.15), player.Rotation + new Vector3(90, 0, 0), 255, 0);
            money_bag.SetSharedData("TYPE", "MoneyBag");
            money_bag.SetSharedData("PICKEDT", false);
            NAPI.Data.SetEntityData(money_bag, "MONEY_IN_BAG", money);

            player.ResetData("HAND_MONEY");
            GameLog.Items($"player({Main.Players[player].UUID})", "ground", Convert.ToInt32(ItemType.BagWithMoney), 1, $"{money}");
        }

        public static void dropDrillBag(Player player)
        {
            if (Main.Players[player].InsideHouseID != -1 || Main.Players[player].InsideGarageID != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете сделать это, находясь в доме/гараже", 3000);
                return;
            }

            var pos = NAPI.Entity.GetEntityPosition(player);

            player.SetClothes(5, 0, 0);
            var drillbag = NAPI.Object.CreateObject(-651206088, player.Position + new Vector3(0, 0, -1.1), player.Rotation + new Vector3(0, 30, 110), 255, 0);
            drillbag.SetSharedData("TYPE", "DrillBag");
            drillbag.SetSharedData("PICKEDT", false);
            player.ResetData("HEIST_DRILL");
            var item = nInventory.Find(Main.Players[player].UUID, ItemType.BagWithDrill);
            GameLog.Items($"player({Main.Players[player].UUID})", "ground", Convert.ToInt32(ItemType.BagWithDrill), 1, $"{item.Data}");
            nInventory.Remove(player, ItemType.BagWithDrill, 1);
        }

        public static void MoneyFlow(Player player)
        {
            if (!NAPI.Data.HasEntityData(player, "HAND_MONEY"))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~b~[Сергей Мавроди] ~w~Чё ты тут забыл?");
                return;
            }
            var all_money = 0;

            foreach (var item in nInventory.Items[Main.Players[player].UUID])
            {
                if (item.Type != ItemType.BagWithMoney) continue;

                var money = Convert.ToInt32(item.Data);
                all_money += money;
                player.SetClothes(5, 0, 0);
            }
            nInventory.Remove(player, ItemType.BagWithMoney, 1);

            player.ResetData("HAND_MONEY");
            Wallet.Change(player, (int)(all_money * 0.97));
            GameLog.Money($"server", $"player({Main.Players[player].UUID})", (int)(all_money * 0.97), $"moneyFlow");
            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы отмыли {(int)(all_money * 0.97)}$. Мавроди забрал {(int)(all_money * 0.03)}$ за свои услуги", 3000);
        }

        public static void SafeCracker_Disconnect(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("HAND_MONEY")) dropMoneyBag(player);
                if (player.HasData("HEIST_DRILL")) dropDrillBag(player);

                if (!player.HasData("temp_SafeID")) return;

                Safe safe = Safes.FirstOrDefault(s => s.ID == player.GetData<int>("temp_SafeID"));
                if (safe == null) return;

                safe.Occupier = null;
                return;
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.ResourceStop)]
        public void SafeCracker_Exit()
        {
            try
            {
                foreach (Safe safe in Safes) safe.Destroy();
                Safes.Clear();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        #region commands
        public static void CMD_ReloadSafes(Player player)
        {
            if (!Group.CanUseCmd(player, "reloadsafes")) return;
        }
        public static void CMD_CreateSafe(Player player, int id, float distance, int minamount, int maxamount, string address)
        {
            if (!Group.CanUseCmd(player, "createsafe")) return;
            Safe safe = Safes.FirstOrDefault(s => s.ID == id);
            if (safe != null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~[Ошибка] ~w~Сейф с таким ID уже существует.");
                return;
            }

            Vector3 position = XYInFrontOfPoint(player.Position, player.Rotation.Z, distance) - new Vector3(0.0, 0.0, 0.25);
            CreateSafe(id, position, player.Rotation.Z, minamount, maxamount, address);
        }

        public static void CMD_RemoveSafe(Player player)
        {
            if (!Group.CanUseCmd(player, "removesafe")) return;
            if (!player.HasData("temp_SafeID"))
            {
                player.SendChatMessage("~r~[Ошибка] ~w~Вы должны быть возле сейфа.");
                return;
            }

            RemoveSafe(player.GetData<int>("temp_SafeID"));
        }
        #endregion

        public static void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (player.HasData("HAND_MONEY")) dropMoneyBag(player);
                if (player.HasData("HEIST_DRILL")) dropDrillBag(player);

                if (!player.HasData("temp_SafeID")) return;

                Safe safe = Safes.FirstOrDefault(s => s.ID == player.GetData<int>("temp_SafeID"));
                if (safe == null) return;

                safe.Occupier = null;
                return;
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        #region Menus
        public static void OpenMoneyFlowMenu(Player player)
        {
            Trigger.ClientEvent(player, "mavrshop");
            //player.SendChatMessage("Вызов");
        }
        [RemoteEvent("mavrbuy")]
        public static void callback_moneyflow(Player player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        MoneyFlow(player);
                        return;
                    case 1:
                        if (player.HasData("HEIST_DRILL") || player.HasData("HAND_MONEY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас уже есть дрель или деньги в руках", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -20000))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно денег", 3000);
                            return;
                        }
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 20000, $"buyMavr(drill)");
                        player.SetClothes(5, 41, 0);
                        nInventory.Add(player, new nItem(ItemType.BagWithDrill));
                        player.SetData("HEIST_DRILL", true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили сумку с дрелью для ограблений", 3000);
                        return;
                    case 2:
                        if (Main.Players[player].Money < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно денег", 3000);
                            return;
                        }
                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Lockpick));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }
                        Wallet.Change(player, -200);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 200, $"buyMavr(lockpick)");
                        nInventory.Add(player, new nItem(ItemType.Lockpick, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили отмычку для замков", 3000);
                        return;
                    case 3:
                        if (Main.Players[player].Money < 1200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно денег", 3000);
                            return;
                        }
                        tryAdd = nInventory.TryAdd(player, new nItem(ItemType.ArmyLockpick));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }
                        Wallet.Change(player, -1200);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 1200, $"buyMavr(armylockpick)");
                        nInventory.Add(player, new nItem(ItemType.ArmyLockpick, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили военную отмычку", 3000);
                        return;
                    case 4:
                        if (Main.Players[player].Money < 600)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно денег", 3000);
                            return;
                        }
                        tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Cuffs));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }
                        Wallet.Change(player, -600);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 600, $"buyMavr(cuffs)");
                        nInventory.Add(player, new nItem(ItemType.Cuffs, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили стяжки для рук", 3000);
                        return;
                    case 5:
                        if (Main.Players[player].Money < 600)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно денег", 3000);
                            return;
                        }
                        tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Pocket));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }
                        Wallet.Change(player, -600);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 600, $"buyMavr(pocket)");
                        nInventory.Add(player, new nItem(ItemType.Pocket, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили мешок на голову", 3000);
                        return;
                    case 6:
                        if (Main.Players[player].WantedLVL == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не находитесь в розыске", 3000);
                            return;
                        }
                        if (Main.Players[player].Money < 800)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно средств", 3000);
                            return;
                        }
                        Wallet.Change(player, -800);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 800, $"buyMavr(wanted)");
                        Main.Players[player].WantedLVL.Level--;
                        if (Main.Players[player].WantedLVL.Level == 0) Main.Players[player].WantedLVL = null;
                        Police.setPlayerWantedLevel(player, Main.Players[player].WantedLVL);
                        return;
                }
            }
            catch (Exception e) { Log.Write("mavrbuy: " + e.Message, nLog.Type.Error); }
        }

        public static void OpenSafedoorMenu(Player player)
        {
            Menu menu = new Menu("safedoor", false, false)
            {
                Callback = callback_safedoor
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Дверь хранилища"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("change", Menu.MenuItem.Button)
            {
                Text = "Открыть/Закрыть"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("crack", Menu.MenuItem.Button)
            {
                Text = "Взломать"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_safedoor(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "change":
                    MenuManager.Close(player);
                    if (Main.Players[player].FractionID == 6 && Main.Players[player].FractionLVL >= 14)
                    {
                        if (isCracking)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно сделать это сейчас", 3000);
                            return;
                        }
                        if (!canBeClosed)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно сделать это сейчас", 3000);
                            return;
                        }
                        if (isOpen)
                        {
                            isOpen = false;
                            Doormanager.SetDoorLocked(2, true, 0);
                        }
                        else
                        {
                            isOpen = true;
                            Doormanager.SetDoorLocked(2, true, 45f);
                        }
                        string msg = "Вы закрыли дверь";
                        if (isOpen) msg = "Вы открыли дверь";
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, msg, 3000);
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете сделать это", 3000);
                    return;
                case "crack":
                    MenuManager.Close(player);
                    startSafeDoorCracking(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }
        #endregion
    }
}
