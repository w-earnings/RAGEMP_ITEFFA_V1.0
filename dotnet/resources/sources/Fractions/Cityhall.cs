using System;
using System.Collections.Generic;
using GTANetworkAPI;
using iTeffa.Kernel;
using iTeffa.Settings;

namespace iTeffa.Fractions
{
    class Cityhall : Script
    {
        private static readonly nLog Log = new nLog("Cityhall");
        public static int lastHourTax = 0;
        public static int canGetMoney = 1000000;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {
                #region Интерьер Мерии №1
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(CityhallChecksCoords[0].X, CityhallChecksCoords[0].Y, CityhallChecksCoords[0].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(CityhallChecksCoords[1].X, CityhallChecksCoords[1].Y, CityhallChecksCoords[1].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(CityhallChecksCoords[2].X, CityhallChecksCoords[2].Y, CityhallChecksCoords[2].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255));
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255));
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255));
                #endregion Интерьер Мерии №1
                #region Интерьер Мерии №2
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Press E"), new Vector3(CityhallChecksCoords[3].X, CityhallChecksCoords[3].Y, CityhallChecksCoords[3].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(0, 255, 255));
                #endregion Интерьер Мерии №2

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[0], 1f, 2, 0));
                Cols[0].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 9);
                
                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[1], 1f, 2, 0));
                Cols[1].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 1);

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[2], 1f, 2, 0));
                Cols[7].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[7].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[7].SetData("INTERACT", 62);

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[3], 3, 2, 0));
            } 
            catch(Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_CITYHALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static readonly Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();

        public static List<Vector3> CityhallChecksCoords = new List<Vector3>
        {
            new Vector3(),   // 0 - Оружейная
            new Vector3(-572.94464, -201.82872, 41.58397),      // 1 - Раздевалка
            new Vector3(),      // 2 - Крафт оружия
            new Vector3(-1304.6462, -560.2332, 33.25491),      // 3 - Раздевалка
        };

        private void city_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                if (shape.HasData("DOOR")) NAPI.Data.SetEntityData(entity, "DOOR", shape.GetData<int>("DOOR"));
            }
            catch (Exception e) { Log.Write("city_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void city_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("city_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {

                case 3:
                    if (Main.Players[player].FractionID == 6 && Main.Players[player].FractionLVL > 1)
                    {
                        Doormanager.SetDoorLocked(player.GetData<int>("DOOR"), !Doormanager.GetDoorLocked(player.GetData<int>("DOOR")), 0);
                        string msg = "Вы открыли дверь";
                        if (Doormanager.GetDoorLocked(player.GetData<int>("DOOR"))) msg = "Вы закрыли дверь";
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, msg, 3000);
                    }
                    return;

                case 4:
                    SafeMain.OpenSafedoorMenu(player);
                    return;
                case 5:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    return;
                case 62:
                    if (Main.Players[player].FractionID != 6)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник мэрии", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[2].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 6);
                    Interface.Dashboard.OpenOut(player, Stocks.fracStocks[2].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID == 6)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы начали рабочий день", 3000);
                    Manager.setSkin(player, 6, Main.Players[player].FractionLVL);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                    if (Main.Players[player].FractionLVL >= 3)
                        player.Armor = 100;
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                    Customization.ApplyCharacter(player);
                    if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                    else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                    return;
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник мэрии", 3000);
        }

        #region menu
        public static void OpenCityhallGunMenu(Player player)
        {

            if (Main.Players[player].FractionID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа", 3000);
                return;
            }
            if (!Stocks.fracStocks[2].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                return;
            }
            Trigger.ClientEvent(player, "govguns");
        }
        [RemoteEvent("govgun")]
        public static void callback_cityhallGuns(Player client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Manager.giveGun(client, Weapons.Hash.StunGun, "stungun");
                        return;
                    case 1:
                        Manager.giveGun(client, Weapons.Hash.Pistol, "pistol");
                        return;
                    case 2:
                        Manager.giveGun(client, Weapons.Hash.AdvancedRifle, "assaultrifle");
                        return;
                    case 3:
                        Manager.giveGun(client, Weapons.Hash.Gusenberg, "gusenberg");
                        return;
                    case 4:
                        if (!Manager.canGetWeapon(client, "armor")) return;

                        var aItem = nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(client, "Medkits")) return;

                        if (Stocks.fracStocks[2].Medkits == 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Stocks.fracStocks[2].Medkits--;
                        Stocks.fracStocks[2].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 6:
                        if (!Manager.canGetWeapon(client, "PistolAmmo")) return;
                        Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 7:
                        if (!Manager.canGetWeapon(client, "SMGAmmo")) return;
                        Manager.giveAmmo(client, ItemType.SMGAmmo, 30);
                        return;
                    case 8:
                        if (!Manager.canGetWeapon(client, "RiflesAmmo")) return;
                        Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                        return;
                }
            }
            catch (Exception e) { Log.Write("Govgun: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
