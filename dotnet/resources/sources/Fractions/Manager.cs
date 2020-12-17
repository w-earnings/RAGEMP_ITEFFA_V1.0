using System.Collections.Generic;
using System.Data;
using System;
using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;
using iTeffa.Interface;
using iTeffa.Globals.Character;
using System.Linq;
using Newtonsoft.Json;

namespace iTeffa.Fractions
{
    class Manager : Script
    {
        private static readonly nLog Log = new nLog("Fractions");
        public static void OnResourceStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(771, new Vector3(4840.571, -5174.425, 2.0), 0.75F, 46, Main.StringToU16("Остров невезений"), 255, 0, true, 0);
                NAPI.Blip.CreateBlip(437, FractionSpawns[1], 0.85F, 52, Main.StringToU16("The Families"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(437, FractionSpawns[2], 0.85F, 58, Main.StringToU16("The Ballas"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(437, FractionSpawns[3], 0.85f, 28, Main.StringToU16("Los Santos Vagos"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(437, FractionSpawns[4], 0.85F, 74, Main.StringToU16("Marabunta Grande"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(437, FractionSpawns[5], 0.85F, 49, Main.StringToU16("Blood Street"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(674, FractionSpawns[6], 0.85F, 14, Main.StringToU16("CityHall"), 255, 0, true, 0, 0); // DEVELOPER
                NAPI.Blip.CreateBlip(526, FractionSpawns[7], 0.85F, 38, Main.StringToU16("Police Dept"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(61, FractionSpawns[8], 0.75F, 49, Main.StringToU16("Medical Center"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(88, FractionSpawns[9], 0.75F, 58, Main.StringToU16("FIB"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(640, FractionSpawns[14], 0.75F, 52, Main.StringToU16("Army"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(526, FractionSpawns[18], 0.75F, 47, Main.StringToU16("Sheriff"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, FractionSpawns[10], 0.75F, 5, Main.StringToU16("La Cosa Nostra"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, FractionSpawns[11], 0.75F, 4, Main.StringToU16("Русская мафия"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, FractionSpawns[12], 0.75F, 76, Main.StringToU16("Якудза"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, FractionSpawns[13], 0.75F, 40, Main.StringToU16("Армянская мафия"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(184, LSNews.LSNewsCoords[0], 0.75F, 1, Main.StringToU16("Новости"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(85, Army.ArmyCheckpoints[2], 0.75F, 28, Main.StringToU16("Доки"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(668, new Vector3(-1123.202, 4929.628, 217.7096), 0.75F, 75, Main.StringToU16("Redneck"), 255, 0, true, 0);
                NAPI.Blip.CreateBlip(675, new Vector3(-1304.6462, -560.2332, 33.25491), 0.85F, 14, Main.StringToU16("CityHall"), 255, 0, true, 0);

                var result = Connect.QueryRead("SELECT `uuid`,`firstname`,`lastname`,`fraction`,`fractionlvl` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        var memberData = new MemberData
                        {
                            Name = $"{Convert.ToString(Row["firstname"])}_{Convert.ToString(Row["lastname"])}",
                            FractionID = Convert.ToInt32(Row["fraction"]),
                            FractionLVL = Convert.ToInt32(Row["fractionlvl"])
                        };
                        memberData.inFracName = getNickname(memberData.FractionID, memberData.FractionLVL);

                        if (memberData.FractionID != 0)
                            AllMembers.Add(memberData);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"FRACTIONS_MANAGER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static Dictionary<Weapons.Hash, int> matsForGun = new Dictionary<Weapons.Hash, int>()
        {
            { Weapons.Hash.Pistol, 50 },
            { Weapons.Hash.MarksmanPistol, 300 },
            { Weapons.Hash.SNSPistol, 40 },
            { Weapons.Hash.DoubleBarrelShotgun, 80 },
            { Weapons.Hash.SawnOffShotgun, 100 },
            { Weapons.Hash.MachinePistol, 130 },
            { Weapons.Hash.MiniSMG, 120 },
            { Weapons.Hash.Bat, 30 },
            { Weapons.Hash.Machete, 30 },
            { Weapons.Hash.Pistol50, 80 },
            { Weapons.Hash.CombatPistol, 80 },
            { Weapons.Hash.VintagePistol, 70 },
            { Weapons.Hash.PumpShotgun, 120 },
            { Weapons.Hash.BullpupShotgun, 140 },
            { Weapons.Hash.AssaultRifle, 200 },
            { Weapons.Hash.CompactRifle, 180 },
            { Weapons.Hash.Hatchet, 50 },
            { Weapons.Hash.GolfClub, 50 },
            { Weapons.Hash.SwitchBlade, 50 },
            { Weapons.Hash.Hammer, 50 },
            { Weapons.Hash.MicroSMG, 150 },
            { Weapons.Hash.Nightstick, 30 },
            { Weapons.Hash.SMG, 60 },
            { Weapons.Hash.CombatPDW, 60 },
            { Weapons.Hash.StunGun, 100 },
            { Weapons.Hash.CarbineRifle, 100 },
            { Weapons.Hash.SmokeGrenade, 5 },
            { Weapons.Hash.HeavyShotgun, 500 },
            { Weapons.Hash.BullpupRifle, 500 },
            { Weapons.Hash.Knife, 20 },
            { Weapons.Hash.SniperRifle, 150 },
            { Weapons.Hash.HeavySniper, 1200 },
            { Weapons.Hash.AssaultSMG, 50 },
            { Weapons.Hash.AdvancedRifle, 50 },
            { Weapons.Hash.Gusenberg, 500 },
            { Weapons.Hash.CombatMG, 500 },


        };
        public static int matsForArmor = 250;
        private static readonly List<List<string>> gangGuns = new List<List<string>>
        {
            new List<string>
            {
                "Pistol",
                "SNSPistol",
            },
            new List<string>
            {
                "DoubleBarrelShotgun",
                "SawnOffShotgun",
            },
            new List<string>
            {
                "MicroSMG",
            },
            new List<string>
            {
                "BodyArmor",
            },
            new List<string>(),
        };
        private static readonly List<List<string>> mafiaGuns = new List<List<string>>
        {
            new List<string>
            {
                "Pistol",
                "MarksmanPistol",
                "VintagePistol",
            },
            new List<string>
            {
                "PumpShotgun",
            },
            new List<string>
            {
                "MiniSMG",
            },
            new List<string>
            {
                "AssaultRifle",
                "CompactRifle",
            },
        };

        public static Dictionary<Player, MemberData> Members = new Dictionary<Player, MemberData>();


        public static SortedList<int, Vector3> FractionSpawns = new SortedList<int, Vector3>()
        {
            {1, new Vector3(-25.01989, -1398.197, 29.38819)},    // The Families
            {2, new Vector3(111.9266, -2005.851, 18.18042)},     // The Ballas Gang
            {3, new Vector3(482.8006, -1877.859, 25.97736)},     // Los Santos Vagos
            {4, new Vector3(1445.421, -1486.313, 66.49925)},     // Marabunta Grande
            {5, new Vector3(966.2534, -1833.792, 31.14424)},     // Blood Street
            {6, new Vector3(-572.94464, -201.82872, 42.58397)},  // Cityhall
            {7, new Vector3(457.4271, -991.4473, 31.5696)},      // Police Dept
            {8, new Vector3(346.2975, -1434.87, 32.81624)},      // Medical Center
            {9, new Vector3(149.4746, -756.9065, 243.0319)},     // FBI

            {10, new Vector3(1387.338, 1155.952, 115.2144)},     // La Cosa Nostra 
            {11, new Vector3(-115.1648, 983.5231, 236.6358)},    // Russian Mafia
            {12, new Vector3(-1549.22, -86.07732, 55.20967)},    // Yakuza 
            {13, new Vector3(-1809.738, 444.3138, 129.3889)},    // Armenian Mafia 

            {14, new Vector3(-2355.625, 3254.189, 33.69071)},    // Army
            {15, new Vector3(-1063.046, -249.463, 44.0211)},     // LSNews
            {16, new Vector3(982.2743, -104.14917, 73.72877)},   // The Lost
            {17, new Vector3(2154.641, 2921.034, -63.02243)},    // Merryweather
            {18, new Vector3(-441.9835, 5987.603, 30.59653)},    // Sheriff
        };


        public static SortedList<int, int> FractionTypes = new SortedList<int, int>() // 0 - mafia, 1 gangs, 2 - gov, 
        {
            {0, -1},
            {1, 1}, // The Families
            {2, 1}, // The Ballas Gang
            {3, 1},  // Los Santos Vagos
            {4, 1}, // Marabunta Grande
            {5, 1}, // Blood Street
            {6, 2}, // Cityhall
            {7, 2}, // LSPD police
            {8, 2}, // Emergency care
            {9, 2}, // FBI 
            {10, 0}, // La Cosa Nostra 
            {11, 0}, // Russian Mafia
            {12, 0}, // Yakuza 
            {13, 0}, // Armenian Mafia 
            {14, 2}, // Army
            {15, 2}, // News
            {16, 1}, // The Lost
            {17, 2}, // Merryweather
            {18, 2}, // Sheriff
        };
        public static SortedList<int, string> FractionNames = new SortedList<int, string>()
        {
            {0, "-" },
            {1, "The Families" },
            {2, "The Ballas Gang" },
            {3, "Los Santos Vagos" },
            {4, "Marabunta Grande" },
            {5, "Blood Street" },
            {6, "Cityhall" },
            {7, "Police" },
            {8, "Hospital" },
            {9, "FIB" },
            {10, "La Cosa Nostra" },
            {11, "Russian Mafia" },
            {12, "Yakuza" },
            {13, "Armenian Mafia" },
            {14, "Army" },
            {15, "News" },
            {16, "The Lost" },
            {17, "Merryweather Security" },
            {18, "Sheriff" },
        };
        public static List<MemberData> AllMembers = new List<MemberData>();

        public static void fractionChat(Player sender, string message)
        {
            try
            {
                if (sender == null || !Main.Players.ContainsKey(sender)) return;
                if (Main.Players[sender].FractionID == 0) return;

                if (Main.Players[sender].Unmute > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[sender].Unmute / 60} минут", 3000);
                    return;
                }

                int Fraction = Main.Players[sender].FractionID;
                if (!Members.ContainsKey(sender)) return;
                string msgSender = $"~b~[Рация] {Members[sender].inFracName} " + sender.Name.ToString().Replace('_', ' ') + " (" + sender.Value + "): " + message;
                var fracid = Main.Players[sender].FractionID;
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        NAPI.Chat.SendChatMessageToPlayer(p, msgSender);
                }
            }
            catch (Exception e) { Log.Write($"FractionChat:\n {e}", nLog.Type.Error); }
        }

        public static Dictionary<int, int> GovIds = new Dictionary<int, int>
        {
            { 7, 14 },
            { 6, 8 },
            { 8, 11 },
            { 9, 14 },
            { 14, 15 },
            { 15, 16 },
            { 17, 15 },
            { 18, 14 }
        };
        public static Dictionary<int, string> GovTags = new Dictionary<int, string>
        {
            { 7, "LSPD" },
            { 6, "GOV" },
            { 8, "EMS" },
            { 9, "FIB" },
            { 14, "ARMY" },
            { 15, "NEWS" },
            { 17, "MERRYWEATHER" },
            { 18, "SHERIFF" }
        };
        public static void govFractionChat(Player sender, string message)
        {
            if (!GovIds.ContainsKey(Main.Players[sender].FractionID)) return;
            if (!canUseCommand(sender, "dep")) return;
            if (Main.Players[sender].Unmute > 0)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[sender].Unmute / 60} минут", 3000);
                return;
            }
            int Fraction = Main.Players[sender].FractionID;

            var color = "!{#B8962E}";
            string msgSender = $"{color}[{GovTags[Fraction]}] {Members[sender].inFracName} " + sender.Name.ToString().Replace('_', ' ') + " (" + sender.Value + "): " + message;
            _ = Main.Players[sender].FractionID;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null) continue;
                if (!Main.Players.ContainsKey(p)) continue;
                if (GovIds.ContainsKey(Main.Players[p].FractionID))
                    NAPI.Chat.SendChatMessageToPlayer(p, msgSender);
            }
        }

        public static void Load(Player player, int fractionID, int fractionLVL)
        {
            if (Members.ContainsKey(player)) Members.Remove(player);
            MemberData data = new MemberData
            {
                FractionID = fractionID,
                FractionLVL = fractionLVL,
                inFracName = getNickname(fractionID, fractionLVL),
                Name = player.Name.ToString()
            };
            Members.Add(player, data);

            if (fractionID == 14 && fractionLVL < 6)
                Main.Players[player].OnDuty = true;

            if (Main.Players[player].OnDuty)
            {
                setSkin(player, fractionID, fractionLVL);
                player.SetData("ON_DUTY", true);
            }

            var index = AllMembers.FindIndex(d => d.Name == player.Name);
            if (index == -1) AllMembers.Add(data);
            else
            {
                AllMembers[index].FractionID = data.FractionID;
                AllMembers[index].FractionLVL = data.FractionLVL;
                AllMembers[index].inFracName = data.inFracName;
            }
            Trigger.ClientEvent(player, "fractionChange", fractionID);
            player.SetSharedData("fraction", fractionID);
            Log.Write($"Member {player.Name} loaded. ", nLog.Type.Success);
        }
        public static void UNLoad(Player player)
        {
            try
            {
                if (!Members.ContainsKey(player)) return;
                Members.Remove(player);
                Trigger.ClientEvent(player, "fractionChange", 0);
                player.SetSharedData("fraction", 0);
                Trigger.ClientEvent(player, "closePc");
                player.SetData("ON_DUTY", false);
                MenuManager.Close(player);
                Trigger.ClientEvent(player, "deleteFracBlips");

                if (Main.Players[player].FractionID == 9)
                {
                    var data = (Main.Players[player].Gender) ? "128_0_true" : "98_0_false";
                    var item = nInventory.Items[Main.Players[player].UUID].FirstOrDefault(i => i.Type == ItemType.Jewelry && i.Data == data);
                    if (item != null)
                    {
                        if (item.IsActive)
                        {
                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(0, 0);
                            player.SetClothes(7, 0, 0);
                        }
                    }
                }
                Log.Write($"Member {player.Name} unloaded.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        public static void Spawn(Player player)
        {
            if (!Members.ContainsKey(player)) return;
            Vector3 spawnPos = FractionSpawns[Members[player].FractionID];
            NAPI.Entity.SetEntityPosition(player, spawnPos);
        }
        public static int countOfFractionMembers(int fracID)
        {
            int count = 0;
            foreach (var p in Members.Values)
            {
                if (p.FractionID == fracID) count++;
            }
            return count;
        }
        public static bool isHaveFraction(Player player)
        {
            if (Main.Players[player].FractionID != 0)
                return true;
            return false;
        }
        public static bool inFraction(Player player, int FracID)
        {
            if (Main.Players[player].FractionID == FracID)
                return true;
            return false;
        }
        public static bool isLeader(Player player, int FracID)
        {
            if (Main.Players[player].FractionID == FracID && Main.Players[player].FractionLVL == Configs.FractionRanks[FracID].Count)
                return true;
            return false;
        }
        public static string getName(int FractionID)
        {
            if (!FractionNames.ContainsKey(FractionID))
                return null;
            return FractionNames[FractionID];
        }

        public static void sendFractionMessage(int fracid, string message, bool inChat = false)
        {
            var all_players = Main.Players.Keys.ToList();
            if (inChat)
            {
                foreach (var p in all_players)
                {
                    if (p == null) continue;
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        Notify.Send(p, NotifyType.Warning, NotifyPosition.TopCenter, message, 3000);
                }
            }
            else
            {
                foreach (var p in all_players)
                {
                    if (p == null) continue;
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        Notify.Send(p, NotifyType.Warning, NotifyPosition.TopCenter, message, 3000);
                }
            }
        }

        public static void sendFractionPictureNotification(int fracid, string sender, string submessage, string message, string pic)
        {
            var all_players = NAPI.Pools.GetAllPlayers();
            foreach (var p in all_players)
            {
                if (p == null) continue;
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].FractionID == fracid)
                    p.SendChatMessage(message);
            }
        }

        public static bool canUseCommand(Player player, string command, bool notify = true)
        {
            if (player == null || !NAPI.Entity.DoesEntityExist(player)) return false;
            int fracid = Main.Players[player].FractionID;
            int fraclvl = Main.Players[player].FractionLVL;
            int minrank = 100;
            if (Configs.FractionCommands.ContainsKey(fracid) && Configs.FractionCommands[fracid].ContainsKey(command))
                minrank = Configs.FractionCommands[fracid][command];

            #region Logic
            if (fraclvl < minrank)
            {
                if (notify)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Нет доступа", 3000);
                return false;
            }
            else return true;
            #endregion Logic
        }
        public static bool canGetWeapon(Player player, string weapon, bool notify = true)
        {
            // Get player FractionID
            int fracid = Main.Players[player].FractionID;
            int fraclvl = Main.Players[player].FractionLVL;
            int minrank = 100;
            // Fractions available commands //

            if (Configs.FractionWeapons.ContainsKey(fracid) && Configs.FractionWeapons[fracid].ContainsKey(weapon))
                minrank = Configs.FractionWeapons[fracid][weapon];

            #region Logic
            if (fraclvl < minrank)
            {
                if (notify)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Нет доступа", 3000);
                return false;
            }
            else return true;
            #endregion Logic
        }
        public static void setSkin(Player player, int fracID, int fracLvl)
        { // Only magic 
            bool gender = Main.Players[player].Gender;

            var clothes = (gender) ? Configs.FractionRanks[fracID][fracLvl].Item2 : Configs.FractionRanks[fracID][fracLvl].Item3;
            if (clothes == "null") return;

            Customization.ApplyCharacter(player);
            Customization.ClearClothes(player, gender);

            if (gender)
            {
                switch (clothes)
                {
                    case "sheriff_1":
                        player.SetClothes(11, 26, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][26], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        break;
                    case "sheriff_2":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(10, 8, 1);
                        break;
                    case "sheriff_3":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(10, 8, 2);
                        break;
                    case "sheriff_4":
                        player.SetClothes(11, 53, 0);
                        player.SetClothes(3, 4, 0);
                        player.SetClothes(4, 33, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 122, 0);
                        player.SetClothes(9, 7, 1);
                        player.SetClothes(7, 125, 0);
                        Customization.SetHat(player, 58, 2);
                        break;
                    case "sheriff_5":
                        player.SetClothes(11, 13, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(9, 28, 9);
                        Customization.SetHat(player, 46, 0);
                        break;
                    case "sheriff_6":
                        player.SetClothes(11, 13, 2);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(9, 28, 9);
                        break;
                    case "sheriff_7":
                        player.SetClothes(11, 13, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(9, 28, 9);
                        break;
                    case "sheriff_8":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(9, 28, 9);
                        break;
                    case "city_1":
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(11, 242, 2);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(8, 129, 0);
                        player.SetClothes(6, 54, 0);
                        player.SetClothes(3, 0, 0);
                        break;
                    case "city_2":
                        player.SetClothes(8, 7, 0);
                        player.SetClothes(11, 120, 11);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(3, 11, 0);
                        break;
                    case "city_3":
                        player.SetClothes(8, 71, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(11, 28, 2);
                        player.SetClothes(3, 1, 0);
                        break;
                    case "city_4":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 54, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, Customization.CorrectTorso[true][13], 0);
                        break;
                    case "city_5":
                        player.SetClothes(11, 4, 0);
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(4, 10, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetClothes(1, 121, 0);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_6":
                        player.SetClothes(11, 142, 0);
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(4, 10, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetClothes(1, 121, 0);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_7":
                        player.SetClothes(8, 31, 4);
                        player.SetClothes(7, 28, 4);
                        player.SetClothes(11, 32, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_8":
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(7, 28, 12);
                        player.SetClothes(11, 32, 1);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_9":
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(7, 28, 15);
                        player.SetClothes(11, 32, 2);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "police_1":
                        player.SetClothes(11, 26, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][26], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        break;
                    case "police_2":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(10, 8, 1);
                        break;
                    case "police_3":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(10, 8, 2);
                        break;
                    case "police_4":
                        player.SetClothes(11, 53, 0);
                        player.SetClothes(3, 4, 0);
                        player.SetClothes(4, 33, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 122, 0);
                        player.SetClothes(7, 125, 0);
                        Customization.SetHat(player, 58, 2);
                        break;
                    case "police_5":
                        player.SetClothes(11, 13, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        Customization.SetHat(player, 46, 0);
                        break;
                    case "police_6":
                        player.SetClothes(11, 13, 2);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        break;
                    case "police_7":
                        player.SetClothes(11, 13, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        break;
                    case "police_8":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        break;
                    case "ems_1":
                        player.SetClothes(11, 250, 1);
                        player.SetClothes(3, 85, 1);
                        player.SetClothes(4, 96, 1);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 127, 0);
                        player.SetClothes(10, 58, 0);
                        break;
                    case "ems_2":
                        player.SetClothes(11, 250, 0);
                        player.SetClothes(3, 85, 1);
                        player.SetClothes(4, 96, 0);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 127, 0);
                        player.SetClothes(10, 58, 0);
                        break;
                    case "ems_3":
                        player.SetClothes(11, 249, 0);
                        player.SetClothes(3, 86, 1);
                        player.SetClothes(4, 96, 0);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 126, 0);
                        player.SetClothes(10, 57, 0);
                        break;
                    case "ems_4":
                        player.SetClothes(11, 249, 1);
                        player.SetClothes(3, 86, 1);
                        player.SetClothes(4, 96, 1);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 126, 0);
                        player.SetClothes(10, 57, 0);
                        break;
                    case "army_1":
                        player.SetClothes(11, 208, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][208], 0);
                        player.SetClothes(4, 88, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_2":
                        player.SetClothes(11, 220, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 86, 3);
                        player.SetClothes(6, 63, 6);
                        break;
                    case "army_3":
                        Customization.SetHat(player, 103, 3);
                        player.SetClothes(11, 220, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 87, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_4":
                        Customization.SetHat(player, 103, 3);
                        player.SetClothes(11, 222, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 3);
                        player.SetClothes(6, 63, 6);
                        break;
                    case "army_5":
                        Customization.SetHat(player, 107, 3);
                        player.SetClothes(11, 222, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 87, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_6":
                        Customization.SetHat(player, 105, 3);
                        player.SetClothes(11, 221, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 86, 3);
                        player.SetClothes(6, 63, 6);
                        break;
                    case "army_7":
                        Customization.SetHat(player, 107, 3);
                        player.SetClothes(11, 221, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 87, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_8":
                        Customization.SetHat(player, 112, 11);
                        player.SetClothes(11, 219, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][219], 0);
                        player.SetClothes(4, 87, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_9":
                        Customization.SetHat(player, 106, 3);
                        player.SetClothes(11, 222, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 3);
                        player.SetClothes(6, 63, 6);
                        break;
                    case "army_10":
                        Customization.SetHat(player, 106, 14);
                        player.SetClothes(11, 232, 7);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        player.SetClothes(8, 121, 14);
                        player.SetClothes(4, 87, 14);
                        player.SetClothes(6, 62, 0);
                        break;
                    case "army_11":
                        Customization.SetHat(player, 106, 9);
                        player.SetClothes(11, 228, 15);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][228], 0);
                        player.SetClothes(4, 87, 9);
                        player.SetClothes(6, 62, 6);
                        break;
                    case "army_12":
                        Customization.SetHat(player, 108, 14);
                        player.SetClothes(11, 222, 14);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 14);
                        player.SetClothes(6, 63, 0);
                        break;
                    case "army_13":
                        Customization.SetHat(player, 114, 15);
                        player.SetClothes(11, 221, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 87, 5);
                        player.SetClothes(6, 62, 0);
                        break;
                    case "army_14":
                        Customization.SetHat(player, 114, 15);
                        player.SetClothes(11, 220, 4);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 87, 4);
                        player.SetClothes(6, 62, 0);
                        break;
                    case "army_15":
                        Customization.SetHat(player, 113, 5);
                        player.SetClothes(11, 222, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 87, 3);
                        player.SetClothes(6, 62, 6);
                        break;
                }
            }
            else
            {
                switch (clothes)
                {
                    case "sheriff_1":
                        player.SetClothes(11, 27, 1);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        break;
                    case "sheriff_2":
                        player.SetClothes(11, 48, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 1);
                        break;
                    case "sheriff_3":
                        player.SetClothes(11, 48, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 2);
                        break;
                    case "sheriff_4":
                        player.SetClothes(11, 46, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][46], 0);
                        player.SetClothes(4, 32, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(9, 9, 1);
                        player.SetClothes(7, 95, 0);
                        Customization.SetHat(player, 58, 2);
                        break;
                    case "sheriff_5":
                        player.SetClothes(11, 27, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(9, 27, 9);
                        Customization.SetHat(player, 45, 0);
                        break;
                    case "sheriff_6":
                        player.SetClothes(11, 27, 1);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(9, 27, 9);
                        break;
                    case "sheriff_7":
                        player.SetClothes(11, 27, 4);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(9, 27, 9);
                        break;
                    case "sheriff_8":
                        player.SetClothes(11, 27, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(9, 27, 9);
                        break;
                    case "city_1":
                        player.SetClothes(11, 250, 2);
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(8, 159, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(4, 64, 1);
                        player.SetClothes(3, 14, 0);
                        break;
                    case "city_2":
                        player.SetClothes(8, 24, 0);
                        player.SetClothes(11, 28, 10);
                        player.SetClothes(4, 36, 2);
                        player.SetClothes(6, 6, 0);
                        player.SetClothes(3, 0, 0);
                        break;
                    case "city_3":
                        player.SetClothes(11, 25, 7);
                        player.SetClothes(8, 67, 3);
                        player.SetClothes(4, 47, 0);
                        player.SetClothes(6, 6, 0);
                        player.SetClothes(3, 3, 0);
                        break;
                    case "city_4":
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(11, 27, 0);
                        player.SetClothes(4, 64, 1);
                        player.SetClothes(6, 29, 0);
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(3, Customization.CorrectTorso[false][27], 0);
                        break;
                    case "city_5":
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(11, 7, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 21, 2);
                        player.SetClothes(1, 121, 0);
                        player.SetClothes(4, 6, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(3, 3, 0);
                        break;
                    case "city_6":
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(11, 139, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 21, 2);
                        player.SetClothes(1, 121, 0);
                        player.SetClothes(4, 6, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][139], 0);
                        break;
                    case "city_7":
                        player.SetClothes(11, 6, 0);
                        player.SetClothes(4, 6, 0);
                        player.SetClothes(6, 42, 0);
                        player.SetClothes(8, 20, 1);
                        player.SetClothes(7, 12, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "city_8":
                        player.SetClothes(8, 41, 2);
                        player.SetClothes(11, 6, 2);
                        player.SetClothes(4, 6, 2);
                        player.SetClothes(6, 42, 2);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "city_9":
                        player.SetClothes(4, 50, 0);
                        player.SetClothes(11, 7, 1);
                        player.SetClothes(6, 0, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 22, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "police_1":
                        player.SetClothes(11, 27, 1);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        break;
                    case "police_2":
                        player.SetClothes(11, 48, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 1);
                        break;
                    case "police_3":
                        player.SetClothes(11, 48, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 2);
                        break;
                    case "police_4":
                        player.SetClothes(11, 46, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][46], 0);
                        player.SetClothes(4, 32, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(7, 95, 0);
                        Customization.SetHat(player, 58, 2);
                        break;
                    case "police_5":
                        player.SetClothes(11, 27, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        Customization.SetHat(player, 45, 0);
                        break;
                    case "police_6":
                        player.SetClothes(11, 27, 1);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "police_7":
                        player.SetClothes(11, 27, 4);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "police_8":
                        player.SetClothes(11, 27, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "ems_1":
                        player.SetClothes(11, 73, 0);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 0);
                        player.SetClothes(7, 97, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][73], 0);
                        break;
                    case "ems_2":
                        player.SetClothes(11, 249, 1);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 1);
                        player.SetClothes(7, 96, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][249], 0);
                        break;
                    case "ems_3":
                        player.SetClothes(11, 249, 2);
                        player.SetClothes(4, 23, 0);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 0);
                        player.SetClothes(7, 96, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][249], 0);
                        break;
                    case "ems_4":
                        player.SetClothes(11, 244, 4);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 93, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][244], 0);
                        break;
                    case "army_1":
                        player.SetClothes(11, 212, 3);
                        player.SetClothes(4, 91, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][212], 0);
                        break;
                    case "army_2":
                        player.SetClothes(11, 230, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_3":
                        Customization.SetHat(player, 102, 3);
                        player.SetClothes(11, 230, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_4":
                        Customization.SetHat(player, 102, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_5":
                        Customization.SetHat(player, 106, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_6":
                        Customization.SetHat(player, 104, 3);
                        player.SetClothes(11, 231, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_7":
                        Customization.SetHat(player, 106, 3);
                        player.SetClothes(11, 231, 0);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_8":
                        Customization.SetHat(player, 111, 11);
                        player.SetClothes(11, 226, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, 4, 0);
                        break;
                    case "army_9":
                        Customization.SetHat(player, 105, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_10":
                        Customization.SetHat(player, 105, 14);
                        player.SetClothes(11, 243, 7);
                        player.SetClothes(8, 141, 14);
                        player.SetClothes(4, 90, 14);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][243], 0);
                        break;
                    case "army_11":
                        Customization.SetHat(player, 105, 9);
                        player.SetClothes(11, 238, 15);
                        player.SetClothes(4, 90, 9);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][238], 0);
                        break;
                    case "army_12":
                        Customization.SetHat(player, 107, 14);
                        player.SetClothes(11, 232, 14);
                        player.SetClothes(4, 89, 14);
                        player.SetClothes(6, 66, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_13":
                        Customization.SetHat(player, 113, 15);
                        player.SetClothes(11, 231, 5);
                        player.SetClothes(4, 90, 5);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_14":
                        Customization.SetHat(player, 113, 15);
                        player.SetClothes(11, 230, 4);
                        player.SetClothes(4, 90, 4);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_15":
                        Customization.SetHat(player, 112, 5);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                }
            }

            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
        }
        public static string getNickname(int fracID, int fracLvl)
        { // Only magic
            if (Configs.FractionRanks.ContainsKey(fracID) && Configs.FractionRanks[fracID].ContainsKey(fracLvl))
                return Configs.FractionRanks[fracID][fracLvl].Item1;
            return null;
        }

        public static Dictionary<Weapons.Hash, int> WeaponsMaxAmmo = new Dictionary<Weapons.Hash, int>()
        {
            { Weapons.Hash.Nightstick, 1 },
            { Weapons.Hash.Pistol, 12 },
            { Weapons.Hash.SMG, 30 },
            { Weapons.Hash.PumpShotgun, 8 },
            { Weapons.Hash.StunGun, 100 },
            { Weapons.Hash.Pistol50, 9 },
            { Weapons.Hash.CarbineRifle, 30 },
            { Weapons.Hash.SmokeGrenade, 1 },
            { Weapons.Hash.HeavyShotgun, 6 },
            { Weapons.Hash.Knife, 1 },
            { Weapons.Hash.SniperRifle, 10 },
            { Weapons.Hash.AssaultSMG, 30 },
            { Weapons.Hash.Gusenberg, 50 },
            { Weapons.Hash.CombatPistol, 10 },
            { Weapons.Hash.Revolver, 6 },
            { Weapons.Hash.HeavyPistol, 10 },
            { Weapons.Hash.SawnOffShotgun, 6 },
            { Weapons.Hash.BullpupShotgun, 10 },
            { Weapons.Hash.DoubleBarrelShotgun, 2 },
            { Weapons.Hash.MicroSMG, 15 },
            { Weapons.Hash.MachinePistol, 13 },
            { Weapons.Hash.CombatPDW, 30 },
            { Weapons.Hash.MiniSMG, 13 },
            { Weapons.Hash.SpecialCarbine, 30 },
            { Weapons.Hash.AssaultRifle, 30 },
            { Weapons.Hash.BullpupRifle, 30 },
            { Weapons.Hash.AdvancedRifle, 30 },
            { Weapons.Hash.CompactRifle, 30 },
            { Weapons.Hash.CombatMG, 100 },
        };

        public static void giveGun(Player player, Weapons.Hash gun, string weaponstr)
        {
            if (!Main.Players.ContainsKey(player) || !Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (player.HasData($"GET_{gun}") && DateTime.Now < player.GetData<DateTime>($"GET_{gun}"))
            {
                DateTime date = player.GetData<DateTime>($"GET_{gun}");
                DateTime g = new DateTime((date - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы сможете взять {gun} через {min}:{sec}", 3000);
                return;
            }

            var frac = Main.Players[player].FractionID;
            if (Configs.FractionWeapons[frac].ContainsKey(weaponstr) && Main.Players[player].FractionLVL < Configs.FractionWeapons[frac][weaponstr])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не имеете доступа к данному виду оружия", 3000);
                return;
            }
            if (Stocks.fracStocks[Main.Players[player].FractionID].Materials < matsForGun[gun])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе недостаточно материала", 3000);
                return;
            }

            var wType = (ItemType)Enum.Parse(typeof(ItemType), gun.ToString());
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }

            var serial = Weapons.GetSerial(true, Main.Players[player].FractionID);
            Weapons.GiveWeapon(player, wType, serial);

            Stocks.fracStocks[Main.Players[player].FractionID].Materials -= matsForGun[gun];
            Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();

            var minutes = 5;
            if (Main.Players[player].FractionID == 7) minutes = 10;
            if (Main.Players[player].FractionID == 18) minutes = 10;
            player.SetData($"GET_{gun}", DateTime.Now.AddMinutes(minutes));

            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{gun}({serial})", 1, false);
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы получили {wType}", 3000);
            return;
        }

        public static void giveAmmo(Player player, ItemType ammoType, int ammo)
        {
            if (!Main.Players.ContainsKey(player) || !Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (Stocks.fracStocks[Main.Players[player].FractionID].Materials < MatsForAmmoType[ammoType] * ammo)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе недостаточно материала", 3000);
                return;
            }

            var tryAdd = nInventory.TryAdd(player, new nItem(ammoType, ammo));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].Materials -= MatsForAmmoType[ammoType] * ammo;
            Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();

            nInventory.Add(player, new nItem(ammoType, ammo));
            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, ammoType.ToString(), 1, false);
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы получили {nInventory.ItemsNames[(int)ammoType]} x{ammo}", 3000);
            return;
        }

        public class MemberData
        {
            public string Name { get; set; }
            public int FractionID { get; set; }
            public int FractionLVL { get; set; }
            public string inFracName { get; set; }
        }

        #region CraftMenu
        public static void OpenGunCraftMenu(Player player)
        {
            int fracid = Main.Players[player].FractionID;
            List<List<string>> list = null;

            if (FractionTypes[fracid] == -1 || FractionTypes[fracid] == 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не умеете крафтить оружие", 3000);
                return;
            }
            else if (FractionTypes[fracid] == 1) list = gangGuns;
            else if (FractionTypes[fracid] == 0) list = mafiaGuns;

            List<List<int>> prices = new List<List<int>>();
            for (int i = 0; i < list.Count; i++)
            {
                List<int> p = new List<int>();
                foreach (string g in list[i])
                {
                    p.Add(matsForGun[Weapons.GetHash(g)]);
                }
                prices.Add(p);
            }
            string json = JsonConvert.SerializeObject(prices);
            Log.Debug(json);
            Trigger.ClientEvent(player, "openWCraft", fracid, json);
        }
        [RemoteEvent("wcraft")]
        public static void Event_WCraft(Player client, int frac, int cat, int index)
        {
            int where = -1;
            try
            {
                Log.Debug($"{frac}:{cat}:{index}");
                List<List<string>> list = null;
                if (FractionTypes[frac] == 1) list = gangGuns;
                else if (FractionTypes[frac] == 0) list = mafiaGuns;
                if (list.Count < 1 || list.Count < cat + 1 || list[cat].Count < index + 1) return;

                string selected = list[cat][index];
                if (FractionTypes[frac] == -1 || FractionTypes[frac] == 2)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Вы не умеете крафтить оружие", 3000);
                    return;
                }
                var mItem = nInventory.Find(Main.Players[client].UUID, ItemType.Material);
                var count = (mItem == null) ? 0 : mItem.Count;
                if (count < matsForGun[Weapons.GetHash(selected)])
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "У Вас недостаточно материала", 3000);
                    return;
                }

                var wType = (ItemType)Enum.Parse(typeof(ItemType), selected);
                var tryAdd = nInventory.TryAdd(client, new nItem(wType, 1));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }

                nInventory.Remove(client, ItemType.Material, matsForGun[Weapons.GetHash(selected)]);
                if (selected == "BodyArmor")
                    nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, $"50"));
                else
                    Weapons.GiveWeapon(client, wType, Weapons.GetSerial(true, frac));
                Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы скрафтили {selected} за {matsForGun[Weapons.GetHash(selected)]} матов", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"Event_WCraft/{where}/{frac}/{cat}/{index}/: \n{e}", nLog.Type.Error);
            }
        }
        [RemoteEvent("wcraftammo")]
        public static void Event_WCraftAmmo(Player player, int frac, string text1, string text2)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (FractionTypes[frac] != 0 && FractionTypes[frac] != 1) return;

                var category = Convert.ToInt32(text1.Replace("wcraftslider", null));
                var mats = Convert.ToInt32(text2.Trim('M'));
                var ammo = mats / MatsForAmmo[category];

                if (ammo == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не указали количество патрон", 3000);
                    return;
                }

                var matsItem = nInventory.Find(Main.Players[player].UUID, ItemType.Material);
                var matsCount = (matsItem == null) ? 0 : matsItem.Count;
                if (matsCount < MatsForAmmo[category] * ammo)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно материалов", 3000);
                    return;
                }

                var tryAdd = nInventory.TryAdd(player, new nItem(AmmoTypes[category], ammo));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                nInventory.Remove(player, ItemType.Material, MatsForAmmo[category] * ammo);
                nInventory.Add(player, new nItem(AmmoTypes[category], ammo));
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно скрафтили {nInventory.ItemsNames[(int)AmmoTypes[category]]} x{ammo}", 3000);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        private static readonly Dictionary<ItemType, int> MatsForAmmoType = new Dictionary<ItemType, int>()
        {
            { ItemType.PistolAmmo, 1 },
            { ItemType.ShotgunsAmmo, 4 },
            { ItemType.SMGAmmo, 2 },
            { ItemType.RiflesAmmo, 4 },
            { ItemType.SniperAmmo, 10 },
        };
        private static readonly List<int> MatsForAmmo = new List<int>()
        {
            1, // pistol
            4, // shotgun
            2, // smg
            4, // rifles
        };
        private static readonly List<ItemType> AmmoTypes = new List<ItemType>()
        {
            ItemType.PistolAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.SMGAmmo,
            ItemType.RiflesAmmo,
        };
        #endregion

        [RemoteEvent("setmembers")]
        public static void SetMembersToMenu(Player player)
        {
            try
            {
                Character acc = Main.Players[player];
                if (acc.FractionID == 0) return;
                List<List<object>> people = new List<List<object>>();

                var count = 0;
                var on = 0;
                var off = 0;

                for (int i = 0; i < AllMembers.Count; i++)
                {
                    if (i >= AllMembers.Count) break;
                    var member = AllMembers[i];
                    if (member.FractionID != acc.FractionID) continue;
                    count++;
                    bool online = false;
                    string id = "-";
                    if (Members.Values.FirstOrDefault(m => m.Name == member.Name) != null)
                    {
                        id = NAPI.Player.GetPlayerFromName(member.Name).Value.ToString();
                        online = true;
                        on++;
                    }
                    else
                        off++;
                    List<object> data = new List<object>
                    {
                        online,
                        id,
                        member.Name,
                        "-",
                        member.FractionLVL,
                    };
                    people.Add(data);
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(people);
                Trigger.ClientEvent(player, "setmem", json, count, on, off);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"SETMEMBERS\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("openfmenu")]
        public static void OpenFractionMenu(Player player)
        {
            try
            {
                SetMembersToMenu(player);
                Trigger.ClientEvent(player, "openfm");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"OPENFMENU\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("fmenu")]
        public static void callback_FracMenu(Player player, params object[] args)
        {
            try
            {
                int act = Convert.ToInt32(args[0]);
                string data1 = Convert.ToString(args[1]);
                string data2 = Convert.ToString(args[2]);
                int rank;
                int id;
                switch (act)
                {
                    case 2: //invite
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.InviteToFraction(player, target);
                        }
                        else
                        {
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким именем не найден", 3000);
                                return;
                            }
                            FractionCommands.InviteToFraction(player, target);
                        }
                        break;
                    case 3: //kick
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.UnInviteFromFraction(player, target);
                        }
                        else
                        {
                            if (!Main.PlayerNames.ContainsValue(data1))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким именем не найден", 3000);
                                return;
                            }
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                if (!Manager.canUseCommand(player, "uninvite")) return;

                                int targetLvl = 0;
                                int targetFrac = 0;

                                var split = data1.Split('_');
                                var result = Connect.QueryRead($"SELECT fraction,fractionlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                if (result == null || result.Rows.Count == 0) return;
                                foreach (DataRow Row in result.Rows)
                                {
                                    targetFrac = Convert.ToInt32(Row["fraction"].ToString());
                                    targetLvl = Convert.ToInt32(Row["fractionlvl"].ToString());
                                }

                                if (targetFrac != Main.Players[player].FractionID)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок не состоит в Вашей фракции", 3000);
                                    return;
                                }
                                if (targetLvl >= Main.Players[player].FractionLVL)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете уволить этого игрока", 3000);
                                    return;
                                }
                                Connect.Query($"UPDATE `characters` SET fraction=0,fractionlvl=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы уволили игрока {data1} из Вашей фракции", 3000);

                                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == data1);
                                if (index > -1) Manager.AllMembers.RemoveAt(index);
                                return;
                            }
                            else
                                FractionCommands.UnInviteFromFraction(player, target);
                        }
                        break;
                    case 4: //change
                        if (!Int32.TryParse(data2, out rank))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.SetFracRank(player, target, rank);
                        }
                        else
                        {
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким именем не найден", 3000);
                                return;
                            }
                            FractionCommands.SetFracRank(player, target, rank);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"FRACMENU\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
    }
}
