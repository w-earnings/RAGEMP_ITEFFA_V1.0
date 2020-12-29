using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Interface;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Fractions.Realm
{
    class Ems : Script
    {
        private static readonly Nlogs Log = new Nlogs("EMS");
        public static int HumanMedkitsLefts = 100;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                #region Колшебы
                var col = NAPI.ColShape.CreateCylinderColShape(emsCheckpoints[0], 1, 2, 0);
                #region Больничный запас
                col = NAPI.ColShape.CreateCylinderColShape(emsCheckpoints[1], 1, 2, 0);
                col.SetData("INTERACT", 17);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Открыть склад"), new Vector3(emsCheckpoints[1].X, emsCheckpoints[1].Y, emsCheckpoints[1].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                #endregion
                #region Изменение обязанности
                col = NAPI.ColShape.CreateCylinderColShape(emsCheckpoints[2], 1, 2, 0); // duty change
                col.SetData("INTERACT", 18);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодеться"), new Vector3(emsCheckpoints[2].X, emsCheckpoints[2].Y, emsCheckpoints[2].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                #endregion
                #region Начать курс лечения
                col = NAPI.ColShape.CreateCylinderColShape(emsCheckpoints[3], 1, 2, 0);
                col.SetData("INTERACT", 19);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Начать лечение"), new Vector3(emsCheckpoints[3].X, emsCheckpoints[3].Y, emsCheckpoints[3].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                #endregion
                #region Татуировка удалить
                col = NAPI.ColShape.CreateCylinderColShape(emsCheckpoints[4], 1, 2, 0); // tattoo delete
                col.SetData("INTERACT", 51);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Удаление татуировок"), new Vector3(emsCheckpoints[4].X, emsCheckpoints[4].Y, emsCheckpoints[4].Z + 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));
                #endregion
                col.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        e.SetData("IN_HOSPITAL", true);
                    }
                    catch { }
                };
                #region Загрузить аптечки
                col = NAPI.ColShape.CreateCylinderColShape(new Vector3(3595.796, 3661.733, 32.75175), 4, 5, 0);
                col.SetData("INTERACT", 58);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.Marker.CreateMarker(1, new Vector3(3595.796, 3661.733, 29.75175), new Vector3(), new Vector3(), 4, new Color(255, 0, 0));
                col = NAPI.ColShape.CreateCylinderColShape(new Vector3(3597.154, 3670.129, 32.75175), 1, 2, 0);
                col.SetData("INTERACT", 58);
                col.OnEntityEnterColShape += emsShape_onEntityEnterColShape;
                col.OnEntityExitColShape += emsShape_onEntityExitColShape;
                NAPI.Marker.CreateMarker(1, new Vector3(3597.154, 3670.129, 29.75175), new Vector3(), new Vector3(), 4, new Color(255, 0, 0));
                NAPI.Blip.CreateBlip(305, new Vector3(3588.917, 3661.756, 41.48687), 0.75F, 3, "Склад аптечек", 255, 0, true);
                #endregion
                #endregion

                for (int i = 3; i < emsCheckpoints.Count; i++)
                {
                    Marker marker = NAPI.Marker.CreateMarker(1, emsCheckpoints[i] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        public static List<Vector3> emsCheckpoints = new List<Vector3>()
        {
            new Vector3(-463.52603, -285.40915, 35.0001),
            new Vector3(-457.99988, -310.73212, 33.910816),
            new Vector3(-443.235, -310.82755, 34.410553),
            new Vector3(-436.10864, -326.7106, 33.910763),
            new Vector3(-455.35983, -316.70892, 33.910812)
        };


        public static void callEms(Player player, bool death = false)
        {
            if (!death)
            {
                if (Manager.countOfFractionMembers(8) == 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Нет медиков в Вашем районе. Попробуйте позже", 3000);
                    return;
                }
                if (player.HasData("NEXTCALL_EMS") && DateTime.Now < player.GetData<DateTime>("NEXTCALL_EMS"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы уже вызвали медиков, попробуйте позже", 3000);
                    return;
                }
                player.SetData("NEXTCALL_EMS", DateTime.Now.AddMinutes(7));
            }

            if (death && (Main.Players[player].InsideHouseID != -1 || Main.Players[player].InsideGarageID != -1)) return;

            if (player.HasData("CALLEMS_BLIP"))
                NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLEMS_BLIP")); } catch { } });

            var Blip = NAPI.Blip.CreateBlip(0, player.Position, 0.75F, 70, $"Call from player ({player.Value})", 0, 0, true, 0, NAPI.GlobalDimension);
            NAPI.Blip.SetBlipTransparency(Blip, 0);
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || Main.Players[p].FractionID != 8) continue;
                Trigger.ClientEvent(p, "changeBlipAlpha", Blip, 255);
            }
            player.SetData("CALLEMS_BLIP", Blip);

            var colshape = NAPI.ColShape.CreateCylinderColShape(player.Position, 70, 4, 0);
            colshape.OnEntityExitColShape += (s, e) =>
            {
                if (e == player)
                {
                    try
                    {
                        if (Blip != null) Blip.Delete();
                        e.ResetData("CALLEMS_BLIP");

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                colshape.Delete();
                            }
                            catch { }
                        }, 20);
                        e.ResetData("CALLEMS_COL");
                        e.ResetData("IS_CALLEMS");
                    }
                    catch (Exception ex) { Log.Write("EnterEmsCall: " + ex.Message); }
                }
            };
            player.SetData("CALLEMS_COL", colshape);

            player.SetData("IS_CALLEMS", true);
            Manager.sendFractionMessage(8, $"Поступил вызов от игрока ({player.Value})");
            Manager.sendFractionMessage(8, $"~b~Поступил вызов от игрока ({player.Value})", true);
        }

        public static void acceptCall(Player player, Player target)
        {
            int where = -1;
            try
            {
                where = 0;
                if (!Manager.canUseCommand(player, "ems")) return;
                where = 1;
                if (!target.HasData("IS_CALLEMS"))
                {
                    where = 2;
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок не вызывал EMS, или этот вызов уже кто-то принял", 3000);
                    return;
                }
                where = 3;
                Blip blip = target.GetData<Blip>("CALLEMS_BLIP");

                where = 4;
                Trigger.ClientEvent(player, "changeBlipColor", blip, 38);
                where = 5;
                Trigger.ClientEvent(player, "createWaypoint", blip.Position.X, blip.Position.Y);
                where = 6;

                ColShape colshape = target.GetData<ColShape>("CALLEMS_COL");
                where = 7;
                colshape.OnEntityEnterColShape += (s, e) =>
                {
                    if (e == player)
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
                            target.ResetData("CALLEMS_BLIP");
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    colshape.Delete();
                                }
                                catch { }
                            }, 20);
                        }
                        catch (Exception ex) { Log.Write("EnterEmsCall: " + ex.Message); }
                    }
                };
                where = 8;

                Manager.sendFractionMessage(7, $"{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})");
                where = 9;
                Manager.sendFractionMessage(7, $"~b~{player.Name.Replace('_', ' ')} принял вызов от игрока ({target.Value})", true);
                where = 10;
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) принял Ваш вызов", 3000);
                where = 11;
            }
            catch (Exception e) { Log.Write($"acceptCall/{where}/: {e}"); }
        }

        public static void onPlayerDisconnectedhandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("HEAL_TIMER"))
                {
                    Timers.Stop(player.GetData<string>("HEAL_TIMER"));
                }

                if (player.HasData("DYING_TIMER"))
                {
                    Timers.Stop(player.GetData<string>("DYING_TIMER"));
                }

                if (player.HasData("CALLEMS_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLEMS_BLIP"));

                    Manager.sendFractionMessage(8, $"{player.Name.Replace('_', ' ')} отменил вызов");
                }
                if (player.HasData("CALLEMS_COL"))
                {
                    NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("CALLEMS_COL"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
        }

        private static readonly List<string> deadAnims = new List<string>() { "dead_a", "dead_b", "dead_c", "dead_d", "dead_e", "dead_f", "dead_g", "dead_h" };
        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                Log.Debug($"{player.Name} is died by {weapon}");

                FractionCommands.onPlayerDeathHandler(player, entityKiller, weapon);
                SafeMain.onPlayerDeathHandler(player, entityKiller, weapon);
                Weapons.Event_PlayerDeath(player, entityKiller, weapon);
                Army.Event_PlayerDeath(player, entityKiller, weapon);
                Police.Event_PlayerDeath(player, entityKiller, weapon);
                Houses.HouseManager.Event_OnPlayerDeath(player, entityKiller, weapon);
                Working.Collector.Event_PlayerDeath(player, entityKiller, weapon);
                Working.Gopostal.Event_PlayerDeath(player, entityKiller, weapon);
                if (player.HasData("job_farmer"))
                {
                    Working.FarmerJob.Farmer.StartWork(player, false);
                }
                Working.Diver.Event_PlayerDeath(player, entityKiller, weapon);
                Working.Construction.Event_PlayerDeath(player, entityKiller, weapon);

                VehicleManager.WarpPlayerOutOfVehicle(player);
                Main.Players[player].IsAlive = false;
                if (player.HasData("AdminSkin"))
                {
                    player.ResetData("AdminSkin");
                    player.SetSkin((Main.Players[player].Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                    Customization.ApplyCharacter(player);
                }
                Trigger.ClientEvent(player, "screenFadeOut", 2000);

                var dimension = player.Dimension;

                if (Main.Players[player].DemorganTime != 0 || Main.Players[player].ArrestTime != 0)
                    player.SetData("IS_DYING", true);

                if (!player.HasData("IS_DYING"))
                {
                    if ((Manager.FractionTypes[Main.Players[player].FractionID] == 0 && (MafiaWars.warIsGoing || MafiaWars.warStarting)) ||
                        (Manager.FractionTypes[Main.Players[player].FractionID] == 1 && (GangsCapture.captureIsGoing || GangsCapture.captureStarting)))
                    {
                        player.SetSharedData("InDeath", true);
                        DeathConfirm(player, false);
                    }
                    else
                    {
                        player.SetSharedData("InDeath", true);
                        var medics = 0;
                        foreach (var m in Manager.Members) if (m.Value.FractionID == 8) medics++;
                        Trigger.ClientEvent(player, "openDialog", "DEATH_CONFIRM", $"Вы хотите вызвать медиков ({medics} в сети)?");
                    }
                }
                else
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(player)) return;

                            if (player.HasData("DYING_TIMER"))
                            {
                                Timers.Stop(player.GetData<string>("DYING_TIMER"));
                                player.ResetData("DYING_TIMER");
                            }

                            if (player.HasData("CALLEMS_BLIP"))
                            {
                                NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLEMS_BLIP"));
                                player.ResetData("CALLEMS_BLIP");
                            }

                            if (player.HasData("CALLEMS_COL"))
                            {
                                NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("CALLEMS_COL"));
                                player.ResetData("CALLEMS_COL");
                            }

                            Trigger.ClientEvent(player, "DeathTimer", false);
                            player.SetSharedData("InDeath", false);
                            var spawnPos = new Vector3();

                            if (Main.Players[player].DemorganTime != 0)
                            {
                                spawnPos = Admin.DemorganPosition + new Vector3(0, 0, 1.12);
                                dimension = 1337;
                            }
                            else if (Main.Players[player].ArrestTime != 0)
                                spawnPos = Police.policeCheckpoints[4];
                            else if (Main.Players[player].FractionID == 14)
                                spawnPos = Manager.FractionSpawns[14] + new Vector3(0, 0, 1.12);
                            else
                            {
                                player.SetData("IN_HOSPITAL", true);
                                spawnPos = emsCheckpoints[0];
                            }

                            NAPI.Player.SpawnPlayer(player, spawnPos);
                            NAPI.Player.SetPlayerHealth(player, 20);
                            player.ResetData("IS_DYING");
                            Main.Players[player].IsAlive = true;
                            Main.OffAntiAnim(player);
                            NAPI.Entity.SetEntityDimension(player, dimension);
                        }
                        catch { }
                    }, 4000);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Nlogs.Type.Error); }
        }

        public static void DeathConfirm(Player player, bool call)
        {
            NAPI.Player.SpawnPlayer(player, player.Position);
            NAPI.Entity.SetEntityDimension(player, 0);

            Main.OnAntiAnim(player);
            player.SetData("IS_DYING", true);
            player.SetData("DYING_POS", player.Position);

            if (call) callEms(player, true);
            Modules.Voice.PhoneHCommand(player);

            NAPI.Player.SetPlayerHealth(player, 10);
            var time = (call) ? 600000 : 180000;
            Trigger.ClientEvent(player, "DeathTimer", time);
            var timeMsg = (call) ? "10 минут Вас не вылечит медик или кто-нибудь другой" : "3 минут Вас никто не вылечит";
            player.SetData("DYING_TIMER", Timers.StartOnce(time, () => DeathTimer(player)));
            var deadAnimName = deadAnims[Main.rnd.Next(deadAnims.Count)];
            NAPI.Task.Run(() => { try { player.PlayAnimation("dead", deadAnimName, 39); } catch { } }, 500);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"Если в течение {timeMsg}, то Вы попадёте в больницу", 3000);
        }

        public static void DeathTimer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    player.Health = 0;
                }
                catch { }
            });
        }

        public static void payMedkit(Player player)
        {
            if (Main.Players[player].Money < player.GetData<int>("PRICE"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько денег", 3000);
                return;
            }
            Player seller = player.GetData<Player>("SELLER");
            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы слишком далеко от продавца", 3000);
                return;
            }
            var item = nInventory.Find(Main.Players[seller].UUID, ItemType.HealthKit);
            if (item == null || item.Count < 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У продавца не осталось аптечек", 3000);
                return;
            }
            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.HealthKit));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }

            nInventory.Add(player, new nItem(ItemType.HealthKit));
            nInventory.Remove(seller, ItemType.HealthKit, 1);

            Stocks.fracStocks[6].Money += Convert.ToInt32(player.GetData<int>("PRICE") * 0.85);
            Finance.Wallet.Change(player, -player.GetData<int>("PRICE"));
            Finance.Wallet.Change(seller, Convert.ToInt32(player.GetData<int>("PRICE") * 0.15));

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы купили аптечку", 3000);
            Plugins.Notice.Send(seller, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) купил у Вас аптечку", 3000);
        }

        public static void payHeal(Player player)
        {
            if (Main.Players[player].Money < player.GetData<int>("PRICE"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько денег", 3000);
                return;
            }
            var seller = player.GetData<Player>("SELLER");
            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы слишком далеко от врача", 3000);
                return;
            }
            if (NAPI.Player.IsPlayerInAnyVehicle(seller) && NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                var pveh = seller.Vehicle;
                var tveh = player.Vehicle;
                Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(pveh);
                if (veh.GetData<string>("ACCESS") != "FRACTION" || veh.GetData<string>("TYPE") != "EMS" || !veh.HasData("CANMEDKITS"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы сидите не в карете EMS", 3000);
                    return;
                }
                if (pveh != tveh)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок сидит в другой машине", 3000);
                    return;
                }
                Plugins.Notice.Send(seller, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы вылечили игрока ({player.Value})", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Игрок ({seller.Value}) вылечил Вас", 3000);
                Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                NAPI.Player.SetPlayerHealth(player, 100);
                Finance.Wallet.Change(player, -player.GetData<int>("PRICE"));
                Finance.Wallet.Change(seller, player.GetData<int>("PRICE"));
                Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", player.GetData<int>("PRICE"), $"payHeal");
                return;
            }
            else if (seller.GetData<bool>("IN_HOSPITAL") && player.GetData<bool>("IN_HOSPITAL"))
            {
                Plugins.Notice.Send(seller, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы вылечили игрока ({player.Value})", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Игрок ({seller.Value}) вылечил Вас", 3000);
                NAPI.Player.SetPlayerHealth(player, 100);
                Finance.Wallet.Change(player, -player.GetData<int>("PRICE"));
                Finance.Wallet.Change(seller, player.GetData<int>("PRICE"));
                Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", player.GetData<int>("PRICE"), $"payHeal");
                Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                return;
            }
            else
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны быть в больнице или корете скорой помощи", 3000);
                return;
            }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 17:
                    if (Main.Players[player].FractionID != 8)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не сотрудник EMS", 3000);
                        return;
                    }
                    if (!player.GetData<bool>("ON_DUTY"))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не начали рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[8].IsOpen)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    OpenHospitalStockMenu(player);
                    return;
                case 18:
                    if (Main.Players[player].FractionID == 8)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы начали рабочий день", 3000);
                            Manager.setSkin(player, 8, Main.Players[player].FractionLVL);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                            break;
                        }
                        else
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы закончили рабочий день", 3000);
                            Customization.ApplyCharacter(player);
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                            break;
                        }
                    }
                    else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не сотрудник EMS", 3000);
                    return;
                case 19:
                    if (NAPI.Player.GetPlayerHealth(player) > 99)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не нуждаетесь в лечении", 3000);
                        break;
                    }
                    if (player.HasData("HEAL_TIMER"))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже лечитесь", 3000);
                        break;
                    }
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы начали лечение", 3000);

                    player.SetData("HEAL_TIMER", Timers.Start(3750, () => HealTimer(player)));
                    return;
                case 51:
                    OpenTattooDeleteMenu(player);
                    return;
                case 58:
                    if (Main.Players[player].FractionID != 8)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не сотрудник EMS", 3000);
                        break;
                    }
                    if (!player.IsInVehicle || !player.Vehicle.HasData("CANMEDKITS"))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не в машине или Ваша машина не может перевозить аптечки", 3000);
                        break;
                    }

                    var medCount = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.HealthKit);
                    if (medCount >= 50)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В машине максимум аптечек", 3000);
                        break;
                    }
                    if (HumanMedkitsLefts <= 0)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Аптечки закончились. Приезжайте за новыми через час", 3000);
                        break;
                    }
                    var toAdd = (HumanMedkitsLefts > 50 - medCount) ? 50 - medCount : HumanMedkitsLefts;
                    HumanMedkitsLefts = toAdd;

                    VehicleInventory.Add(player.Vehicle, new nItem(ItemType.HealthKit, toAdd));
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы заполнили машину аптечками", 3000);
                    return;
            }
        }



        private static void HealTimer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (emsCheckpoints[3].DistanceTo(player.Position) > 25)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы отошли слишком далеко. Лечение не возможно", 3000);
                        Timers.Stop(player.GetData<string>("HEAL_TIMER"));
                        player.ResetData("HEAL_TIMER");
                        Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        return;
                    }
                    else
                    {
                        if (player.Health == 100)
                        {
                            Timers.Stop(player.GetData<string>("HEAL_TIMER"));
                            player.ResetData("HEAL_TIMER");
                            player.StopAnimation();
                            Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Ваше лечение закончено", 3000);
                            return;
                        }
                        player.Health += 1;
                    }
                }
                catch { }
            });
        }

        private void emsShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("emsShape_onEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
        }

        private void emsShape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("emsShape_onEntityExitColShape: " + ex.Message, Nlogs.Type.Error); }
        }

        #region menus
        public static void OpenHospitalStockMenu(Player player)
        {
            Menu menu = new Menu("hospitalstock", false, false)
            {
                Callback = callback_hospitalstock
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = $"Склад ({Stocks.fracStocks[8].Medkits}шт)"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("takemed", Menu.MenuItem.Button)
            {
                Text = "Взять аптечку"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("putmed", Menu.MenuItem.Button)
            {
                Text = "Положить аптечку"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("tazer", Menu.MenuItem.Button)
            {
                Text = "Взять электрошокер"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_hospitalstock(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "takemed":
                    if (!Manager.canGetWeapon(client, "Medkits")) return;
                    if (Stocks.fracStocks[8].Medkits <= 0)
                    {
                        Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"На складе не осталось аптечек", 3000);
                        return;
                    }
                    var tryAdd = nInventory.TryAdd(client, new nItem(ItemType.HealthKit));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }
                    nInventory.Add(client, new nItem(ItemType.HealthKit));
                    var itemInv = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы взяли аптечку. У Вас {itemInv.Count} штук", 3000);
                    Stocks.fracStocks[8].Medkits--;
                    Loggings.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                    break;
                case "putmed":
                    itemInv = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                    if (itemInv == null)
                    {
                        Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет аптечек", 3000);
                        return;
                    }
                    nInventory.Remove(client, ItemType.HealthKit, 1);
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы положили аптечку. У Вас осталось {itemInv.Count - 1} штук", 3000);
                    Stocks.fracStocks[8].Medkits++;
                    Loggings.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, true);
                    break;
                case "tazer":
                    if (!Main.Players.ContainsKey(client)) return;

                    if (Main.Players[client].FractionLVL < 3)
                    {
                        Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не имеете доступа к электрошокеру", 3000);
                        return;
                    }

                    Weapons.GiveWeapon(client, ItemType.StunGun, Weapons.GetSerial(true, 8));
                    Trigger.ClientEvent(client, "acguns");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = $"Склад ({Stocks.fracStocks[8].Medkits}шт)"
            };
            menu.Change(client, 0, menuItem);
        }

        public static void OpenTattooDeleteMenu(Player player)
        {
            Menu menu = new Menu("tattoodelete", false, false)
            {
                Callback = callback_tattoodelete
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = $"Сведение татуировок"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("header", Menu.MenuItem.Card)
            {
                Text = $"Выберите зону, в которой хотите свести все татуировки. Стоимость сведения в одной зоне - 3000$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("Torso", Menu.MenuItem.Button)
            {
                Text = "Торс"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("Head", Menu.MenuItem.Button)
            {
                Text = "Голова"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("LeftArm", Menu.MenuItem.Button)
            {
                Text = "Левая рука"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("RightArm", Menu.MenuItem.Button)
            {
                Text = "Правая рука"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("LeftLeg", Menu.MenuItem.Button)
            {
                Text = "Левая нога"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("RightLeg", Menu.MenuItem.Button)
            {
                Text = "Правая нога"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static readonly List<string> TattooZonesNames = new List<string>() { "торса", "головы", "левой руки", "правой руки", "левой ноги", "правой ноги" };
        private static void callback_tattoodelete(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(client);
                return;
            }
            var zone = Enum.Parse<TattooZones>(item.ID);
            if (Customization.CustomPlayerData[Main.Players[client].UUID].Tattoos[Convert.ToInt32(zone)].Count == 0)
            {
                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас нет татуировок в этой зоне", 3000);
                return;
            }
            if (!Finance.Wallet.Change(client, -600))
            {
                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно средств", 3000);
                return;
            }
            Loggings.Money($"player({Main.Players[client].UUID})", $"server", 600, $"tattooRemove");
            Stocks.fracStocks[6].Money += 600;

            foreach (var tattoo in Customization.CustomPlayerData[Main.Players[client].UUID].Tattoos[Convert.ToInt32(zone)])
            {
                var decoration = new Decoration
                {
                    Collection = NAPI.Util.GetHashKey(tattoo.Dictionary),
                    Overlay = NAPI.Util.GetHashKey(tattoo.Hash)
                };
                client.RemoveDecoration(decoration);
            }
            Customization.CustomPlayerData[Main.Players[client].UUID].Tattoos[Convert.ToInt32(zone)] = new List<Tattoo>();
            client.SetSharedData("TATTOOS", Newtonsoft.Json.JsonConvert.SerializeObject(Customization.CustomPlayerData[Main.Players[client].UUID].Tattoos));

            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы свели татуировки с " + TattooZonesNames[Convert.ToInt32(zone)], 3000);
        }
        #endregion
    }
}
