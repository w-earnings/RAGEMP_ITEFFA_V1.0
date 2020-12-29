using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Globals.Character;
using iTeffa.Globals.nAccount;
using iTeffa.Houses;
using iTeffa.Interface;
using iTeffa.Models;
using iTeffa.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// __________________________________________________________ //
//      _____  _____   ____       _ ______ _____ _______      //
//     |  __ \|  __ \ / __ \     | |  ____/ ____|__   __|     //
//     | |__) | |__) | |  | |    | | |__ | |       | |        //
//     |  ___/|  _  /| |  | |_   | |  __|| |       | |        //
//     | |    | | \ \| |__| | |__| | |___| |____   | |        //
//     |_|    |_|  \_\\____/ \____/|______\_____|  |_|        //
//                    ----------------                        //
//                         iTeffa                             //
// ---------------------------------------------------------- //

namespace iTeffa
{
    public class Main : Script
    {
        public static DateTime StartDate { get; } = DateTime.Now;
        public static DateTime CompileDate { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
        public static OldConfig oldconfig;
        private static readonly Config config = new Config("Main");
        private static readonly byte servernum = config.TryGet<byte>("ServerNumber", "1");
        private static readonly int Slots = NAPI.Server.GetMaxPlayers();
        public static Dictionary<string, Tuple<int, int, int>> PromoCodes = new Dictionary<string, Tuple<int, int, int>>();
        public static List<int> UUIDs = new List<int>();
        public static List<string> PersonSIDs = new List<string>();
        public static Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
        public static Dictionary<string, int> PlayerBankAccs = new Dictionary<string, int>();
        public static Dictionary<string, int> PlayerUUIDs = new Dictionary<string, int>();
        public static Dictionary<string, string> PersonPlayerSIDs = new Dictionary<string, string>();
        public static Dictionary<int, Tuple<int, int, int, long>> PlayerSlotsInfo = new Dictionary<int, Tuple<int, int, int, long>>();
        public static Dictionary<string, Player> LoggedIn = new Dictionary<string, Player>();
        public static Dictionary<Player, Character> Players = new Dictionary<Player, Character>();
        public static Dictionary<int, int> SimCards = new Dictionary<int, int>();
        public static Dictionary<int, Player> MaskIds = new Dictionary<int, Player>();
        public static List<string> Usernames = new List<string>();
        public static List<string> SocialClubs = new List<string>();
        public static Dictionary<string, string> Emails = new Dictionary<string, string>();
        public static List<string> HWIDs = new List<string>();
        public static Dictionary<Player, Account> Accounts = new Dictionary<Player, Account>();
        public static Dictionary<Player, Tuple<int, string, string, string>> RestorePass = new Dictionary<Player, Tuple<int, string, string, string>>();
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
        private static readonly Nlogs Log = new Nlogs("GM");
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                NAPI.Server.SetAutoRespawnAfterDeath(false);
                NAPI.Task.Run(() =>
                {
                    NAPI.Server.SetGlobalServerChat(false);
                    NAPI.World.SetTime(DateTime.Now.Hour, 0, 0);
                });

                Settings.Timers.StartOnceTask(10000, () => Modules.Forbes.SyncMajors());

                DataTable result = Database.QueryRead("SELECT `uuid`, `personsid`,`firstname`,`lastname`,`sim`,`lvl`,`exp`,`fraction`,`money`,`bank`,`adminlvl` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            int uuid = Convert.ToInt32(Row["uuid"]);
                            string personsid = Convert.ToString(Row["personsid"]);
                            string name = Convert.ToString(Row["firstname"]);
                            string lastname = Convert.ToString(Row["lastname"]);
                            int lvl = Convert.ToInt32(Row["lvl"]);
                            int exp = Convert.ToInt32(Row["exp"]);
                            int fraction = Convert.ToInt32(Row["fraction"]);
                            long money = Convert.ToInt64(Row["money"]);
                            int adminlvl = Convert.ToInt32(Row["adminlvl"]);
                            int bank = Convert.ToInt32(Row["bank"]);

                            UUIDs.Add(uuid);
                            PersonSIDs.Add(personsid);
                            if (Convert.ToInt32(Row["sim"]) != -1) SimCards.Add(Convert.ToInt32(Row["sim"]), uuid);
                            PlayerNames.Add(uuid, $"{name}_{lastname}");
                            PlayerUUIDs.Add($"{name}_{lastname}", uuid);
                            PersonPlayerSIDs.Add($"{name}_{lastname}", personsid);
                            PlayerBankAccs.Add($"{name}_{lastname}", bank);
                            PlayerSlotsInfo.Add(uuid, new Tuple<int, int, int, long>(lvl, exp, fraction, money));

                            if (adminlvl > 0)
                            {
                                DataTable result2 = Database.QueryRead($"SELECT `socialclub` FROM `accounts` WHERE `character1`={uuid} OR `character2`={uuid} OR `character3`={uuid}");
                                if (result2 == null || result2.Rows.Count == 0) continue;
                                string socialclub = Convert.ToString(result2.Rows[0]["socialclub"]);
                            }
                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
                    }
                }
                else Log.Write("DB `characters` return null result", Nlogs.Type.Warn);

                result = Database.QueryRead("SELECT `login`,`socialclub`,`email`,`hwid` FROM `accounts`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            string login = Convert.ToString(Row["login"]);
                            Usernames.Add(login.ToLower());
                            if (SocialClubs.Contains(Convert.ToString(Row["socialclub"]))) Log.Write("ResourceStart: sc contains " + Convert.ToString(Row["socialclub"]), Nlogs.Type.Error);
                            else SocialClubs.Add(Convert.ToString(Row["socialclub"]));
                            Emails.Add(Convert.ToString(Row["email"]), login);
                            HWIDs.Add(Convert.ToString(Row["hwid"]));

                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
                    }
                }
                else Log.Write("DB `accounts` return null result", Nlogs.Type.Warn);

                result = Database.QueryRead("SELECT `name`,`type`,`count`,`owner` FROM `promocodes`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                        PromoCodes.Add(Convert.ToString(Row["name"]), new Tuple<int, int, int>(Convert.ToInt32(Row["type"]), Convert.ToInt32(Row["count"]), Convert.ToInt32(Row["owner"])));
                }
                else Log.Write("DB `promocodes` return null result", Nlogs.Type.Warn);

                Modules.BanSystem.Sync();

