﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using GTANetworkAPI;
using iTeffa.Kernel;
using iTeffa.Settings;
using iTeffa.Kernel.nAccount;
using iTeffa.Kernel.Character;
using iTeffa.Interface;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Net.Mail;
using iTeffa.Speaking;
using iTeffa.Houses;

namespace iTeffa
{
    public class Main : Script
    { 
        public static string Codename { get; } = "iTeffa";
        public static string Version { get; } = "1.0.0";
        public static string Build { get; } = "Beta";

        public static string Full { get; } = $"{Codename} {Version} {Build}";
        public static DateTime StartDate { get; } = DateTime.Now;
        public static DateTime CompileDate { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

        public static oldConfig oldconfig;
        private static Config config = new Config("Main");
        private static byte servernum = config.TryGet<byte>("ServerNumber", "1");

        private static int Slots = NAPI.Server.GetMaxPlayers();
        public static Dictionary<string, Tuple<int, int, int>> PromoCodes = new Dictionary<string, Tuple<int, int, int>>();

        // Characters
        public static List<int> UUIDs = new List<int>(); // characters UUIDs
        public static Dictionary<int, string> PlayerNames = new Dictionary<int, string>(); // character uuid - character name
        public static Dictionary<string, int> PlayerBankAccs = new Dictionary<string, int>(); // character name - character bank
        public static Dictionary<string, int> PlayerUUIDs = new Dictionary<string, int>(); // character name - character uuid
        public static Dictionary<int, Tuple<int, int, int, long>> PlayerSlotsInfo = new Dictionary<int, Tuple<int, int, int, long>>(); // character uuid - lvl,exp,fraction,money

        public static Dictionary<string, Player> LoggedIn = new Dictionary<string, Player>();
        public static Dictionary<Player, Character> Players = new Dictionary<Player, Character>(); // character in

        public static Dictionary<int, int> SimCards = new Dictionary<int, int>();
        public static Dictionary<int, Player> MaskIds = new Dictionary<int, Player>();
        // Accounts
        public static List<string> Usernames = new List<string>(); // usernames
        public static List<string> SocialClubs = new List<string>(); // socialclubnames
        public static Dictionary<string, string> Emails = new Dictionary<string, string>(); // emails
        public static List<string> HWIDs = new List<string>(); // emails
        public static Dictionary<Player, Account> Accounts = new Dictionary<Player, Account>(); // client's accounts
        public static Dictionary<Player, Tuple<int, string, string, string>> RestorePass = new Dictionary<Player, Tuple<int, string, string, string>>(); // int code, string Login, string SocialClub, string Email
        public static char[] stringBlock = { '\'', '@', '[', ']', ':', '"', '[', ']', '{', '}', '|', '`', '%', '\\' };

        public static string BlockSymbols(string check)
        {
            for (int i = check.IndexOfAny(stringBlock); i >= 0;)
            {
                check = check.Replace(check[i], ' ');
                i = check.IndexOfAny(stringBlock);
            }
            return check;
        }

        public static Random rnd = new Random();

        public static List<string> LicWords = new List<string>()
        {
            "A",
            "B",
            "C",
            "V",
            "LV",
            "LS",
            "G",
            "MED"
        };

        private static nLog Log = new nLog("GM");
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.Server.SetAutoRespawnAfterDeath(false);
                NAPI.Task.Run(() =>
                {
                    NAPI.Server.SetGlobalServerChat(false);
                    NAPI.World.SetTime(DateTime.Now.Hour, 0, 0);
                });

                DataTable result = Connect.QueryRead("SELECT `uuid`,`firstname`,`lastname`,`sim`,`lvl`,`exp`,`fraction`,`money`,`bank`,`adminlvl` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            int uuid = Convert.ToInt32(Row["uuid"]);
                            string name = Convert.ToString(Row["firstname"]);
                            string lastname = Convert.ToString(Row["lastname"]);
                            int lvl = Convert.ToInt32(Row["lvl"]);
                            int exp = Convert.ToInt32(Row["exp"]);
                            int fraction = Convert.ToInt32(Row["fraction"]);
                            long money = Convert.ToInt64(Row["money"]);
                            int adminlvl = Convert.ToInt32(Row["adminlvl"]);
                            int bank = Convert.ToInt32(Row["bank"]);

                            UUIDs.Add(uuid);
                            if (Convert.ToInt32(Row["sim"]) != -1) SimCards.Add(Convert.ToInt32(Row["sim"]), uuid);
                            PlayerNames.Add(uuid, $"{name}_{lastname}");
                            PlayerUUIDs.Add($"{name}_{lastname}", uuid);
                            PlayerBankAccs.Add($"{name}_{lastname}", bank);
                            PlayerSlotsInfo.Add(uuid, new Tuple<int, int, int, long>(lvl, exp, fraction, money));

                            if (adminlvl > 0)
                            {
                                DataTable result2 = Connect.QueryRead($"SELECT `socialclub` FROM `accounts` WHERE `character1`={uuid} OR `character2`={uuid} OR `character3`={uuid}");
                                if (result2 == null || result2.Rows.Count == 0) continue;
                                string socialclub = Convert.ToString(result2.Rows[0]["socialclub"]);
                            }
                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
                    }
                }
                else Log.Write("DB `characters` return null result", nLog.Type.Warn);

                result = Connect.QueryRead("SELECT `login`,`socialclub`,`email`,`hwid` FROM `accounts`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            string login = Convert.ToString(Row["login"]);

                            Usernames.Add(login.ToLower());
                            if (SocialClubs.Contains(Convert.ToString(Row["socialclub"]))) Log.Write("ResourceStart: sc contains " + Convert.ToString(Row["socialclub"]), nLog.Type.Error);
                            else SocialClubs.Add(Convert.ToString(Row["socialclub"]));
                            Emails.Add(Convert.ToString(Row["email"]), login);
                            HWIDs.Add(Convert.ToString(Row["hwid"]));

                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
                    }
                }
                else Log.Write("DB `accounts` return null result", nLog.Type.Warn);

                result = Connect.QueryRead("SELECT `name`,`type`,`count`,`owner` FROM `promocodes`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                        PromoCodes.Add(Convert.ToString(Row["name"]), new Tuple<int, int, int>(Convert.ToInt32(Row["type"]), Convert.ToInt32(Row["count"]), Convert.ToInt32(Row["owner"])));
                }
                else Log.Write("DB `promocodes` return null result", nLog.Type.Warn);

                Ban.Sync();

                int time = 3600 - (DateTime.Now.Minute * 60) - DateTime.Now.Second;
                Timers.StartOnceTask("paydayFirst", time * 1000, () =>
                {

                    Timers.StartTask("payday", 3600000, () => payDayTrigger());
                    payDayTrigger();

                });
                Timers.StartTask("savedb", 180000, () => saveDatabase());
                Timers.StartTask("playedMins", 60000, () => playedMinutesTrigger());
                Timers.StartTask("envTimer", 1000, () => enviromentChangeTrigger());
                result = Connect.QueryRead($"SELECT * FROM `othervehicles`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        int type = Convert.ToInt32(Row["type"]);

                        string number = Row["number"].ToString();
                        VehicleHash model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                        Vector3 position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        int color1 = Convert.ToInt32(Row["color1"]);
                        int color2 = Convert.ToInt32(Row["color2"]);
                        int price = Convert.ToInt32(Row["price"]);
                        CarInfo data = new CarInfo(number, model, position, rotation, color1, color2, price);

                        switch (type)
                        {
                            case 0:
                                Rentcar.CarInfos.Add(data);
                                break;
                            case 3:
                                Working.Taxi.CarInfos.Add(data);
                                break;
                            case 4:
                                Working.Bus.CarInfos.Add(data);
                                break;
                            case 5:
                                Working.Lawnmower.CarInfos.Add(data);
                                break;
                            case 6:
                                Working.Truckers.CarInfos.Add(data);
                                break;
                            case 7:
                                Working.Collector.CarInfos.Add(data);
                                break;
                            case 8:
                                Working.AutoMechanic.CarInfos.Add(data);
                                break;
                            case 100:
                                DrivingSchool.CarInfos.Add(data);
                                break;
                        }
                    }

