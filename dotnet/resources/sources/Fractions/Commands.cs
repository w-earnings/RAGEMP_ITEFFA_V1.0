using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Interface;
using iTeffa.Models;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace iTeffa.Fractions
{
    class FractionCommands : Script
    {
        private static readonly Nlogs Log = new Nlogs("FractionCommangs");

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "CUFFED") && player.VehicleSeat == 0)
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Отпустите человека", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, Nlogs.Type.Error); }
        }
        private static readonly Dictionary<int, DateTime> NextCarRespawn = new Dictionary<int, DateTime>()
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
            { 6, DateTime.Now },
            { 7, DateTime.Now },
            { 8, DateTime.Now },
            { 9, DateTime.Now },
            { 10, DateTime.Now },
            { 11, DateTime.Now },
            { 12, DateTime.Now },
            { 13, DateTime.Now },
            { 14, DateTime.Now },
            { 15, DateTime.Now },
            { 16, DateTime.Now },
            { 17, DateTime.Now },
        };
        public static void respawnFractionCars(Player player)
        {
            if (Main.Players[player].FractionID == 0 || Main.Players[player].FractionLVL < (Configs.FractionRanks[Main.Players[player].FractionID].Count - 1)) return;
            if (DateTime.Now < NextCarRespawn[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((NextCarRespawn[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы сможете сделать это только через {min}:{sec}", 3000);
                return;
            }

            var all_vehicles = NAPI.Pools.GetAllVehicles();
            foreach (var vehicle in all_vehicles)
            {
                var occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    var newOccupants = new List<Player>();
                    foreach (var occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (var vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                var color1 = vehicle.PrimaryColor;
                var color2 = vehicle.SecondaryColor;
                if (!vehicle.HasData("ACCESS")) continue;

                if (vehicle.GetData<string>("ACCESS") == "FRACTION" && vehicle.GetData<int>("FRACTION") == Main.Players[player].FractionID)
                    Admin.RespawnFractionCar(vehicle);
            }

            NextCarRespawn[Main.Players[player].FractionID] = DateTime.Now.AddHours(2);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы зареспавнили все фракционные машины", 3000);
        }
        public static void playerPressCuffBut(Player player)
        {
            var fracid = Main.Players[player].FractionID;
            if (!Manager.canUseCommand(player, "cuff")) return;
            if (NAPI.Data.GetEntityData(player, "CUFFED"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы в наручниках или связаны", 3000);
                return;
            }
            var target = Main.GetNearestPlayer(player, 2);
            if (target == null) return;
            var cuffmesp = "";
            var cuffmest = "";
            var cuffme = "";
            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            string uncuffmest;
            string uncuffmesp;
            string uncuffme;
            if (Manager.FractionTypes[fracid] == 2)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны сначала начать рабочий день", 3000);
                    return;
                }
                if (target.GetData<bool>("CUFFED_BY_MAFIA"))
                {
                    uncuffmesp = $"Вы развязали игрока {target.Name}";
                    uncuffmest = $"Игрок {player.Name} развязал Вас";
                    uncuffme = "развязал(а) игрока {name}";
                }
                else
                {
                    cuffmesp = $"Вы надели наручники на игрока {target.Name}";
                    cuffmest = $"Игрок {player.Name} надел на Вас наручники";
                    cuffme = "надел(а) наручники на игрока {name}";
                    uncuffmesp = $"Вы сняли наручники с игрока {target.Name}";
                    uncuffmest = $"Игрок {player.Name} снял с Вас наручники";
                    uncuffme = "снял(а) наручники с игрока {name}";
                }
            }
            else // for mafia
            {
                if (target.GetData<bool>("CUFFED_BY_COP"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет ключей от наручников", 3000);
                    return;
                }
                var cuffs = nInventory.Find(Main.Players[player].UUID, ItemType.Cuffs);
                var count = (cuffs == null) ? 0 : cuffs.Count;

                if (!target.GetData<bool>("CUFFED") && count == 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет стяжек для рук", 3000);
                    return;
                }
                else if (!target.GetData<bool>("CUFFED"))
                    nInventory.Remove(player, ItemType.Cuffs, 1);

                cuffmesp = $"Вы связали игрока {target.Name}";
                cuffmest = $"Игрок {player.Name} связал Вас";
                cuffme = "связал(а) игрока {name}";
                uncuffmesp = $"Вы развязали игрока {target.Name}";
                uncuffmest = $"Игрок {player.Name} развязал Вас";
                uncuffme = "развязал(а) игрока {name}";
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы в машине", 3000);
                return;
            }
            if (NAPI.Player.IsPlayerInAnyVehicle(target))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок в машине", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING") || NAPI.Data.HasEntityData(target, "FOLLOWER") || Main.Players[target].ArrestTime != 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно применить на данном игроке", 3000);
                return;
            }
            if (!target.GetData<bool>("CUFFED"))
            {
                if (NAPI.Data.HasEntityData(target, "HAND_MONEY")) SafeMain.dropMoneyBag(target);
                if (NAPI.Data.HasEntityData(target, "HEIST_DRILL")) SafeMain.dropDrillBag(target);
                NAPI.Data.SetEntityData(target, "CUFFED", true);
                Speaking.Voice.PhoneHCommand(target);
                Main.OnAntiAnim(player);
                NAPI.Player.PlayPlayerAnimation(target, 49, "mp_arresting", "idle");
                BasicSync.AttachObjectToPlayer(target, NAPI.Util.GetHashKey("p_cs_cuffs_02_s"), 6286, new Vector3(-0.02f, 0.063f, 0.0f), new Vector3(75.0f, 0.0f, 76.0f));
                Trigger.ClientEvent(target, "CUFFED", true);
                if (fracid == 6 || fracid == 7 || fracid == 9) target.SetData("CUFFED_BY_COP", true);
                else target.SetData("CUFFED_BY_MAFIA", true);
                Dashboard.Close(target);
                Trigger.ClientEvent(target, "blockMove", true);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, cuffmesp, 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, cuffmest, 3000);
                Commands.Controller.RPChat("me", player, cuffme, target);
                return;
            }
            unCuffPlayer(target);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, uncuffmesp, 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, uncuffmest, 3000);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_COP", false);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_MAFIA", false);
            Commands.Controller.RPChat("me", player, uncuffme, target);
            return;
        }

        public static void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "CUFFED"))
                {
                    unCuffPlayer(player);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    unFollow(player, target);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Player cop = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    unFollow(cop, player);
                }
                if (player.HasData("HEAD_POCKET"))
                {
                    player.ClearAccessory(1);
                    player.SetClothes(1, 0, 0);

                    Trigger.ClientEvent(player, "setPocketEnabled", false);
                    player.ResetData("HEAD_POCKET");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Nlogs.Type.Error); }
        }

        #region every fraction commands

        [Command("delad", GreedyArg = true)]
        public static void CMD_deleteAdvert(Player player, int AdID, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID == 15)
                {
                    if (!Manager.canUseCommand(player, "delad")) return;
                    Fractions.Realm.LSNews.AddAnswer(player, AdID, reason, true);
                }
                else if (Group.CanUseCmd(player, "delad")) Fractions.Realm.LSNews.AddAnswer(player, AdID, reason, true);
            }
            catch (Exception e) { Log.Write("delad: " + e.Message, Nlogs.Type.Error); }
        }

        [Command("openstock")]
        public static void CMD_OpenFractionStock(Player player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Склад уже открыт", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = true;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы открыли склад", 3000);
        }

        [Command("closestock")]
        public static void CMD_CloseFractionStock(Player player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (!Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Склад уже закрыт", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = false;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы закрыли склад", 3000);
        }

        public static void GetMembers(Player sender)
        {
            if (Manager.canUseCommand(sender, "members"))
            {
                sender.SendChatMessage("Члены организации онлайн:");
                int fracid = Main.Players[sender].FractionID;
                foreach (var m in Manager.Members)
                    if (m.Value.FractionID == fracid) sender.SendChatMessage($"[{m.Value.inFracName}] {m.Value.Name}");
            }
        }

        public static void GetAllMembers(Player sender)
        {
            if (Manager.canUseCommand(sender, "offmembers"))
            {
                string message = "Все члены организации: ";
                NAPI.Chat.SendChatMessageToPlayer(sender, message);
                int fracid = Main.Players[sender].FractionID;
                var result = Database.QueryRead($"SELECT * FROM `characters` WHERE `fraction`='{fracid}'");
                foreach (DataRow Row in result.Rows)
                {
                    var fraclvl = Convert.ToInt32(Row["fractionlvl"]);
                    NAPI.Chat.SendChatMessageToPlayer(sender, $"~g~[{Manager.getNickname(fracid, fraclvl)}]: ~w~" + Row["name"].ToString().Replace('_', ' '));
                }
                return;
            }
        }

        public static void SetFracRank(Player sender, Player target, int newrank)
        {
            if (Manager.canUseCommand(sender, "setrank"))
            {
                if (newrank <= 0)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Нельзя установить отрицательный или нулевой ранг", 3000);
                    return;
                }
                int senderlvl = Main.Players[sender].FractionLVL;
                int playerlvl = Main.Players[target].FractionLVL;
                int senderfrac = Main.Players[sender].FractionID;
                if (!Manager.inFraction(target, senderfrac)) return;

                if (newrank >= senderlvl)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете повысить до этого ранга", 3000);
                    return;
                }
                if (playerlvl > senderlvl)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете повысить этого игрока", 3000);
                    return;
                };
                Manager.UNLoad(target);

                Main.Players[target].FractionLVL = newrank;
                Manager.Load(target, Main.Players[target].FractionID, Main.Players[target].FractionLVL);
                int index = Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1)
                {
                    Manager.AllMembers[index].FractionLVL = newrank;
                    Manager.AllMembers[index].inFracName = Manager.getNickname(senderfrac, newrank);
                }
                Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Теперь ты {Manager.Members[target].inFracName} во фракции", 3000);
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы повысили игрока {target.Name} до {Manager.Members[target].inFracName}", 3000);
                Dashboard.sendStats(target);
                return;
            }
        }

        public static void InviteToFraction(Player sender, Player target)
        {
            if (Manager.canUseCommand(sender, "invite"))
            {
                if (sender.Position.DistanceTo(target.Position) > 3)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
                    return;
                }
                if (Manager.isHaveFraction(target))
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок уже состоит организации", 3000);
                    return;
                }
                if (Main.Players[target].LVL < 1)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Необходим как минимум 1 lvl для приглашения игрока во фракцию", 3000);
                    return;
                }
                if (Main.Players[target].Warns > 0)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно принять этого игрока", 3000);
                    return;
                }
                if (Manager.FractionTypes[Main.Players[sender].FractionID] == 2 && !Main.Players[target].Licenses[7])
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет мед.карты", 3000);
                    return;
                }

                target.SetData("INVITEFRACTION", Main.Players[sender].FractionID);
                target.SetData("SENDERFRAC", sender);
                Trigger.ClientEvent(target, "openDialog", "INVITED", $"{sender.Name} пригласил Вас в {Manager.FractionNames[Main.Players[sender].FractionID]}");

                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы пригласили во фракцию {target.Name}", 3000);
                Dashboard.sendStats(target);
            }
        }

        public static void UnInviteFromFraction(Player sender, Player target, bool mayor = false)
        {
            if (!Manager.canUseCommand(sender, "uninvite")) return;
            if (sender == target)
            {
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете уволить сами себя", 3000);
                return;
            }

            int senderlvl = Main.Players[sender].FractionLVL;
            int playerlvl = Main.Players[target].FractionLVL;
            int senderfrac = Main.Players[sender].FractionID;

            if (senderlvl <= playerlvl)
            {
                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете выгнать этого игрока", 3000);
                return;
            }

            if (mayor)
            {
                if (Manager.FractionTypes[Main.Players[target].FractionID] != 2)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете выгнать этого игрока", 3000);
                    return;
                }
            }
            else
            {
                if (senderfrac != Main.Players[target].FractionID)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок состоит в другой организации", 3000);
                    return;
                }
            }

            Manager.UNLoad(target);

            int index = Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Manager.AllMembers.RemoveAt(index);

            if (Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            Customization.ApplyCharacter(target);
            if (target.HasData("HAND_MONEY")) target.SetClothes(5, 45, 0);
            else if (target.HasData("HEIST_DRILL")) target.SetClothes(5, 41, 0);
            target.SetData("ON_DUTY", false);
            Interface.MenuManager.Close(sender);

            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вас выгнали из фракции {Manager.FractionNames[Main.Players[sender].FractionID]}", 3000);
            Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выгнали из фракции {target.Name}", 3000);
            Dashboard.sendStats(target);
            return;
        }
        #endregion

        #region cops and cityhall commands
        public static void ticketToTarget(Player player, Player target, int sum, string reason)
        {
            if (!Manager.canUseCommand(player, "ticket")) return;
            if (sum > 7000)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Ограничение по штрафу 7000$", 3000);
                return;
            }
            if (reason.Length > 100)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Слишком большая причина", 3000);
                return;
            }
            if (Main.Players[target].Money < sum && Finance.Bank.Accounts[Main.Players[target].Bank].Balance < sum)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно средств", 3000);
                return;
            }

            target.SetData("TICKETER", player);
            target.SetData("TICKETSUM", sum);
            target.SetData("TICKETREASON", reason);
            Trigger.ClientEvent(target, "openDialog", "TICKET", $"{player.Name} выписал Вам штраф в размере {sum}$ за {reason}. Оплатить?");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выписали штраф для {target.Name} в размере {sum}$ за {reason}", 3000);
        }
        public static void ticketConfirm(Player target, bool confirm)
        {
            Player player = target.GetData<Player>("TICKETER");
            if (player == null || !Main.Players.ContainsKey(player)) return;
            int sum = target.GetData<int>("TICKETSUM");
            string reason = target.GetData<string>("TICKETREASON");

            if (confirm)
            {
                if (!Finance.Wallet.Change(target, -sum) && !Finance.Bank.Change(Main.Players[target].Bank, -sum, false))
                {
                    Plugins.Notice.Send(target, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств", 3000);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно средств", 3000);
                }

                Stocks.fracStocks[6].Money += Convert.ToInt32(sum * 0.9);
                Finance.Wallet.Change(player, Convert.ToInt32(sum * 0.1));
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы оплатили штраф в размере {sum}$ за {reason}", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{target.Name} оплатил штраф в размере {sum}$ за {reason}", 3000);
                Commands.Controller.RPChat("me", player, " выписал штраф для {name}", target);
                Manager.sendFractionMessage(7, $"{player.Name} оштрафовал {target.Name} на {sum}$ ({reason})", true);
                Loggings.Ticket(Main.Players[player].UUID, Main.Players[target].UUID, sum, reason, player.Name, target.Name);
            }
            else
            {
                Plugins.Notice.Send(target, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы отказались платить штраф в размере {sum}$ за {reason}", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"{target.Name} отказался платить штраф в размере {sum}$ за {reason}", 3000);
            }
        }
        public static void arrestTarget(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "arrest")) return;
            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно применить на себе", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны быть возле камеры", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime != 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок уже в тюрьме", 3000);
                return;
            }
            if (Main.Players[target].WantedLVL == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в розыске", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в наручниках", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                unFollow(target.GetData<Player>("FOLLOWING"), target);
            }
            unCuffPlayer(target);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы посадили игрока ({target.Value}) на {Main.Players[target].WantedLVL.Level * 20} минут", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) посадил Вас на {Main.Players[target].WantedLVL.Level * 20} минут", 3000);
            Commands.Controller.RPChat("me", player, " поместил {name} в КПЗ", target);
            Manager.sendFractionMessage(7, $"{player.Name} посадил в КПЗ {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Manager.sendFractionMessage(9, $"{player.Name} посадил в КПЗ {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Main.Players[target].ArrestTime = Main.Players[target].WantedLVL.Level * 20 * 60;
            Loggings.Arrest(Main.Players[player].UUID, Main.Players[target].UUID, Main.Players[target].WantedLVL.Reason, Main.Players[target].WantedLVL.Level, player.Name, target.Name);
            arrestPlayer(target);
        }

        public static void releasePlayerFromPrison(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "rfp")) return;
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно применить на себе", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 3)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны быть возле камеры", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime == 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в тюрьме", 3000);
                return;
            }
            freePlayer(target);
            Main.Players[target].ArrestTime = 0;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы освободили игрока ({target.Value}) из тюрьмы", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) освободил Вас из тюрьмы", 3000);
            Commands.Controller.RPChat("me", player, " освободил {name} из КПЗ", target);
        }

        public static void arrestTimer(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].ArrestTime == 0)
                {
                    freePlayer(player);
                    return;
                }
                Main.Players[player].ArrestTime--;
            }
            catch (Exception e)
            {
                Log.Write("ARRESTTIMER: " + e.ToString(), Nlogs.Type.Error);
            }

        }

        public static void freePlayer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("ARREST_TIMER")) return;
                    Settings.Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER"));
                    NAPI.Data.ResetEntityData(player, "ARREST_TIMER");
                    Fractions.Realm.Police.setPlayerWantedLevel(player, null);
                    NAPI.Entity.SetEntityPosition(player, Fractions.Realm.Police.policeCheckpoints[5]);
                    NAPI.Entity.SetEntityPosition(player, Fractions.Realm.Sheriff.sheriffCheckpoints[5]);
                    NAPI.Entity.SetEntityDimension(player, 0);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы были освобождены из тюрьмы", 3000);
                }
                catch { }
            });
        }

        public static void arrestPlayer(Player target)
        {
            NAPI.Entity.SetEntityPosition(target, Fractions.Realm.Police.policeCheckpoints[4]);
            Fractions.Realm.Police.setPlayerWantedLevel(target, null);
            NAPI.Entity.SetEntityPosition(target, Fractions.Realm.Sheriff.sheriffCheckpoints[4]);
            Fractions.Realm.Sheriff.setPlayerWantedLevel(target, null);
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Settings.Timers.Start(1000, () => arrestTimer(target)));
            Weapons.RemoveAll(target, true);
        }

        public static void unCuffPlayer(Player player)
        {
            Trigger.ClientEvent(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Player.StopPlayerAnimation(player);
            BasicSync.DetachObject(player);
            Trigger.ClientEvent(player, "blockMove", false);
            Main.OffAntiAnim(player);
        }

        [RemoteEvent("playerPressFollowBut")]
        public void ClientEvent_playerPressFollow(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Manager.canUseCommand(player, "follow", false)) return;
                if (player.HasData("FOLLOWER"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отпустили игрока ({player.GetData<Player>("FOLLOWER").Value})", 3000);
                    Plugins.Notice.Send(player.GetData<Player>("FOLLOWER"), Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) отпустил Вас", 3000);
                    unFollow(player, player.GetData<Player>("FOLLOWER"));
                }
                else
                {
                    var target = Main.GetNearestPlayer(player, 2);
                    if (target == null || !Main.Players.ContainsKey(target)) return;
                    targetFollowPlayer(player, target);
                }
            }
            catch (Exception e) { Log.Write($"PlayerPressFollow: {e} // {e.TargetSite} // ", Nlogs.Type.Error); }
        }

        public static void unFollow(Player cop, Player suspect)
        {
            NAPI.Data.ResetEntityData(cop, "FOLLOWER");
            NAPI.Data.ResetEntityData(suspect, "FOLLOWING");
            Trigger.ClientEvent(suspect, "setFollow", false);
        }

        public static void targetFollowPlayer(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            var fracid = Main.Players[player].FractionID;
            if (Manager.FractionTypes[fracid] == 2) // for gov factions
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны сначала начать рабочий день", 3000);
                    return;
                }
            }
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно применить на себе", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже тащите за собой игрока", 3000);
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в наручниках", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрока уже тащат", 3000);
                return;
            }

            NAPI.Data.SetEntityData(player, "FOLLOWER", target);
            NAPI.Data.SetEntityData(target, "FOLLOWING", player);
            Trigger.ClientEvent(target, "setFollow", true, player);
            Commands.Controller.RPChat("me", player, "потащил(а) {name} за собой", target);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы потащили за собой игрока ({target.Value})", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) потащил Вас за собой", 3000);
        }
        public static void targetUnFollowPlayer(Player player)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            _ = Main.Players[player].FractionID;
            if (!NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы никого не тащите за собой", 3000);
                return;
            }
            Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
            unFollow(player, target);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отпустили игрока ({target.Value})", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) отпустил Вас", 3000);
        }

        public static void suPlayer(Player player, int pasport, int stars, string reason)
        {
            if (!Manager.canUseCommand(player, "su")) return;
            if (!Main.PlayerNames.ContainsKey(pasport))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Паспорта с таким номером не существует", 3000);
                return;
            }
            Player target = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
            if (target == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Владелец паспорта должен быть в сети", 3000);
                return;
            }
            if (player != target)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны начать рабочий день", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок в тюрьме", 3000);
                    return;
                }

                if (stars > 5)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете выдать такое кол-во звёзд", 3000);
                    return;
                }

                if (Main.Players[target].WantedLVL == null || Main.Players[target].WantedLVL.Level + stars <= 5)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы объявили игрока " + target.Name.Replace('_', ' ') + " в розыск", 3000);
                    Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"{player.Name.Replace('_', ' ')} объявил Вас в розыск ({reason})", 3000);
                    var oldStars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
                    var wantedLevel = new WantedLevel(oldStars + stars, player.Name, DateTime.Now, reason);
                    Fractions.Realm.Police.setPlayerWantedLevel(target, wantedLevel);
                    Fractions.Realm.Sheriff.setPlayerWantedLevel(target, wantedLevel);
                    return;
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете выдать такое кол-во звёзд", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете объявить в розыск самого себя", 3000);
        }

        public static void playerInCar(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "incar")) return;
            if (player == target)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно использовать на себе", 3000);
                return;
            }
            var vehicle = VehicleManager.getNearestVehicle(player, 3);
            if (vehicle == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Рядом нет машин", 3000);
                return;
            }
            if (player.VehicleSeat != 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны быть на водительском месте", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 5)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок должен быть в наручниках", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                var cop = NAPI.Data.GetEntityData(target, "FOLLOWING");
                unFollow(cop, target);
            }

            var emptySlots = new List<int>
            {
                2,
                1,
                0
            };

            var players = NAPI.Pools.GetAllPlayers();
            foreach (var p in players)
            {
                if (p == null || !p.IsInVehicle || p.Vehicle != vehicle) continue;
                if (emptySlots.Contains(p.VehicleSeat)) emptySlots.Remove(p.VehicleSeat);
            }

            if (emptySlots.Count == 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В машине нет места", 3000);
                return;
            }

            NAPI.Player.SetPlayerIntoVehicle(target, vehicle, emptySlots[0]);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы запихали игрока ({target.Value}) в машину", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) запихал Вас в машину", 3000);
            Commands.Controller.RPChat("me", player, " открыл дверь и усадил {name} в машину", target);
        }

        public static void playerOutCar(Player player, Player target)
        {
            if (player != target)
            {
                if (!Manager.canUseCommand(player, "pull")) return;
                _ = NAPI.Entity.GetEntityPosition(player);
                _ = NAPI.Entity.GetEntityPosition(target);
                if (player.Position.DistanceTo(target.Position) < 5)
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(target))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выкинули игрока ({target.Value}) из машины", 3000);
                        Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) Выкинул Вас из машины", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(target);
                        Commands.Controller.RPChat("me", player, " открыл дверь и вытащил {name} из машины", target);
                    }
                    else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в машине", 3000);
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
            }
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете Выкинуть сами себя из машины", 3000);
        }

        public static void setWargPoliceMode(Player player)
        {
            if (!Manager.canUseCommand(player, "warg"))
            {
                return;
            }
            if (Main.Players[player].FractionID == 18)
            {
                Fractions.Realm.Police.is_warg = !Fractions.Realm.Police.is_warg;
                string message;
                if (Fractions.Realm.Police.is_warg) message = $"{NAPI.Player.GetPlayerName(player)} объявил режим ЧП!!!";
                else message = $"{NAPI.Player.GetPlayerName(player)} отключил режим ЧП.";
                Manager.sendFractionMessage(7, message);
            }
            if (Main.Players[player].FractionID == 7)
            {
                Fractions.Realm.Police.is_warg = !Fractions.Realm.Police.is_warg;
                string message;
                if (Fractions.Realm.Police.is_warg) message = $"{NAPI.Player.GetPlayerName(player)} объявил режим ЧП!!!";
                else message = $"{NAPI.Player.GetPlayerName(player)} отключил режим ЧП.";
                Manager.sendFractionMessage(7, message);
            }
            else if (Main.Players[player].FractionID == 9)
            {
                Fractions.Realm.Fbi.warg_mode = !Fractions.Realm.Fbi.warg_mode;
                string message;
                if (Fractions.Realm.Fbi.warg_mode) message = $"{NAPI.Player.GetPlayerName(player)} объявил режим ЧП!!!";
                else message = $"{NAPI.Player.GetPlayerName(player)} отключил режим ЧП.";
                Manager.sendFractionMessage(9, message);
            }

        }

        public static void takeGunLic(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "takegunlic")) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (!Main.Players[target].Licenses[6])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет лицензии на оружие", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы отобрали лицензию на оружие у игрока ({target.Value})", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) отобрал у Вас лицензию на оружие", 3000);
            Main.Players[target].Licenses[6] = false;
            Dashboard.sendStats(target);
        }

        public static void giveGunLic(Player player, Player target, int price)
        {
            if (!Manager.canUseCommand(player, "givegunlic")) return;
            if (player == target) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }
            if (price < 5000 || price > 6000)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Цена некорректна", 3000);
                return;
            }
            if (Main.Players[target].Licenses[6])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока уже есть лицензия на оружие", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно средств", 3000);
                return;
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили купить лицензию на оружие игроку ({target.Value}) за ${price}", 3000);

            Trigger.ClientEvent(target, "openDialog", "GUN_LIC", $"Игрок ({player.Value}) предложил Вам купить лицензию на оружие за ${price}");
            target.SetData("SELLER", player);
            target.SetData("GUN_PRICE", price);
        }

        public static void acceptGunLic(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;

            Player seller = player.GetData<Player>("SELLER");
            if (!Main.Players.ContainsKey(seller)) return;
            int price = player.GetData<int>("GUN_PRICE");
            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Продавец слишком далеко", 3000);
                return;
            }

            if (!Finance.Wallet.Change(player, -price))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств", 3000);
                return;
            }

            Finance.Wallet.Change(seller, price / 20);
            Stocks.fracStocks[6].Money += Convert.ToInt32(price * 0.95);
            Loggings.Money($"player({Main.Players[player].UUID})", $"frac(6)", price, $"buyGunlic({Main.Players[seller].UUID})");
            Loggings.Money($"frac(6)", $"player({Main.Players[seller].UUID})", price / 20, $"sellGunlic({Main.Players[player].UUID})");

            Main.Players[player].Licenses[6] = true;
            Dashboard.sendStats(player);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы купили лицензию на оружие у игрока ({seller.Value}) за {price}$", 3000);
            Plugins.Notice.Send(seller, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) купил у Вас лицензию на оружие", 3000);
        }

        public static void playerTakeoffMask(Player player, Player target)
        {
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (!target.HasSharedData("IS_MASK") || !target.HasSharedData("IS_MASK"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет маски", 3000);
                return;
            }

            var maskItem = nInventory.Items[Main.Players[target].UUID].FirstOrDefault(i => i.Type == ItemType.Mask && i.IsActive);
            nInventory.Remove(target, maskItem);
            Customization.CustomPlayerData[Main.Players[target].UUID].Clothes.Mask = new ComponentItem(0, 0);
            if (maskItem != null) Items.onDrop(player, maskItem, null);

            Customization.SetMask(target, 0, 0); ;

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы сорвали маску с игрока ({target.Value})", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) сорвал с Вас маску", 3000);
            Commands.Controller.RPChat("me", player, " сорвал маску с {name}", target);
        }
        #endregion

        #region crimeCommands
        public static void robberyTarget(Player player, Player target)
        {
            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (!target.GetData<bool>("CUFFED") && !target.HasData("HANDS_UP"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок должен быть связан или с поднятыми руками", 3000);
                return;
            }

            if (!player.HasSharedData("IS_MASK") || !player.GetSharedData<bool>("IS_MASK"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Ограбление возможно только в маске", 3000);
                return;
            }

            if (Main.Players[target].LVL < 2 || Main.Players[target].Money <= 1000 || (target.HasData("NEXT_ROB") && DateTime.Now < target.GetData<DateTime>("NEXT_ROB")))
            {
                Commands.Controller.RPChat("me", player, "хорошенько обшарив {name}, ничего не нашёл", target);
                return;
            }

            var max = (Main.Players[target].Money >= 3000) ? 3000 : Convert.ToInt32(Main.Players[target].Money) - 200;
            var min = (max - 200 < 0) ? max : max - 200;

            var found = Main.rnd.Next(min, max + 1);
            Finance.Wallet.Change(target, -found);
            Finance.Wallet.Change(player, found);
            Loggings.Money($"player({Main.Players[target].UUID})", $"player({Main.Players[player].UUID})", found, $"robbery");
            target.SetData("NEXT_ROB", DateTime.Now.AddMinutes(60));

            Commands.Controller.RPChat("me", player, "хорошенько обшарив {name}" + $", нашёл ${found}", target);
        }

        public static void playerChangePocket(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "pocket")) return;
            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            if (target.HasData("HEAD_POCKET"))
            {
                target.ClearAccessory(1);
                target.SetClothes(1, 0, 0);

                Trigger.ClientEvent(target, "setPocketEnabled", false);
                target.ResetData("HEAD_POCKET");

                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы сняли мешок с игрока ({target.Value})", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) снял с Вас мешок", 3000);
                Commands.Controller.RPChat("me", player, "снял(а) мешок с {name}", target);
            }
            else
            {
                if (nInventory.Find(Main.Players[player].UUID, ItemType.Pocket) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет мешков", 3000);
                    return;
                }

                target.SetAccessories(1, 24, 2);
                target.SetClothes(1, 56, 1);

                Trigger.ClientEvent(target, "setPocketEnabled", true);
                target.SetData("HEAD_POCKET", true);

                nInventory.Remove(player, ItemType.Pocket, 1);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы надели мешок на игрока ({target.Value})", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) надел на Вас мешок", 3000);
                Commands.Controller.RPChat("me", player, "надел(а) мешок на {name}", target);
            }
        }
        #endregion

        #region EMS commands
        public static void giveMedicalLic(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "givemedlic")) return;

            if (Main.Players[target].Licenses[7])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока уже есть мед. карта", 3000);
                return;
            }

            Main.Players[target].Licenses[7] = true;
            Dashboard.sendStats(target);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку {target.Name} медицинскую карту", 3000);
            Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{player.Name} выдал Вам медицинскую карту", 3000);
        }
        public static void sellMedKitToTarget(Player player, Player target, int price)
        {
            if (Manager.canUseCommand(player, "medkit"))
            {
                if (!player.GetData<bool>("ON_DUTY"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны начать рабочий день", 3000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (item == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны взять аптечки со склада", 3000);
                    return;
                }
                if (price < 500 || price > 1500)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны установить цену от 500$ до 1500$", 3000);
                    return;
                }
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                if (Main.Players[target].Money < price)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет столько денег", 3000);
                    return;
                }
                Trigger.ClientEvent(target, "openDialog", "PAY_MEDKIT", $"Медик ({player.Value}) предложил купить Вам аптечку за ${price}.");
                target.SetData("SELLER", player);
                target.SetData("PRICE", price);

                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили купить игроку ({target.Value}) аптечку за {price}$", 3000);
            }
        }

        public static void acceptEMScall(Player player, Player target)
        {
            if (Manager.canUseCommand(player, "accept"))
            {
                if (!player.GetData<bool>("ON_DUTY"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не начали рабочий день", 3000);
                    return;
                }
                if (!target.HasData("IS_CALL_EMS"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не вызывал скорую", 3000);
                    return;
                }
                Trigger.ClientEvent(player, "createWaypoint", target.Position.X, target.Position.Y);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Медик ({player.Value}) принял Ваш вызов", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы приняли вызов игрока ({target.Value})", 3000);
                target.ResetData("IS_CALL_EMS");
                return;
            }
        }

        public static void healTarget(Player player, Player target, int price)
        {
            if (Manager.canUseCommand(player, "heal"))
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                if (price < 50 || price > 400)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны установить цену от 50$ до 400$", 3000);
                    return;
                }
                if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.IsPlayerInAnyVehicle(target))
                {
                    var pveh = player.Vehicle;
                    var tveh = target.Vehicle;
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
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Медик ({player.Value}) предложил лечение за ${price}");

                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили лечение игроку ({target.Value}) за {price}$", 3000);
                    return;
                }
                else if (player.GetData<bool>("IN_HOSPITAL") && target.GetData<bool>("IN_HOSPITAL"))
                {
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Медик ({player.Value}) предложил лечение за ${price}");
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили лечение игроку ({target.Value}) за {price}$", 3000);
                    return;
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны быть в больнице или корете скорой помощи", 3000); ;
                    return;
                }
            }
        }
        #endregion
    }
}