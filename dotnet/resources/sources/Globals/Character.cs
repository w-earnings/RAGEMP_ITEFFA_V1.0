﻿using GTANetworkAPI;
using iTeffa.Houses;
using iTeffa.Infodata;
using iTeffa.Interface;
using iTeffa.Models;
using iTeffa.Settings;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace iTeffa.Globals.Character
{
    public class Character : CharacterData
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Character");
        private static readonly Random Rnd = new Random();

        public void Spawn(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        player.SetSharedData("IS_MASK", false);

                        // Logged in state, money, phone init
                        Plugins.Trigger.ClientEvent(player, "loggedIn");
                        player.SetData("LOGGED_IN", true);

                        Plugins.Trigger.ClientEvent(player, "UpdateMoney", Money);
                        Plugins.Trigger.ClientEvent(player, "UpdateEat", Main.Players[player].Eat);
                        Plugins.Trigger.ClientEvent(player, "UpdateWater", Main.Players[player].Water);
                        Plugins.Trigger.ClientEvent(player, "UpdateBank", Finance.Bank.Accounts[Bank].Balance);
                        Plugins.Trigger.ClientEvent(player, "initPhone");
                        Working.WorkManager.load(player);
                        if (IsBonused)
                        {
                            Plugins.Trigger.ClientEvent(player, "updlastbonus", $"~w~Бонус получен!");
                        }
                        else
                        {
                            DateTime date = new DateTime((new DateTime().AddMinutes(Main.oldconfig.LastBonusMin - LastBonus)).Ticks);
                            var hour = date.Hour;
                            var min = date.Minute;
                            Plugins.Trigger.ClientEvent(player, "updlastbonus", $"Eжедневный подарок: Через {hour}ч. {min}м.");
                        }
                        player.SetSkin((Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                        player.Health = (Health > 5) ? Health : 5;
                        player.Armor = Armor;
                        player.SetSharedData("REMOTE_ID", player.Value);
                        player.SetSharedData("PERSON_SID", PersonSID);
                        Modules.Voice.PlayerJoin(player);
                        player.SetSharedData("voipmode", -1);

                        if (Fractions.Manager.FractionTypes[FractionID] == 1 || AdminLVL > 0) Fractions.GangsCapture.LoadBlips(player);
                        if (WantedLVL != null) Plugins.Trigger.ClientEvent(player, "setWanted", WantedLVL.Level);

                        player.SetData("RESIST_STAGE", 0);
                        player.SetData("RESIST_TIME", 0);
                        if (AdminLVL > 0) player.SetSharedData("IS_ADMIN", true);

                        Dashboard.sendStats(player);
                        Dashboard.sendItems(player);
                        if (Main.Players[player].LVL == 0)
                        {
                            NAPI.Task.Run(() => { try { Plugins.Trigger.ClientEvent(player, "disabledmg", true); } catch { } }, 5000);
                        }

                        House house = HouseManager.GetHouse(player);
                        if (house != null)
                        {


                            Plugins.Trigger.ClientEvent(player, "changeBlipColor", house.blip, 73);

                            Plugins.Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
                            Plugins.Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
                        }

                        if (!Customization.CustomPlayerData.ContainsKey(UUID) || !Customization.CustomPlayerData[UUID].IsCreated)
                        {
                            Plugins.Trigger.ClientEvent(player, "spawnShow", false);
                            Customization.CreateCharacter(player);
                        }
                        else
                        {
                            try
                            {
                                NAPI.Entity.SetEntityPosition(player, Main.Players[player].SpawnPos);
                                List<bool> prepData = new List<bool>
                                {
                                    true,
                                    (FractionID > 0),
                                    (house != null || HotelID != -1),
                                };
                                Plugins.Trigger.ClientEvent(player, "spawnShow", JsonConvert.SerializeObject(prepData));
                                Customization.ApplyCharacter(player);
                            }
                            catch { }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write($"EXCEPTION AT \"Spawn.NAPI.Task.Run\":\n" + e.ToString(), Plugins.Logs.Type.Error);
                    }
                });

                if (Warns > 0 && DateTime.Now > Unwarn)
                {
                    Warns--;

                    if (Warns > 0)
                        Unwarn = DateTime.Now.AddDays(14);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Одно предупреждение было снято. У Вас осталось {Warns}", 3000);
                }

                if (!Dashboard.isopen.ContainsKey(player))
                    Dashboard.isopen.Add(player, false);

                nInventory.Check(UUID);
                if (nInventory.Find(UUID, ItemType.BagWithMoney) != null)
                    nInventory.Remove(player, ItemType.BagWithMoney, 1);
                if (nInventory.Find(UUID, ItemType.BagWithDrill) != null)
                    nInventory.Remove(player, ItemType.BagWithDrill, 1);

                if (FractionID == 15)
                {
                    Plugins.Trigger.ClientEvent(player, "enableadvert", true);
                    Fractions.Realm.LSNews.onLSNPlayerLoad(player);
                }
                if (AdminLVL > 0)
                {
                    ReportSys.onAdminLoad(player);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Spawn\":\n" + e.ToString());
            }
        }

        public async Task Load(Player player, int uuid)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                    Main.Players.Remove(player);

                DataTable result = await Database.QueryReadAsync($"SELECT * FROM `characters` WHERE uuid={uuid}");
                if (result == null || result.Rows.Count == 0) return;

                NAPI.Task.Run(() =>
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        UUID = Convert.ToInt32(Row["uuid"]);
                        PersonSID = Convert.ToString(Row["personsid"]);
                        FirstName = Convert.ToString(Row["firstname"]);
                        LastName = Convert.ToString(Row["lastname"]);
                        Gender = Convert.ToBoolean(Row["gender"]);
                        Health = Convert.ToInt32(Row["health"]);
                        Armor = Convert.ToInt32(Row["armor"]);
                        LVL = Convert.ToInt32(Row["lvl"]);
                        EXP = Convert.ToInt32(Row["exp"]);
                        Eat = Convert.ToInt32(Row["eat"]);
                        Water = Convert.ToInt32(Row["water"]);
                        Money = Convert.ToInt64(Row["money"]);
                        Bank = Convert.ToInt32(Row["bank"]);
                        WorkID = Convert.ToInt32(Row["work"]);
                        FractionID = Convert.ToInt32(Row["fraction"]);
                        FractionLVL = Convert.ToInt32(Row["fractionlvl"]);
                        ArrestTime = Convert.ToInt32(Row["arrest"]);
                        DemorganTime = Convert.ToInt32(Row["demorgan"]);
                        WantedLVL = JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                        BizIDs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                        AdminLVL = Convert.ToInt32(Row["adminlvl"]);
                        Licenses = JsonConvert.DeserializeObject<List<bool>>(Row["licenses"].ToString());
                        Unwarn = ((DateTime)Row["unwarn"]);
                        Unmute = Convert.ToInt32(Row["unmute"]);
                        Warns = Convert.ToInt32(Row["warns"]);
                        LastVeh = Convert.ToString(Row["lastveh"]);
                        OnDuty = Convert.ToBoolean(Row["onduty"]);
                        LastHourMin = Convert.ToInt32(Row["lasthour"]);
                        LastBonus = Convert.ToInt32(Row["lastbonus"]);
                        IsBonused = Convert.ToBoolean(Row["isbonused"]);
                        HotelID = Convert.ToInt32(Row["hotel"]);
                        HotelLeft = Convert.ToInt32(Row["hotelleft"]);
                        Contacts = JsonConvert.DeserializeObject<Dictionary<int, string>>(Row["contacts"].ToString());
                        Achievements = JsonConvert.DeserializeObject<List<bool>>(Row["achiev"].ToString());

                        if (PersonSID == null || PersonSID == "")
                        {
                            PersonSID = GeneratePersonSID(uuid, true);
                        }

                        if (Achievements == null)
                        {
                            Achievements = new List<bool>();
                            for (uint i = 0; i != 401; i++) Achievements.Add(false);
                        }
                        Sim = Convert.ToInt32(Row["sim"]);

                        CreateDate = ((DateTime)Row["createdate"]);

                        SpawnPos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                        if (Row["pos"].ToString().Contains("NaN"))
                        {
                            Log.Debug("Detected wrong coordinates!", Plugins.Logs.Type.Warn);
                            if (LVL <= 1) SpawnPos = new Vector3(-1036.3226, -2732.918, 13.766636);
                            else SpawnPos = new Vector3(-1036.3226, -2732.918, 13.766636);
                        }
                    }
                    player.Name = FirstName + "_" + LastName;
                    Main.Players.Add(player, this);
                    CheckAchievements(player);
                    Loggings.Connected(player.Name, UUID, player.GetData<string>("RealSocialClub"), player.GetData<string>("RealHWID"), player.Value, player.Address);
                    Spawn(player);
                });
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Load\":\n" + e.ToString());
            }
        }


        public static void CheckAchievements(Player player)
        {
            try
            {
                if (Main.Players[player].Achievements[1] && !Main.Players[player].Achievements[2]) player.SetData("CollectThings", 0);
                else if (Main.Players[player].Achievements[2] && !Main.Players[player].Achievements[4] && !Main.Players[player].Achievements[5]) Plugins.Trigger.ClientEvent(player, "createWaypoint", 1924.4f, 4922.0f);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CheckAchievements\":\n" + e.ToString());
            }
        }

        public async Task<bool> Save(Player player)
        {
            try
            {
                Customization.SaveCharacter(player);

                Vector3 LPos = (player.IsInVehicle) ? player.Vehicle.Position + new Vector3(0, 0, 0.5) : player.Position;
                string pos = JsonConvert.SerializeObject(LPos);
                try
                {
                    if (InsideHouseID != -1)
                    {
                        House house = HouseManager.Houses.FirstOrDefault(h => h.ID == InsideHouseID);
                        if (house != null)
                            pos = JsonConvert.SerializeObject(house.Position + new Vector3(0, 0, 1.12));
                    }
                    if (InsideGarageID != -1)
                    {
                        Garage garage = GarageManager.Garages[InsideGarageID];
                        pos = JsonConvert.SerializeObject(garage.Position + new Vector3(0, 0, 1.12));
                    }
                    if (ExteriorPos != new Vector3())
                    {
                        Vector3 position = ExteriorPos;
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                    if (InsideHotelID != -1)
                    {
                        Vector3 position = Houses.Hotel.HotelEnters[InsideHotelID];
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                    if (TuningShop != -1)
                    {
                        Vector3 position = BusinessManager.BizList[TuningShop].EnterPoint;
                        pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadPos\":\n" + e.ToString()); }

                try
                {
                    if (IsSpawned && !IsAlive)
                    {
                        pos = JsonConvert.SerializeObject(Fractions.Realm.Ems.emsCheckpoints[0]);
                        Health = 10;
                        Armor = 0;
                    }
                    else
                    {
                        Health = player.Health;
                        Armor = player.Armor;
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadHP\":\n" + e.ToString()); }

                try
                {
                    var aItem = nInventory.Find(UUID, ItemType.BodyArmor);
                    if (aItem != null && aItem.IsActive)
                        aItem.Data = $"{Armor}";
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadArmorItem\":\n" + e.ToString()); }

                try
                {
                    var all_vehicles = VehicleManager.getAllPlayerVehicles($"{FirstName}_{LastName}");
                    foreach (var number in all_vehicles)
                        VehicleManager.Save(number);
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadVehicles\":\n" + e.ToString()); }

                if (!IsSpawned)
                    pos = JsonConvert.SerializeObject(SpawnPos);

                Main.PlayerSlotsInfo[UUID] = new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money);

                await Database.QueryAsync($"UPDATE `characters` SET `pos`='{pos}',`gender`={Gender},`health`={Health},`armor`={Armor},`lvl`={LVL},`exp`={EXP}," +
                    $"`money`={Money},`bank`={Bank},`work`={WorkID},`fraction`={FractionID},`fractionlvl`={FractionLVL},`arrest`={ArrestTime}," +
                    $"`wanted`='{JsonConvert.SerializeObject(WantedLVL)}',`biz`='{JsonConvert.SerializeObject(BizIDs)}',`adminlvl`={AdminLVL}," +
                    $"`licenses`='{JsonConvert.SerializeObject(Licenses)}',`unwarn`='{Database.ConvertTime(Unwarn)}',`unmute`='{Unmute}'," +
                    $"`warns`={Warns},`hotel`={HotelID},`hotelleft`={HotelLeft},`lastveh`='{LastVeh}',`onduty`={OnDuty},`lasthour`={LastHourMin},`lastbonus`={LastBonus},`isbonused`={IsBonused}," +
                    $"`demorgan`={DemorganTime},`contacts`='{JsonConvert.SerializeObject(Contacts)}',`achiev`='{JsonConvert.SerializeObject(Achievements)}',`sim`={Sim},`personsid`='{PersonSID}',`eat`='{Eat}',`water`='{Water}' WHERE `uuid`={UUID}");

                Finance.Bank.Save(Bank);
                await Log.DebugAsync($"Player [{FirstName}:{LastName}] was saved.");
                return true;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Save\":\n" + e.ToString());
                return false;
            }
        }

        public async Task<int> Create(Player player, string firstName, string lastName)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                {
                    Log.Debug("Main.Players.ContainsKey(player)", Plugins.Logs.Type.Error);
                    return -1;
                }

                if (firstName.Length < 1 || lastName.Length < 1)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Ошибка в длине имени/фамилии", 3000);
                    return -1;
                }
                if (Main.PlayerNames.ContainsValue($"{firstName}_{lastName}"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данное имя уже занято", 3000);
                    return -1;
                }

                UUID = GenerateUUID();
                PersonSID = GeneratePersonSID();

                FirstName = firstName;
                LastName = lastName;

                Bank = Finance.Bank.Create($"{firstName}_{lastName}");

                Main.PlayerBankAccs.Add($"{firstName}_{lastName}", Bank);

                Licenses = new List<bool>() { false, false, false, false, false, false, false, false };

                Achievements = new List<bool>();

                for (uint i = 0; i != 401; i++) Achievements.Add(false);

                SpawnPos = new Vector3(-1036.3226, -2732.918, 12.766636);

                Main.PlayerSlotsInfo.Add(UUID, new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money));
                Main.PlayerUUIDs.Add($"{firstName}_{lastName}", UUID);
                Main.PlayerNames.Add(UUID, $"{firstName}_{lastName}");

                await Database.QueryAsync($"INSERT INTO `characters`(`uuid`,`personsid`,`firstname`,`lastname`,`gender`,`health`,`armor`,`lvl`,`exp`,`money`,`bank`,`work`,`fraction`,`fractionlvl`,`arrest`,`demorgan`,`wanted`," +
                    $"`biz`,`adminlvl`,`licenses`,`unwarn`,`unmute`,`warns`,`lastveh`,`onduty`,`lasthour`,`lastbonus`,`isbonused`,`hotel`,`hotelleft`,`contacts`,`achiev`,`sim`,`pos`,`createdate`,`eat`,`water`) " +
                    $"VALUES({UUID},'{PersonSID}','{FirstName}','{LastName}',{Gender},{Health},{Armor},{LVL},{EXP},{Money},{Bank},{WorkID},{FractionID},{FractionLVL},{ArrestTime},{DemorganTime}," +
                    $"'{JsonConvert.SerializeObject(WantedLVL)}','{JsonConvert.SerializeObject(BizIDs)}',{AdminLVL},'{JsonConvert.SerializeObject(Licenses)}','{Database.ConvertTime(Unwarn)}'," +
                    $"'{Unmute}',{Warns},'{LastVeh}',{OnDuty},{LastHourMin},{LastBonus},{IsBonused},{HotelID},{HotelLeft},'{JsonConvert.SerializeObject(Contacts)}','{JsonConvert.SerializeObject(Achievements)}',{Sim}," +
                    $"'{JsonConvert.SerializeObject(SpawnPos)}','{Database.ConvertTime(CreateDate)}','{Eat}','{Water}')");
                NAPI.Task.Run(() => { player.Name = FirstName + "_" + LastName; });
                nInventory.Check(UUID);
                Main.Players.Add(player, this);

                return UUID;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Create\":\n" + e.ToString());
                return -1;
            }
        }

        private int GenerateUUID()
        {
            var result = 100000;
            while (Main.UUIDs.Contains(result))
                result = Rnd.Next(000001, 999999);

            Main.UUIDs.Add(result);
            return result;
        }

        private string GeneratePersonSID(int uuid = -1, bool save = false)
        {
            string result = "";
            while (Main.PersonSIDs.Contains(result))
            {
                result += (char)Rnd.Next(0x0030, 0x0039);
                result += (char)Rnd.Next(0x0041, 0x005A);
                result += (char)Rnd.Next(0x0030, 0x0039);
                result += (char)Rnd.Next(0x0041, 0x005A);
            }
            Main.PersonSIDs.Add(result);
            if (save)
            {
                Database.Query($"UPDATE `characters` SET `personsid`='{result}' WHERE `uuid`={uuid}");
            }
            return result;
        }





        public static Dictionary<string, string> toChange = new Dictionary<string, string>();
        private static MySqlCommand nameCommand;

        public Character()
        {
            nameCommand = new MySqlCommand("UPDATE `characters` SET `firstname`=@fn, `lastname`=@ln WHERE `uuid`=@uuid");
        }

        public static async Task changeName(string oldName)
        {
            try
            {
                if (!toChange.ContainsKey(oldName)) return;

                string newName = toChange[oldName];
                int Uuid = Main.PlayerUUIDs.GetValueOrDefault(oldName);
                if (Uuid <= 0)
                {
                    await Log.WriteAsync($"Cant'find UUID of player [{oldName}]", Plugins.Logs.Type.Warn);
                    return;
                }

                string[] split = newName.Split("_");

                Main.PlayerNames[Uuid] = newName;
                Main.PlayerUUIDs.Remove(oldName);
                Main.PlayerUUIDs.Add(newName, Uuid);
                try
                {
                    if (Main.PlayerBankAccs.ContainsKey(oldName))
                    {
                        int bank = Main.PlayerBankAccs[oldName];
                        Main.PlayerBankAccs.Add(newName, bank);
                        Main.PlayerBankAccs.Remove(oldName);
                    }
                }
                catch { }

                MySqlCommand cmd = nameCommand;
                cmd.Parameters.AddWithValue("@fn", split[0]);
                cmd.Parameters.AddWithValue("@ln", split[1]);
                cmd.Parameters.AddWithValue("@uuid", Uuid);
                await Database.QueryAsync(cmd);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        VehicleManager.changeOwner(oldName, newName);
                        BusinessManager.changeOwner(oldName, newName);
                        Finance.Bank.changeHolder(oldName, newName);
                        Houses.HouseManager.ChangeOwner(oldName, newName);
                    }
                    catch { }
                });

                await Log.DebugAsync("Nickname has been changed!", Plugins.Logs.Type.Success);
                toChange.Remove(oldName);
                Finance.Donations.Rename(oldName, newName);
                Loggings.Name(Uuid, oldName, newName);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CHANGENAME\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
    }
}