                    Kernel.Rentcar.rentCarsSpawner();
                    Working.Bus.busCarsSpawner();
                    Working.Lawnmower.mowerCarsSpawner();
                    Working.Taxi.taxiCarsSpawner();
                    Working.Truckers.truckerCarsSpawner();
                    Working.Collector.collectorCarsSpawner();
                    Working.AutoMechanic.mechanicCarsSpawner();
                    DrivingSchool.SchoolCarsSpawner();
                }
                else Log.Write("DB `othervehicles` return null result", nLog.Type.Warn);

                Fractions.Configs.LoadFractionConfigs();

                NAPI.World.SetWeather(config.TryGet<string>("Weather", "CLEAR")); // XMAS - Зима

                if (oldconfig.DonateChecker)
                    Finance.Donations.Start();

                // Assembly information //
                Log.Write(Full + " started at " + StartDate.ToString("s"), nLog.Type.Success);
                Log.Write($"Assembly compiled {CompileDate.ToString("s")}", nLog.Type.Success);

                Console.Title = "RageMP - " + oldconfig.ServerName;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.EntityCreated)]
        public void Event_entityCreated(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Vehicle) return;
                Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(entity);
                vehicle.SetData("BAGINUSE", false);

                string[] keys = NAPI.Data.GetAllEntityData(vehicle);
                foreach (string key in keys) vehicle.ResetData(key);

                if (VehicleManager.VehicleTank.ContainsKey(vehicle.Class))
                {
                    vehicle.SetSharedData("PETROL", VehicleManager.VehicleTank[vehicle.Class]);
                    vehicle.SetSharedData("MAXPETROL", VehicleManager.VehicleTank[vehicle.Class]);
                }
                vehicle.SetSharedData("hlcolor", 0);
                vehicle.SetSharedData("LOCKED", false);
                vehicle.SetData("ITEMS", new List<nItem>());
                vehicle.SetData("SPAWNPOS", vehicle.Position);
                vehicle.SetData("SPAWNROT", vehicle.Rotation);
            } catch (Exception e) { Log.Write("EntityCreated: " + e.Message, nLog.Type.Error); }
        }

        #region Player
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (type == DisconnectionType.Timeout)
                    Log.Write($"{player.Name} crashed", nLog.Type.Warn);
                Log.Debug($"DisconnectionType: {type.ToString()}");

                Log.Debug("DISCONNECT STARTED");

                if (Accounts.ContainsKey(player))
                {
                    if(LoggedIn.ContainsKey(Accounts[player].Login)) LoggedIn.Remove(Accounts[player].Login);
                }
                if (Players.ContainsKey(player))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    try
                    {
                        if (player.HasData("ON_DUTY"))
                            Players[player].OnDuty = player.GetData<bool>("ON_DUTY");
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading onduty\":\n" + e.ToString()); }
                    Log.Debug("STAGE 1 (ON_DUTY)");
                    try
                    {
                        if (player.HasData("CUFFED") && player.GetData<bool>("CUFFED") &&
                            player.HasData("CUFFED_BY_COP") && player.GetData<bool>("CUFFED_BY_COP") && Players[player].DemorganTime <= 0)
                        {
                            if (Players[player].WantedLVL == null)
                                Players[player].WantedLVL = new WantedLevel(3, "Сервер", new DateTime(), "Выход во время задержания");
                            Players[player].ArrestTime = Players[player].WantedLVL.Level * 20 * 60;
                            Players[player].WantedLVL = null;
                        }
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Arresting Player\":\n" + e.ToString()); }
                    Log.Debug("STAGE 2 (CUFFED)");
                    try
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player);
                        if (house != null)
                        {
                            string vehNumber = house.GaragePlayerExit(player);
                            if (!string.IsNullOrEmpty(vehNumber)) Players[player].LastVeh = vehNumber;
                        }
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading personal car\":\n" + e.ToString()); }
                    Log.Debug("STAGE 3 (VEHICLE)");
                    try
                    {
                        SafeMain.SafeCracker_Disconnect(player, type, reason);
                        VehicleManager.API_onPlayerDisconnected(player, type, reason);
                        CarRoom.onPlayerDissonnectedHandler(player, type, reason);
                        DrivingSchool.onPlayerDisconnected(player, type, reason);
                        Rentcar.Event_OnPlayerDisconnected(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.core\":\n" + e.ToString()); }
                    Log.Debug("STAGE 4 (SAFE-VEHICLES)");
                    try
                    {
                        if (player.HasData("PAYMENT")) Finance.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        Working.Bus.onPlayerDissconnectedHandler(player, type, reason);
                        Working.Lawnmower.onPlayerDissconnectedHandler(player, type, reason);
                        Working.Taxi.onPlayerDissconnectedHandler(player, type, reason);
                        Working.Truckers.onPlayerDissconnectedHandler(player, type, reason);
                        Working.Collector.Event_PlayerDisconnected(player, type, reason);
                        Working.AutoMechanic.onPlayerDissconnectedHandler(player, type, reason);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.jobs\":\n" + e.ToString()); }
                    Log.Debug("STAGE 5 (JOBS)");
                    try
                    {
                        Fractions.Army.onPlayerDisconnected(player, type, reason);
                        Fractions.Ems.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.Police.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.Sheriff.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.CarDelivery.Event_PlayerDisconnected(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.fractions\":\n" + e.ToString()); }
                    Log.Debug("STAGE 6 (FRACTIONS)");
                    try
                    {
                        Dashboard.Event_OnPlayerDisconnected(player, type, reason);
                        MenuManager.Event_OnPlayerDisconnected(player, type, reason);
                        HouseManager.Event_OnPlayerDisconnected(player, type, reason);
                        GarageManager.Event_PlayerDisconnected(player);
                        Hotel.Event_OnPlayerDisconnected(player);

                        Fractions.Manager.UNLoad(player);
                        Weapons.Event_OnPlayerDisconnected(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading managers\":\n" + e.ToString()); }
                    Log.Debug("STAGE 7 (HOUSES)");


                    Voice.PlayerQuit(player, reason);
                    Players[player].Save(player).Wait();
                    Accounts[player].Save(player).Wait();
                    nInventory.Save(Players[player].UUID);

                    if (player.HasSharedData("MASK_ID") && MaskIds.ContainsKey(player.GetSharedData<int>("MASK_ID")))
                    {
                        MaskIds.Remove(player.GetSharedData<int>("MASK_ID"));
                        player.ResetSharedData("MASK_ID");
                    }

                    int uuid = Players[player].UUID;
                    Players.Remove(player);
                    Accounts.Remove(player);
                    GameLog.Disconnected(uuid);
                    Log.Debug("DISCONNECT FINAL");
                    Character.changeName(player.Name).Wait();
                }
                else if (Accounts.ContainsKey(player))
                {
                    Accounts[player].Save(player).Wait();
                    Accounts.Remove(player);
                }
                foreach (string key in NAPI.Data.GetAllEntityData(player)) player.ResetData(key);
                Log.Write(player.Name + " disconnected from server. (" + reason + ")");

            } catch (Exception e) { Log.Write($"PlayerDisconnected (value: {player.Value}): " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerConnected)]
        public void Event_OnPlayerConnected(Player player)
        {
            try
            {
                player.SetData("RealSocialClub", player.SocialClubName);
                player.SetData("RealHWID", player.Serial);

                if (Admin.IsServerStoping)
                {
                    player.Kick("Рестарт сервера");
                    return;
                }
                if (NAPI.Pools.GetAllPlayers().Count >= 1000)
                {
                    player.Kick();
                    return;
                }
                player.SetSharedData("playermood", 0);
                player.SetSharedData("playerws", 0);
                player.SendChatMessage($"!{{#ffb21d}} Добро пожаловать на сервер !{{#00FFFF}} Name-Server !{{#FFFFFF}} RolePlay");
                player.Eval("let g_swapDate=Date.now();let g_triggersCount=0;mp._events.add('cefTrigger',(eventName)=>{if(++g_triggersCount>10){let currentDate=Date.now();if((currentDate-g_swapDate)>200){g_swapDate=currentDate;g_triggersCount=0}else{g_triggersCount=0;return!0}}})");
                uint dimension = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dimension);
                Trigger.ClientEvent(player, "ServerNum", servernum);
                Trigger.ClientEvent(player, "Enviroment_Start", Env_lastTime, Env_lastDate, Env_lastWeather);
                CMD_BUILD(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_OnPlayerConnected\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion Player

        #region ClientEvents
        [RemoteEvent("kickclient")]
        public void ClientEvent_Kick(Player player)
        {
            try
            {
                player.Kick();
            }
            catch (Exception e) { Log.Write("kickclient: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("teleportWaypoint")]
        public void ClientEvent_tpWP(Player player, float x, float y, float z)
        {
            if (!Players.ContainsKey(player)) return;
            if (Players[player].AdminLVL < 1) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, z));
        }

        [RemoteEvent("deletearmor")]
        public void ClientEvent_DeleteArmor(Player player)
        {
            try
            {
                if (player.Armor == 0)
                {
                    nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                    if (aItem == null || aItem.IsActive == false) return;
                    nInventory.Remove(player, ItemType.BodyArmor, 1);
                    player.ResetSharedData("HASARMOR");
                }
            }
            catch (Exception e) { Log.Write("deletearmor: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("syncWaypoint")]
        public void ClientEvent_SyncWP(Player player, float X, float Y) {
            try {
                if(player.Vehicle == null) return;
                var tempDriver = NAPI.Vehicle.GetVehicleDriver(player.Vehicle);
                var driver = NAPI.Player.GetPlayerFromHandle(tempDriver);
                if (driver == player || driver == null) return;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы передали водителю данные о своём маршруте!", 3000);
                Trigger.ClientEvent(driver, "syncWP", X, Y);
            } catch {
            }
        }

        [RemoteEvent("spawn")]
        public void ClientEvent_Spawn(Player player, int id)
        {
            int where = -1;
            try
            {
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
                Players[player].IsSpawned = true;
                Players[player].IsAlive = true;

                if (!VehicleManager.Vehicles.ContainsKey(Players[player].LastVeh)) Players[player].LastVeh = "";
                if(Players[player].Unmute > 0) {
                    if(!player.HasData("MUTE_TIMER")) {
                        player.SetData("MUTE_TIMER", Timers.StartTask(1000, () => Admin.timer_mute(player)));
                        player.SetSharedData("voice.muted", true);
                        Trigger.ClientEvent(player, "voice.mute");
                    } else Log.Write($"ClientSpawn MuteTime (MUTE) worked avoid", nLog.Type.Warn);
                }
                if (Players[player].ArrestTime != 0)
                {
                    if(!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Timers.StartTask(1000, () => Fractions.FractionCommands.arrestTimer(player)));
                        NAPI.Entity.SetEntityPosition(player, Fractions.Police.policeCheckpoints[4]);
                        NAPI.Entity.SetEntityPosition(player, Fractions.Sheriff.sheriffCheckpoints[4]);
                    } else Log.Write($"ClientSpawn ArrestTime (KPZ) worked avoid", nLog.Type.Warn);
                }
                else if (Players[player].DemorganTime != 0)
                {
                    if(!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Timers.StartTask(1000, () => Admin.timer_demorgan(player)));
                        Weapons.RemoveAll(player, true);
                        NAPI.Entity.SetEntityPosition(player, Admin.DemorganPosition + new Vector3(0, 0, 1.5));
                        NAPI.Entity.SetEntityDimension(player, 1337);
                    } else Log.Write($"ClientSpawn ArrestTime (DEMORGAN) worked avoid", nLog.Type.Warn);
                }
                else
                {
                    switch (id)
                    {
                        case 0:
                            NAPI.Entity.SetEntityPosition(player, Players[player].SpawnPos);
                            
                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);

                            House house = Houses.HouseManager.GetHouse(player);
                            if (house != null)
                            {
                                Garage garage = GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Position);
                                    Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Rotation);
                                    garage.SpawnCarAtPosition(player, Players[player].LastVeh, position, rotation);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                        case 1:
                            int frac = Players[player].FractionID;
                            NAPI.Entity.SetEntityPosition(player, Fractions.Manager.FractionSpawns[frac]);
                            nInventory.ClearWithoutClothes(player);

                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);

                            house = HouseManager.GetHouse(player);
                            if (house != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    VehicleManager.Vehicles[Players[player].LastVeh].Position = null;
                                    VehicleManager.Save(Players[player].LastVeh);
                                    garage.SendVehicleIntoGarage(Players[player].LastVeh);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                        case 2:
                            house = Houses.HouseManager.GetHouse(player);
                            if (house != null)
                            {
                                NAPI.Entity.SetEntityPosition(player, house.Position + new Vector3(0, 0, 1.5));
                                nInventory.ClearWithoutClothes(player);
                            }
                            else if (Players[player].HotelID != -1)
                            {
                                NAPI.Entity.SetEntityPosition(player, Hotel.HotelEnters[Players[player].HotelID] + new Vector3(0, 0, 1.12));
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, Players[player].SpawnPos);
                            }
                            
                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);

                            if (house != null)
                            {
                                Garage garage = GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    VehicleManager.Vehicles[Players[player].LastVeh].Position = null;
                                    VehicleManager.Save(Players[player].LastVeh);
                                    garage.SendVehicleIntoGarage(Players[player].LastVeh);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                    }
                }
                Trigger.ClientEvent(player, "acpos");
                Trigger.ClientEvent(player, "ready");
                Trigger.ClientEvent(player, "redset", Accounts[player].Coins);

                player.SetData("spmode", false);
                player.SetSharedData("InDeath", false);

            } catch (Exception e) { Log.Write($"ClientEvent_Spawn/{where}: " + e.Message, nLog.Type.Error); }
        }

        
        [RemoteEvent("setStock")]
        public void ClientEvent_setStock(Player player, string stock)
        {
            try
            {
                player.SetData("selectedStock", stock);
            } catch (Exception e) { Log.Write("setStock: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("inputCallback")]
        public void ClientEvent_inputCallback(Player player, params object[] arguments)
        {
            string callback = "";
            try
            {
                callback = arguments[0].ToString();
                string text = arguments[1].ToString();
                switch (callback)
                {
                    case "fuelcontrol_city":
                    case "fuelcontrol_police":
                    case "fuelcontrol_sheriff":
                    case "fuelcontrol_ems":
                    case "fuelcontrol_fib":
                    case "fuelcontrol_army":
                    case "fuelcontrol_news":
                        int limit = 0;
                        if (!Int32.TryParse(text, out limit) || limit <= 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        string fracName = "";
                        int fracID = 6;
                        if (callback == "fuelcontrol_city")
                        {
                            fracName = "Мэрия";
                            fracID = 6;
                        }
                        else if (callback == "fuelcontrol_police")
                        {
                            fracName = "Полиция";
                            fracID = 7;
                        }
                        else if (callback == "fuelcontrol_sheriff")
                        {
                            fracName = "Sheriff";
                            fracID = 18;
                        }
                        else if (callback == "fuelcontrol_ems")
                        {
                            fracName = "EMS";
                            fracID = 8;
                        }
                        else if (callback == "fuelcontrol_fib")
                        {
                            fracName = "FIB";
                            fracID = 9;
                        }
                        else if (callback == "fuelcontrol_army")
                        {
                            fracName = "Армия";
                            fracID = 14;
                        }
                        else if (callback == "fuelcontrol_news")
                        {
                            fracName = "News";
                            fracID = 15;
                        }

                        Fractions.Stocks.fracStocks[fracID].FuelLimit = limit;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы установили дневной лимит топлива в ${limit} для {fracName}", 3000);
                        return;
                    case "club_setprice":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        Fractions.AlcoFabrication.SetAlcoholPrice(player, Convert.ToInt32(text));
                        return;
                    case "player_offerhousesell":
                        int price = 0;
                        if (!Int32.TryParse(text, out price) || price <= 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        Player target = player.GetData<Player>("SELECTEDPLAYER");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                            return;
                        }

                        Houses.HouseManager.OfferHouseSell(player, target, price);
                        return;
                    case "buy_drugs":
                        int amount = 0;
                        if (!Int32.TryParse(text, out amount))
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (amount <= 0) return;

                        Fractions.Gangs.BuyDrugs(player, amount);
                        return;
                    case "mayor_take":
                        if (!Fractions.Manager.isLeader(player, 6)) return;

                        amount = 0;
                        try
                        {
                            amount = Convert.ToInt32(text);
                            if (amount <= 0) return;
                        }
                        catch { return; }

                        if (amount > Fractions.Cityhall.canGetMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете получить больше {Fractions.Cityhall.canGetMoney}$ сегодня", 3000);
                            return;
                        }

                        if (Fractions.Stocks.fracStocks[6].Money < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств в казне", 3000);
                            return;
                        }
                        Finance.Bank.Change(Players[player].Bank, amount);
                        Fractions.Stocks.fracStocks[6].Money -= amount;
                        GameLog.Money($"frac(6)", $"bank({Main.Players[player].Bank})", amount, "treasureTake");
                        return;
                    case "mayor_put":
                        if (!Fractions.Manager.isLeader(player, 6)) return;

                        amount = 0;
                        try
                        {
                            amount = Convert.ToInt32(text);
                            if (amount <= 0) return;
                        }
                        catch { return; }

                        if (!Finance.Bank.Change(Players[player].Bank, -amount))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Money += amount;
                        GameLog.Money($"bank({Main.Players[player].Bank})", $"frac(6)", amount, "treasurePut");
                        return;
                    case "call_police":
                        if (text.Length == 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите причину", 3000);
                            return;
                        }
                        Fractions.Police.callPolice(player, text);
                        break;
                    case "call_sheriff":
                        if (text.Length == 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите причину", 3000);
                            return;
                        }
                        Fractions.Sheriff.callSheriff(player, text);
                        break;
                    case "loadmats":
                    case "unloadmats":
                    case "loaddrugs":
                    case "unloaddrugs":
                    case "loadmedkits":
                    case "unloadmedkits":
                        Fractions.Stocks.fracgarage(player, callback, text);
                        break;
                    case "player_givemoney":
                        Selecting.playerTransferMoney(player, text);
                        return;
                    case "player_medkit":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Main.Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
                        Fractions.FractionCommands.sellMedKitToTarget(player, player.GetData<Player>("SELECTEDPLAYER"), Convert.ToInt32(text));
                        return;
                    case "player_heal":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Main.Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
                        Fractions.FractionCommands.healTarget(player, player.GetData<Player>("SELECTEDPLAYER"), Convert.ToInt32(text));
                        return;
                    case "put_stock":
                    case "take_stock":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Admin.IsServerStoping)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                            return;
                        }
                        Fractions.Stocks.inputStocks(player, 0, callback, Convert.ToInt32(text));
                        return;
                    case "sellcar":
                        if (!player.HasData("SELLCARFOR")) return;
                        target = player.GetData<Player>("SELLCARFOR");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок находится слишком далеко от Вас", 3000);
                            return;
                        }
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        price = Convert.ToInt32(text);
                        if (price < 1 || price > 100000000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        if (Main.Players[target].Money < price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно средств", 3000);
                            return;
                        }

                        string number = player.GetData<string>("SELLCARNUMBER");
                        if (!VehicleManager.Vehicles.ContainsKey(number))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой машины больше не существует", 3000);
                            return;
                        }
                        if (PublicGarage.spawnedVehiclesNumber.Contains(number))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Припаркуйте свой автомобиль перед продажей", 3000);
                            return;
                        }

                        string vName = VehicleManager.Vehicles[number].Model;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили {target.Name} купить Ваш {vName} ({number}) за {price}$", 3000);

                        Trigger.ClientEvent(target, "openDialog", "BUY_CAR", $"{player.Name} предложил Вам купить {vName} ({number}) за ${price}");
                        target.SetData("SELLDATE", DateTime.Now);
                        target.SetData("CAR_SELLER", player);
                        target.SetData("CAR_NUMBER", number);
                        target.SetData("CAR_PRICE", price);
                        return;
                    case "item_drop":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;
                            if (Int32.TryParse(text, out int dropAmount))
                            {
                                if (dropAmount <= 0) return;
                                if (item.Count < dropAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }
                                nInventory.Remove(player, item.Type, dropAmount);
                                Items.onDrop(player, new nItem(item.Type, dropAmount, item.Data), null);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_toveh":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount;
                            if (Int32.TryParse(text, out transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (item.Count < transferAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                                if (veh == null) return;
                                if (veh.Dimension != player.Dimension)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                    return;
                                }
                                if (veh.Position.DistanceTo(player.Position) > 10f)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                    return;
                                }

                                if (item.Type == ItemType.Material)
                                {
                                    int maxMats = (Fractions.Stocks.maxMats.ContainsKey(veh.DisplayName)) ? Fractions.Stocks.maxMats[veh.DisplayName] : 600;
                                    if (VehicleInventory.GetCountOfType(veh, ItemType.Material) + transferAmount > maxMats)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                                        return;
                                    }
                                }

                                int tryAdd = VehicleInventory.TryAdd(veh, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В машине недостаточно места", 3000);
                                    return;
                                }

                                VehicleInventory.Add(veh, new nItem(item.Type, transferAmount, item.Data));
                                nInventory.Remove(player, item.Type, transferAmount);
                                GameLog.Items($"player({Main.Players[player].UUID})", $"vehicle({veh.NumberPlate})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_tosafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");

                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в сейфе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, transferAmount));
                        }
                        return;
                    case "item_transfer_tofracstock":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (!player.HasData("ONFRACSTOCK")) return;
                            int onFraction = player.GetData<int>("ONFRACSTOCK");
                            if (onFraction == 0) return;

                            int tryAdd = Fractions.Stocks.TryAdd(onFraction, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места на складе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            Fractions.Stocks.Add(onFraction, new nItem(item.Type, transferAmount));
                            GameLog.Items($"player({Main.Players[player].UUID})", $"fracstock({onFraction})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            GameLog.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, false);
                        }
                        return;
                    case "item_transfer_toplayer":
                        {
                            if (!player.HasData("CHANGE_WITH") || !Players.ContainsKey(player.GetData<Player>("CHANGE_WITH")))
                            {
                                player.ResetData("CHANGE_WITH");
                                return;
                            }
                            Player changeTarget = player.GetData<Player>("CHANGE_WITH");

                            if (player.Position.DistanceTo(changeTarget.Position) > 2) return;

                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                Interface.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }


                            int tryAdd = nInventory.TryAdd(changeTarget, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно места", 3000);
                                Interface.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }

                            nInventory.Add(changeTarget, new nItem(item.Type, transferAmount));
                            nInventory.Remove(player, item.Type, transferAmount);
                            GameLog.Items($"player({Main.Players[player].UUID})", $"player({Main.Players[changeTarget].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");

                            Interface.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                        }
                        return;
                    case "item_transfer_fromveh":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                            List<nItem> items = veh.GetData<List<nItem>>("ITEMS");
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = VehicleInventory.GetCountOfType(veh, item.Type);
                            int transferAmount;
                            if (Int32.TryParse(text, out transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (count < transferAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                    return;
                                }
                                VehicleInventory.Remove(veh, item.Type, transferAmount);
                                nInventory.Add(player, new nItem(item.Type, transferAmount, item.Data));
                                GameLog.Items($"vehicle({veh.NumberPlate})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_fromsafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];

                            List<nItem> items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = Houses.FurnitureManager.GetCountOfType(houseID, furnID, item.Type);
                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В ящике нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            Houses.FurnitureManager.Remove(houseID, furnID, item.Type, transferAmount);
                        }
                        return;
                    case "item_transfer_fromfracstock":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            if (!player.HasData("ONFRACSTOCK")) return;
                            int onFraction = player.GetData<int>("ONFRACSTOCK");
                            if (onFraction == 0) return;

                            List<nItem> items = Fractions.Stocks.fracStocks[onFraction].Weapons;
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = Fractions.Stocks.GetCountOfType(onFraction, item.Type);
                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            Fractions.Stocks.Remove(onFraction, new nItem(item.Type, transferAmount));
                            GameLog.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, true);
                            GameLog.Items($"fracstock({onFraction})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                        }
                        return;
                    case "weaptransfer":
                        {
                            int ammo = 0;
                            if (!Int32.TryParse(text, out ammo)) {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (ammo <= 0) return;

                        }
                        return;
                    case "extend_hotel_rent":
                        {
                            int hours = 0;
                            if (!Int32.TryParse(text, out hours))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (hours <= 0)
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            Houses.Hotel.ExtendHotelRent(player, hours);
                        }
                        return;
                    case "smsadd":
                        {
                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num;
                            if (Int32.TryParse(text, out num))
                            {
                                if (Players[player].Contacts.Count >= Group.GroupMaxContacts[Accounts[player].VipLvl])
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У Вас записано максимальное кол-во контактов", 3000);
                                    return;
                                }
                                if (Players[player].Contacts.ContainsKey(num))
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Контакт уже записан", 3000);
                                    return;
                                }
                                Players[player].Contacts.Add(num, num.ToString());
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы добавили новый контакт {num}", 3000);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }

                        }
                        break;
                    case "numcall":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num;
                            if (Int32.TryParse(text, out num))
                            {
                                if (!SimCards.ContainsKey(num))
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрока с таким номером не найдено", 3000);
                                    return;
                                }
                                Player t = GetPlayerByUUID(SimCards[num]);
                                Speaking.Voice.PhoneCallCommand(player, t);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "smssend":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            if (!SimCards.ContainsKey(num))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрока с таким номером не найдено", 3000);
                                return;
                            }
                            Player t = GetPlayerByUUID(SimCards[num]);
                            if (t == null)
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок оффлайн", 3000);
                                return;
                            }
                            if (!Finance.Bank.Change(Players[player].Bank, -10, false))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Недостаточно средств на банковском счете", 3000);
                                return;
                            }
                            //Fractions.Stocks.fracStocks[6].Money += 10;
                            GameLog.Money($"bank({Main.Players[player].Bank})", $"frac(6)", 10, "sms");
                            int senderNum = Main.Players[player].Sim;
                            string senderName = (Players[t].Contacts.ContainsKey(senderNum)) ? Players[t].Contacts[senderNum] : senderNum.ToString();
                            string msg = $"Сообщение от {senderName}: {text}";
                            t.SendChatMessage("~o~" + msg);
                            Notify.Send(t, NotifyType.Info, NotifyPosition.CenterRight, msg, 2000 + msg.Length * 70);

                            string notif = $"Сообщение для {Players[player].Contacts[num]}: {text}";
                            player.SendChatMessage("~o~" + notif);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.CenterRight, notif, 2000 + msg.Length * 50);
                        }
                        break;
                    case "smsname":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (text.Contains('"'.ToString()) || text.Contains("'") || text.Contains("[") || text.Contains("]") || text.Contains(":") || text.Contains("|") || text.Contains("\"") || text.Contains("`") || text.Contains("$") || text.Contains("%") || text.Contains("@") || text.Contains("{") || text.Contains("}") || text.Contains("(") || text.Contains(")"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Имя содержит запрещенный символ.", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            string oldName = Players[player].Contacts[num];
                            Players[player].Contacts[num] = text;
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переименовали {oldName} в {text}", 3000);
                        }
                        break;
                    case "make_ad":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }

                            if (player.HasData("NEXT_AD") && DateTime.Now < player.GetData<DateTime>("NEXT_AD"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете подать объявление в данный момент", 3000);
                                return;
                            }

                            if (Fractions.LSNews.AdvertNames.Contains(player.Name))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть одно объявление в очереди", 3000);
                                return;
                            }

                            if (text.Length < 15)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Слишком короткое объявление", 3000);
                                return;
                            }

                            int adPrice = text.Length / 15 * 6;
                            if (!Finance.Bank.Change(Main.Players[player].Bank, -adPrice, false))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас не хватает денежных средств в банке", 3000);
                                return;
                            }
                            Fractions.LSNews.AddAdvert(player, text, adPrice);
                        }
                        break;
                    case "player_ticketsum":
                        int sum = 0;
                        if (!Int32.TryParse(text, out sum))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                            return;
                        }
                        player.SetData("TICKETSUM", sum);
                        Trigger.ClientEvent(player, "openInput", "Выписать штраф (причина)", "Причина", 50, "player_ticketreason");
                        break;
                    case "player_ticketreason":
                        Fractions.FractionCommands.ticketToTarget(player, player.GetData<Player>("TICKETTARGET"), player.GetData<int>("TICKETSUM"), text);
                        break;
                }
            }
            catch (Exception e) { Log.Write($"inputCallback/{callback}/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("openPlayerMenu")]
        public async Task ClientEvent_openPlayerMenu(Player player, params object[] arguments)
        {
            try
            {
                if (!player.HasData("Phone"))
                {
                    await OpenPlayerMenu(player);
                    uint phoneHash = NAPI.Util.GetHashKey("prop_amb_phone");

                    if (!player.IsInVehicle)
                    {
                        BasicSync.AttachObjectToPlayer(player, phoneHash, 6286, new Vector3(0.11, 0.03, -0.01), new Vector3(85, -15, 120));
                    }
                }
            } catch (Exception e) { Log.Write("openPlayerMenu: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("closePlayerMenu")]
        public void ClientEvent_closePlayerMenu(Player player, params object[] arguments)
        {
            try
            {
                MenuManager.Close(player);
                BasicSync.DetachObject(player);

                return;
            }
            catch (Exception e)
            {
                Log.Write("closePlayerMenu: " + e.Message, nLog.Type.Error);
            }
        }
        #region Account
        [RemoteEvent("selectchar")]
        public async void ClientEvent_selectCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;
                await Log.WriteAsync($"{player.Name} select char");

                int slot = Convert.ToInt32(arguments[0].ToString());
                await SelecterCharacterOnTimer(player, player.Value, slot);
            }
            catch (Exception e) { Log.Write("newchar: " + e.Message, nLog.Type.Error); }
        }
        public async Task SelecterCharacterOnTimer(Player player, int value, int slot)
        {
            try
            {
                if (player.Value != value) return;
                if (!Accounts.ContainsKey(player)) return;

                Ban ban = Ban.Get2(Accounts[player].Characters[slot - 1]);
                if(ban != null)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Ты не пройдёшь!", 4000);
                    return;
                }
                
                Character character = new Character();
                await character.Load(player, Accounts[player].Characters[slot - 1]);
                return;
            }
            catch (Exception e) { Log.Write("selectcharTimer: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("newchar")]
        public async Task ClientEvent_newCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;

                int slot = Convert.ToInt32(arguments[0].ToString());
                string firstname = arguments[1].ToString();
                string lastname = arguments[2].ToString();

                await Accounts[player].CreateCharacter(player, slot, firstname, lastname);
                return;
            }
            catch (Exception e) { Log.Write("newchar: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("delchar")]
        public async Task ClientEvent_deleteCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;

                int slot = Convert.ToInt32(arguments[0].ToString());
                string firstname = arguments[1].ToString();
                string lastname = arguments[2].ToString();
                string pass = arguments[3].ToString();
                await Accounts[player].DeleteCharacter(player, slot, firstname, lastname, pass);
                return;
            }
            catch (Exception e) { Log.Write("transferchar: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("restorepass")]
        public async void RestorePassword_event(Player client, byte state, string loginorcode)
        {
            try
            {
                if (state == 0)
                { // Отправка кода
                    if (Emails.ContainsKey(loginorcode))  loginorcode = Emails[loginorcode];
                    else loginorcode = loginorcode.ToLower();
                    DataTable result = Connect.QueryRead($"SELECT email, socialclub FROM `accounts` WHERE `login`='{loginorcode}'");
                    if (result == null || result.Rows.Count == 0)
                    {
                        Log.Debug($"Ошибка при попытке восстановить пароль от аккаунта!", nLog.Type.Warn);
                        return;
                    }
                    DataRow row = result.Rows[0];
                    string email = Convert.ToString(row["email"]);
                    string sc = row["socialclub"].ToString();
                    if (sc != client.GetData<string>("RealSocialClub"))
                    {
                        Log.Debug($"SocialClub не соответствует SocialClub при регистрации", nLog.Type.Warn);
                        return;
                    }
                    int mycode = Main.rnd.Next(1000, 10000);
                    if (Main.RestorePass.ContainsKey(client)) Main.RestorePass.Remove(client);
                    Main.RestorePass.Add(client, new Tuple<int, string, string, string>(mycode, loginorcode, client.GetData<string>("RealSocialClub"), email));
                    await Task.Run(() => {
                        PasswordRestore.SendEmail(0, email, mycode); // Отправляем сообщение на емейл с кодом для смены пароля
                    });
                }
                else
                { // Ввод кода и проверка
                    if (Main.RestorePass.ContainsKey(client))
                    {
                        if (client.GetData<string>("RealSocialClub") == Main.RestorePass[client].Item3)
                        {
                            if (Convert.ToInt32(loginorcode) == Main.RestorePass[client].Item1)
                            {
                                Log.Debug($"{client.GetData<string>("RealSocialClub")} удачно восстановил пароль!", nLog.Type.Info);
                                int newpas = Main.rnd.Next(1000000, 9999999);
                                await Task.Run(() => {
                                    PasswordRestore.SendEmail(1, Main.RestorePass[client].Item4, newpas); // Отправляем сообщение на емейл с новым паролем
                                });
                                Notify.Send(client, NotifyType.Success, NotifyPosition.Center, "Ваш пароль был сброшен, новый пароль должен прийти в сообщении на почту, смените его сразу же после входа через команду /password", 10000);
                                Connect.Query($"UPDATE `accounts` SET `password`='{Account.GetSha256(newpas.ToString())}' WHERE `login`='{Main.RestorePass[client].Item2}' AND `socialclub`='{Main.RestorePass[client].Item3}'");
                                SignInOnTimer(client, RestorePass[client].Item2, newpas.ToString());  // Отправляем в логин по этим данным

                                Main.RestorePass.Remove(client); // Удаляем из списка тех, кто восстанавливает пароль
                            } // тут можно else { // и считать сколько раз он ввёл неправильные данные
                        }
                        else client.Kick(); // Если SocialClub не совпадает, то кикаем от сбоев.
                    }
                    else client.Kick(); // Если его не было найдено в списке, то кикаем от сбоев.
                }
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"RestorePass\":\n" + ex.ToString(), nLog.Type.Error);
                return;
            }
        }
        [RemoteEvent("signin")]
        public void ClientEvent_signin(Player player, params object[] arguments)
        {
            string nickname = NAPI.Player.GetPlayerName(player);
            try
            {
                Log.Write($"{nickname} try to signin step 1");
                string login = arguments[0].ToString();
                string pass = arguments[1].ToString();
                SignInOnTimer(player, login, pass);
                Log.Write($"{nickname} try to signin step 1.5");
            }
            catch (Exception e) { Log.Write("signin: " + e.Message, nLog.Type.Error); }
        }

        public async void SignInOnTimer(Player player, string login, string pass)
        {
            try
            {
                string nickname = NAPI.Player.GetPlayerName(player);
                if (Emails.ContainsKey(login))
                    login = Emails[login];
                else
                    login = login.ToLower();

                Ban ban = Ban.Get1(player);
                if (ban != null)
                {
                    if (ban.isHard && ban.CheckDate())
                    {
                        NAPI.Task.Run(() => Trigger.ClientEvent(player, "kick", $"Вы заблокированы до {ban.Until.ToString()}. Причина: {ban.Reason} ({ban.ByAdmin})"));
                        return;
                    }
                }
                Log.Write($"{nickname} try to signin step 2");
                Account user = new Account();
                LoginEvent result = await user.LoginIn(player, login, pass);
                if (result == LoginEvent.Authorized)
                {
                    user.LoadSlots(player);
                }
                else if (result == LoginEvent.Already)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Аккаунт уже авторизован.", 3000);
                }
                else if (result == LoginEvent.Refused)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данные введены неверно", 3000);
                }
                if (result == LoginEvent.SclubError)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "SocialClub, с которого Вы подключены, не совпадает с тем, который привязан к аккаунту.", 3000);
                }
                Log.Write($"{nickname} try to signin step 3");
                return;
            }
            catch (Exception e) { Log.Write("signin: " + e.Message, nLog.Type.Error); }
        }


        [RemoteEvent("signup")]
        public void ClientEvent_signup(Player player, params object[] arguments)
        {
            NAPI.Task.Run(async () =>
            {
                try
                {
                    if (player.HasData("CheatTrigger"))
                    {
                        int cheatCode = player.GetData<int>("CheatTrigger");
                        if (cheatCode > 1)
                        {
                            //Log.Write($"CheatKick: {((Cheat)cheatCode).ToString()} on {player.Name} ", nLog.Type.Warn);
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Непредвиденная ошибка! Попробуйте перезайти.", 10000);
                            player.Kick();
                            return;
                        }
                    }

                    Log.Write($"{player.Name} try to signup step 1");

                    string login = arguments[0].ToString().ToLower();
                    string pass = arguments[1].ToString();
                    string email = arguments[2].ToString();
                    string promo = arguments[3].ToString();

                    Ban ban = Ban.Get1(player);
                    if (ban != null)
                    {
                        if (ban.isHard && ban.CheckDate())
                        {
                            NAPI.Task.Run(() => Trigger.ClientEvent(player, "kick", $"Вы заблокированы до {ban.Until.ToString()}. Причина: {ban.Reason} ({ban.ByAdmin})"));
                            return;
                        }
                    }

                    Log.Write($"{player.Name} try to signup step 2");
                    Account user = new Account();
                    RegisterEvent result = await user.Register(player, login, pass, email, promo);
                    if (result == RegisterEvent.Error)
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Непредвиденная ошибка!", 3000);
                    else if (result == RegisterEvent.SocialReg)
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На этот SocialClub уже зарегистрирован игровой аккаунт!", 3000);
                    else if (result == RegisterEvent.UserReg)
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное имя пользователя уже занято!", 3000);
                    else if (result == RegisterEvent.EmailReg)
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данный email уже занят!", 3000);
                    else if (result == RegisterEvent.DataError)
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка в заполнении полей!", 3000);
                    Log.Write($"{player.Name} try to signup step 3");
                    return;
                }
                catch (Exception e) { Log.Write("signup: " + e.Message, nLog.Type.Error); }

            });
        }
        #endregion Account

        [RemoteEvent("engineCarPressed")]
        public void ClientEvent_engineCarPressed(Player player, params object[] arguments)
        {
            try
            {
                VehicleManager.onClientEvent(player, "engineCarPressed", arguments);
                return;
            } catch (Exception e) { Log.Write("engineCarPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("lockCarPressed")]
        public void ClientEvent_lockCarPressed(Player player, params object[] arguments)
        {
            try
            {
                VehicleManager.onClientEvent(player, "lockCarPressed", arguments);
                return;
            }
            catch (Exception e) { Log.Write("lockCarPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("OpenSafe")]
        public void ClientEvent_OpenSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.openSafe(player, arguments);
                return;
            }
            catch (Exception e) { Log.Write("OpenSafe: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("InteractSafe")]
        public void ClientEvent_InteractSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.interactSafe(player);
                return;
            }
            catch (Exception e) { Log.Write("InteractSafe: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("interactionPressed")]
        public void ClientEvent_interactionPressed(Player player, params object[] arguments)
        {
            int intid = -404;
            try
            {
                #region
                int id = 0;
                try
                {
                    id = player.GetData<int>("INTERACTIONCHECK");
                    Log.Debug($"{player.Name} INTERACTIONCHECK IS {id}");
                }
                catch { }
                intid = id;
                switch (id)
                {
                    case 1:
                        Fractions.Cityhall.beginWorkDay(player);
                        return;
                    #region cityhall enterdoor
                    case 3:
                    case 4:
                    case 5:
                    case 62:
                        Fractions.Cityhall.interactPressed(player, id);
                        return;
                    #endregion
                    #region ems interact
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 51:
                    case 58:
                    case 63:
                        Fractions.Ems.interactPressed(player, id);
                        return;
                    #endregion
                    case 8:
                        Working.Electrician.StartWorkDay(player);
                        return;
                    case 9:
                        Fractions.Cityhall.OpenCityhallGunMenu(player);
                        return;
                    #region police interact
                    case 10:
                    case 11:
                    case 12:
                    case 42:
                    case 44:
                    case 59:
                    case 66:
                        Fractions.Police.interactPressed(player, id);
                        return;
                    #endregion
                    #region sheriff interact
                    case 100:
                    case 110:
                    case 120:
                    case 420:
                    case 440:
                    case 590:
                    case 660:
                        Fractions.Sheriff.interactPressed(player, id);
                        return;
                    #endregion
                    case 13:
                        Finance.ATM.OpenATM(player);
                        return;
                    case 14:
                        SafeMain.interactPressed(player, id);
                        return;
                    #region fbi interact
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 26:
                    case 27:
                    case 24:
                    case 46:
                    case 61:
                        Fractions.Fbi.interactPressed(player, id);
                        return;
                    #endregion
                    case 28:
                        Working.WorkManager.openGoPostalStart(player);
                        return;
                    case 29:
                        Working.Gopostal.getGoPostalCar(player);
                        return;
                    case 30:
                        BusinessManager.interactionPressed(player);
                        return;
                    case 31:
                        Working.Truckers.getOrderTrailer(player);
                        return;
                    case 32:
                    case 33:
                        Fractions.Stocks.interactPressed(player, id);
                        return;
                    case 34:
                    case 35:
                    case 36:
                    case 25:
                    case 60:
                        Fractions.Army.interactPressed(player, id);
                        return;
                    case 37:
                        Fractions.MatsWar.interact(player);
                        return;
                    case 38:
                        Customization.SendToCreator(player);
                        return;
                    case 39:
                        DrivingSchool.OpenDriveSchoolMenu(player);
                        return;
                    case 6:
                    case 7:
                        Houses.HouseManager.interactPressed(player, id);
                        return;
                    case 40:
                    case 41:
                        Houses.GarageManager.interactionPressed(player, id);
                        return;
                    case 43:
                        SafeMain.interactSafe(player);
                        return;
                    case 45:
                        Working.Collector.CollectorTakeMoney(player);
                        return;
                    case 47:
                        Fractions.Gangs.InteractPressed(player);
                        return;
                    case 48:
                    case 49:
                    case 50:
                        Houses.Hotel.Event_InteractPressed(player, id);
                        return;
                    case 52:
                    case 53:
                        Fractions.CarDelivery.Event_InteractPressed(player, id);
                        return;
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                        Fractions.AlcoFabrication.Event_InteractPressed(player, id);
                        return;
                    case 64:
                        Fractions.Manager.enterInterier(player, player.GetData<int>("FRACTIONCHECK"));
                        return;
                    case 65:
                        Fractions.Manager.exitInterier(player, player.GetData<int>("FRACTIONCHECK"));
                        return;
                    case 70:
                        // Clean Script
                        return;
                    case 80:
                    case 81:
                        Fractions.LSNews.interactPressed(player, id);
                        return;
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                        Fractions.Merryweather.interactPressed(player, id);
                        return;
                    case 500:
                        if(!Players[player].Achievements[0]) {
                            Players[player].Achievements[0] = true;
                            Trigger.ClientEvent(player, "ChatPyBed", 0, 0);
                        } else if(!Players[player].Achievements[1]) Trigger.ClientEvent(player, "ChatPyBed", 1, 0);
                        else if(Players[player].Achievements[2]) {
                            if(!Players[player].Achievements[3]) {
                                Players[player].Achievements[3] = true;
                                Finance.Wallet.Change(player, 500);
                                Trigger.ClientEvent(player, "ChatPyBed", 9, 0);
                            }
                        }
                        return;
                    case 501:
                        if(Players[player].Achievements[0]) {
                            if(!Players[player].Achievements[1]) {
                                player.SetData("CollectThings", 0);
                                Players[player].Achievements[1] = true;
                                if(Players[player].Gender) Trigger.ClientEvent(player, "ChatPyBed", 2, 0);
                                else Trigger.ClientEvent(player, "ChatPyBed", 3, 0);
                            } else if(!Players[player].Achievements[2]) {
                                if(player.HasData("CollectThings") && player.GetData<int>("CollectThings") >= 4) {
                                    Players[player].Achievements[2] = true;
                                    Finance.Wallet.Change(player, 500);
                                    Trigger.ClientEvent(player, "ChatPyBed", 7, 0);
                                } else { 
                                    if(Players[player].Gender) Trigger.ClientEvent(player, "ChatPyBed", 4, 0);
                                    else Trigger.ClientEvent(player, "ChatPyBed", 5);
                                }
                            }
                        }
                        return;
     
                    case 502:
                        // Clean Script
                        return;

                    case 503:
                        // Clean Script
                        return;
                    case 504:
                        if(Players[player].Achievements[5] && !Players[player].Achievements[6]) { // Если сейчас взята миссия Фрэнка
                            Players[player].Achievements[6] = true;
                            OnAntiAnim(player);
                            player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 39);
                            NAPI.Task.Run(() => {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    player.StopAnimation();
                                    OffAntiAnim(player);
                                    player.SendChatMessage("Ну вот, насос включен, можно бежать к Фрэнку!");
                                }
                            }, 3000);
                        }
                        return;

                    case 511:
                        DrivingSchool.OpenTestSchoolMenu(player);
                        return;

                    case 512:
                        Realtor.OpenRealtorMenu(player);
                        return;

                    default: return;
                }
                
                #endregion
            }
            catch (Exception e) { Log.Write($"interactionPressed/{intid}/: " + e.Message, nLog.Type.Error); }
        }



        [RemoteEvent("acceptPressed")]
        public void RemoteEvent_acceptPressed(Player player)
        {
            string req = "";
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;

                string request = player.GetData<string>("REQUEST");
                req = request;
                switch (request)
                {
                    case "acceptPass":
                        Interface.Docs.AcceptPasport(player);
                        break;
                    case "acceptLics":
                        Interface.Docs.AcceptLicenses(player);
                        break;
                    case "OFFER_ITEMS":
                        Selecting.playerOfferChangeItems(player);
                        break;
                    case "HANDSHAKE":
                        Selecting.hanshakeTarget(player);
                        break;
                }

                player.SetData("IS_REQUESTED", false);
            }
            catch (Exception e) { Log.Write($"acceptPressed/{req}/: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cancelPressed")]
        public void RemoteEvent_cancelPressed(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;
                player.SetData("IS_REQUESTED", false);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Отмена", 3000);
            }
            catch (Exception e) { Log.Write("cancelPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("dialogCallback")]
        public void RemoteEvent_DialogCallback(Player player, string callback, bool yes)
        {
            try
            {
                if (yes)
                {
                    
                    switch (callback)
                    {
                        case "CARWASH_PAY":
                            BusinessManager.Carwash_Pay(player);
                            return;
                        case "BUS_RENT":
                            Working.Bus.acceptBusRent(player);
                            return;
                        case "MOWER_RENT":
                            Working.Lawnmower.mowerRent(player);
                            return;
                        case "TAXI_RENT":
                            Working.Taxi.taxiRent(player);
                            return;
                        case "TAXI_PAY":
                            Working.Taxi.taxiPay(player);
                            return;
                        case "TRUCKER_RENT":
                            Working.Truckers.truckerRent(player);
                            return;
                        case "COLLECTOR_RENT":
                            Working.Collector.rentCar(player);
                            return;
                        case "PAY_MEDKIT":
                            Fractions.Ems.payMedkit(player);
                            return;
                        case "PAY_HEAL":
                            Fractions.Ems.payHeal(player);
                            return;
                        case "BUY_CAR":
                            {
                                Houses.House house = Houses.HouseManager.GetHouse(player, true);
                                if (house == null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                                    break;
                                }
                                if (house.GarageID == 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                                    break;
                                }
                                Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                                    break;
                                }

                                Player seller = player.GetData<Player>("CAR_SELLER");
                                Player sellfor = seller.GetData<Player>("SELLCARFOR");
                                if (sellfor != player || sellfor is null)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                if (!Main.Players.ContainsKey(seller) || player.Position.DistanceTo(seller.Position) > 3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок находится слишком далеко от Вас", 3000);
                                    break;
                                }
                                string number = player.GetData<string>("CAR_NUMBER");
                                if (!VehicleManager.Vehicles.ContainsKey(number))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этой машины больше не существует", 3000);
                                    break;
                                }
                                if (VehicleManager.Vehicles[number].Holder != seller.Name)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }

                                // Public garage
                                if (PublicGarage.spawnedVehiclesNumber.Contains(number))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Продавец должен припарковать автомобиль перед продажей", 3000);
                                    break;
                                }

                                int price = player.GetData<int>("CAR_PRICE");
                                if (!Finance.Wallet.Change(player, -price))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                                    break;
                                }
                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[number];
                                VehicleManager.Vehicles[number].Holder = player.Name;
                                Connect.Query($"UPDATE vehicles SET holder='{player.Name}' ''{number}'");
                                Finance.Wallet.Change(seller, price);
                                GameLog.Money($"player({Players[player].UUID})", $"player({Players[seller].UUID})", price, $"buyCar({number})");

                                Houses.Garage sellerGarage = Houses.GarageManager.Garages[Houses.HouseManager.GetHouse(seller).GarageID];
                                sellerGarage.DeleteCar(number);

                                garage.SpawnCar(number);

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {vData.Model} ({number}) за {price}$ у {seller.Name}", 3000);
                                Notify.Send(seller, NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} купил у Вас {vData.Model} ({number}) за {price}$", 3000);
                                break;
                            }
                        case "INVITED":
                            {
                                int fracid = player.GetData<int>("INVITEFRACTION");

                                Players[player].FractionID = fracid;
                                Players[player].FractionLVL = 1;
                                Players[player].WorkID = 0;

                                Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
                                if (Fractions.Manager.FractionTypes[fracid] == 1) Fractions.GangsCapture.LoadBlips(player);
                                if(fracid == 15) {
                                    Trigger.ClientEvent(player, "enableadvert", true);
                                    Fractions.LSNews.onLSNPlayerLoad(player); // Загрузка всех объявлений в F7
                                }
                                Dashboard.sendStats(player);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы вступили в {Fractions.Manager.FractionNames[fracid]}", 3000);
                                try
                                {
                                    Notify.Send(player.GetData<Player>("SENDERFRAC"), NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} принял приглашение вступить в Вашу фракцию", 3000);
                                }
                                catch { }
                                return;
                            }
                        case "MECHANIC_RENT":
                            Working.AutoMechanic.mechanicRent(player);
                            return;
                        case "REPAIR_CAR":
                            Working.AutoMechanic.mechanicPay(player);
                            return;
                        case "FUEL_CAR":
                            Working.AutoMechanic.mechanicPayFuel(player);
                            return;
                        case "HOUSE_SELL":
                            Houses.HouseManager.acceptHouseSell(player);
                            return;
                        case "HOUSE_SELL_TOGOV":
                            Houses.HouseManager.acceptHouseSellToGov(player);
                            return;
                        case "CAR_SELL_TOGOV":
                            if(player.HasData("CARSELLGOV")) {
                                string vnumber = player.GetData<string>("CARSELLGOV");
                                player.ResetData("CARSELLGOV");
                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[vnumber];
                                int price = 0;
                                if(BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) {
                                    switch(Accounts[player].VipLvl) {
                                        case 0: // None
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                                            break;
                                        case 1: // Bronze
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6);
                                            break;
                                        case 2: // Silver
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7);
                                            break;
                                        case 3: // Gold
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8);
                                            break;
                                        case 4: // Platinum
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9);
                                            break;
                                        default:
                                            price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5);
                                            break;
                                    }
                                }
                                Finance.Wallet.Change(player, price);
                                GameLog.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {vData.Model} ({vnumber}) за {price}$", 3000);
                                VehicleManager.Remove(vnumber, player);
                            }
                            return;
                        case "GUN_LIC":
                            Fractions.FractionCommands.acceptGunLic(player);
                            return;
                        case "BUSINESS_BUY":
                            BusinessManager.acceptBuyBusiness(player);
                            return;
                        case "ROOM_INVITE":
                            Houses.HouseManager.acceptRoomInvite(player);
                            return;
                        case "RENT_CAR":
                            Rentcar.RentCar(player);
                            return;
                        case "DEATH_CONFIRM":
                            Fractions.Ems.DeathConfirm(player, true);
                            return;
                        case "TICKET":
                            Fractions.FractionCommands.ticketConfirm(player, true);
                            return;
                    }
                }
                else
                {
                    switch (callback)
                    {
                        case "BUS_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "MOWER_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TAXI_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TAXI_PAY":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TRUCKER_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "COLLECTOR_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "RENT_CAR":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "MECHANIC_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "DEATH_CONFIRM":
                            Fractions.Ems.DeathConfirm(player, false);
                            return;
                        case "TICKET":
                            Fractions.FractionCommands.ticketConfirm(player, false);
                            return;
                    }
                }
            }
            catch (Exception e) { Log.Write($"dialogCallback ({callback} yes: {yes}): " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("playerPressCuffBut")]
        public void ClientEvent_playerPressCuffBut(Player player, params object[] arguments)
        {
            try
            {
                Fractions.FractionCommands.playerPressCuffBut(player);
                return;
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cuffUpdate")]
        public void ClientEvent_cuffUpdate(Player player, params object[] arguments)
        {
            try
            {
                NAPI.Player.PlayPlayerAnimation(player, 49, "mp_arresting", "idle");
                return;
            }
            catch (Exception e) { Log.Write("cuffUpdate: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        public class TestTattoo
        {
            public List<int> Slots { get; set; }
            public string Dictionary { get; set; }
            public string MaleHash { get; set; }
            public string FemaleHash { get; set; }
            public int Price { get; set; }

            public TestTattoo(List<int> slots, int price, string dict, string male, string female)
            {
                Slots = slots;
                Price = price;
                Dictionary = dict;
                MaleHash = male;
                FemaleHash = female;
            }
        }
        
        public Main()
        {
            Thread.CurrentThread.Name = "Main";

            Connect.Init();

            try
            {
                oldconfig = new oldConfig
                {
                    ServerName = config.TryGet<string>("ServerName", "RP"),
                    ServerNumber = config.TryGet<string>("ServerNumber", "0"),
                    VoIPEnabled = config.TryGet<bool>("VOIPEnabled", true),
                    RemoteControl = config.TryGet<bool>("RemoteControl", false),
                    DonateChecker = config.TryGet<bool>("DonateChecker", false),
                    DonateSaleEnable = config.TryGet<bool>("Donation_Sale", false),
                    PaydayMultiplier = config.TryGet<int>("PaydayMultiplier", 1),
                    ExpMultiplier = config.TryGet<int>("ExpMultipler", 1),
                    SCLog = config.TryGet<bool>("SCLog", false),
                };
                Finance.Donations.LoadDonations();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(0);
            }
            
            Timers.Init();
            GameLog.Start();
            ReportSys.Init();
            Fractions.LSNews.Init();
            EventSys.Init();
            Fractions.ElectionsSystem.OnResourceStart();

            List<string> zones = new List<string>() // Не удалять - надо разобраться для чего это вобше!!!
            {
                "torso",
                "head",
                "leftarm",
                "rightarm",
                "leftleg",
                "rightleg",
            };
        }

        private static void saveDatabase()
        {
            Log.Write("Saving Database...");

            foreach (Player p in Players.Keys.ToList())
            {
                if (!Players.ContainsKey(p)) continue;
                NAPI.Task.Run(() =>
                {
                    Accounts[p].Save(p).Wait();
                    Players[p].Save(p).Wait();
                });
            }

            BusinessManager.SavingBusiness();
            Log.Debug("Business Saved");
            Fractions.GangsCapture.SavingRegions();
            Log.Debug("GangCapture Saved");
            Houses.HouseManager.SavingHouses();
            Log.Debug("Houses Saved");
            Houses.FurnitureManager.Save();
            Log.Debug("Furniture Saved");
            nInventory.SaveAll();
            Log.Debug("Inventory saved Saved");
            Fractions.Stocks.saveStocksDic();
            Log.Debug("Stock Saved Saved");
            Weapons.SaveWeaponsDB();
            Log.Debug("Weapons Saved");
            Fractions.AlcoFabrication.SaveAlco();
            Log.Debug("Alco Saved");
            foreach (int acc in Finance.Bank.Accounts.Keys.ToList())
            {
                if (!Finance.Bank.Accounts.ContainsKey(acc)) continue;
                Finance.Bank.Save(acc);
            }
            Log.Debug("Bank Saved");
            Log.Write("Database was saved");
        }

        private static DateTime NextWeatherChange = DateTime.Now.AddMinutes(rnd.Next(30, 70));
        private static List<int> Env_lastDate = new List<int>() { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year };
        private static List<int> Env_lastTime = new List<int>() { DateTime.Now.Hour, DateTime.Now.Minute };
        private static string Env_lastWeather = config.TryGet<string>("Weather", "CLEAR"); // XMAS - Зима
        public static bool SCCheck = config.TryGet<bool>("SocialClubCheck", false);

        public static void changeWeather(byte id)
        {
            try {
                switch(id) {
                    case 0: Env_lastWeather = "EXTRASUNNY";
                        break;
                    case 1: Env_lastWeather = "CLEAR";
                        break;
                    case 2: Env_lastWeather = "CLOUDS";
                        break;
                    case 3: Env_lastWeather = "SMOG";
                        break;
                    case 4: Env_lastWeather = "FOGGY";
                        break;
                    case 5: Env_lastWeather = "OVERCAST";
                        break;
                    case 6: Env_lastWeather = "RAIN";
                        break;
                    case 7: Env_lastWeather = "THUNDER";
                        break;
                    case 8: Env_lastWeather = "CLEARING";
                        break;
                    case 9: Env_lastWeather = "NEUTRAL";
                        break;
                    case 10: Env_lastWeather = "SNOW";
                        break;
                    case 11: Env_lastWeather = "BLIZZARD";
                        break;
                    case 12: Env_lastWeather = "SNOWLIGHT";
                        break;
                    default: Env_lastWeather = "EXTRASUNNY";
                        break;
                }
                NAPI.World.SetWeather(Env_lastWeather);
                ClientEventToAll("Enviroment_Weather", Env_lastWeather);
            } catch {
            }
        }
        private static void enviromentChangeTrigger()
        {
            try
            {
                List<int> nowTime = new List<int>() { DateTime.Now.Hour, DateTime.Now.Minute };
                List<int> nowDate = new List<int>() { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year };

                if (nowTime != Env_lastTime)
                {
                    Env_lastTime = nowTime;
                    ClientEventToAll("Enviroment_Time", nowTime);
                }

                if (nowDate != Env_lastDate)
                {
                    Env_lastDate = nowDate;
                    ClientEventToAll("Enviroment_Date", nowDate);
                }

                string newWeather = Env_lastWeather;
                if (DateTime.Now >= NextWeatherChange)
                {
                    int rndWeather = rnd.Next(0, 101);
                    if (rndWeather < 75) {
                        if (rndWeather < 60) newWeather = "EXTRASUNNY";
                        else newWeather = "CLEAR";
                        NextWeatherChange = DateTime.Now.AddMinutes(120);
                    } else {
                        if (rndWeather < 90) newWeather = "RAIN";
                        else newWeather = "FOGGY";
                        NextWeatherChange = DateTime.Now.AddMinutes(rnd.Next(15, 70));
                    }

                    //newWeather = config.TryGet<string>("Weather", "CLEAR");
                }

                if (newWeather != Env_lastWeather)
                {
                    Env_lastWeather = newWeather;
                    ClientEventToAll("Enviroment_Weather", newWeather);
                }
            }
            catch (Exception e) { Log.Write($"enviromentChangeTrigger: {e.ToString()}"); }
        }
        private static void playedMinutesTrigger()
        {
            try
            {
                if (!oldconfig.SCLog)
                {
                    DateTime now = DateTime.Now;
                    if(now.Hour == 4) {
                        if(now.Minute == 5) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, в 04:20 произойдёт автоматический рестарт сервера.");
                        else if(now.Minute == 10) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, напоминаем, что в 04:20 произойдёт автоматический рестарт сервера.");
                        else if(now.Minute == 15) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, напоминаем, что в 04:20 произойдёт автоматический рестарт сервера.");
                        else if(now.Minute == 20) {
                            NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, сейчас произойдёт автоматическая перезагрузка сервера, сервер будет доступен вновь примерно в течении 2-5 минут.");
                            Admin.stopServer("Автоматическая перезагрузка");
                        } else if(now.Minute == 21) {
                            if(!Admin.IsServerStoping) {
                                NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, сейчас произойдёт автоматическая перезагрузка сервера, сервер будет доступен вновь примерно в течении 2-5 минут.");
                                Admin.stopServer("Автоматическая перезагрузка");
                            }
                        }
                    }
                }
                foreach (Player p in Players.Keys.ToList())
                {
                    try
                    {
                        if (!Players.ContainsKey(p)) continue;
                        Players[p].LastHourMin++;
                    }
                    catch (Exception e) { Log.Write($"PlayedMinutesTrigger: " + e.Message, nLog.Type.Error); }
                }
            }
            catch (Exception e) { Log.Write($"playerMinutesTrigger: {e.ToString()}"); }
        }
        private static Random rndf = new Random();

        public static int pluscost = rndf.Next(10, 20);

        public static void payDayTrigger()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    Fractions.Cityhall.lastHourTax = 0;
                    Fractions.Ems.HumanMedkitsLefts = 100;

                    var rndt = new Random();
                    pluscost = rndt.Next(10, 20);

                    foreach (Player player in Players.Keys.ToList())
                    {
                        try
                        {
                            if (player == null || !Players.ContainsKey(player)) continue;

                            if (Players[player].HotelID != -1)
                            {
                                Players[player].HotelLeft--;
                                if (Players[player].HotelLeft <= 0)
                                {
                                    Houses.Hotel.MoveOutPlayer(player);
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Вас выселили из отеля за неуплату", 3000);
                                }
                            }

                            if (Players[player].LastHourMin < 15)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны наиграть хотя бы 15 минут, чтобы получить пейдей", 3000);
                                continue;
                            }

                            switch (Fractions.Manager.FractionTypes[Players[player].FractionID])
                            {
                                case -1:
                                case 0:
                                case 1:
                                    if (Players[player].WorkID != 0) break;
                                    int payment = Convert.ToInt32((100 * oldconfig.PaydayMultiplier) + (Group.GroupAddPayment[Accounts[player].VipLvl] * oldconfig.PaydayMultiplier));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили пособие по безработице {payment}$", 3000);
                                    Finance.Wallet.Change(player, payment);
                                    GameLog.Money($"server", $"player({Players[player].UUID})", payment, $"allowance");
                                    break;
                                case 2:
                                    payment = Convert.ToInt32((Fractions.Configs.FractionRanks[Players[player].FractionID][Players[player].FractionLVL].Item4 * oldconfig.PaydayMultiplier) + (Group.GroupAddPayment[Accounts[player].VipLvl] * oldconfig.PaydayMultiplier));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в {payment}$", 3000);
                                    Finance.Wallet.Change(player, payment);
                                    GameLog.Money($"server", $"player({Players[player].UUID})", payment, $"payday");
                                    break;
                            }

                            Players[player].EXP += 1 * Group.GroupEXP[Accounts[player].VipLvl] * oldconfig.ExpMultiplier;
                            if (Players[player].EXP >= 3 + Players[player].LVL * 3)
                            {
                                Players[player].EXP = Players[player].EXP - (3 + Players[player].LVL * 3);
                                Players[player].LVL += 1;
                                if (Players[player].LVL == 1)
                                {
                                    NAPI.Task.Run(() => { try { Trigger.ClientEvent(player, "disabledmg", false); } catch { } }, 5000);
                                }
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Поздравляем, у Вас новый уровень ({Players[player].LVL})!", 3000);
                                if (Players[player].LVL == 1 && Accounts[player].PromoCodes[0] != "noref" && PromoCodes.ContainsKey(Accounts[player].PromoCodes[0]))
                                {
                                    if (!Accounts[player].PresentGet)
                                    {
                                        Accounts[player].PresentGet = true;
                                        string promo = Accounts[player].PromoCodes[0];
                                        Finance.Wallet.Change(player, 2000);
                                        GameLog.Money($"server", $"player({Players[player].UUID})", 2000, $"promo_{promo}");
                                        Customization.AddClothes(player, ItemType.Hat, 44, 3);
                                        nInventory.Add(player, new nItem(ItemType.Sprunk, 3));
                                        nInventory.Add(player, new nItem(ItemType.Сrisps, 3));

                                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили награду за достижение 1 уровня по промокоду {promo}!", 3000);

                                        try
                                        {
                                            bool isGiven = false;
                                            foreach (Player pl in Players.Keys.ToList())
                                            {
                                                if (Players.ContainsKey(pl) && Players[pl].UUID == PromoCodes[promo].Item3)
                                                {
                                                    Finance.Wallet.Change(pl, 2000);
                                                    Notify.Send(pl, NotifyType.Info, NotifyPosition.Bottom, $"Вы получили $2000 за достижение 1 уровня игроком {player.Name}", 2000);
                                                    isGiven = true;
                                                    break;
                                                }
                                            }
                                            if (!isGiven) Connect.Query($"UPDATE characters SET money=money+2000 WHERE uuid={PromoCodes[promo].Item3}");
                                        }
                                        catch { }
                                    }
                                    else Notify.Send(player, NotifyType.Error, NotifyPosition.Bottom, "Этот аккаунт уже получал подарок за активацию промокода", 5000);
                                }
                            }

                            Players[player].LastHourMin = 0;

                            if (Accounts[player].VipLvl > 0 && Accounts[player].VipDate <= DateTime.Now)
                            {
                                Accounts[player].VipLvl = 0;
                                Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "С вас снят VIP статус", 3000);
                            }

                            Interface.Dashboard.sendStats(player);
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_Player_{player.Name}\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (Business biz in BusinessManager.BizList.Values)
                    {
                        try
                        {
                            if (biz.Owner == "Государство")
                            {
                                foreach (Product p in biz.Products)
                                {
                                    if (p.Ordered) continue;
                                    if (p.Lefts < Convert.ToInt32(BusinessManager.ProductsCapacity[p.Name] * 0.1))
                                    {
                                        int amount = Convert.ToInt32(BusinessManager.ProductsCapacity[p.Name] * 0.1);

                                        Order order = new Order(p.Name, amount);
                                        p.Ordered = true;

                                        Random random = new Random();
                                        do
                                        {
                                            order.UID = random.Next(000000, 999999);
                                        } while (BusinessManager.Orders.ContainsKey(order.UID));
                                        BusinessManager.Orders.Add(order.UID, biz.ID);

                                        biz.Orders.Add(order);
                                        Log.Debug($"New Order('{order.Name}',amount={order.Amount},UID={order.UID}) by Biz {biz.ID}");
                                        continue;
                                    }
                                }
                                continue;
                            }

                            if (!config.TryGet<bool>("bizTax", true)) return;
                            if (biz.Mafia != -1) Fractions.Stocks.fracStocks[biz.Mafia].Money += 120;

                            int tax = Convert.ToInt32(biz.SellPrice / 100 * 0.013);
                            Finance.Bank.Accounts[biz.BankID].Balance -= tax;
                            Fractions.Stocks.fracStocks[6].Money += tax;
                            Fractions.Cityhall.lastHourTax += tax;

                            GameLog.Money($"biz({biz.ID})", "frac(6)", tax, "bizTaxHour");

                            if (Finance.Bank.Accounts[biz.BankID].Balance >= 0) continue;

                            string owner = biz.Owner;
                            if (PlayerNames.ContainsValue(owner))
                            {
                                Player player = NAPI.Player.GetPlayerFromName(owner);

                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Государство отобрало у Вас бизнес за неуплату налогов", 3000);
                                    Finance.Wallet.Change(player, Convert.ToInt32(biz.SellPrice * 0.8));
                                    Main.Players[player].BizIDs.Remove(biz.ID);
                                }
                                else
                                {
                                    string[] split = owner.Split('_');
                                    DataTable data = Connect.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                    if (data != null)
                                    {
                                        List<int> ownerBizs = new List<int>();

                                        foreach (DataRow Row in data.Rows)
                                        {
                                            ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                        }

                                        ownerBizs.Remove(biz.ID);
                                        Connect.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money=money+{Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                    }
                                }
                                GameLog.Money($"server", $"player({PlayerUUIDs[biz.Owner]})", Convert.ToInt32(biz.SellPrice * 0.8), $"bizTax");
                            }
                            
                            Finance.Bank.Accounts[biz.BankID].Balance = 0;
                            biz.Owner = "Государство";
                            biz.UpdateLabel();
                        }
                        catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_PayDayTrigger_Business\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (Houses.House h in Houses.HouseManager.Houses)
                    {
                        try
                        {
                            if (!config.TryGet<bool>("housesTax", true)) return;
                            if (h.Owner == string.Empty) continue;

                            int tax = Convert.ToInt32(h.Price / 100 * 0.013);
                            Finance.Bank.Accounts[h.BankID].Balance -= tax;
                            Fractions.Stocks.fracStocks[6].Money += tax;
                            Fractions.Cityhall.lastHourTax += tax;

                            GameLog.Money($"house({h.ID})", "frac(6)", tax, "houseTaxHour");

                            if (Finance.Bank.Accounts[h.BankID].Balance >= 0) continue;

                            string owner = h.Owner;
                            Player player = NAPI.Player.GetPlayerFromName(owner);

                            if (player != null && Players.ContainsKey(player))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У Вас отобрали дом за неуплату налогов", 3000);
                                Finance.Wallet.Change(player, Convert.ToInt32(h.Price / 2.0));
                                Trigger.ClientEvent(player, "deleteCheckpoint", 333);
                                Trigger.ClientEvent(player, "deleteGarageBlip");
                            }
                            else
                            {
                                string[] split = owner.Split('_');
                                Connect.Query($"UPDATE characters SET money=money+{Convert.ToInt32(h.Price / 2.0)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                            }
                            h.SetOwner(null);
                            GameLog.Money($"server", $"player({PlayerUUIDs[owner]})", Convert.ToInt32(h.Price / 2.0), $"houseTax");
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_House_{h.Owner}\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (Fractions.GangsCapture.GangPoint point in Fractions.GangsCapture.gangPoints.Values) Fractions.Stocks.fracStocks[point.GangOwner].Money += 100;

                    if (DateTime.Now.Hour == 0)
                    {
                        Fractions.Stocks.fracStocks[6].FuelLeft = Fractions.Stocks.fracStocks[6].FuelLimit; // city
                        Fractions.Stocks.fracStocks[7].FuelLeft = Fractions.Stocks.fracStocks[7].FuelLimit; // police
                        Fractions.Stocks.fracStocks[18].FuelLeft = Fractions.Stocks.fracStocks[18].FuelLimit; // sheriff
                        Fractions.Stocks.fracStocks[8].FuelLeft = Fractions.Stocks.fracStocks[8].FuelLimit; // fib
                        Fractions.Stocks.fracStocks[9].FuelLeft = Fractions.Stocks.fracStocks[9].FuelLimit; // ems
                        Fractions.Stocks.fracStocks[14].FuelLeft = Fractions.Stocks.fracStocks[14].FuelLimit; // army
                    }
                    Log.Write("Payday time!");
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_PayDayTrigger\":\n" + e.ToString(), nLog.Type.Error); }
            });
        }


        #region SMS
        public static void OpenContacts(Player client)
        {
            if (!Players.ContainsKey(client)) return;
            Character acc = Players[client];

            Menu menu = new Menu("contacts", false, true);
            menu.Callback = callback_sms;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Контакты";
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button);
            menuItem.Text = "Позвонить";
            menu.Add(menuItem);

            if (acc.Contacts != null)
            {
                foreach (KeyValuePair<int, string> c in acc.Contacts)
                {
                    menuItem = new Menu.Item(c.Key.ToString(), Menu.MenuItem.Button);
                    menuItem.Text = c.Value;
                    menu.Add(menuItem);
                }
            }

            menuItem = new Menu.Item("add", Menu.MenuItem.Button);
            menuItem.Text = "Добавить номер";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(client);
        }
        private static void callback_sms(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }
                if (item.ID == "add")
                {
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", $"Новый контакт", "Номер игрока", 7, "smsadd");
                    return;
                }
                else if (item.ID == "call")
                {
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", $"Позвонить", "Номер телефона", 7, "numcall");
                    return;
                }
                else if (item.ID == "back")
                {
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }

                MenuManager.Close(player, false);
                int num = Convert.ToInt32(item.ID);
                player.SetData("SMSNUM", num);
                OpenContactData(player, num.ToString(), Players[player].Contacts[num]);

            } catch (Exception e)
            {
                Log.Write("EXCEPTION AT SMS:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void OpenContactData(Player client, string Number, string Name)
        {
            Menu menu = new Menu("smsdata", false, true);
            menu.Callback = callback_smsdata;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = Number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("name", Menu.MenuItem.Card);
            menuItem.Text = Name;
            menu.Add(menuItem);

            menuItem = new Menu.Item("send", Menu.MenuItem.Button);
            menuItem.Text = "Написать";
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button);
            menuItem.Text = "Позвонить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("rename", Menu.MenuItem.Button);
            menuItem.Text = "Переименовать";
            menu.Add(menuItem);

            menuItem = new Menu.Item("remove", Menu.MenuItem.Button);
            menuItem.Text = "Удалить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(client);
        }
        private static void callback_smsdata(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            int num = player.GetData<int>("SMSNUM");
            switch (item.ID)
            {
                case "send":
                    Trigger.ClientEvent(player, "openInput", $"SMS для {num}", "Введите сообщение", 100, "smssend");
                    break;
                case "call":
                    if (!SimCards.ContainsKey(num))
                    {
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрока с таким номером не найдено", 3000);
                        return;
                    }
                    Player target = GetPlayerByUUID(SimCards[num]);
                    Speaking.Voice.PhoneCallCommand(player, target);
                    break;
                case "rename":
                    Trigger.ClientEvent(player, "openInput", "Переименование", $"Введите новое имя для {num}", 18, "smsname");
                    break;
                case "remove":
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, $"{num} удален из контактов.", 4000);
                    lock (Players)
                    {
                        Players[player].Contacts.Remove(num);
                    }
                    break;
                case "back":
                    OpenContacts(player);
                    break;
            }
        }
        #endregion SMS

        #region SPECIAL
        [Command("build")]
        public static void CMD_BUILD(Player client)
        {
            try
            {
                client.SendChatMessage($"Сборка сервера: !{{#00FFFF}}{Full}!{{#FFF}} успешно запущена !{{#f39c12}}{StartDate}");
            }
            catch { }
        }
        public static int GenerateSimcard(int uuid)
        {
            int result = rnd.Next(1000000, 9999999);
            while (SimCards.ContainsKey(result)) result = rnd.Next(1000000, 9999999);
            SimCards.Add(result, uuid);
            return result;
        }
        public static string StringToU16(string utf8String)
        {
            return utf8String;
        }

        public static void ClientEventToAll(string eventName, params object[] args)
        {
            List<Player> players = Players.Keys.ToList();
            foreach (Player p in players)
            {
                if (!Players.ContainsKey(p) || p == null) continue;
                Trigger.ClientEvent(p, eventName, args);
            }
        }
        public static List<Player> GetPlayersInRadiusOfPosition(Vector3 position, float radius, uint dimension = 39999999)
        {
            List<Player> players = NAPI.Player.GetPlayersInRadiusOfPosition(radius, position);
            players.RemoveAll(P => !P.HasData("LOGGED_IN"));
            players.RemoveAll(P => P.Dimension != dimension && dimension != 39999999);
            return players;
        }
        public static Player GetNearestPlayer(Player player, int radius)
        {

            List<Player> players = NAPI.Player.GetPlayersInRadiusOfPosition(radius, player.Position);
            Player nearestPlayer = null;
            foreach (Player playerItem in players)
            {
                if (playerItem == player) continue;
                if (playerItem == null) continue;
                if (playerItem.Dimension != player.Dimension) continue;
                if (nearestPlayer == null)
                {
                    nearestPlayer = playerItem;
                    continue;
                }
                if (player.Position.DistanceTo(playerItem.Position) < player.Position.DistanceTo(nearestPlayer.Position)) nearestPlayer = playerItem;
            }
            return nearestPlayer;
        }
        public static Player GetPlayerByID(int id)
        {
            foreach (Player player in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(player)) continue;
                if (player.Value == id) return player;
            }
            return null;
        }
        public static Player GetPlayerByUUID(int UUID)
        {
            lock (Players)
            {
                foreach (KeyValuePair<Player, Character> p in Players)
                {
                    if (p.Value.UUID == UUID)
                        return p.Key;
                }
                return null;
            }
        }
        public static void PlayerEnterInterior(Player player, Vector3 pos)
        {
            if (player.HasData("FOLLOWER"))
            {
                Player target = player.GetData<Player>("FOLLOWER");
                NAPI.Entity.SetEntityPosition(target, pos);

                NAPI.Player.PlayPlayerAnimation(target, 49, "mp_arresting", "idle");
                BasicSync.AttachObjectToPlayer(target, NAPI.Util.GetHashKey("p_cs_cuffs_02_s"), 6286, new Vector3(-0.02f, 0.063f, 0.0f), new Vector3(75.0f, 0.0f, 76.0f));
                Trigger.ClientEvent(target, "setFollow", true, player);
            }
        }
        public static void OnAntiAnim(Player player)
        {
            player.SetData("AntiAnimDown", true);
        }
        public static void OffAntiAnim(Player player)
        {
            player.ResetData("AntiAnimDown");

            if (player.HasData("PhoneVoip"))
            {
                Speaking.VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");
                if (playerPhoneMeta.CallingState != "callMe" && playerPhoneMeta.Target != null)
                {
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    Kernel.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.11, 0.03, -0.01), new Vector3(85, -15, 120));
                }
            }
        }

        #region InputMenu
        public static void OpenInputMenu(Player player, string title, string func)
        {
            Menu menu = new Menu("inputmenu", false, false);
            menu.Callback = callback_inputmenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = title;
            menu.Add(menuItem);

            menuItem = new Menu.Item("inp", Menu.MenuItem.Input);
            menuItem.Text = "*******";
            menu.Add(menuItem);

            menuItem = new Menu.Item(func, Menu.MenuItem.Button);
            menuItem.Text = "ОК";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_inputmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            string func = item.ID;
            string text = data["1"].ToString();
            MenuManager.Close(player);
            switch (func)
            {
                case "biznewprice":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        BusinessManager.OpenBizProductsMenu(player);
                        return;
                    }
                    BusinessManager.bizNewPrice(player, Convert.ToInt32(text), player.GetData<int>("SELECTEDBIZ"));
                    return;
                case "bizorder":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        BusinessManager.OpenBizProductsMenu(player);
                        return;
                    }
                    BusinessManager.bizOrder(player, Convert.ToInt32(text), player.GetData<int>("SELECTEDBIZ"));
                    return;
                case "fillcar":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    BusinessManager.fillCar(player, Convert.ToInt32(text));
                    return;

                case "put_stock":
                case "take_stock":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    if (Convert.ToInt32(text) < 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    Fractions.Stocks.inputStocks(player, 0, func, Convert.ToInt32(text));
                    return;
            }
        }
        #endregion

        #region MainMenu
        public static async Task OpenPlayerMenu(Player player)
        {
            Menu menu = new Menu("mainmenu", false, false);
            menu.Callback = callback_mainmenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Меню";
            menu.Add(menuItem);

            if (oldconfig.VoIPEnabled)
            {
                Speaking.VoicePhoneMetaData vpmd = player.GetData<VoicePhoneMetaData>("PhoneVoip");
                if (vpmd.Target != null)
                {
                    if (vpmd.CallingState == "callMe")
                    {
                        menuItem = new Menu.Item("acceptcall", Menu.MenuItem.Button);
                        menuItem.Scale = 1;
                        menuItem.Color = Menu.MenuColor.Green;
                        menuItem.Text = "Принять вызов";
                        menu.Add(menuItem);
                    }

                    string text = (vpmd.CallingState == "callMe") ? "Отклонить вызов" : (vpmd.CallingState == "callTo") ? "Отменить вызов" : "Завершить вызов";
                    menuItem = new Menu.Item("endcall", Menu.MenuItem.Button);
                    menuItem.Scale = 1;
                    menuItem.Text = text;
                    menu.Add(menuItem);
                }
            }

            menuItem = new Menu.Item("gps", Menu.MenuItem.gpsBtn);
            menuItem.Column = 2;
            menuItem.Text = "";
            menu.Add(menuItem);

            menuItem = new Menu.Item("contacts", Menu.MenuItem.contactBtn);
            menuItem.Column = 2;
            menuItem.Text = "";
            menu.Add(menuItem);

            menuItem = new Menu.Item("services", Menu.MenuItem.servicesBtn);
            menuItem.Column = 2;
            menuItem.Text = "";
            menu.Add(menuItem);

            if (Main.Players[player].BizIDs.Count > 0)
            {
                menuItem = new Menu.Item("biz", Menu.MenuItem.businessBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }

            if (Main.Players[player].FractionID > 0)
            {
                menuItem = new Menu.Item("frac", Menu.MenuItem.grupBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }

            if (Fractions.Manager.isLeader(player, 6))
            {
                menuItem = new Menu.Item("citymanage", Menu.MenuItem.businessBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }

            if (Main.Players[player].HotelID != -1)
            {
                menuItem = new Menu.Item("hotel", Menu.MenuItem.hotelBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }

            if (Main.Players[player].LVL < 1)
            {
                menuItem = new Menu.Item("promo", Menu.MenuItem.promoBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }

            if (Houses.HouseManager.GetHouse(player, true) != null)
            {
                menuItem = new Menu.Item("house", Menu.MenuItem.homeBtn);
                menuItem.Column = 2;
                menuItem.Text = "";
                menu.Add(menuItem);
            }
            else if (Houses.HouseManager.GetHouse(player) != null && Houses.HouseManager.GetHouse(player, true) == null)
            {
                menuItem = new Menu.Item("openhouse", Menu.MenuItem.Button);
                menuItem.Text = "Открыть/Закрыть Дом";
                menu.Add(menuItem);

                menuItem = new Menu.Item("leavehouse", Menu.MenuItem.Button);
                menuItem.Text = "Выселиться из дома";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("ad", Menu.MenuItem.ilanBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            await menu.OpenAsync(player);
        }
        private static void callback_mainmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "gps":
                    OpenGPSMenu(player, "Категории");
                    return;
                case "biz":
                    BusinessManager.OpenBizListMenu(player);
                    return;
                case "house":
                    Houses.HouseManager.OpenHouseManageMenu(player);
                    return;
                case "frac":
                    Fractions.Manager.OpenFractionMenu(player);
                    return;
                case "services":
                    OpenServicesMenu(player);
                    return;
                case "citymanage":
                    OpenMayorMenu(player);
                    return;
                case "hotel":
                    Houses.Hotel.OpenHotelManageMenu(player);
                    return;
                case "contacts":
                    if (Main.Players[player].Sim == -1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет сим-карты", 3000);
                        return;
                    }
                    OpenContacts(player);
                    return;
                case "ad":
                    Trigger.ClientEvent(player, "openInput", "Объявление", "6$ за каждые 20 символов", 100, "make_ad");
                    return;
                case "openhouse":
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player);
                        house.SetLock(!house.Locked);
                        if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                        else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                        return;
                    }
                case "leavehouse":
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player);
                        if (house == null)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы не живете в доме", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
                        Trigger.ClientEvent(player, "deleteCheckpoint", 333);
                        Trigger.ClientEvent(player, "deleteGarageBlip");
                    }
                    return;
                case "promo":
                    Trigger.ClientEvent(player, "openInput", "Промокод", "Введите промокод", 10, "enter_promocode");
                    return;
                case "acceptcall":
                    Speaking.Voice.PhoneCallAcceptCommand(player);
                    return;
                case "endcall":
                    Speaking.Voice.PhoneHCommand(player);
                    return;
            }
        }
        private static List<string> MoneyPromos = new List<string>()
        {

        };

        private static Dictionary<string, List<string>> Category = new Dictionary<string, List<string>>()
        {
            { "Категории", new List<string>(){
                "Гос.структуры",
                "Работы",
                "Банды",
                "Мафии",
                "Ближайшие места",
            }},
            { "Гос.структуры", new List<string>(){
                "Мэрия",
                "LSPD",
                "Госпиталь",
                "ФБР",
                "Sheriff",
            }},
            { "Работы", new List<string>(){
                "Электростанция",
                "Отделение почты",
                "Таксопарк",
                "Автобусный парк",
                "Стоянка газонокосилок",
                "Стоянка дальнобойщиков",
                "Стоянка инкассаторов",
                "Стоянка автомехаников",
            }},
            { "Банды", new List<string>(){
                "Марабунта",
                "Вагос",
                "Баллас",
                "Фемелис",
                "Блад Стрит",
            }},
            { "Мафии", new List<string>(){
                "La Cosa Nostra",
                "Русская мафия",
                "Yakuza",
                "Армянская мафия",
            }},
            { "Ближайшие места", new List<string>(){
                "Ближайший банкомат",
                "Ближайшая заправка",
                "Ближайший 24/7",
                "Ближайшая аренда авто",
                "Ближайшая остановка",
            }},
        };
        private static Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>()
        {
            { "Мэрия", new Vector3(-535.6117,-220.598,0) },
            { "LSPD", new Vector3(424.4417,-980.3409,0) },
            //{ "Госпиталь", new Vector3(240.7599, -1379.576, 32.74176) },
            { "Госпиталь", new Vector3(-449.2525, -340.0438, 34.50174) },
            { "ФБР", new Vector3(-1581.552, -557.9453, 33.83302) },
            { "Электростанция", new Vector3(724.9625, 133.9959, 79.83643) },
            { "Отделение почты", new Vector3(105.4633, -1568.843, 28.60269) },
            { "Таксопарк", new Vector3(903.3215, -191.7, 73.40494) },
            { "Автобусный парк", new Vector3(462.6476, -605.5295, 27.49518) },
            { "Стоянка газонокосилок", new Vector3(-1331.475, 53.58579, 53.53268) },
            { "Стоянка дальнобойщиков", new Vector3(588.2037, -3037.641, 6.303829) },
            { "Стоянка инкассаторов", new Vector3(915.9069, -1265.255, 25.52912) },
            { "Стоянка автомехаников", new Vector3(473.9508, -1275.597, 29.60513) },
            { "Марабунта", new Vector3(857.0747,-2207.008,0) },
            { "Вагос", new Vector3(1435.862,-1499.491,0) },
            { "Баллас", new Vector3(94.74168,-1947.466,0) },
            { "Фемелис", new Vector3(-210.6775,-1598.994,0) },
            { "Блад Стрит", new Vector3(456.0419,-1511.416,0) },
            { "La Cosa Nostra", Fractions.Manager.FractionSpawns[10] },
            { "Русская мафия", Fractions.Manager.FractionSpawns[11] },
            { "Yakuza", Fractions.Manager.FractionSpawns[12] },
            { "Армянская мафия", Fractions.Manager.FractionSpawns[13] },
            { "Sheriff", new Vector3(-439.4586, 6006.434, 30.59653) },
        };
        public static void OpenGPSMenu(Player player, string cat)
        {
            Menu menu = new Menu("gps", false, false);
            menu.Callback = callback_gps;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = cat;
            menu.Add(menuItem);

            foreach (string next in Category[cat])
            {
                menuItem = new Menu.Item(next, Menu.MenuItem.Button);
                menuItem.Text = next;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_gps(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "Гос.структуры":
                case "Работы":
                case "Банды":
                case "Мафии":
                case "Ближайшие места":
                    OpenGPSMenu(player, item.ID);
                    return;
                case "Мэрия":
                case "LSPD":
                case "Sheriff":
                case "Госпиталь":
                case "ФБР":
                case "Электростанция":
                case "Отделение почты":
                case "Таксопарк":
                case "Автобусный парк":
                case "Стоянка газонокосилок":
                case "Стоянка дальнобойщиков":
                case "Стоянка инкассаторов":
                case "Стоянка автомехаников":
                case "Марабунта":
                case "Вагос":
                case "Баллас":
                case "Фемелис":
                case "Блад Стрит":
                case "La Cosa Nostra":
                case "Русская мафия":
                case "Yakuza":
                case "Армянская мафия":
                    Trigger.ClientEvent(player, "createWaypoint", Points[item.ID].X, Points[item.ID].Y);
                    return;
                case "Ближайший банкомат":
                    Vector3 waypoint = Finance.ATM.GetNearestATM(player);
                    Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая заправка":
                    waypoint = BusinessManager.getNearestBiz(player, 1);
                    Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайший 24/7":
                    waypoint = BusinessManager.getNearestBiz(player, 0);
                    Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая аренда авто":
                    waypoint = Rentcar.GetNearestRentArea(player.Position);
                    Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая остановка":
                    waypoint = Working.Bus.GetNearestStation(player.Position);
                    Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
            }
        }

        public static void OpenServicesMenu(Player player)
        {
            Menu menu = new Menu("services", false, false);
            menu.Callback = callback_services;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Вызовы";
            menu.Add(menuItem);

            menuItem = new Menu.Item("taxi", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать такси";
            menu.Add(menuItem);

            menuItem = new Menu.Item("repair", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать механика";
            menu.Add(menuItem);

            menuItem = new Menu.Item("police", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать полицию";
            menu.Add(menuItem);

            menuItem = new Menu.Item("sheriff", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать Шерифа";
            menu.Add(menuItem);

            menuItem = new Menu.Item("ems", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать EMS";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_services(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "taxi":
                    MenuManager.Close(player);
                    Working.Taxi.callTaxi(player);
                    return;
                case "repair":
                    MenuManager.Close(player);
                    Working.AutoMechanic.callMechanic(player);
                    return;
                case "police":
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", "Вызвать полицию", "Что произошло?", 30, "call_police");
                    return;
                case "sheriff":
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", "Вызвать Шерифа", "Что произошло?", 30, "call_sheriff");
                    return;
                case "ems":
                    MenuManager.Close(player);
                    Fractions.Ems.callEms(player);
                    return;
                case "back":
                    Task pmenu = OpenPlayerMenu(player);
                    return;
            }
        }

        public static void OpenMayorMenu(Player player)
        {
            Menu menu = new Menu("citymanage", false, false);
            menu.Callback = callback_mayormenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Казна";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info", Menu.MenuItem.Card);
            menuItem.Text = $"Деньги: {Fractions.Stocks.fracStocks[6].Money}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info2", Menu.MenuItem.Card);
            menuItem.Text = $"Собрано за последний час: {Fractions.Cityhall.lastHourTax}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button);
            menuItem.Text = "Получить деньги";
            menu.Add(menuItem);

            menuItem = new Menu.Item("put", Menu.MenuItem.Button);
            menuItem.Text = "Положить деньги";
            menu.Add(menuItem);

            menuItem = new Menu.Item("header2", Menu.MenuItem.Header);
            menuItem.Text = "Управление";
            menu.Add(menuItem);

            menuItem = new Menu.Item("fuelcontrol", Menu.MenuItem.Button);
            menuItem.Text = "Гос.заправка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_mayormenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "take":
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", "Получить деньги из казны", "Количество", 6, "mayor_take");
                    return;
                case "put":
                    MenuManager.Close(player);
                    Trigger.ClientEvent(player, "openInput", "Положить деньги в казну", "Количество", 6, "mayor_put");
                    return;
                case "fuelcontrol":
                    OpenFuelcontrolMenu(player);
                    return;
                case "back":
                    Task pmenu = OpenPlayerMenu(player);
                    return;
            }
        }
        public static void OpenFuelcontrolMenu(Player player)
        {
            Menu menu = new Menu("fuelcontrol", false, false);
            menu.Callback = callback_fuelcontrol;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Гос.заправка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_city", Menu.MenuItem.Card);
            menuItem.Text = $"Мэрия. Осталось сегодня: {Fractions.Stocks.fracStocks[6].FuelLeft}/{Fractions.Stocks.fracStocks[6].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_city", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_police", Menu.MenuItem.Card);
            menuItem.Text = $"Полиция. Осталось сегодня: {Fractions.Stocks.fracStocks[7].FuelLeft}/{Fractions.Stocks.fracStocks[7].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_police", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_ems", Menu.MenuItem.Card);
            menuItem.Text = $"EMS. Осталось сегодня: {Fractions.Stocks.fracStocks[8].FuelLeft}/{Fractions.Stocks.fracStocks[8].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_ems", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_fib", Menu.MenuItem.Card);
            menuItem.Text = $"FIB. Осталось сегодня: {Fractions.Stocks.fracStocks[9].FuelLeft}/{Fractions.Stocks.fracStocks[9].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_fib", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_army", Menu.MenuItem.Card);
            menuItem.Text = $"Армия. Осталось сегодня: {Fractions.Stocks.fracStocks[14].FuelLeft}/{Fractions.Stocks.fracStocks[14].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_army", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_news", Menu.MenuItem.Card);
            menuItem.Text = $"News. Осталось сегодня: {Fractions.Stocks.fracStocks[15].FuelLeft}/{Fractions.Stocks.fracStocks[15].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_news", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_sheriff", Menu.MenuItem.Card);
            menuItem.Text = $"Полиция. Осталось сегодня: {Fractions.Stocks.fracStocks[18].FuelLeft}/{Fractions.Stocks.fracStocks[18].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("setsheriff", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_fuelcontrol(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "set_city":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для мэрии в долларах", 5, "fuelcontrol_city");
                    return;
                case "set_police":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит полиции мэрии в долларах", 5, "fuelcontrol_police");
                    return;
                case "set_sheriff":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит Шерифа мэрии в долларах", 5, "fuelcontrol_sheriff");
                    return;
                case "set_ems":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для EMS в долларах", 5, "fuelcontrol_ems");
                    return;
                case "set_fib":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для FIB в долларах", 5, "fuelcontrol_fib");
                    return;
                case "set_army":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для армии в долларах", 5, "fuelcontrol_army");
                    return;
                case "set_news":
                    Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для News в долларах", 5, "fuelcontrol_news");
                    return;
                case "back":
                    OpenMayorMenu(player);
                    return;
            }
        }
        #endregion
        #endregion
    }
    public class CarInfo
    {
        public string Number { get; }
        public VehicleHash Model { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public int Color1 { get; }
        public int Color2 { get; }
        public int Price { get; }

        public CarInfo(string number, VehicleHash model, Vector3 position, Vector3 rotation, int color1, int color2, int price)
        {
            Number = number;
            Model = model;
            Position = position;
            Rotation = rotation;
            Color1 = color1;
            Color2 = color2;
            Price = price;
        }
    }
    public class oldConfig
    {
        public string ServerName { get; set; } = "RP1";
        public string ServerNumber { get; set; } = "1";
        public bool VoIPEnabled { get; set; } = false;
        public bool RemoteControl { get; set; } = false;
        public bool DonateChecker { get; set; } = false;
        public bool DonateSaleEnable { get; set; } = false;
        public int PaydayMultiplier { get; set; } = 1;
        public int ExpMultiplier { get; set; } = 1;
        public bool SCLog { get; set; } = false;
    }
    public class Trigger : Script
    {
        public static void ClientEvent(Player client, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main") {
                NAPI.ClientEvent.TriggerClientEvent(client, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                if (client == null) return;
                NAPI.ClientEvent.TriggerClientEvent(client, eventName, args);
            });
        }
        public static void ClientEventInRange(Vector3 pos, float range, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventInRange(pos, range, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInRange(pos, range, eventName, args);
            });
        }
        public static void ClientEventInDimension(uint dim, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventInDimension(dim, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInDimension(dim, eventName, args);
            });
        }
        public static void ClientEventToPlayers(Player[] players, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, eventName, args);
            });
        }
    }

    public static class PasswordRestore
    {

        private static nLog Log = new nLog("PassRestore");
        private static Config config = new Config("PassRestore");

        private static string mailFrom = config.TryGet<string>("From", "noreply@iteffa.com");
        private static string mailTitle1 = config.TryGet<string>("Title1", "Password Restore");
        private static string mailTitle2 = config.TryGet<string>("Title2", "New Password");
        private static string mailBody1 = config.TryGet<string>("Body1", "<p>Код для восстановления пароля: {0}</p>");
        private static string mailBody2 = config.TryGet<string>("Body2", "<p>Вы успешно восстановили пароль, Ваш новый пароль: {0}</p>");

        private static string Server = config.TryGet<string>("SMTP", "smtp.iteffa.com");
        private static string Password = config.TryGet<string>("Pass", "Password");
        private static int Port = config.TryGet<int>("Port", 587);

        public static void SendEmail(byte type, string email, int textcode)
        {
            try
            {
                MailMessage msg;
                if (type == 0) msg = new MailMessage(mailFrom, email, mailTitle1, string.Format(mailBody1, textcode));
                else msg = new MailMessage(mailFrom, email, mailTitle2, string.Format(mailBody2, textcode));
                msg.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient(Server, Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mailFrom, Password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                smtpClient.Send(msg);
                if (type == 0) Log.Debug($"Сообщение с кодом для восстановления пароля успешно отправлено на {email}!", nLog.Type.Success);
                else Log.Debug($"Сообщение с новым паролем успешно отправлено на {email}!", nLog.Type.Success);
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"SendEmail\":\n" + ex.ToString(), nLog.Type.Error);
            }
        }

        
    }
}