                int time = 3600 - (DateTime.Now.Minute * 60) - DateTime.Now.Second;
                Settings.Timers.StartOnceTask("paydayFirst", time * 1000, () =>
                {

                    Settings.Timers.StartTask("payday", 3600000, () => payDayTrigger());
                    payDayTrigger();

                });
                Settings.Timers.StartTask("savedb", 180000, () => saveDatabase());
                Settings.Timers.StartTask("playedMins", 60000, () => playedMinutesTrigger());
                Settings.Timers.StartTask("envTimer", 1000, () => enviromentChangeTrigger());
                result = Database.QueryRead($"SELECT * FROM `othervehicles`");
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
                        }
                    }

                    Rentcar.rentCarsSpawner();
                    Working.Bus.busCarsSpawner();
                    Working.Lawnmower.mowerCarsSpawner();
                    Working.Taxi.taxiCarsSpawner();
                    Working.Truckers.truckerCarsSpawner();
                    Working.Collector.collectorCarsSpawner();
                    Working.AutoMechanic.mechanicCarsSpawner();
                }
                else Log.Write("DB `othervehicles` return null result", Nlogs.Type.Warn);

                Fractions.Configs.LoadFractionConfigs();

                NAPI.World.SetWeather(config.TryGet<string>("Weather", "XMAS")); // CLEAR.

                if (oldconfig.DonateChecker)
                    Finance.Donations.Start();

                Log.Write(Constants.GM_VERSION + " started at " + StartDate.ToString("s"), Nlogs.Type.Success);
                Log.Write($"Assembly compiled {CompileDate:s}", Nlogs.Type.Success);

                Console.Title = "RageMP - " + oldconfig.ServerName;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
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
            }
            catch (Exception e) { Log.Write("EntityCreated: " + e.Message, Nlogs.Type.Error); }
        }
        #region Player
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (type == DisconnectionType.Timeout)
                    Log.Write($"{player.Name} crashed", Nlogs.Type.Warn);
                Log.Debug($"DisconnectionType: {type}");

                Log.Debug("DISCONNECT STARTED");

                if (Accounts.ContainsKey(player))
                {
                    if (LoggedIn.ContainsKey(Accounts[player].Login)) LoggedIn.Remove(Accounts[player].Login);
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
                        House house = HouseManager.GetHouse(player);
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
                        Modules.VehicleLicense.onPlayerDisconnected(player, type, reason);
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
                        if (player.GetData<string>("jobname") == "farmer" && player.HasData("job_farmer"))
                        {
                            Working.FarmerJob.Farmer.StartWork(player, false);
                        }
                        Working.Construction.Event_PlayerDisconnected(player, type, reason);
                        Working.Diver.Event_PlayerDisconnected(player, type, reason);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.jobs\":\n" + e.ToString()); }
                    Log.Debug("STAGE 5 (JOBS)");
                    try
                    {
                        Fractions.Realm.Army.onPlayerDisconnected(player, type, reason);
                        Fractions.Realm.Ems.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.Realm.Police.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.Realm.Sheriff.onPlayerDisconnectedhandler(player, type, reason);
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


                    Modules.Voice.PlayerQuit(player, reason);
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
                    Loggings.Disconnected(uuid);
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

            }
            catch (Exception e) { Log.Write($"PlayerDisconnected (value: {player.Value}): " + e.Message, Nlogs.Type.Error); }
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
                player.Eval("let g_swapDate=Date.now();let g_triggersCount=0;mp._events.add('cefTrigger',(eventName)=>{if(++g_triggersCount>10){let currentDate=Date.now();if((currentDate-g_swapDate)>200){g_swapDate=currentDate;g_triggersCount=0}else{g_triggersCount=0;return!0}}})");
                uint dimension = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dimension);
                Plugins.Trigger.ClientEvent(player, "ServerNum", servernum);
                Plugins.Trigger.ClientEvent(player, "Enviroment_Start", Env_lastTime, Env_lastDate, Env_lastWeather);
                Commands.PlayerCommands.CMD_BUILD(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_OnPlayerConnected\":\n" + e.ToString(), Nlogs.Type.Error); }
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
            catch (Exception e) { Log.Write("kickclient: " + e.Message, Nlogs.Type.Error); }
        }

        [RemoteEvent("reloadcef")]
        public static void ClientEvent_ReloadCef(Player player)
        {
            try
            {
                Plugins.Trigger.ClientEvent(player, "CUFFED", true);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Голосовой чат и интерфейс перезагружен.", 3000);
                Dashboard.Close(player);
                Plugins.Trigger.ClientEvent(player, "CUFFED", false);
                if (Players[player].FractionID == 7 || Players[player].FractionID == 9)
                {
                    Plugins.Trigger.ClientEvent(player, "CUFFED", false);
                }
                player.StopAnimation();
                return;
            }
            catch (Exception e) { Log.Write($"reloadcef: " + e.Message); }
        }
        [RemoteEvent("deletearmor")]
        public void ClientEvent_DeleteArmor(Player player)
        {
            try
            {
                if (player.Armor == 0)
                {
                    nItem aItem = nInventory.Find(Players[player].UUID, ItemType.BodyArmor);
                    if (aItem == null || aItem.IsActive == false) return;
                    nInventory.Remove(player, ItemType.BodyArmor, 1);
                    player.ResetSharedData("HASARMOR");
                }
            }
            catch (Exception e) { Log.Write("deletearmor: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("syncWaypoint")]
        public void Event_SyncWP(Player player, float X, float Y)
        {
            try
            {
                if (player.Vehicle == null || !player.HasData("TAXI_DRIVER")) return;
                Player driver = player.GetData<Player>("TAXI_DRIVER");
                if (driver == player || driver == null) return;
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы передали водителю данные о своём маршруте!", 3000);
                Plugins.Trigger.ClientEvent(driver, "syncWP", X, Y);
            }
            catch (Exception e)
            {
                Log.Write("WP: " + e.Message);
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
                if (Players[player].Unmute > 0)
                {
                    if (!player.HasData("MUTE_TIMER"))
                    {
                        player.SetData("MUTE_TIMER", Settings.Timers.StartTask(1000, () => Admin.timer_mute(player)));
                        player.SetSharedData("voice.muted", true);
                        Plugins.Trigger.ClientEvent(player, "voice.mute");
                    }
                    else Log.Write($"ClientSpawn MuteTime (MUTE) worked avoid", Nlogs.Type.Warn);
                }
                if (Players[player].ArrestTime != 0)
                {
                    if (!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Settings.Timers.StartTask(1000, () => Fractions.FractionCommands.arrestTimer(player)));
                        NAPI.Entity.SetEntityPosition(player, Fractions.Realm.Police.policeCheckpoints[4]);
                        NAPI.Entity.SetEntityPosition(player, Fractions.Realm.Sheriff.sheriffCheckpoints[4]);
                    }
                    else Log.Write($"ClientSpawn ArrestTime (KPZ) worked avoid", Nlogs.Type.Warn);
                }
                else if (Players[player].DemorganTime != 0)
                {
                    if (!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Settings.Timers.StartTask(1000, () => Admin.timer_demorgan(player)));
                        Weapons.RemoveAll(player, true);
                        NAPI.Entity.SetEntityPosition(player, Admin.DemorganPosition + new Vector3(0, 0, 1.5));
                        NAPI.Entity.SetEntityDimension(player, 1337);
                    }
                    else Log.Write($"ClientSpawn ArrestTime (DEMORGAN) worked avoid", Nlogs.Type.Warn);
                }
                else
                {
                    switch (id)
                    {
                        case 0:
                            NAPI.Entity.SetEntityPosition(player, Players[player].SpawnPos);

                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);

                            House house = HouseManager.GetHouse(player);
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
                        case 2:
                            house = HouseManager.GetHouse(player);
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
                Plugins.Trigger.ClientEvent(player, "acpos");
                Plugins.Trigger.ClientEvent(player, "ready");
                Plugins.Trigger.ClientEvent(player, "redset", Accounts[player].Coins);

                player.SetData("spmode", false);
                player.SetSharedData("InDeath", false);

            }
            catch (Exception e) { Log.Write($"ClientEvent_Spawn/{where}: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("setStock")]
        public void ClientEvent_setStock(Player player, string stock)
        {
            try
            {
                player.SetData("selectedStock", stock);
            }
            catch (Exception e) { Log.Write("setStock: " + e.Message, Nlogs.Type.Error); }
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
                        if (!int.TryParse(text, out limit) || limit <= 0)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
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
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы установили дневной лимит топлива в ${limit} для {fracName}", 3000);
                        return;
                    case "club_setprice":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        Fractions.AlcoFabrication.SetAlcoholPrice(player, Convert.ToInt32(text));
                        return;
                    case "player_offerhousesell":
                        int price = 0;
                        if (!int.TryParse(text, out price) || price <= 0)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        Player target = player.GetData<Player>("SELECTEDPLAYER");
                        if (!Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
                            return;
                        }

                        HouseManager.OfferHouseSell(player, target, price);
                        return;
                    case "buy_drugs":
                        int amount = 0;
                        if (!int.TryParse(text, out amount))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (amount <= 0) return;

                        Fractions.Gangs.Gangs.BuyDrugs(player, amount);
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

                        if (amount > Fractions.Realm.Cityhall.canGetMoney)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете получить больше {Fractions.Realm.Cityhall.canGetMoney}$ сегодня", 3000);
                            return;
                        }

                        if (Fractions.Stocks.fracStocks[6].Money < amount)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств в казне", 3000);
                            return;
                        }
                        Finance.Bank.Change(Players[player].Bank, amount);
                        Fractions.Stocks.fracStocks[6].Money -= amount;
                        Loggings.Money($"frac(6)", $"bank({Players[player].Bank})", amount, "treasureTake");
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
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Money += amount;
                        Loggings.Money($"bank({Players[player].Bank})", $"frac(6)", amount, "treasurePut");
                        return;
                    case "call_police":
                        if (text.Length == 0)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введите причину", 3000);
                            return;
                        }
                        Fractions.Realm.Police.callPolice(player, text);
                        break;
                    case "call_sheriff":
                        if (text.Length == 0)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введите причину", 3000);
                            return;
                        }
                        Fractions.Realm.Sheriff.callSheriff(player, text);
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
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
                        Fractions.FractionCommands.sellMedKitToTarget(player, player.GetData<Player>("SELECTEDPLAYER"), Convert.ToInt32(text));
                        return;
                    case "player_heal":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
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
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Admin.IsServerStoping)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Сервер сейчас не может принять это действие", 3000);
                            return;
                        }
                        Fractions.Stocks.inputStocks(player, 0, callback, Convert.ToInt32(text));
                        return;
                    case "sellcar":
                        if (!player.HasData("SELLCARFOR")) return;
                        target = player.GetData<Player>("SELLCARFOR");
                        if (!Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 3)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок находится слишком далеко от Вас", 3000);
                            return;
                        }
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        price = Convert.ToInt32(text);
                        if (price < 1 || price > 100000000)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        if (Players[target].Money < price)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно средств", 3000);
                            return;
                        }

                        string number = player.GetData<string>("SELLCARNUMBER");
                        if (!VehicleManager.Vehicles.ContainsKey(number))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Такой машины больше не существует", 3000);
                            return;
                        }
                        if (PublicGarage.spawnedVehiclesNumber.Contains(number))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Припаркуйте свой автомобиль перед продажей", 3000);
                            return;
                        }

                        string vName = VehicleManager.Vehicles[number].Model;
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили {target.Name} купить Ваш {vName} ({number}) за {price}$", 3000);

                        Plugins.Trigger.ClientEvent(target, "openDialog", "BUY_CAR", $"{player.Name} предложил Вам купить {vName} ({number}) за ${price}");
                        target.SetData("SELLDATE", DateTime.Now);
                        target.SetData("CAR_SELLER", player);
                        target.SetData("CAR_NUMBER", number);
                        target.SetData("CAR_PRICE", price);
                        return;
                    case "item_drop":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;
                            if (int.TryParse(text, out int dropAmount))
                            {
                                if (dropAmount <= 0) return;
                                if (item.Count < dropAmount)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }
                                nInventory.Remove(player, item.Type, dropAmount);
                                Items.onDrop(player, new nItem(item.Type, dropAmount, item.Data), null);
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Некорректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_toveh":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            if (int.TryParse(text, out int transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (item.Count < transferAmount)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                                if (veh == null) return;
                                if (veh.Dimension != player.Dimension)
                                {
                                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                    return;
                                }
                                if (veh.Position.DistanceTo(player.Position) > 10f)
                                {
                                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                    return;
                                }

                                if (item.Type == ItemType.Material)
                                {
                                    int maxMats = (Fractions.Stocks.maxMats.ContainsKey(veh.DisplayName)) ? Fractions.Stocks.maxMats[veh.DisplayName] : 600;
                                    if (VehicleInventory.GetCountOfType(veh, ItemType.Material) + transferAmount > maxMats)
                                    {
                                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                                        return;
                                    }
                                }

                                int tryAdd = VehicleInventory.TryAdd(veh, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "В машине недостаточно места", 3000);
                                    return;
                                }

                                VehicleInventory.Add(veh, new nItem(item.Type, transferAmount, item.Data));
                                nInventory.Remove(player, item.Type, transferAmount);
                                Loggings.Items($"player({Players[player].UUID})", $"vehicle({veh.NumberPlate})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_tosafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (Players[player].InsideHouseID == -1) return;
                            int houseID = Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");

                            int tryAdd = FurnitureManager.TryAdd(houseID, furnID, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в сейфе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            FurnitureManager.Add(houseID, furnID, new nItem(item.Type, transferAmount));
                        }
                        return;
                    case "item_transfer_tofracstock":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (!player.HasData("ONFRACSTOCK")) return;
                            int onFraction = player.GetData<int>("ONFRACSTOCK");
                            if (onFraction == 0) return;

                            int tryAdd = Fractions.Stocks.TryAdd(onFraction, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места на складе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            Fractions.Stocks.Add(onFraction, new nItem(item.Type, transferAmount));
                            Loggings.Items($"player({Players[player].UUID})", $"fracstock({onFraction})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            Loggings.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, false);
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
                            Character acc = Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }


                            int tryAdd = nInventory.TryAdd(changeTarget, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока недостаточно места", 3000);
                                Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }

                            nInventory.Add(changeTarget, new nItem(item.Type, transferAmount));
                            nInventory.Remove(player, item.Type, transferAmount);
                            Loggings.Items($"player({Players[player].UUID})", $"player({Players[changeTarget].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");

                            Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
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
                            if (int.TryParse(text, out int transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (count < transferAmount)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В машине нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                    return;
                                }
                                VehicleInventory.Remove(veh, item.Type, transferAmount);
                                nInventory.Add(player, new nItem(item.Type, transferAmount, item.Data));
                                Loggings.Items($"vehicle({veh.NumberPlate})", $"player({Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_fromsafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            if (Players[player].InsideHouseID == -1) return;
                            int houseID = Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            HouseFurniture furniture = FurnitureManager.HouseFurnitures[houseID][furnID];

                            List<nItem> items = FurnitureManager.FurnituresItems[houseID][furnID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = FurnitureManager.GetCountOfType(houseID, furnID, item.Type);
                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (count < transferAmount)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В ящике нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            FurnitureManager.Remove(houseID, furnID, item.Type, transferAmount);
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
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"На складе нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            Fractions.Stocks.Remove(onFraction, new nItem(item.Type, transferAmount));
                            Loggings.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, true);
                            Loggings.Items($"fracstock({onFraction})", $"player({Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                        }
                        return;
                    case "weaptransfer":
                        {
                            if (!int.TryParse(text, out int ammo))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (ammo <= 0) return;

                        }
                        return;
                    case "extend_hotel_rent":
                        {
                            if (!int.TryParse(text, out int hours))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (hours <= 0)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            Hotel.ExtendHotelRent(player, hours);
                        }
                        return;
                    case "smsadd":
                        {
                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (int.TryParse(text, out int num))
                            {
                                if (Players[player].Contacts.Count >= Group.GroupMaxContacts[Accounts[player].VipLvl])
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "У Вас записано максимальное кол-во контактов", 3000);
                                    return;
                                }
                                if (Players[player].Contacts.ContainsKey(num))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Контакт уже записан", 3000);
                                    return;
                                }
                                Players[player].Contacts.Add(num, num.ToString());
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы добавили новый контакт {num}", 3000);
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Некорректные данные", 3000);
                                return;
                            }

                        }
                        break;
                    case "numcall":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (int.TryParse(text, out int num))
                            {
                                if (!SimCards.ContainsKey(num))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрока с таким номером не найдено", 3000);
                                    return;
                                }
                                Player t = GetPlayerByUUID(SimCards[num]);
                                Modules.Voice.PhoneCallCommand(player, t);
                            }
                            else
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "smssend":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            if (!SimCards.ContainsKey(num))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрока с таким номером не найдено", 3000);
                                return;
                            }
                            Player t = GetPlayerByUUID(SimCards[num]);
                            if (t == null)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрок оффлайн", 3000);
                                return;
                            }
                            if (!Finance.Bank.Change(Players[player].Bank, -10, false))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Недостаточно средств на банковском счете", 3000);
                                return;
                            }

                            Loggings.Money($"bank({Players[player].Bank})", $"frac(6)", 10, "sms");
                            int senderNum = Players[player].Sim;
                            string senderName = (Players[t].Contacts.ContainsKey(senderNum)) ? Players[t].Contacts[senderNum] : senderNum.ToString();
                            string msg = $"Сообщение от {senderName}: {text}";
                            t.SendChatMessage("~o~" + msg);
                            Plugins.Notice.Send(t, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, msg, 2000 + msg.Length * 70);

                            string notif = $"Сообщение для {Players[player].Contacts[num]}: {text}";
                            player.SendChatMessage("~o~" + notif);
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, notif, 2000 + msg.Length * 50);
                        }
                        break;
                    case "smsname":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (text.Contains('"'.ToString()) || text.Contains("'") || text.Contains("[") || text.Contains("]") || text.Contains(":") || text.Contains("|") || text.Contains("\"") || text.Contains("`") || text.Contains("$") || text.Contains("%") || text.Contains("@") || text.Contains("{") || text.Contains("}") || text.Contains("(") || text.Contains(")"))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Имя содержит запрещенный символ.", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            string oldName = Players[player].Contacts[num];
                            Players[player].Contacts[num] = text;
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы переименовали {oldName} в {text}", 3000);
                        }
                        break;
                    case "make_ad":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                                return;
                            }

                            if (player.HasData("NEXT_AD") && DateTime.Now < player.GetData<DateTime>("NEXT_AD"))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы не можете подать объявление в данный момент", 3000);
                                return;
                            }

                            if (Fractions.Realm.LSNews.AdvertNames.Contains(player.Name))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас уже есть одно объявление в очереди", 3000);
                                return;
                            }

                            if (text.Length < 15)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Слишком короткое объявление", 3000);
                                return;
                            }

                            int adPrice = text.Length / 15 * 6;
                            if (!Finance.Bank.Change(Players[player].Bank, -adPrice, false))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас не хватает денежных средств в банке", 3000);
                                return;
                            }
                            Fractions.Realm.LSNews.AddAdvert(player, text, adPrice);
                        }
                        break;
                    case "player_ticketsum":
                        int sum = 0;
                        if (!Int32.TryParse(text, out sum))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Некорректные данные", 3000);
                            return;
                        }
                        player.SetData("TICKETSUM", sum);
                        Plugins.Trigger.ClientEvent(player, "openInput", "Выписать штраф (причина)", "Причина", 50, "player_ticketreason");
                        break;
                    case "player_ticketreason":
                        Fractions.FractionCommands.ticketToTarget(player, player.GetData<Player>("TICKETTARGET"), player.GetData<int>("TICKETSUM"), text);
                        break;
                }
            }
            catch (Exception e) { Log.Write($"inputCallback/{callback}/: {e}\n{e.StackTrace}", Nlogs.Type.Error); }
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
            }
            catch (Exception e) { Log.Write("openPlayerMenu: " + e.Message, Nlogs.Type.Error); }
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
                Log.Write("closePlayerMenu: " + e.Message, Nlogs.Type.Error);
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
            catch (Exception e) { Log.Write("newchar: " + e.Message, Nlogs.Type.Error); }
        }
        public async Task SelecterCharacterOnTimer(Player player, int value, int slot)
        {
            try
            {
                if (player.Value != value) return;
                if (!Accounts.ContainsKey(player)) return;

                Modules.BanSystem ban = Modules.BanSystem.Get2(Accounts[player].Characters[slot - 1]);
                if (ban != null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Ты не пройдёшь!", 4000);
                    return;
                }

                Character character = new Character();
                await character.Load(player, Accounts[player].Characters[slot - 1]);
                return;
            }
            catch (Exception e) { Log.Write("selectcharTimer: " + e.Message, Nlogs.Type.Error); }
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
            catch (Exception e) { Log.Write("newchar: " + e.Message, Nlogs.Type.Error); }
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
            catch (Exception e) { Log.Write("transferchar: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("restorepass")]
        public async void RestorePassword_event(Player client, byte state, string loginorcode)
        {
            try
            {
                if (state == 0)
                {
                    if (Emails.ContainsKey(loginorcode)) loginorcode = Emails[loginorcode];
                    else loginorcode = loginorcode.ToLower();
                    DataTable result = Database.QueryRead($"SELECT email, socialclub FROM `accounts` WHERE `login`='{loginorcode}'");
                    if (result == null || result.Rows.Count == 0)
                    {
                        Log.Debug($"Ошибка при попытке восстановить пароль от аккаунта!", Nlogs.Type.Warn);
                        return;
                    }
                    DataRow row = result.Rows[0];
                    string email = Convert.ToString(row["email"]);
                    string sc = row["socialclub"].ToString();
                    if (sc != client.GetData<string>("RealSocialClub"))
                    {
                        Log.Debug($"SocialClub не соответствует SocialClub при регистрации", Nlogs.Type.Warn);
                        return;
                    }
                    int mycode = rnd.Next(1000, 10000);
                    if (RestorePass.ContainsKey(client)) RestorePass.Remove(client);
                    RestorePass.Add(client, new Tuple<int, string, string, string>(mycode, loginorcode, client.GetData<string>("RealSocialClub"), email));
                    await Task.Run(() =>
                    {
                        Modules.PassReset.SendEmail(0, email, mycode);
                    });
                }
                else
                {
                    if (RestorePass.ContainsKey(client))
                    {
                        if (client.GetData<string>("RealSocialClub") == RestorePass[client].Item3)
                        {
                            if (Convert.ToInt32(loginorcode) == RestorePass[client].Item1)
                            {
                                Log.Debug($"{client.GetData<string>("RealSocialClub")} удачно восстановил пароль!", Nlogs.Type.Info);
                                int newpas = rnd.Next(1000000, 9999999);
                                await Task.Run(() =>
                                {
                                    Modules.PassReset.SendEmail(1, RestorePass[client].Item4, newpas);
                                });
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Ваш пароль был сброшен, новый пароль должен прийти в сообщении на почту, смените его сразу же после входа через команду /password", 10000);
                                Database.Query($"UPDATE `accounts` SET `password`='{Account.GetSha256(newpas.ToString())}' WHERE `login`='{RestorePass[client].Item2}' AND `socialclub`='{RestorePass[client].Item3}'");
                                SignInOnTimer(client, RestorePass[client].Item2, newpas.ToString());

                                RestorePass.Remove(client);
                            }
                        }
                        else client.Kick();
                    }
                    else client.Kick();
                }
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"RestorePass\":\n" + ex.ToString(), Nlogs.Type.Error);
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
            catch (Exception e) { Log.Write("signin: " + e.Message, Nlogs.Type.Error); }
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

                Modules.BanSystem ban = Modules.BanSystem.Get1(player);
                if (ban != null)
                {
                    if (ban.isHard && ban.CheckDate())
                    {
                        NAPI.Task.Run(() => Plugins.Trigger.ClientEvent(player, "kick", $"Вы заблокированы до {ban.Until}. Причина: {ban.Reason} ({ban.ByAdmin})"));
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
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Аккаунт уже авторизован.", 3000);
                }
                else if (result == LoginEvent.Refused)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данные введены неверно", 3000);
                }
                if (result == LoginEvent.SclubError)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "SocialClub, с которого Вы подключены, не совпадает с тем, который привязан к аккаунту.", 3000);
                }
                Log.Write($"{nickname} try to signin step 3");
                return;
            }
            catch (Exception e) { Log.Write("signin: " + e.Message, Nlogs.Type.Error); }
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
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Непредвиденная ошибка! Попробуйте перезайти.", 10000);
                            player.Kick();
                            return;
                        }
                    }

                    Log.Write($"{player.Name} try to signup step 1");

                    string login = arguments[0].ToString().ToLower();
                    string pass = arguments[1].ToString();
                    string email = arguments[2].ToString();
                    string promo = arguments[3].ToString();

                    Modules.BanSystem ban = Modules.BanSystem.Get1(player);
                    if (ban != null)
                    {
                        if (ban.isHard && ban.CheckDate())
                        {
                            NAPI.Task.Run(() => Plugins.Trigger.ClientEvent(player, "kick", $"Вы заблокированы до {ban.Until}. Причина: {ban.Reason} ({ban.ByAdmin})"));
                            return;
                        }
                    }

                    Log.Write($"{player.Name} try to signup step 2");
                    Account user = new Account();
                    RegisterEvent result = await user.Register(player, login, pass, email, promo);
                    if (result == RegisterEvent.Error)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Непредвиденная ошибка!", 3000);
                    else if (result == RegisterEvent.SocialReg)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "На этот SocialClub уже зарегистрирован игровой аккаунт!", 3000);
                    else if (result == RegisterEvent.UserReg)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данное имя пользователя уже занято!", 3000);
                    else if (result == RegisterEvent.EmailReg)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данный email уже занят!", 3000);
                    else if (result == RegisterEvent.DataError)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Ошибка в заполнении полей!", 3000);
                    Log.Write($"{player.Name} try to signup step 3");
                    return;
                }
                catch (Exception e) { Log.Write("signup: " + e.Message, Nlogs.Type.Error); }

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
            }
            catch (Exception e) { Log.Write("engineCarPressed: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("lockCarPressed")]
        public void ClientEvent_lockCarPressed(Player player, params object[] arguments)
        {
            try
            {
                VehicleManager.onClientEvent(player, "lockCarPressed", arguments);
                return;
            }
            catch (Exception e) { Log.Write("lockCarPressed: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("OpenSafe")]
        public void ClientEvent_OpenSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.openSafe(player, arguments);
                return;
            }
            catch (Exception e) { Log.Write("OpenSafe: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("InteractSafe")]
        public void ClientEvent_InteractSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.interactSafe(player);
                return;
            }
            catch (Exception e) { Log.Write("InteractSafe: " + e.Message, Nlogs.Type.Error); }
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
                        Fractions.Realm.Cityhall.beginWorkDay(player);
                        return;

                    case 506:
                        Finance.Branch.OpenBRANCH(player);
                        return;

                    #region cityhall enterdoor
                    case 3:
                    case 4:
                    case 5:
                    case 62:
                        Fractions.Realm.Cityhall.interactPressed(player, id);
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
                        Fractions.Realm.Ems.interactPressed(player, id);
                        return;
                    #endregion
                    case 8:
                        Working.Electrician.StartWorkDay(player);
                        return;
                    case 9:
                        Fractions.Realm.Cityhall.OpenCityhallGunMenu(player);
                        return;
                    #region police interact
                    case 10:
                    case 11:
                    case 12:
                    case 42:
                    case 44:
                    case 59:
                    case 66:
                        Fractions.Realm.Police.interactPressed(player, id);
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
                        Fractions.Realm.Sheriff.interactPressed(player, id);
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
                        Fractions.Realm.Fbi.interactPressed(player, id);
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
                        Fractions.Realm.Army.interactPressed(player, id);
                        return;
                    case 37:
                        Fractions.MatsWar.interact(player);
                        return;
                    case 38:
                        Customization.SendToCreator(player);
                        return;
                    case 39:
                        Modules.VehicleLicense.OpenDriveSchoolMenu(player);
                        return;
                    case 6:
                    case 7:
                        HouseManager.interactPressed(player, id);
                        return;
                    case 40:
                    case 41:
                        GarageManager.interactionPressed(player, id);
                        return;
                    case 43:
                        SafeMain.interactSafe(player);
                        return;
                    case 45:
                        Working.Collector.CollectorTakeMoney(player);
                        return;
                    case 47:
                        Fractions.Gangs.Gangs.InteractPressed(player);
                        return;
                    case 48:
                    case 49:
                    case 50:
                        Hotel.Event_InteractPressed(player, id);
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
                    case 80:
                    case 81:
                        Fractions.Realm.LSNews.interactPressed(player, id);
                        return;
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                        Fractions.Realm.Merryweather.interactPressed(player, id);
                        return;
                    case 500:
                        if (!Players[player].Achievements[0])
                        {
                            Players[player].Achievements[0] = true;
                            Plugins.Trigger.ClientEvent(player, "ChatPyBed", 0, 0);
                        }
                        else if (!Players[player].Achievements[1]) Plugins.Trigger.ClientEvent(player, "ChatPyBed", 1, 0);
                        else if (Players[player].Achievements[2])
                        {
                            if (!Players[player].Achievements[3])
                            {
                                Players[player].Achievements[3] = true;
                                Finance.Wallet.Change(player, 500);
                                Plugins.Trigger.ClientEvent(player, "ChatPyBed", 9, 0);
                            }
                        }
                        return;
                    case 501:
                        if (Players[player].Achievements[0])
                        {
                            if (!Players[player].Achievements[1])
                            {
                                player.SetData("CollectThings", 0);
                                Players[player].Achievements[1] = true;
                                if (Players[player].Gender) Plugins.Trigger.ClientEvent(player, "ChatPyBed", 2, 0);
                                else Plugins.Trigger.ClientEvent(player, "ChatPyBed", 3, 0);
                            }
                            else if (!Players[player].Achievements[2])
                            {
                                if (player.HasData("CollectThings") && player.GetData<int>("CollectThings") >= 4)
                                {
                                    Players[player].Achievements[2] = true;
                                    Finance.Wallet.Change(player, 500);
                                    Plugins.Trigger.ClientEvent(player, "ChatPyBed", 7, 0);
                                }
                                else
                                {
                                    if (Players[player].Gender) Plugins.Trigger.ClientEvent(player, "ChatPyBed", 4, 0);
                                    else Plugins.Trigger.ClientEvent(player, "ChatPyBed", 5);
                                }
                            }
                        }
                        return;


                    case 509:
                        Working.Construction.StartWorkDayConstruction(player);
                        return;
                    case 510:
                        Working.Diver.StartWorkDayDiver(player);
                        return;

                    case 512:
                        Realtor.OpenRealtorMenu(player);
                        return;
                    case 520:
                        Working.FarmerJob.Farmer.OpenFarmerMenu(player);
                        return;
                    case 521:
                        Working.FarmerJob.Market.OpenMarketMenu(player, 0);
                        return;
                    case 571:
                        Modules.InfoPed.Interact1(player);
                        return;
                    case 814:
                        Modules.Containers.OpenMenuContainer(player);
                        break;


                    default: return;
                }

                #endregion
            }
            catch (Exception e) { Log.Write($"interactionPressed/{intid}/: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("acceptPressed")]
        public void RemoteEvent_acceptPressed(Player player)
        {
            string req = "";
            try
            {
                if (!Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;

                string request = player.GetData<string>("REQUEST");
                req = request;
                switch (request)
                {
                    case "acceptPass":
                        Docs.AcceptPasport(player);
                        break;
                    case "acceptLics":
                        Docs.AcceptLicenses(player);
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
            catch (Exception e) { Log.Write($"acceptPressed/{req}/: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("cancelPressed")]
        public void RemoteEvent_cancelPressed(Player player)
        {
            try
            {
                if (!Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;
                player.SetData("IS_REQUESTED", false);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Отмена", 3000);
            }
            catch (Exception e) { Log.Write("cancelPressed: " + e.Message, Nlogs.Type.Error); }
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
                            Fractions.Realm.Ems.payMedkit(player);
                            return;
                        case "PAY_HEAL":
                            Fractions.Realm.Ems.payHeal(player);
                            return;
                        case "BUY_CAR":
                            {
                                Houses.House house = Houses.HouseManager.GetHouse(player, true);
                                if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет личного дома", 3000);
                                    break;
                                }
                                if (house != null)
                                {
                                    if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                                    {
                                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет гаража", 3000);
                                        break;
                                    }
                                    Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                    if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                    {
                                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас максимальное кол-во машин", 3000);
                                        break;
                                    }
                                }

                                Player seller = player.GetData<Player>("CAR_SELLER");
                                Player sellfor = seller.GetData<Player>("SELLCARFOR");
                                if (sellfor != player || sellfor is null)
                                {
                                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                if (!Main.Players.ContainsKey(seller) || player.Position.DistanceTo(seller.Position) > 3)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок находится слишком далеко от Вас", 3000);
                                    break;
                                }
                                string number = player.GetData<string>("CAR_NUMBER");
                                if (!VehicleManager.Vehicles.ContainsKey(number))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Этой машины больше не существует", 3000);
                                    break;
                                }
                                if (VehicleManager.Vehicles[number].Holder != seller.Name)
                                {
                                    Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }

                                // Public garage
                                if (PublicGarage.spawnedVehiclesNumber.Contains(number))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Продавец должен припарковать автомобиль перед продажей", 3000);
                                    break;
                                }

                                int price = player.GetData<int>("CAR_PRICE");
                                if (!Finance.Wallet.Change(player, -price))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас недостаточно средств", 3000);
                                    break;
                                }
                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[number];
                                VehicleManager.Vehicles[number].Holder = player.Name;
                                Database.Query($"UPDATE vehicles SET holder='{player.Name}' WHERE number='{number}'");

                                Finance.Wallet.Change(seller, price);
                                Loggings.Money($"player({Players[player].UUID})", $"player({Players[seller].UUID})", price, $"buyCar({number})");

                                var houset = Houses.HouseManager.GetHouse(seller, true);

                                if (houset != null)
                                {
                                    Houses.Garage sellerGarage = Houses.GarageManager.Garages[Houses.HouseManager.GetHouse(seller).GarageID];
                                    sellerGarage.DeleteCar(number);
                                }

                                if (house != null)
                                {
                                    Houses.Garage Garage = Houses.GarageManager.Garages[Houses.HouseManager.GetHouse(player).GarageID];
                                    Garage.SpawnCar(number);
                                }

                                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы купили {vData.Model} ({number}) за {price}$ у {seller.Name}", 3000);
                                Plugins.Notice.Send(seller, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{player.Name} купил у Вас {vData.Model} ({number}) за {price}$", 3000);
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
                                if (fracid == 15)
                                {
                                    Plugins.Trigger.ClientEvent(player, "enableadvert", true);
                                    Fractions.Realm.LSNews.onLSNPlayerLoad(player); // Загрузка всех объявлений в F7
                                }
                                Dashboard.sendStats(player);
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы вступили в {Fractions.Manager.FractionNames[fracid]}", 3000);
                                try
                                {
                                    Plugins.Notice.Send(player.GetData<Player>("SENDERFRAC"), Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{player.Name} принял приглашение вступить в Вашу фракцию", 3000);
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
                            HouseManager.acceptHouseSell(player);
                            return;
                        case "HOUSE_SELL_TOGOV":
                            HouseManager.acceptHouseSellToGov(player);
                            return;
                        case "CAR_SELL_TOGOV":
                            if (player.HasData("CARSELLGOV"))
                            {
                                string vnumber = player.GetData<string>("CARSELLGOV");
                                player.ResetData("CARSELLGOV");
                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[vnumber];
                                int price = 0;
                                if (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model))
                                {
                                    price = Accounts[player].VipLvl switch
                                    {
                                        // None
                                        0 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                                        // Bronze
                                        1 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6),
                                        // Silver
                                        2 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7),
                                        // Gold
                                        3 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8),
                                        // Platinum
                                        4 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9),
                                        _ => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                                    };
                                }
                                Finance.Wallet.Change(player, price);
                                Loggings.Money($"server", $"player({Players[player].UUID})", price, $"carSell({vData.Model})");
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы продали {vData.Model} ({vnumber}) за {price}$", 3000);
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
                            HouseManager.acceptRoomInvite(player);
                            return;
                        case "RENT_CAR":
                            Rentcar.RentCar(player);
                            return;
                        case "DEATH_CONFIRM":
                            Fractions.Realm.Ems.DeathConfirm(player, true);
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
                            Fractions.Realm.Ems.DeathConfirm(player, false);
                            return;
                        case "TICKET":
                            Fractions.FractionCommands.ticketConfirm(player, false);
                            return;
                    }
                }
            }
            catch (Exception e) { Log.Write($"dialogCallback ({callback} yes: {yes}): " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("SellHomeCallback")]
        public void RemoteEvent_SellHomeCallback(Player player, string callback, bool yes)
        {
            try
            {
                if (yes)
                {
                    switch (callback)
                    {

                        case "HOUSE_SELL":
                            Houses.HouseManager.acceptHouseSell(player);
                            return;
                        case "HOUSE_SELL_TOGOV":
                            Houses.HouseManager.acceptHouseSellToGov(player);
                            return;
                    }
                }

            }
            catch (Exception e) { Log.Write($"SellHomeCallback ({callback} yes: {yes}): " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("playerPressCuffBut")]
        public void ClientEvent_playerPressCuffBut(Player player, params object[] arguments)
        {
            try
            {
                Fractions.FractionCommands.playerPressCuffBut(player);
                return;
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("cuffUpdate")]
        public void ClientEvent_cuffUpdate(Player player, params object[] arguments)
        {
            try
            {
                NAPI.Player.PlayPlayerAnimation(player, 49, "mp_arresting", "idle");
                return;
            }
            catch (Exception e) { Log.Write("cuffUpdate: " + e.Message, Nlogs.Type.Error); }
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

            Database.Init();

            try
            {
                oldconfig = new OldConfig
                {
                    ServerName = config.TryGet<string>("ServerName", "RP"),
                    ServerNumber = config.TryGet<string>("ServerNumber", "0"),
                    RemoteControl = config.TryGet<bool>("RemoteControl", false),
                    DonateChecker = config.TryGet<bool>("DonateChecker", false),
                    DonateSaleEnable = config.TryGet<bool>("Donation_Sale", false),
                    PaydayMultiplier = config.TryGet<int>("PaydayMultiplier", 1),
                    ExpMultiplier = config.TryGet<int>("ExpMultipler", 1),
                    SCLog = config.TryGet<bool>("SCLog", false),
                };
                Finance.Donations.LoadDonations();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(0);
            }

            Settings.Timers.Init();
            Loggings.Start();
            ReportSys.Init();
            Fractions.Realm.LSNews.Init();
            EventSys.Init();
            Fractions.ElectionsSystem.OnResourceStart();
            _ = new List<string>()
            {
                "torso",
                "head",
                "leftarm",
                "rightarm",
                "leftleg",
                "rightleg",
            };
        }
        public static void saveDatabase()
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
            HouseManager.SavingHouses();
            Log.Debug("Houses Saved");
            FurnitureManager.Save();
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
        private static string Env_lastWeather = config.TryGet<string>("Weather", "XMAS"); // CLEAR
        public static bool SCCheck = config.TryGet<bool>("SocialClubCheck", false);
        public static void changeWeather(byte id)
        {
            try
            {
                Env_lastWeather = id switch
                {
                    0 => "EXTRASUNNY",
                    1 => "CLEAR",
                    2 => "CLOUDS",
                    3 => "SMOG",
                    4 => "FOGGY",
                    5 => "OVERCAST",
                    6 => "RAIN",
                    7 => "THUNDER",
                    8 => "CLEARING",
                    9 => "NEUTRAL",
                    10 => "SNOW",
                    11 => "BLIZZARD",
                    12 => "SNOWLIGHT",
                    _ => "EXTRASUNNY",
                };
                NAPI.World.SetWeather(Env_lastWeather);
                ClientEventToAll("Enviroment_Weather", Env_lastWeather);
            }
            catch
            {
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
                    if (rndWeather < 75)
                    {
                        if (rndWeather < 60) newWeather = "EXTRASUNNY";
                        else newWeather = "CLEAR";
                        NextWeatherChange = DateTime.Now.AddMinutes(120);
                    }
                    else
                    {
                        if (rndWeather < 90) newWeather = "RAIN";
                        else newWeather = "FOGGY";
                        NextWeatherChange = DateTime.Now.AddMinutes(rnd.Next(15, 70));
                    }
                }

                if (newWeather != Env_lastWeather)
                {
                    Env_lastWeather = newWeather;
                    ClientEventToAll("Enviroment_Weather", newWeather);
                }
            }
            catch (Exception e) { Log.Write($"enviromentChangeTrigger: {e}"); }
        }
        private static void playedMinutesTrigger()
        {
            try
            {
                if (!oldconfig.SCLog)
                {
                    DateTime now = DateTime.Now;
                    if (now.Hour == 4)
                    {
                        if (now.Minute == 5) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, в 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 10) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, напоминаем, что в 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 15) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, напоминаем, что в 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 20)
                        {
                            NAPI.Chat.SendChatMessageToAll("!{#DF5353}[AUTO RESTART] Дорогие игроки, сейчас произойдёт автоматическая перезагрузка сервера, сервер будет доступен вновь примерно в течении 2-5 минут.");
                            Admin.stopServer("Автоматическая перезагрузка");
                        }
                        else if (now.Minute == 21)
                        {
                            if (!Admin.IsServerStoping)
                            {
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

                        #region D2U Bonussystem
                        if (!Players[p].IsBonused)
                        {
                            if (Players[p].LastBonus < oldconfig.LastBonusMin) //todo bonus
                            {
                                Players[p].LastBonus++;
                            }
                            else
                            {
                                Random rnd = new Random();
                                int type = rnd.Next(0, nInventory.PresentsTypes.Count);
                                nInventory.Add(p, new nItem(ItemType.Present, 1, type));
                                Plugins.Notice.Send(p, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, "Вы получили 20 донат валюты и подарок, за 2 часа онлайна сегодня", 3000);
                                Players[p].LastBonus = 0;
                                Players[p].IsBonused = true;
                                Accounts[p].Coins += 20;
                                Plugins.Trigger.ClientEvent(p, "updlastbonus", $"следующий бонус можно получить только завтра");
                                return;
                            }
                            DateTime date = new DateTime((new DateTime().AddMinutes(oldconfig.LastBonusMin - Players[p].LastBonus)).Ticks);
                            var hour = date.Hour;
                            var min = date.Minute;
                            Plugins.Trigger.ClientEvent(p, "updlastbonus", $"Eжедневный подарок: Через {hour}ч. {min}м.");
                        }
                        #endregion

                    }
                    catch (Exception e) { Log.Write($"PlayedMinutesTrigger: " + e.Message, Nlogs.Type.Error); }
                }
            }
            catch (Exception e) { Log.Write($"playerMinutesTrigger: {e}"); }
        }
        private static readonly Random rndf = new Random();
        public static int pluscost = rndf.Next(10, 20);
        public static void payDayTrigger()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (DateTime.Now.Hour == 19)
                    {
                        try
                        {
                            foreach (var item in Modules.Containers.containers)
                            {
                                item.Visible(true);
                            }
                            NAPI.Chat.SendChatMessageToAll("!{#fc4615} [Порт]: !{#ffffff}" + "В штат привезли новую партию контейнеров!");
                        }
                        catch (Exception e)
                        {
                            Log.Write($"Ошибка контейнеров: {e.Message}", Nlogs.Type.Error);
                        }
                    }

                    Fractions.Realm.Cityhall.lastHourTax = 0;
                    Fractions.Realm.Ems.HumanMedkitsLefts = 100;
                    Modules.Forbes.SyncMajors();
                    Working.FarmerJob.Market.marketmultiplier = rnd.Next(15, 30);
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
                                    Hotel.MoveOutPlayer(player);
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Вас выселили из отеля за неуплату", 3000);
                                }
                            }

                            if (Players[player].LastHourMin < 15)
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны наиграть хотя бы 15 минут, чтобы получить пейдей", 3000);
                                continue;
                            }

                            switch (Fractions.Manager.FractionTypes[Players[player].FractionID])
                            {
                                case -1:
                                case 0:
                                case 1:
                                    if (Players[player].WorkID != 0) break;
                                    int payment = Convert.ToInt32((100 * oldconfig.PaydayMultiplier) + (Group.GroupAddPayment[Accounts[player].VipLvl] * oldconfig.PaydayMultiplier));
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы получили пособие по безработице {payment}$", 3000);
                                    Finance.Wallet.Change(player, payment);
                                    Loggings.Money($"server", $"player({Players[player].UUID})", payment, $"allowance");
                                    break;
                                case 2:
                                    payment = Convert.ToInt32((Fractions.Configs.FractionRanks[Players[player].FractionID][Players[player].FractionLVL].Item4 * oldconfig.PaydayMultiplier) + (Group.GroupAddPayment[Accounts[player].VipLvl] * oldconfig.PaydayMultiplier));
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы получили зарплату в {payment}$", 3000);
                                    Finance.Wallet.Change(player, payment);
                                    Loggings.Money($"server", $"player({Players[player].UUID})", payment, $"payday");
                                    break;
                            }

                            Players[player].EXP += 1 * Group.GroupEXP[Accounts[player].VipLvl] * oldconfig.ExpMultiplier;
                            if (Players[player].EXP >= 3 + Players[player].LVL * 3)
                            {
                                Players[player].EXP = Players[player].EXP - (3 + Players[player].LVL * 3);
                                Players[player].LVL += 1;
                                if (Players[player].LVL == 1)
                                {
                                    NAPI.Task.Run(() => { try { Plugins.Trigger.ClientEvent(player, "disabledmg", false); } catch { } }, 5000);
                                }
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Поздравляем, у Вас новый уровень ({Players[player].LVL})!", 3000);
                                if (Players[player].LVL == 1 && Accounts[player].PromoCodes[0] != "noref" && PromoCodes.ContainsKey(Accounts[player].PromoCodes[0]))
                                {
                                    if (!Accounts[player].PresentGet)
                                    {
                                        Accounts[player].PresentGet = true;
                                        string promo = Accounts[player].PromoCodes[0];
                                        Finance.Wallet.Change(player, 2000);
                                        Loggings.Money($"server", $"player({Players[player].UUID})", 2000, $"promo_{promo}");
                                        Customization.AddClothes(player, ItemType.Hat, 44, 3);
                                        nInventory.Add(player, new nItem(ItemType.Sprunk, 3));
                                        nInventory.Add(player, new nItem(ItemType.Сrisps, 3));

                                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Поздравляем, Вы получили награду за достижение 1 уровня по промокоду {promo}!", 3000);

                                        try
                                        {
                                            bool isGiven = false;
                                            foreach (Player pl in Players.Keys.ToList())
                                            {
                                                if (Players.ContainsKey(pl) && Players[pl].UUID == PromoCodes[promo].Item3)
                                                {
                                                    Finance.Wallet.Change(pl, 2000);
                                                    Plugins.Notice.Send(pl, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы получили $2000 за достижение 1 уровня игроком {player.Name}", 2000);
                                                    isGiven = true;
                                                    break;
                                                }
                                            }
                                            if (!isGiven) Database.Query($"UPDATE characters SET money=money+2000 WHERE uuid={PromoCodes[promo].Item3}");
                                        }
                                        catch { }
                                    }
                                    else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Этот аккаунт уже получал подарок за активацию промокода", 5000);
                                }
                            }

                            Players[player].LastHourMin = 0;

                            if (Accounts[player].VipLvl > 0 && Accounts[player].VipDate <= DateTime.Now)
                            {
                                Accounts[player].VipLvl = 0;
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "С вас снят VIP статус", 3000);
                            }

                            Dashboard.sendStats(player);
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_Player_{player.Name}\":\n" + e.ToString(), Nlogs.Type.Error); }
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
                            Fractions.Realm.Cityhall.lastHourTax += tax;

                            Loggings.Money($"biz({biz.ID})", "frac(6)", tax, "bizTaxHour");

                            if (Finance.Bank.Accounts[biz.BankID].Balance >= 0) continue;

                            string owner = biz.Owner;
                            if (PlayerNames.ContainsValue(owner))
                            {
                                Player player = NAPI.Player.GetPlayerFromName(owner);

                                if (player != null && Players.ContainsKey(player))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Государство отобрало у Вас бизнес за неуплату налогов", 3000);
                                    Finance.Wallet.Change(player, Convert.ToInt32(biz.SellPrice * 0.8));
                                    Players[player].BizIDs.Remove(biz.ID);
                                }
                                else
                                {
                                    string[] split = owner.Split('_');
                                    DataTable data = Database.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                    if (data != null)
                                    {
                                        List<int> ownerBizs = new List<int>();

                                        foreach (DataRow Row in data.Rows)
                                        {
                                            ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                        }

                                        ownerBizs.Remove(biz.ID);
                                        Database.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money=money+{Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                    }
                                }
                                Loggings.Money($"server", $"player({PlayerUUIDs[biz.Owner]})", Convert.ToInt32(biz.SellPrice * 0.8), $"bizTax");
                            }

                            Finance.Bank.Accounts[biz.BankID].Balance = 0;
                            biz.Owner = "Государство";
                            biz.UpdateLabel();
                        }
                        catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_PayDayTrigger_Business\":\n" + e.ToString(), Nlogs.Type.Error); }
                    }
                    foreach (House h in HouseManager.Houses)
                    {
                        try
                        {
                            if (!config.TryGet<bool>("housesTax", true)) return;
                            if (h.Owner == string.Empty) continue;

                            int tax = Convert.ToInt32(h.Price / 100 * 0.013);
                            Finance.Bank.Accounts[h.BankID].Balance -= tax;
                            Fractions.Stocks.fracStocks[6].Money += tax;
                            Fractions.Realm.Cityhall.lastHourTax += tax;

                            Loggings.Money($"house({h.ID})", "frac(6)", tax, "houseTaxHour");

                            if (Finance.Bank.Accounts[h.BankID].Balance >= 0) continue;

                            string owner = h.Owner;
                            Player player = NAPI.Player.GetPlayerFromName(owner);

                            if (player != null && Players.ContainsKey(player))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "У Вас отобрали дом за неуплату налогов", 3000);
                                Finance.Wallet.Change(player, Convert.ToInt32(h.Price / 2.0));
                                Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 333);
                                Plugins.Trigger.ClientEvent(player, "deleteGarageBlip");
                            }
                            else
                            {
                                string[] split = owner.Split('_');
                                Database.Query($"UPDATE characters SET money=money+{Convert.ToInt32(h.Price / 2.0)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                            }
                            h.SetOwner(null);
                            Loggings.Money($"server", $"player({PlayerUUIDs[owner]})", Convert.ToInt32(h.Price / 2.0), $"houseTax");
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_House_{h.Owner}\":\n" + e.ToString(), Nlogs.Type.Error); }
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
                catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_PayDayTrigger\":\n" + e.ToString(), Nlogs.Type.Error); }
            });
        }
        #region SMS
        public static void OpenContacts(Player client)
        {
            if (!Players.ContainsKey(client)) return;
            Character acc = Players[client];

            Menu menu = new Menu("contacts", false, true)
            {
                Callback = callback_sms
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Контакты"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button)
            {
                Text = "Позвонить"
            };
            menu.Add(menuItem);

            if (acc.Contacts != null)
            {
                foreach (KeyValuePair<int, string> c in acc.Contacts)
                {
                    menuItem = new Menu.Item(c.Key.ToString(), Menu.MenuItem.Button)
                    {
                        Text = c.Value
                    };
                    menu.Add(menuItem);
                }
            }

            menuItem = new Menu.Item("add", Menu.MenuItem.Button)
            {
                Text = "Добавить номер"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
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
                    Plugins.Trigger.ClientEvent(player, "openInput", $"Новый контакт", "Номер игрока", 7, "smsadd");
                    return;
                }
                else if (item.ID == "call")
                {
                    MenuManager.Close(player);
                    Plugins.Trigger.ClientEvent(player, "openInput", $"Позвонить", "Номер телефона", 7, "numcall");
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

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT SMS:\n" + e.ToString(), Nlogs.Type.Error);
            }
        }
        public static void OpenContactData(Player client, string Number, string Name)
        {
            Menu menu = new Menu("smsdata", false, true)
            {
                Callback = callback_smsdata
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = Number
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("name", Menu.MenuItem.Card)
            {
                Text = Name
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("send", Menu.MenuItem.Button)
            {
                Text = "Написать"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button)
            {
                Text = "Позвонить"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("rename", Menu.MenuItem.Button)
            {
                Text = "Переименовать"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("remove", Menu.MenuItem.Button)
            {
                Text = "Удалить"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
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
                    Plugins.Trigger.ClientEvent(player, "openInput", $"SMS для {num}", "Введите сообщение", 100, "smssend");
                    break;
                case "call":
                    if (!SimCards.ContainsKey(num))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрока с таким номером не найдено", 3000);
                        return;
                    }
                    Player target = GetPlayerByUUID(SimCards[num]);
                    Modules.Voice.PhoneCallCommand(player, target);
                    break;
                case "rename":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Переименование", $"Введите новое имя для {num}", 18, "smsname");
                    break;
                case "remove":
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{num} удален из контактов.", 4000);
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
                Plugins.Trigger.ClientEvent(p, eventName, args);
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
            foreach (Player player in Players.Keys.ToList())
            {
                if (!Players.ContainsKey(player)) continue;
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
                Plugins.Trigger.ClientEvent(target, "setFollow", true, player);
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
                VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");
                if (playerPhoneMeta.CallingState != "callMe" && playerPhoneMeta.Target != null)
                {
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.11, 0.03, -0.01), new Vector3(85, -15, 120));
                }
            }
        }
        #region InputMenu
        public static void OpenInputMenu(Player player, string title, string func)
        {
            Menu menu = new Menu("inputmenu", false, false)
            {
                Callback = callback_inputmenu
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = title
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("inp", Menu.MenuItem.Input)
            {
                Text = "*******"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item(func, Menu.MenuItem.Button)
            {
                Text = "ОК"
            };
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
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
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
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
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
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
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
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    if (Convert.ToInt32(text) < 1)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите корректные данные", 3000);
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
            Menu menu = new Menu("mainmenu", false, false)
            {
                Callback = callback_mainmenu
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = ""
            };
            menu.Add(menuItem);

            if (oldconfig.VoIPEnabled)
            {
                VoicePhoneMetaData vpmd = player.GetData<VoicePhoneMetaData>("PhoneVoip");
                if (vpmd.Target != null)
                {
                    if (vpmd.CallingState == "callMe")
                    {
                        menuItem = new Menu.Item("acceptcall", Menu.MenuItem.Button)
                        {
                            Scale = 1,
                            Color = Menu.MenuColor.Green,
                            Text = "Принять вызов"
                        };
                        menu.Add(menuItem);
                    }

                    string text = (vpmd.CallingState == "callMe") ? "Отклонить вызов" : (vpmd.CallingState == "callTo") ? "Отменить вызов" : "Завершить вызов";
                    menuItem = new Menu.Item("endcall", Menu.MenuItem.Button)
                    {
                        Scale = 1,
                        Text = text
                    };
                    menu.Add(menuItem);
                }
            }

            menuItem = new Menu.Item("gps", Menu.MenuItem.gpsBtn)
            {
                Column = 2,
                Text = "Навигатор"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("contacts", Menu.MenuItem.contactBtn)
            {
                Column = 2,
                Text = "Контакты"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("services", Menu.MenuItem.servicesBtn)
            {
                Column = 2,
                Text = "Сервисы"
            };
            menu.Add(menuItem);

            if (Players[player].BizIDs.Count > 0)
            {
                menuItem = new Menu.Item("biz", Menu.MenuItem.businessBtn)
                {
                    Column = 2,
                    Text = "Бизнес"
                };
                menu.Add(menuItem);
            }

            if (Players[player].FractionID > 0)
            {
                menuItem = new Menu.Item("frac", Menu.MenuItem.grupBtn)
                {
                    Column = 2,
                    Text = "Фракция"
                };
                menu.Add(menuItem);
            }

            if (Fractions.Manager.isLeader(player, 6))
            {
                menuItem = new Menu.Item("citymanage", Menu.MenuItem.businessBtn)
                {
                    Column = 2,
                    Text = "Управление"
                };
                menu.Add(menuItem);
            }

            if (Players[player].HotelID != -1)
            {
                menuItem = new Menu.Item("hotel", Menu.MenuItem.hotelBtn)
                {
                    Column = 2,
                    Text = "Отель"
                };
                menu.Add(menuItem);
            }

            if (Players[player].LVL < 50)
            {
                menuItem = new Menu.Item("promo", Menu.MenuItem.promoBtn)
                {
                    Column = 2,
                    Text = "Промо"
                };
                menu.Add(menuItem);
            }

            if (HouseManager.GetHouse(player, true) != null)
            {
                menuItem = new Menu.Item("house", Menu.MenuItem.homeBtn)
                {
                    Column = 2,
                    Text = "Мой дом"
                };
                menu.Add(menuItem);
            }
            else if (HouseManager.GetHouse(player) != null && HouseManager.GetHouse(player, true) == null)
            {
                menuItem = new Menu.Item("openhouse", Menu.MenuItem.Button)
                {
                    Text = "Открыть/Закрыть Дом"
                };
                menu.Add(menuItem);

                menuItem = new Menu.Item("leavehouse", Menu.MenuItem.Button)
                {
                    Text = "Выселиться из дома"
                };
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("ad", Menu.MenuItem.ilanBtn)
            {
                Text = "Обьявления"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("forb", Menu.MenuItem.forbBtn)
            {
                Text = "Рейтинг"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn)
            {
                Text = "Выход"
            };
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
                    HouseManager.OpenHouseManageMenu(player);
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
                    Hotel.OpenHotelManageMenu(player);
                    return;
                case "contacts":
                    if (Players[player].Sim == -1)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет сим-карты", 3000);
                        return;
                    }
                    OpenContacts(player);
                    return;
                case "ad":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Объявление", "6$ за каждые 20 символов", 100, "make_ad");
                    return;
                case "openhouse":
                    {
                        House house = HouseManager.GetHouse(player);
                        house.SetLock(!house.Locked);
                        if (house.Locked) Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы закрыли дом", 3000);
                        else Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы открыли дом", 3000);
                        return;
                    }
                case "leavehouse":
                    {
                        House house = HouseManager.GetHouse(player);
                        if (house == null)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы не живете в доме", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
                        Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 333);
                        Plugins.Trigger.ClientEvent(player, "deleteGarageBlip");
                    }
                    return;

                case "forb":
                    Modules.Forbes.OpenForbes(player);
                    return;

                case "promo":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Промокод", "Введите промокод", 10, "enter_promocode");
                    return;
                case "acceptcall":
                    Modules.Voice.PhoneCallAcceptCommand(player);
                    return;
                case "endcall":
                    Modules.Voice.PhoneHCommand(player);
                    return;
            }
        }
        private static readonly List<string> MoneyPromos = new List<string>() { };
        private static readonly Dictionary<string, List<string>> Category = new Dictionary<string, List<string>>()
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
                "Ближайший банк",
                "Ближайший банкомат",
                "Ближайшая заправка",
                "Ближайший 24/7",
                "Ближайшая аренда авто",
                "Ближайшая остановка",
            }},
        };
        private static readonly Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>()
        {
            { "Мэрия", new Vector3(-535.6117,-220.598,0) },
            { "LSPD", new Vector3(424.4417,-980.3409,0) },
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
            Menu menu = new Menu("gps", false, false)
            {
                Callback = callback_gps
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = cat
            };
            menu.Add(menuItem);

            foreach (string next in Category[cat])
            {
                menuItem = new Menu.Item(next, Menu.MenuItem.Button)
                {
                    Text = next
                };
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
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
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", Points[item.ID].X, Points[item.ID].Y);
                    return;
                case "Ближайший банк":
                    Vector3 waypoint = Finance.Branch.GetNearestBRANCH(player);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайший банкомат":
                    waypoint = Finance.ATM.GetNearestATM(player);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая заправка":
                    waypoint = BusinessManager.getNearestBiz(player, 1);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайший 24/7":
                    waypoint = BusinessManager.getNearestBiz(player, 0);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая аренда авто":
                    waypoint = Rentcar.GetNearestRentArea(player.Position);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая остановка":
                    waypoint = Working.Bus.GetNearestStation(player.Position);
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
            }
        }
        public static void OpenServicesMenu(Player player)
        {
            Menu menu = new Menu("services", false, false)
            {
                Callback = callback_services
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Вызовы"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("taxi", Menu.MenuItem.Button)
            {
                Text = "Вызвать такси"
            };
            menu.Add(menuItem);

            Menu.Item item = new Menu.Item("repair", Menu.MenuItem.Button);
            menuItem = item;
            menuItem.Text = "Вызвать механика";
            menu.Add(menuItem);

            menuItem = new Menu.Item("police", Menu.MenuItem.Button)
            {
                Text = "Вызвать полицию"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("sheriff", Menu.MenuItem.Button)
            {
                Text = "Вызвать Шерифа"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("ems", Menu.MenuItem.Button)
            {
                Text = "Вызвать EMS"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
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
                    Plugins.Trigger.ClientEvent(player, "openInput", "Вызвать полицию", "Что произошло?", 30, "call_police");
                    return;
                case "sheriff":
                    MenuManager.Close(player);
                    Plugins.Trigger.ClientEvent(player, "openInput", "Вызвать Шерифа", "Что произошло?", 30, "call_sheriff");
                    return;
                case "ems":
                    MenuManager.Close(player);
                    Fractions.Realm.Ems.callEms(player);
                    return;
                case "back":
                    _ = OpenPlayerMenu(player);
                    return;
            }
        }
        public static void OpenMayorMenu(Player player)
        {
            Menu menu = new Menu("citymanage", false, false)
            {
                Callback = callback_mayormenu
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Казна"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info", Menu.MenuItem.Card)
            {
                Text = $"Деньги: {Fractions.Stocks.fracStocks[6].Money}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info2", Menu.MenuItem.Card)
            {
                Text = $"Собрано за последний час: {Fractions.Realm.Cityhall.lastHourTax}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button)
            {
                Text = "Получить деньги"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("put", Menu.MenuItem.Button)
            {
                Text = "Положить деньги"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("header2", Menu.MenuItem.Header)
            {
                Text = "Управление"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("fuelcontrol", Menu.MenuItem.Button)
            {
                Text = "Гос.заправка"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_mayormenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "take":
                    MenuManager.Close(player);
                    Plugins.Trigger.ClientEvent(player, "openInput", "Получить деньги из казны", "Количество", 6, "mayor_take");
                    return;
                case "put":
                    MenuManager.Close(player);
                    Plugins.Trigger.ClientEvent(player, "openInput", "Положить деньги в казну", "Количество", 6, "mayor_put");
                    return;
                case "fuelcontrol":
                    OpenFuelcontrolMenu(player);
                    return;
                case "back":
                    _ = OpenPlayerMenu(player);
                    return;
            }
        }
        public static void OpenFuelcontrolMenu(Player player)
        {
            Menu menu = new Menu("fuelcontrol", false, false)
            {
                Callback = callback_fuelcontrol
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Гос.заправка"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_city", Menu.MenuItem.Card)
            {
                Text = $"Мэрия. Осталось сегодня: {Fractions.Stocks.fracStocks[6].FuelLeft}/{Fractions.Stocks.fracStocks[6].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_city", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_police", Menu.MenuItem.Card)
            {
                Text = $"Полиция. Осталось сегодня: {Fractions.Stocks.fracStocks[7].FuelLeft}/{Fractions.Stocks.fracStocks[7].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_police", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_ems", Menu.MenuItem.Card)
            {
                Text = $"EMS. Осталось сегодня: {Fractions.Stocks.fracStocks[8].FuelLeft}/{Fractions.Stocks.fracStocks[8].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_ems", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_fib", Menu.MenuItem.Card)
            {
                Text = $"FIB. Осталось сегодня: {Fractions.Stocks.fracStocks[9].FuelLeft}/{Fractions.Stocks.fracStocks[9].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_fib", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_army", Menu.MenuItem.Card)
            {
                Text = $"Армия. Осталось сегодня: {Fractions.Stocks.fracStocks[14].FuelLeft}/{Fractions.Stocks.fracStocks[14].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_army", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_news", Menu.MenuItem.Card)
            {
                Text = $"News. Осталось сегодня: {Fractions.Stocks.fracStocks[15].FuelLeft}/{Fractions.Stocks.fracStocks[15].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_news", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_sheriff", Menu.MenuItem.Card)
            {
                Text = $"Полиция. Осталось сегодня: {Fractions.Stocks.fracStocks[18].FuelLeft}/{Fractions.Stocks.fracStocks[18].FuelLimit}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("setsheriff", Menu.MenuItem.Button)
            {
                Text = "Установить лимит"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_fuelcontrol(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "set_city":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для мэрии в долларах", 5, "fuelcontrol_city");
                    return;
                case "set_police":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит полиции мэрии в долларах", 5, "fuelcontrol_police");
                    return;
                case "set_sheriff":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит Шерифа мэрии в долларах", 5, "fuelcontrol_sheriff");
                    return;
                case "set_ems":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для EMS в долларах", 5, "fuelcontrol_ems");
                    return;
                case "set_fib":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для FIB в долларах", 5, "fuelcontrol_fib");
                    return;
                case "set_army":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для армии в долларах", 5, "fuelcontrol_army");
                    return;
                case "set_news":
                    Plugins.Trigger.ClientEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для News в долларах", 5, "fuelcontrol_news");
                    return;
                case "back":
                    OpenMayorMenu(player);
                    return;
            }
        }
        #endregion
        #endregion
    }
}
