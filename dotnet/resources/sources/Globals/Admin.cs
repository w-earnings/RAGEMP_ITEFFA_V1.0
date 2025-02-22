﻿using GTANetworkAPI;
using iTeffa.Infodata;
using iTeffa.Interface;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace iTeffa.Globals
{
    class Admin : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Admin");
        public static bool IsServerStoping = false;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            ColShape colShape = NAPI.ColShape.CreateCylinderColShape(DemorganPosition, 100, 50, 1337);
            colShape.OnEntityExitColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                if (Main.Players[e].DemorganTime > 0) NAPI.Entity.SetEntityPosition(e, DemorganPosition + new Vector3(0, 0, 1.5));
            };
            Group.LoadCommandsConfigs();
        }


        [RemoteEvent("openAdminPanel")]
        private static void OpenAdminPanel(Player player)
        {
            CharacterData acc = Main.Players[player];
            List<Group.GroupCommand> cmds = new List<Group.GroupCommand>();
            List<object> players = new List<object>();
            if (acc.AdminLVL > 0)
            {
                foreach (Group.GroupCommand item in Group.GroupCommands)
                {
                    if (item.IsAdmin)
                    {
                        if (item.MinLVL <= acc.AdminLVL)
                        {
                            cmds.Add(item);
                        }
                    }
                }
                foreach (var p in Main.Players.Keys.ToList())
                {
                    string[] data = { Main.Players[p].AdminLVL.ToString(), p.Value.ToString(), p.Name.ToString(), p.Ping.ToString() };
                    players.Add(data);
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(cmds);
                string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(players);
                Plugins.Trigger.ClientEvent(player, "openAdminPanel", json, json2);
            }
            cmds.Clear();
            players.Clear();
        }

        [RemoteEvent("getPlayerInfoToAdminPanel")]
        private static void LoadPlayerInfoToPanel(Player player, int id)
        {
            Player target = Main.GetPlayerByID(id);
            if (target == null) return;
            CharacterData ccr = Main.Players[target];
            AccountData acc = Main.Accounts[target];
            Houses.House house = Houses.HouseManager.GetHouse(target);
            int houseID = -1;
            if (house != null) houseID = house.ID;
            List<object> data = new List<object>()
            {
                new Dictionary<string, object>()
                {
                    { "Character", ccr },
                    { "Account", acc },
                    { "Props", new List<object>()
                        {
                            houseID,
                            Finance.Bank.Accounts[ccr.Bank].Balance,
                        }
                    }
                }
            };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Plugins.Trigger.ClientEvent(player, "loadPlayerInfo", json);
        }

        public static void sendCoins(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "givecoins")) return;

            if (Main.Accounts[target].Coins + amount < 0) amount = 0;
            Main.Accounts[target].Coins += amount;
            Plugins.Trigger.ClientEvent(target, "starset", Main.Accounts[target].Coins);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы отправили {target.Name} {amount} coins", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"+{amount} coins", 3000);

            Loggings.Admin(player.Name, $"givecoins({amount})", target.Name);
        }
        public static void stopServer(Player sender, string reason = "Сервер выключен.")
        {
            if (!Group.CanUseCmd(sender, "stop")) return;
            IsServerStoping = true;
            Loggings.Admin($"{sender.Name}", $"stopServer({reason})", "");

            Log.Write("Force saving database...", Plugins.Logs.Type.Warn);
            BusinessManager.SavingBusiness();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            Weapons.SaveWeaponsDB();
            Log.Write("All data has been saved!", Plugins.Logs.Type.Success);

            Log.Write("Force kicking players...", Plugins.Logs.Type.Warn);
            foreach (Player player in NAPI.Pools.GetAllPlayers())
                NAPI.Task.Run(() => NAPI.Player.KickPlayer(player, reason));
            Log.Write("All players has kicked!", Plugins.Logs.Type.Success);

            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }
        public static void stopServer(string reason = "Сервер выключен.")
        {
            IsServerStoping = true;
            Loggings.Admin("server", $"stopServer({reason})", "");

            Log.Write("Force saving database...", Plugins.Logs.Type.Warn);
            BusinessManager.SavingBusiness();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            Weapons.SaveWeaponsDB();
            Log.Write("All data has been saved!", Plugins.Logs.Type.Success);

            Log.Write("Force kicking players...", Plugins.Logs.Type.Warn);
            foreach (Player player in NAPI.Pools.GetAllPlayers())
                NAPI.Player.KickPlayer(player, reason);
            Log.Write("All players has kicked!", Plugins.Logs.Type.Success);

            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }
        public static void saveCoords(Player player, string msg)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {

                StreamWriter saveCoords = new StreamWriter("coords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"{msg}   Coords: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{msg}   Rotation: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Close();
            }

            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }

            finally
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords: " + NAPI.Entity.GetEntityPosition(player));
            }
        }

        public static void setPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "setadmin")) return;
            if (Main.Players[target].AdminLVL >= 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока уже есть админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 1;
            target.SetSharedData("IS_ADMIN", true);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы Выдали админ. права игроку {target.Name}", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{player.Name} Выдал Вам админ. права", 3000);
            Loggings.Admin($"{player.Name}", $"setAdmin", $"{target.Name}");
        }

        public static void delPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "deladmin")) return;
            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете забрать админ. права у себя", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете забрать права у этого администратора", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 0;
            target.ResetSharedData("IS_ADMIN");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы забрали права у администратора {target.Name}", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{player.Name} забрал у Вас админ. права", 3000);
            Loggings.Admin($"{player.Name}", $"delAdmin", $"{target.Name}");
        }
        public static void setPlayerAdminRank(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setadminrank")) return;
            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете установить себе ранг", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок не является администратором!", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете изменить уровень прав у этого администратора", 3000);
                return;
            }
            if (rank < 1 || rank >= Main.Players[player].AdminLVL)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно выдать такой ранг", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку {target.Name} {rank} уровень админ. прав", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{player.Name} выдал Вам {rank} уровень админ. прав", 3000);
            Main.Players[target].AdminLVL = rank;

            Loggings.Admin($"{player.Name}", $"setAdminRank({rank})", $"{target.Name}");
        }
        public static void setPlayerVipLvl(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setviplvl")) return;
            if (rank > 4 || rank < 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно выдать такой уровень ВИП аккаунта", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку {target.Name} {Group.GroupNames[rank]}", 3000);
            Main.Accounts[target].VipLvl = rank;
            Main.Accounts[target].VipDate = DateTime.Now.AddDays(30);
            Interface.Dashboard.sendStats(target);
            Loggings.Admin($"{player.Name}", $"setVipLvl({rank})", $"{target.Name}");
        }

        public static void setFracLeader(Player sender, Player target, int fracid)
        {
            if (!Group.CanUseCmd(sender, "setleader")) return;
            if (fracid != 0 && fracid <= 18)
            {
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                int new_fraclvl = Fractions.Configs.FractionRanks[fracid].Count;
                Main.Players[target].FractionLVL = new_fraclvl;
                Main.Players[target].FractionID = fracid;
                Main.Players[target].WorkID = 0;
                if (fracid == 15)
                {
                    Plugins.Trigger.ClientEvent(target, "enableadvert", true);
                    Fractions.Realm.LSNews.onLSNPlayerLoad(target);
                }
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы стали лидером фракции {Fractions.Manager.getName(fracid)}", 3000);
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы поставили {target.Name} на лидерство {Fractions.Manager.getName(fracid)}", 3000);
                Fractions.Manager.Load(target, fracid, new_fraclvl);
                Dashboard.sendStats(target);
                Loggings.Admin($"{sender.Name}", $"setFracLeader({fracid})", $"{target.Name}");
                return;
            }
        }
        public static void delFracLeader(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delleader")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 18)
            {
                if (Main.Players[target].FractionLVL < Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не является лидером", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Plugins.Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{sender.Name.Replace('_', ' ')} снял Вас с поста лидера фракции", 3000);
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с поста лидера фракции", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                Loggings.Admin($"{sender.Name}", $"delFracLeader", $"{target.Name}");
            }
            else Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет фракции", 3000);
        }
        public static void delJob(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "deljob")) return;
            if (Main.Players[target].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(target, "ON_WORK") == true)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок должен быть не в рабочей форме", 3000);
                    return;
                }
                Main.Players[target].WorkID = 0;
                Dashboard.sendStats(target);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{sender.Name.Replace('_', ' ')} снял трудоустройство с Вашего персонажа", 3000);
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с трудоустройства", 3000);
                Dashboard.sendStats(target);
                Loggings.Admin($"{sender.Name}", $"delJob", $"{target.Name}");
            }
            else Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет работы", 3000);
        }
        public static void delFrac(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delfrac")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 17)
            {
                if (Main.Players[target].FractionLVL >= Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок - лидер фракции", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Plugins.Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Администратор {sender.Name.Replace('_', ' ')} выгнал Вас из фракции", 3000);
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы выгнали {target.Name.Replace('_', ' ')} из фракции", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                Loggings.Admin($"{sender.Name}", $"delFrac", $"{target.Name}");
            }
            else Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет фракции", 3000);
        }

        public static void teleportTargetToPlayerWithCar(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "tpcar")) return;
            NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position);
            NAPI.Entity.SetEntityRotation(target.Vehicle, player.Rotation);
            NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
            NAPI.Entity.SetEntityDimension(target, player.Dimension);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы телепортировали {target.Name} к себе", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Администратор {player.Name} телепортировал Вас к себе", 3000);
        }
        public static void adminLSnews(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "lsn")) return;
            NAPI.Chat.SendChatMessageToAll("!{#D47C00}" + $"LS News от {player.Name.Replace('_', ' ')} ({player.Value}): {message}");
        }
        public static void giveMoney(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "givemoney")) return;
            Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", amount, "admin");
            Modules.Wallet.Change(target, amount);
            Loggings.Admin($"{player.Name}", $"giveMoney({amount})", $"{target.Name}");
        }
        public static void OffMutePlayer(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mute")) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    mutePlayer(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрок был онлайн, поэтому offmute заменён на mute", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (time > 480)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                    return;
                }
                var split = target.Split('_');
                Database.QueryRead($"UPDATE `characters` SET `unmute`={time * 60} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} выдал мут игроку {target} на {time} минут");
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
                Loggings.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target}");
            }
            catch { }

        }
        public static void mutePlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "mute")) return;
            if (player == target) return;
            if (time > 480)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                return;
            }
            Main.Players[target].Unmute = time * 60;
            Main.Players[target].VoiceMuted = true;
            if (target.HasData("MUTE_TIMER")) Timers.Stop(target.GetData<string>("MUTE_TIMER"));
            NAPI.Data.SetEntityData(target, "MUTE_TIMER", Timers.StartTask(1000, () => timer_mute(target)));
            target.SetSharedData("voice.muted", true);
            Plugins.Trigger.ClientEvent(target, "voice.mute");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} выдал мут игроку {target.Name} на {time} минут");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
            Loggings.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target.Name}");
        }
        public static void unmutePlayer(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "unmute")) return;

            Main.Players[target].Unmute = 2;
            Main.Players[target].VoiceMuted = false;
            target.SetSharedData("voice.muted", false);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} снял мут с игрока {target.Name}");
            Loggings.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
        }
        public static void banPlayer(Player player, Player target, int time, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "sban" : "ban";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[BAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} забанил игрока {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");

            Modules.BanSystem.Online(target, unbanTime, false, reason, player.Name);

            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы заблокированы до {unbanTime}", 30000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            Loggings.Ban(AUUID, TUUID, unbanTime, reason, false);

            target.Kick(reason);
        }
        public static void hardbanPlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[HARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} ударил банхаммером игрока {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");

            Modules.BanSystem.Online(target, unbanTime, true, reason, player.Name);

            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Ты словил банхаммер до {unbanTime}", 30000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            Loggings.Ban(AUUID, TUUID, unbanTime, reason, true);

            target.Kick(reason);
        }
        public static void offBanPlayer(Player player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name == name) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = Database.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Modules.BanSystem ban = Modules.BanSystem.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м"; // Можно использовать char
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Modules.BanSystem.Offline(name, unbanTime, false, reason, player.Name);

            Loggings.Ban(AUUID, TUUID, unbanTime, reason, false);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} забанил игрока {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
        }
        public static void offHardBanPlayer(Player player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name.Equals(name)) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = Database.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Modules.BanSystem ban = Modules.BanSystem.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Modules.BanSystem.Offline(name, unbanTime, true, reason, player.Name);

            Loggings.Ban(AUUID, TUUID, unbanTime, reason, true);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} ударил банхаммером игрока {name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
        }
        public static void unbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Modules.BanSystem.Pardon(name))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Игрок разблокирован!", 3000);
            Loggings.Admin($"{player.Name}", $"unban", $"{name}");
        }
        public static void unhardbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Modules.BanSystem.PardonHard(name))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "С игрока снят хардбан!", 3000);
        }
        public static void kickPlayer(Player player, Player target, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "skick" : "kick";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[KICK-DENIED] {player.Name} ({player.Value}) попытался кикнуть {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} кикнул игрока {target.Name} по причине {reason}");
            else
            {
                foreach (Player p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"!{{#f25c49}}{player.Name} тихо кикнул игрока {target.Name}");
                    }
                }
            }
            Loggings.Admin($"{player.Name}", $"kickPlayer({reason})", $"{target.Name}");
            NAPI.Player.KickPlayer(target, reason);
        }
        public static void warnPlayer(Player player, Player target, string reason)
        {
            if (!Group.CanUseCmd(player, "warn")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[WARN-DENIED] {player.Name} ({player.Value}) попытался предупредить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            Main.Players[target].Warns++;
            Main.Players[target].Unwarn = DateTime.Now.AddDays(14);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} выдал предупреждение игроку {target.Name} ({Main.Players[target].Warns}/3)");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");

            if (Main.Players[target].Warns >= 3)
            {
                DateTime unbanTime = DateTime.Now.AddMinutes(43200);
                Main.Players[target].Warns = 0;
                Modules.BanSystem.Online(target, unbanTime, false, "Warns 3/3", "Server_Serverniy");
            }

            Loggings.Admin($"{player.Name}", $"warnPlayer({reason})", $"{target.Name}");
            target.Kick("Предупреждение");
        }
        public static void kickPlayerByName(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "nkick")) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target == null) return;
            NAPI.Player.KickPlayer(target);
            Loggings.Admin($"{player.Name}", $"kickPlayer", $"{name}");
        }

        public static void killTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "kill")) return;
            NAPI.Player.SetPlayerHealth(target, 0);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы убили игрока {target.Name}", 3000);
            Loggings.Admin($"{player.Name}", $"killPlayer", $"{target.Name}");
        }
        public static void healTarget(Player player, Player target, int hp)
        {
            if (!Group.CanUseCmd(player, "hp")) return;
            NAPI.Player.SetPlayerHealth(target, hp);
            Loggings.Admin($"{player.Name}", $"healPlayer({hp})", $"{target.Name}");
        }
        public static void armorTarget(Player player, Player target, int ar)
        {
            if (!Group.CanUseCmd(player, "ar")) return;

            nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
            if (aItem == null)
                nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, ar.ToString()));
            Loggings.Admin($"{player.Name}", $"armorPlayer({ar})", $"{target.Name}");
        }
        public static void checkGamemode(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "gm")) return;
            int targetHealth = target.Health;
            int targetArmor = target.Armor;
            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 10));
            NAPI.Task.Run(() => { try { Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"{target.Name} было {targetHealth} HP {targetArmor} Armor | Стало {target.Health} HP {target.Armor} Armor.", 3000); } catch { } }, 3000);
            Loggings.Admin($"{player.Name}", $"checkGm", $"{target.Name}");
        }
        public static void checkMoney(Player player, Player target)
        {
            try
            {
                if (!Group.CanUseCmd(player, "checkmoney")) return;
                Finance.Bank.Data bankAcc = Finance.Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target.Name).Value;
                int bankMoney = 0;
                if (bankAcc != null) bankMoney = (int)bankAcc.Balance;
                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"У {target.Name} {Main.Players[target].Money}$ | Bank: {bankMoney}", 3000);
                Loggings.Admin($"{player.Name}", $"checkMoney", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("CheckMoney: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void teleportTargetToPlayer(Player player, Player target, bool withveh = false)
        {
            if (!Group.CanUseCmd(player, "metp")) return;
            if (!withveh)
            {
                Loggings.Admin($"{player.Name}", $"metp", $"{target.Name}");
                NAPI.Entity.SetEntityPosition(target, player.Position);
                NAPI.Entity.SetEntityDimension(target, player.Dimension);
            }
            else
            {
                if (!target.IsInVehicle) return;
                NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position + new Vector3(2, 2, 2));
                NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
                Loggings.Admin($"{player.Name}", $"gethere", $"{target.Name}");
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы телепортировали {target.Name} к себе", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{player.Name} телепортировал Вас к себе", 3000);
        }

        public static void freezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "fz")) return;
            Plugins.Trigger.ClientEvent(target, "freeze", true);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы заморозили игрока {target.Name}", 3000);
            Loggings.Admin($"{player.Name}", $"freeze", $"{target.Name}");
        }
        public static void unFreezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "ufz")) return;
            Plugins.Trigger.ClientEvent(target, "freeze", false);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы разморозили игрока {target.Name}", 3000);
            Loggings.Admin($"{player.Name}", $"unfreeze", $"{target.Name}");
        }

        public static void giveTargetGun(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "guns")) return;
            if (serial.Length != 9)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Серийный номер состоит из 9 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType == ItemType.Mask || wType == ItemType.Gloves || wType == ItemType.Leg || wType == ItemType.Bag || wType == ItemType.Feet ||
                wType == ItemType.Jewelry || wType == ItemType.Undershit || wType == ItemType.BodyArmor || wType == ItemType.Unknown || wType == ItemType.Top ||
                wType == ItemType.Hat || wType == ItemType.Glasses || wType == ItemType.Accessories)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Предметы одежды выдавать запрещено", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку {target.Name} оружие ({weapon})", 3000);
            Loggings.Admin($"{player.Name}", $"giveGun({weapon},{serial})", $"{target.Name}");
        }
        public static void giveTargetSkin(Player player, Player target, string pedModel)
        {
            if (!Group.CanUseCmd(player, "setskin")) return;
            if (pedModel.Equals("-1"))
            {
                if (target.HasData("AdminSkin"))
                {
                    target.ResetData("AdminSkin");
                    target.SetSkin((Main.Players[target].Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                    Customization.ApplyCharacter(target);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, "Вы восстановили игроку внешность", 3000);
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игроку не меняли внешность", 3000);
                    return;
                }
            }
            else
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                if (pedHash != 0)
                {
                    target.SetData("AdminSkin", true);
                    target.SetSkin(pedHash);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы сменили игроку {target.Name} внешность на ({pedModel})", 3000);
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Внешности с таким названием не было найдено", 3000);
                    return;
                }
            }
        }
        public static void giveTargetClothes(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "giveclothes")) return;
            if (serial.Length < 6 || serial.Length > 12)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Серийный номер состоит из 6-12 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType != ItemType.Mask && wType != ItemType.Gloves && wType != ItemType.Leg && wType != ItemType.Bag && wType != ItemType.Feet &&
                wType != ItemType.Jewelry && wType != ItemType.Undershit && wType != ItemType.BodyArmor && wType != ItemType.Unknown && wType != ItemType.Top &&
                wType != ItemType.Hat && wType != ItemType.Glasses && wType != ItemType.Accessories)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Этой командой можно выдавать только предметы одежды", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку {target.Name} одежду ({weapon})", 3000);
        }
        public static void takeTargetGun(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "oguns")) return;
            Weapons.RemoveAll(target, true);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы забрали у игрока {target.Name} всё оружие", 3000);
            Loggings.Admin($"{player.Name}", $"takeGuns", $"{target.Name}");
        }

        public static void adminSMS(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "asms")) return;
            target.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
            player.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
        }
        public static void answerReport(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "ans")) return;
            if (!target.HasData("IS_REPORT")) return;

            player.SendChatMessage($"~y~Вы ответили для {target.Name}:~w~ {message}");
            target.SendChatMessage($"~r~[Помощь] ~y~{player.Name} ({player.Value}):~w~ {message}");
            target.ResetData("IS_REPORT");
            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage($"~b~[ANSWER] {player.Name}({player.Value})->{target.Name}({target.Value}): {message}");
                }
            }
            Loggings.Admin($"{player.Name}", $"answer({message})", $"{target.Name}");
        }
        public static void adminChat(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "a")) return;
            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage("!{#2b8234}" + $"[Админ-чат] {player.Name} ({player.Value}): {message}");
                }
            }
        }
        public static void adminGlobal(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "global")) return;
            NAPI.Chat.SendChatMessageToAll("!{#f25c49}" + $"{player.Name.Replace('_', ' ')}: {message}");
            Loggings.Admin($"{player.Name}", $"global({message})", $"");
        }
        public static void sendPlayerToDemorgan(Player admin, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(admin, "demorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;
            if (admin == target) return;
            int firstTime = time * 60;
            string deTimeMsg = "м";
            if (time > 60)
            {
                deTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    deTimeMsg = "д";
                    time /= 24;
                }
            }

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{admin.Name} посадил в тюрьму игрока {target.Name} на {time}{deTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
            Main.Players[target].ArrestTime = 0;
            Main.Players[target].DemorganTime = firstTime;
            Fractions.FractionCommands.unCuffPlayer(target);
            NAPI.Entity.SetEntityPosition(target, DemorganPosition + new Vector3(0, 0, 1.5));
            if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData<string>("ARREST_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => timer_demorgan(target)));
            NAPI.Entity.SetEntityDimension(target, 1337);
            Weapons.RemoveAll(target, true);
            Loggings.Admin($"{admin.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target.Name}");
        }
        public static void releasePlayerFromDemorgan(Player admin, Player target)
        {
            if (!Group.CanUseCmd(admin, "udemorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;

            Main.Players[target].DemorganTime = 0;
            Plugins.Notice.Send(admin, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы освободили {target.Name} из админ. тюрьмы", 3000);
            Loggings.Admin($"{admin.Name}", $"undemorgan", $"{target.Name}");
        }

        #region Demorgan
        public static Vector3 DemorganPosition = new Vector3(1651.217, 2570.393, 44.44485);
        public static void timer_demorgan(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].DemorganTime <= 0)
                {
                    Fractions.FractionCommands.freePlayer(player);
                    return;
                }
                Main.Players[player].DemorganTime--;
            }
            catch (Exception e)
            {
                Log.Write("DEMORGAN_TIMER: " + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        public static void timer_mute(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Unmute <= 0)
                {
                    if (!player.HasData("MUTE_TIMER")) return;
                    Timers.Stop(NAPI.Data.GetEntityData(player, "MUTE_TIMER"));
                    NAPI.Data.ResetEntityData(player, "MUTE_TIMER");
                    Main.Players[player].VoiceMuted = false;
                    player.SetSharedData("voice.muted", false);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Mute был снят, не нарушайте больше!", 3000);
                    return;
                }
                Main.Players[player].Unmute--;
            }
            catch (Exception e)
            {
                Log.Write("MUTE_TIMER: " + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        #endregion
        // need refactor
        public static void respawnAllCars(Player player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();

            foreach (Vehicle vehicle in all_vehicles)
            {
                List<Player> occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    List<Player> newOccupants = new List<Player>();
                    foreach (Player occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (Vehicle vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    Player owner = vehicle.GetData<Player>("OWNER");
                    NAPI.Entity.DeleteEntity(vehicle);
                }
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);
            }
        }

        public static void RespawnWorkCar(Vehicle vehicle)
        {
            if (vehicle.GetData<bool>("ON_WORK") && Main.Players.ContainsKey(vehicle.GetData<Player>("DRIVER"))) return;
            var type = vehicle.GetData<string>("TYPE");
            switch (type)
            {
                case "MOWER":
                    Working.Lawnmower.respawnCar(vehicle);
                    break;
                case "BUS":
                    Working.Bus.respawnBusCar(vehicle);
                    break;
                case "TAXI":
                    Working.Taxi.respawnCar(vehicle);
                    break;
                case "TRUCKER":
                    Working.Truckers.respawnCar(vehicle);
                    break;
                case "COLLECTOR":
                    Working.Collector.respawnCar(vehicle);
                    break;
                case "MECHANIC":
                    Working.AutoMechanic.respawnCar(vehicle);
                    break;
            }
        }

        public static void RespawnFractionCar(Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(vehicle, "loaderMats"))
            {
                Player loader = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                Plugins.Trigger.ClientEvent(loader, "hideLoader");
                Plugins.Notice.Send(loader, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                if (loader.HasData("loadMatsTimer"))
                {
                    //Main.StopT(loader.GetData("loadMatsTimer"), "timer_35");
                    Timers.Stop(loader.GetData<string>("loadMatsTimer"));
                    loader.ResetData("loadMatsTime");
                }
                NAPI.Data.ResetEntityData(vehicle, "loaderMats");
            }
            Fractions.Configs.RespawnFractionCar(vehicle);
        }
    }
}
