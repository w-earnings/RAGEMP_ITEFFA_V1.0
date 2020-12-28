using System;
using System.Collections.Generic;
using System.Data;
using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;
using iTeffa.Interface;
using Newtonsoft.Json;
using iTeffa.Models;

namespace iTeffa.Fractions.Realm
{
    class Sheriff : Script
    {
        private static readonly Nlogs Log = new Nlogs("Sheriff");
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[0], 6, 3, 0));
                Cols[0].OnEntityEnterColShape += arrestShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += arrestShape_onEntityExitColShape;

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 100);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Открыть меню"), new Vector3(sheriffCheckpoints[1].X, sheriffCheckpoints[1].Y, sheriffCheckpoints[1].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[2], 1, 2, 0));
                Cols[2].SetData("INTERACT", 110);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Чтобы переодется"), new Vector3(sheriffCheckpoints[2].X, sheriffCheckpoints[2].Y, sheriffCheckpoints[2].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 120);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Открыть ES меню"), new Vector3(sheriffCheckpoints[3].X, sheriffCheckpoints[3].Y, sheriffCheckpoints[3].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[7], 1, 2, 0));
                Cols[5].SetData("INTERACT", 420);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Сдать суммку"), new Vector3(sheriffCheckpoints[7].X, sheriffCheckpoints[7].Y, sheriffCheckpoints[7].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[8], 1, 2, 0));
                Cols[6].SetData("INTERACT", 590);
                Cols[6].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[6].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Открыть меню"), new Vector3(sheriffCheckpoints[8].X, sheriffCheckpoints[8].Y, sheriffCheckpoints[8].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[9], 4, 5, 0));
                Cols[7].SetData("INTERACT", 660);
                Cols[7].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[7].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Улучшение"), new Vector3(sheriffCheckpoints[9].X, sheriffCheckpoints[9].Y, sheriffCheckpoints[9].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[7] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[8] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[9] - new Vector3(0, 0, 3.7), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        private static readonly Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> sheriffCheckpoints = new List<Vector3>()
        {
            new Vector3(-438.36707, 5988.892, 31.716532),
            new Vector3(-430.7331, 5999.503, 30.59653),
            new Vector3(-433.6318, 5990.971, 30.59653),
            new Vector3(-455.9738, 6014.119, 30.59654),
            new Vector3(-441.9835, 5987.603, 30.59653),
            new Vector3(-436.764, 6020.909, 30.37011),
            new Vector3(441.9336, -981.5965, 29.6896),
            new Vector3(-448.1254, 6014.227, 30.59655),
            new Vector3(-426.0116, 5998.237, 30.59653),
            new Vector3(-464.8731, 6042.925, 30.22054),
        };

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.VehicleSeat != -1 || player.VehicleSeat != 0) return;
                if (Main.Players[player].FractionID != 18 || Main.Players[player].FractionID != 9) return;
                Trigger.ClientEvent(player, "closePc");
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, Nlogs.Type.Error); }
        }

        public static void callSheriff(Player player, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (Manager.countOfFractionMembers(18) == 0 && Manager.countOfFractionMembers(9) == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Нет полицейских в Вашем районе. Попробуйте позже", 3000);
                        return;
                    }
                    if (player.HasData("NEXTCALL_SHERIFF") && DateTime.Now < player.GetData<DateTime>("NEXTCALL_SHERIFF"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Вы уже вызвали полицию, попробуйте позже", 3000);
                        return;
                    }
                    player.SetData("NEXTCALL_SHERIFF", DateTime.Now.AddMinutes(7));

                    if (player.HasData("CALLSHERIFF_BLIP"))
                        NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLSHERIFF_BLIP"));

                    var Blip = NAPI.Blip.CreateBlip(0, player.Position, 0.75F, 70, "Call from " + player.Name.Replace('_', ' ') + $" ({player.Value})", 0, 0, true, 0, 0);
                    Blip.Transparency = 0;
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (Main.Players[p].FractionID != 18 && Main.Players[p].FractionID != 9) continue;
                        p.TriggerEvent("changeBlipAlpha", Blip, 255);
                    }
                    player.SetData("CALLSHERIFF_BLIP", Blip);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(player.Position, 70, 4, 0);
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        if (e == player)
                        {
                            try
                            {
                                Blip.Delete();
                                e.ResetData("CALLSHERIFF_BLIP");

                                Manager.sendFractionMessage(18, $"{e.Name.Replace('_', ' ')} отменил вызов");
                                Manager.sendFractionMessage(9, $"{e.Name.Replace('_', ' ')} отменил вызов");

                                colshape.Delete();

                                e.ResetData("CALLSHERIFF_COL");
                                e.ResetData("IS_CALLSHERIFF");
                            }
                            catch (Exception ex) { Log.Write("EnterSheriffCall: " + ex.Message); }
                        }
                    };
                    player.SetData("CALLSHERIFF_COL", colshape);

                    player.SetData("IS_CALLSHERIFF", true);
                    Manager.sendFractionMessage(18, $"Поступил вызов от игрока ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(18, $"~b~Поступил вызов от игрока ({player.Value}) - {reason}", true);
                    Manager.sendFractionMessage(9, $"Поступил вызов от игрока ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(9, $"~b~Поступил вызов от игрока ({player.Value}) - {reason}", true);
                }
                catch { }
            });
        }

        public static void acceptCall(Player player, Player target)
        {
            try
            {
                if (!Manager.canUseCommand(player, "pd")) return;
                if (target == null || !NAPI.Entity.DoesEntityExist(target)) return;
                if (!target.HasData("IS_CALLSHERIFF"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Игрок не вызывал полицию или этот вызов уже кто-то принял", 3000);
                    return;
                }
                Blip blip = target.GetData<Blip>("CALLSHERIFF_BLIP");

                Trigger.ClientEvent(player, "changeBlipColor", blip, 38);
                Trigger.ClientEvent(player, "createWaypoint", blip.Position.X, blip.Position.Y);

                ColShape colshape = target.GetData<ColShape>("CALLSHERIFF_COL");
                colshape.OnEntityEnterColShape += (s, e) =>
                {
                    if (e == player)
                    {
                        try
                        {
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLSHERIFF_BLIP"));
                                    target.ResetData("CALLSHERIFF_BLIP");
                                    colshape.Delete();
                                }
                                catch { }
                            });
                        }
                        catch (Exception ex) { Log.Write("EnterSheriffCall: " + ex.Message); }
                    }
                };

                Manager.sendFractionMessage(18, $"{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})");
                Manager.sendFractionMessage(18, $"~b~{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})", true);
                Notify.Send(target, NotifyType.Info, NotifyPosition.TopCenter, $"Игрок ({player.Value}) принял Ваш вызов", 3000);
            }
            catch
            {
            }
        }

        [RemoteEvent("clearWantedLvl1")]
        public static void clearWantedLvl1(Player sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Player player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }

                var split = target.Split('_');
                Database.Query($"UPDATE characters SET wanted=null WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                try
                {
                    setPlayerWantedLevel(player, null);
                }
                catch { }
                Notify.Send(sender, NotifyType.Success, NotifyPosition.TopCenter, $"Вы сняли розыск с владельца паспорта {target}", 3000);
            }
            catch (Exception e) { Log.Write("ClearWantedLvl1: " + e.Message, Nlogs.Type.Error); }
        }

        [RemoteEvent("checkNumber1")]
        public static void checkNumber1(Player sender, params object[] arguments)
        {
            try
            {
                var number = (string)arguments[0];
                VehicleManager.VehicleData vehicle;
                try
                {
                    vehicle = VehicleManager.Vehicles[number];
                }
                catch
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Машины с таким номером не найдено", 3000);
                    return;
                }
                Trigger.ClientEvent(sender, "executeCarInfo", Convert.ToString(vehicle.Model), vehicle.Holder.Replace('_', ' '));
            }
            catch (Exception e) { Log.Write("checkNumber1: " + e.Message, Nlogs.Type.Error); }
        }

        [RemoteEvent("checkPerson1")]
        public static void checkPerson1(Player sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Player player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }

                try
                {
                    var acc = Main.Players[player];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    var gender = (acc.Gender) ? "Мужской" : "Женский";
                    var lic = "";
                    for (int i = 0; i < acc.Licenses.Count; i++)
                        if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                    if (lic == "") lic = "Отсутствуют";

                    Trigger.ClientEvent(sender, "executePersonInfo", $"{acc.FirstName}", $"{acc.LastName}", $"{acc.UUID}", $"{gender}", $"{wantedLvl}", $"{lic}");
                }
                catch
                {
                    var split = target.Split('_');
                    var result = Database.QueryRead($"SELECT * FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    foreach (DataRow Row in result.Rows)
                    {
                        var firstName = Convert.ToString(Row["firstname"]);
                        var lastName = Convert.ToString(Row["lastname"]);
                        var genderBool = Convert.ToBoolean(Row["gender"]);
                        var uuid = Convert.ToInt32(Row["uuid"].ToString());
                        var gender = (genderBool) ? "Мужской" : "Женский";
                        var wanted = JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                        var wantedLvl = (wanted == null) ? 0 : wanted.Level;
                        var licenses = JsonConvert.DeserializeObject<List<bool>>(Convert.ToString(Row["licenses"]));
                        var lic = "";
                        for (int i = 0; i < licenses.Count; i++)
                            if (licenses[i]) lic += $"{Main.LicWords[i]} / ";
                        if (lic == "") lic = "Отсутствуют";

                        Trigger.ClientEvent(sender, "executePersonInfo", $"{firstName}", $"{lastName}", $"{uuid}", $"{gender}", $"{wantedLvl}", $"{lic}", "Лицензия на оружие", "Водительские права");
                    }
                }
            }
            catch (Exception e) { Log.Write("checkPerson1: " + e.Message, Nlogs.Type.Error); }
        }

        [RemoteEvent("checkWantedList1")]
        public static void checkWantedList1(Player sender, params object[] arguments)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    var acc = Main.Players[p];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    if (wantedLvl != 0) list.Add($"{acc.FirstName} {acc.LastName} - {wantedLvl}*");
                }
                var json = JsonConvert.SerializeObject(list);
                Log.Debug(json);
                Trigger.ClientEvent(sender, "executeWantedList1", json);
            }
            catch (Exception e) { Log.Write("checkWantedList1: " + e.Message, Nlogs.Type.Error); }
        }

        [RemoteEvent("openCopCarMenu1")]
        public static void openCopcarmenu1(Player sender, params object[] arguments)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                var vehicle = sender.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION" &&
                    (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 18 || NAPI.Data.GetEntityData(vehicle, "FRACTION") == 9) &&
                    (sender.VehicleSeat == -1 || sender.VehicleSeat == 0))
                {
                    MenuManager.Close(sender);
                    if (Main.Players[sender].FractionID == 18 || Main.Players[sender].FractionID == 9)
                    {
                        Trigger.ClientEvent(sender, "openPc");
                        Commands.Controller.RPChat("me", sender, "включил(а) бортовой компьютер");
                    }
                }
                return;
            }
            catch (Exception e) { Log.Write("openCopCarMenu1: " + e.Message, Nlogs.Type.Error); }
        }

        public static void Event_PlayerDeath(Player player, Player killer, uint reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    if (NAPI.Data.GetEntityData(player, "IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Nlogs.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            if (!Main.Players.ContainsKey(player)) return;
            switch (interact)
            {
                case 100:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не сотрудник Sheriff", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[18].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    OpenSheriffGunMenu(player);
                    return;
                case 110:
                    if (Main.Players[player].FractionID == 18)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы начали рабочий день", 3000);
                            Manager.setSkin(player, 18, Main.Players[player].FractionLVL);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                            break;
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы закончили рабочий день", 3000); ;
                            Customization.ApplyCharacter(player);
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                            break;
                        }
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не сотрудник Sheriff", 3000);
                    return;
                case 120:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не сотрудник Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!is_warg)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Не включен режим ЧП", 3000);
                        return;
                    }
                    OpenSpecialSheriffMenu(player);
                    return;
                case 420:
                    if (!player.HasData("HAND_MONEY") && !player.HasData("HEIST_DRILL"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет ни сумки с деньгами, ни сумки с дрелью", 3000);
                        return;
                    }
                    if (player.HasData("HAND_MONEY"))
                    {
                        nInventory.Remove(player, ItemType.BagWithMoney, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HAND_MONEY");
                    }
                    if (player.HasData("HEIST_DRILL"))
                    {
                        nInventory.Remove(player, ItemType.BagWithDrill, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HEIST_DRILL");
                    }
                    Finance.Wallet.Change(player, 200);
                    Loggings.Money($"server", $"player({Main.Players[player].UUID})", 200, $"sheriffAward");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы получили вознаграждение в 200$", 3000);
                    return;
                case 440:
                    if (Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас уже есть лицензия на оружие", 3000);
                        return;
                    }
                    if (!Finance.Wallet.Change(player, -30000))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно средств.", 3000);
                        return;
                    }
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили лицензию на оружие", 3000);
                    Main.Players[player].Licenses[6] = true;
                    Dashboard.sendStats(player);
                    return;
                case 590:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не сотрудник Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[18].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 18);
                    Interface.Dashboard.OpenOut(player, Stocks.fracStocks[18].Weapons, "Склад оружия", 6);
                    return;
                case 660:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не сотрудник Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!player.IsInVehicle || (player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff") &&
                        player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff2") && player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff3") && player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff4")))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в рабочей машине", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "svem", 20, 20);
                    player.Vehicle.SetSharedData("BOOST", 20);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Пробущено", 3000);
                    return;
            }
        }

        #region shapes
        private void arrestShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "IS_IN_ARREST_AREA", true);
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
        }

        private void arrestShape_onEntityExitColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                NAPI.Data.SetEntityData(player, "IS_IN_ARREST_AREA", false);
                if (Main.Players[player].ArrestTime != 0)
                {
                    NAPI.Entity.SetEntityPosition(player, Sheriff.sheriffCheckpoints[4]);
                }
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityExitColShape: " + ex.Message, Nlogs.Type.Error); }
        }

        private void onEntityEnterColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterColshape: " + ex.Message, Nlogs.Type.Error); }
        }

        private void onEntityExitColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitColshape: " + ex.Message, Nlogs.Type.Error); }
        }
        #endregion

        public static void onPlayerDisconnectedhandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (NAPI.Data.HasEntityData(player, "ARREST_TIMER"))
                {
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER"));
                }

                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    NAPI.Data.ResetEntityData(target, "FOLLOWER");
                }
                else if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    NAPI.Data.ResetEntityData(target, "FOLLOWING");
                    Trigger.ClientEvent(target, "follow", false);
                }

                if (player.HasData("CALLSHERIFF_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLSHERIFF_BLIP"));

                    Manager.sendFractionMessage(18, $"{player.Name.Replace('_', ' ')} отменил вызов");
                    Manager.sendFractionMessage(9, $"{player.Name.Replace('_', ' ')} отменил вызов");
                }
                if (player.HasData("CALLSHERIFF_COL"))
                {
                    NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("CALLSHERIFF_COL"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
        }

        public static void setPlayerWantedLevel(Player player, WantedLevel wantedlevel)
        {
            Main.Players[player].WantedLVL = wantedlevel;
            if (wantedlevel != null) Trigger.ClientEvent(player, "setWanted", wantedlevel.Level);
            else Trigger.ClientEvent(player, "setWanted", 0);
        }

        public static bool is_warg = false;

        #region menus
        public static void OpenSheriffGunMenu(Player player)
        {
            Trigger.ClientEvent(player, "sheriffg");
        }
        [RemoteEvent("sheriffgun")]
        public static void callback_sheriffGuns(Player client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Manager.giveGun(client, Weapons.Hash.Nightstick, "Nightstick");
                        return;
                    case 1:
                        Manager.giveGun(client, Weapons.Hash.Pistol, "Pistol");
                        return;
                    case 2:
                        Manager.giveGun(client, Weapons.Hash.SMG, "SMG");
                        return;
                    case 3:
                        Manager.giveGun(client, Weapons.Hash.PumpShotgun, "PumpShotgun");
                        return;
                    case 4:
                        Manager.giveGun(client, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(client, "armor")) return;
                        if (Stocks.fracStocks[18].Materials < Manager.matsForArmor)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "На складе недостаточно материала", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        Stocks.fracStocks[18].Materials -= Manager.matsForArmor;
                        Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы получили бронежилет", 3000);
                        Loggings.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                        return;
                    case 6:
                        if (!Manager.canGetWeapon(client, "Medkits")) return;
                        if (Stocks.fracStocks[18].Medkits == 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Stocks.fracStocks[18].Medkits--;
                        Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        Loggings.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 7:
                        if (!Manager.canGetWeapon(client, "PistolAmmo")) return;
                        Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 8:
                        if (!Manager.canGetWeapon(client, "SMGAmmo")) return;
                        Manager.giveAmmo(client, ItemType.SMGAmmo, 30);
                        return;
                    case 9:
                        if (!Manager.canGetWeapon(client, "ShotgunsAmmo")) return;
                        Manager.giveAmmo(client, ItemType.ShotgunsAmmo, 6);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write($"Sheriffgun: " + e.Message, Nlogs.Type.Error);
            }
        }

        public static void OpenSpecialSheriffMenu(Player player)
        {
            Menu menu = new Menu("sheriffSpecial", false, false);
            menu.Callback += callback_sheriffSpecial;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Оружейная"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("changeclothes", Menu.MenuItem.Button)
            {
                Text = "Переодеться"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("pistol50", Menu.MenuItem.Button)
            {
                Text = "Desert Eagle"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("carbineRifle", Menu.MenuItem.Button)
            {
                Text = "Штурмовая винтовка"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("riflesammo", Menu.MenuItem.Button)
            {
                Text = "Автоматный калибр x30"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("heavyshotgun", Menu.MenuItem.Button)
            {
                Text = "Тяжелый дробовик"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("stungun", Menu.MenuItem.Button)
            {
                Text = "Tazer"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_sheriffSpecial(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "changeclothes":
                    if (!NAPI.Data.GetEntityData(client, "IN_CP_MODE"))
                    {
                        bool gender = Main.Players[client].Gender;
                        Customization.ApplyCharacter(client);
                        Customization.ClearClothes(client, gender);
                        if (gender)
                        {
                            Customization.SetHat(client, 39, 0);
                            client.SetClothes(11, 53, 0);
                            client.SetClothes(4, 31, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 15, 2);
                            client.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(client, 38, 0);
                            client.SetClothes(11, 46, 0);
                            client.SetClothes(4, 30, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 17, 2);
                            client.SetClothes(3, 53, 0);
                        }
                        if (client.HasData("HAND_MONEY")) client.SetClothes(5, 45, 0);
                        else if (client.HasData("HEIST_DRILL")) client.SetClothes(5, 41, 0);
                        NAPI.Data.SetEntityData(client, "IN_CP_MODE", true);
                        return;
                    }
                    Manager.setSkin(client, 18, Main.Players[client].FractionLVL);
                    client.SetData("IN_CP_MODE", false);
                    return;
                case "pistol50":
                    Manager.giveGun(client, Weapons.Hash.Pistol50, "pistol50");
                    return;
                case "carbineRifle":
                    Manager.giveGun(client, Weapons.Hash.CarbineRifle, "carbineRifle");
                    return;
                case "riflesammo":
                    if (!Manager.canGetWeapon(client, "RiflesAmmo")) return;
                    Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                    return;
                case "heavyshotgun":
                    Manager.giveGun(client, Weapons.Hash.HeavyShotgun, "heavyshotgun");
                    return;
                case "stungun":
                    Manager.giveGun(client, Weapons.Hash.StunGun, "stungun");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }
        #endregion
    }
}
