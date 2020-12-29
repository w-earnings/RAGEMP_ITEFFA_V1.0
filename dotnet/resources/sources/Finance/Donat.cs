using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using GTANetworkAPI;
using MySqlConnector;
using iTeffa.Globals;
using iTeffa.Settings;
using iTeffa.Interface;
using iTeffa.Globals.nAccount;
using iTeffa.Globals.Character;

namespace iTeffa.Finance
{
    class Donations : Script
    {
        public static Queue<KeyValuePair<string, string>> toChange = new Queue<KeyValuePair<string, string>>();
        public static Queue<string> newNames = new Queue<string>();
        private static readonly Nlogs Log = new Nlogs("Donations");
        private static Timer scanTimer;
        private static string SYNCSTR;
        private static string CHNGSTR;
        private static string NEWNSTR;
        private static string Connection;

        public static void LoadDonations()
        {
            Connection = "SERVER=localhost;PORT=;DATABASE=;UID=;PASSWORD=;SSL Mode=;pooling =;convert zero datetime=";
            SYNCSTR = string.Format("select * from completed where srv={0}", Main.oldconfig.ServerNumber);
            CHNGSTR = "update nicknames SET name='{0}' WHERE name='{1}' and srv={2}";
            NEWNSTR = "insert into nicknames(srv, name) VALUES ({0}, '{1}')";
        }
        #region Работа с таймером
        public static void Start()
        {
            scanTimer = new Timer(new TimerCallback(Tick), null, 90000, 90000);
        }

        public static void Stop()
        {
            scanTimer.Change(Timeout.Infinite, 0);
        }
        #endregion
        #region Проверка никнеймов и донатов
        private static void Tick(object state)
        {
            try
            {
                Log.Debug("Donate time");
                using MySqlConnection connection = new MySqlConnection(Connection);
                connection.Open();
                MySqlCommand command = new MySqlCommand
                {
                    Connection = connection
                };
                while (toChange.Count > 0)
                {
                    KeyValuePair<string, string> kvp = toChange.Dequeue();
                    command.CommandText = string.Format(CHNGSTR, kvp.Value, kvp.Key, Main.oldconfig.ServerNumber);
                    command.ExecuteNonQuery();
                }
                while (newNames.Count > 0)
                {
                    string nickname = newNames.Dequeue();
                    command.CommandText = string.Format(NEWNSTR, Main.oldconfig.ServerNumber, nickname);
                    command.ExecuteNonQuery();
                }
                command.CommandText = SYNCSTR;
                MySqlDataReader reader = command.ExecuteReader();

                DataTable result = new DataTable();
                result.Load(reader);
                reader.Close();
                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"]);
                    string name = Convert.ToString(Row["account"]).ToLower();
                    long reds = Convert.ToInt64(Row["amount"]);
                    try
                    {
                        if (Main.oldconfig.DonateSaleEnable)
                        {
                            reds = SaleEvent(reds);
                        }
                        if (!Main.Usernames.Contains(name))
                        {
                            Log.Write($"Can't find registred name for {name}!", Nlogs.Type.Warn);
                            continue;
                        }
                        var client = Main.Accounts.FirstOrDefault(a => a.Value.Login == name).Key;
                        if (client == null || client.IsNull || !Main.Accounts.ContainsKey(client))
                        {
                            Database.Query($"update `accounts` set `coins`=`coins`+{reds} where `login`='{name}'");
                        }
                        else
                        {
                            lock (Main.Players)
                            {
                                Main.Accounts[client].Coins += reds;
                            }
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    if (!Main.Accounts.ContainsKey(client)) return;
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вам пришли {reds} Coins", 3000);
                                    Trigger.ClientEvent(client, "starset", Main.Accounts[client].Coins);
                                }
                                catch { }
                            });
                        }
                        Loggings.Money("server", name, reds, "donateRed");
                        command.CommandText = $"delete from completed where id={id}";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Log.Write($"Exception At Tick_Donations on {name}:\n" + e.ToString(), Nlogs.Type.Error);
                    }
                }
                connection.Close();
            }
            catch (Exception e)
            {
                Log.Write("Exception At Tick_Donations:\n" + e.ToString(), Nlogs.Type.Error);
            }
        }
        #endregion
        #region Действия в донат-меню
        internal enum Type
        {
            Character,     // 0
            Nickname,      // 1
            Convert,       // 2
            BronzeVIP,     // 3
            SilverVIP,     // 4
            GoldVIP,       // 5
            PlatinumVIP,   // 6
            Warn,          // 7
            Slot,          // 8
            GiveBox,       // 9
            WheelsRun,     //10
            Money1,        //11
            Money2,        //12
            Money3,        //13
            Money4,        //14
            Box1,          //15
            Box2,          //16
            Box3,          //17
            Box4,          //18
            Lic1,          //19
            Lic2,          //20
        }
        private static readonly SortedList<int, string> CarNameS = new SortedList<int, string>
        {
            {1, "Neon" },
            {2, "Sultan" },
            {3, "nero" },
            {4, "caracara2" },
        };
        private static readonly SortedList<int, string> CarName = new SortedList<int, string>
        {
            {3, "g65" },
            {4, "c63coupe" },
            {5, "apriora" },
            {6, "bmwe34" },
        };
        [RemoteEvent("wheelAddsrv")]
        public void wheelAdd(Player client, int id, bool add)
        {
            if (!Main.Accounts.ContainsKey(client)) return;
            switch (id)
            {
                case 0:
                    Wallet.Change(client, 50000);
                    Log.Write("Деньги пришли в размере 50 000", Nlogs.Type.Success);
                    break;
                case 1:
                    Wallet.Change(client, 100000);
                    Log.Write("Деньги пришли в размере 100 000", Nlogs.Type.Success);
                    break;
                case 2:
                    Wallet.Change(client, 150000);
                    Log.Write("Деньги пришли в размере 150 000", Nlogs.Type.Success);
                    break;
                case 3:
                    if (add)
                    {
                        var vNumber = VehicleManager.Create(client.Name, CarName[id], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                        var house = Houses.HouseManager.GetHouse(client, false);
                        if (house != null)
                        {
                            if (house.GarageID != 0)
                            {
                                var garage = Houses.GarageManager.Garages[house.GarageID];
                                if (VehicleManager.getAllPlayerVehicles(client.Name).Count < Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                {
                                    garage.SpawnCar(vNumber);
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"", 3000);
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    if (add)
                    {
                        var vNumber = VehicleManager.Create(client.Name, CarName[id], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                        var house = Houses.HouseManager.GetHouse(client, false);
                        if (house != null)
                        {
                            if (house.GarageID != 0)
                            {
                                var garage = Houses.GarageManager.Garages[house.GarageID];
                                if (VehicleManager.getAllPlayerVehicles(client.Name).Count < Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                {
                                    garage.SpawnCar(vNumber);
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"", 3000);
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    if (add)
                    {
                        var vNumber = VehicleManager.Create(client.Name, CarName[id], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                        var house = Houses.HouseManager.GetHouse(client, false);
                        if (house != null)
                        {
                            if (house.GarageID != 0)
                            {
                                var garage = Houses.GarageManager.Garages[house.GarageID];
                                if (VehicleManager.getAllPlayerVehicles(client.Name).Count < Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                {
                                    garage.SpawnCar(vNumber);
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"", 3000);
                                }
                            }
                        }
                    }
                    break;
                case 6:
                    if (add)
                    {
                        var vNumber = VehicleManager.Create(client.Name, CarName[id], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                        var house = Houses.HouseManager.GetHouse(client, false);
                        if (house != null)
                        {
                            if (house.GarageID != 0)
                            {
                                var garage = Houses.GarageManager.Garages[house.GarageID];
                                if (VehicleManager.getAllPlayerVehicles(client.Name).Count < Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                {
                                    garage.SpawnCar(vNumber);
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"", 3000);
                                }
                            }
                        }
                    }
                    break;
                case 7:
                    Weapons.GiveWeapon(client, ItemType.Bat, "donatejrp");
                    break;
                case 8:
                    Customization.AddClothes(client, ItemType.Mask, 159, 0);
                    break;
                case 9:
                    Main.Players[client].EXP += 10;
                    break;
                case 10:
                    Customization.AddClothes(client, ItemType.Top, 178, 0);
                    Customization.AddClothes(client, ItemType.Leg, 77, 0);
                    Customization.AddClothes(client, ItemType.Feet, 55, 0);
                    break;
                case 11:
                    Wallet.Change(client, 200000);
                    Log.Write("Деньги пришли в размере 200 000", Nlogs.Type.Success);
                    break;
                case 12:
                    Wallet.Change(client, 250000);
                    Log.Write("Деньги пришли в размере 250 000", Nlogs.Type.Success);
                    break;
                case 13:
                    Wallet.Change(client, 300000);
                    Log.Write("Деньги пришли в размере 300 000", Nlogs.Type.Success);
                    break;
                case 14:
                    Wallet.Change(client, 350000);
                    Log.Write("Деньги пришли в размере 350 000", Nlogs.Type.Success);
                    break;
                case 15:
                    Wallet.Change(client, 400000);
                    Log.Write("Деньги пришли в размере 400 000", Nlogs.Type.Success);
                    break;
                case 16:
                    Wallet.Change(client, 450000);
                    Log.Write("Деньги пришли в размере 450 000", Nlogs.Type.Success);
                    break;
                case 17:
                    Wallet.Change(client, 500000);
                    Log.Write("Деньги пришли в размере 500 000", Nlogs.Type.Success);
                    break;
            }
        }
        [RemoteEvent("donate")]
        public void MakeDonate(Player client, int id, string data)
        {
            try
            {
                Log.Write($"Data: {id} {data}");
                if (!Main.Accounts.ContainsKey(client)) return;
                Account acc = Main.Accounts[client];
                Type type = (Type)id;

                switch (type)
                {
                    case Type.WheelsRun:
                        {
                            if (Main.Accounts[client].Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateChar");
                            Trigger.ClientEvent(client, "WheelsRun");
                            break;
                        }
                    case Type.Character:
                        {
                            if (acc.Coins < 100)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 100;
                            Loggings.Money(acc.Login, "server", 100, "donateChar");
                            Customization.SendToCreator(client);
                            break;
                        }
                    case Type.Nickname:
                        {
                            if (acc.Coins < 25)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }

                            if (!Main.PlayerNames.ContainsValue(client.Name)) return;
                            try
                            {
                                string[] split = data.Split("_");
                                Log.Debug($"SPLIT: {split[0]} {split[1]}");

                                if (split[0] == "null" || string.IsNullOrEmpty(split[0]))
                                {
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы не указали имя!", 3000);
                                    return;
                                }
                                else if (split[1] == "null" || string.IsNullOrEmpty(split[1]))
                                {
                                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы не указали фамилию!", 3000);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Write("ERROR ON CHANGENAME DONATION\n" + e.ToString());
                                return;
                            }

                            if (Main.PlayerNames.ContainsValue(data))
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Такое имя уже существует!", 3000);
                                return;
                            }

                            Player target = NAPI.Player.GetPlayerFromName(client.Name);

                            if (target == null || target.IsNull) return;
                            else
                            {
                                Character.toChange.Add(client.Name, data);
                                Main.Accounts[client].Coins -= 25;
                                NAPI.Player.KickPlayer(target, "Смена ника");
                            }
                            Loggings.Money(acc.Login, "server", 25, "donateName");
                            break;
                        }
                    case Type.Convert:
                        {
                            if (!int.TryParse(data, out int amount))
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Возникла ошибка, попоробуйте еще раз", 3000);
                                return;
                            }
                            amount = Math.Abs(amount);
                            if (amount <= 0)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Введите количество, равное 1 или больше.", 3000);
                                return;
                            }
                            if (Main.Accounts[client].Coins < amount)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= amount;
                            Loggings.Money(acc.Login, "server", amount, "donateConvert");
                            amount *= 100;
                            Wallet.Change(client, +amount);
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно перевели RF в {amount}", 3000);
                            Loggings.Money($"donate", $"player({Main.Players[client].UUID})", amount, $"donate");
                            break;
                        }
                    case Type.BronzeVIP:
                        {
                            if (Main.Accounts[client].VipLvl >= 1)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.Coins < 300)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 300;
                            Loggings.Money(acc.Login, "server", 300, "donateBVip");
                            Main.Accounts[client].VipLvl = 1;
                            Main.Accounts[client].VipDate = DateTime.Now.AddDays(30);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.SilverVIP:
                        {
                            if (acc.VipLvl >= 1)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.Coins < 600)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 600;
                            Loggings.Money(acc.Login, "server", 600, "donateSVip");
                            Main.Accounts[client].VipLvl = 2;
                            Main.Accounts[client].VipDate = DateTime.Now.AddDays(30);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.GoldVIP:
                        {
                            if (Main.Accounts[client].VipLvl >= 1)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.Coins < 800)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 800;
                            Loggings.Money(acc.Login, "server", 800, "donateGVip");
                            Main.Accounts[client].VipLvl = 3;
                            Main.Accounts[client].VipDate = DateTime.Now.AddDays(30);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.PlatinumVIP:
                        {
                            if (Main.Accounts[client].VipLvl >= 1)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.Coins < 1000)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 1000;
                            Loggings.Money(acc.Login, "server", 1000, "donatePVip");
                            Main.Accounts[client].VipLvl = 4;
                            Main.Accounts[client].VipDate = DateTime.Now.AddDays(30);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Warn:
                        {
                            if (Main.Players[client].Warns <= 0)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У вас нет Warn'a!", 3000);
                                return;
                            }
                            if (acc.Coins < 250)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 250;
                            Loggings.Money(acc.Login, "server", 250, "donateWarn");
                            Main.Players[client].Warns -= 1;
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Slot:
                        {
                            Log.Debug("Unlock slot");
                            if (acc.Coins < 1000)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 1000;
                            Loggings.Money(acc.Login, "server", 1000, "donateSlot");

                            if (acc.VipLvl == 0)
                            {
                                Main.Accounts[client].VipLvl = 3;
                                Main.Accounts[client].VipDate = DateTime.Now.AddDays(30);
                            }
                            else if (acc.VipLvl <= 3)
                            {
                                Main.Accounts[client].VipLvl = 3;
                                Main.Accounts[client].VipDate = Main.Accounts[client].VipDate.AddDays(30);
                            }
                            else Main.Accounts[client].VipDate = Main.Accounts[client].VipDate.AddDays(30);

                            Main.Accounts[client].Characters[2] = -1;
                            Trigger.ClientEvent(client, "unlockSlot", Main.Accounts[client].Coins);
                            Database.Query($"update `accounts` set `coins`={Main.Accounts[client].Coins} where `login`='{Main.Accounts[client].Login}'");
                            return;
                        }
                    case Type.Money1:
                        {
                            if (acc.Coins < 2500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 2500;
                            Loggings.Money(acc.Login, "server", 2500, "donateMoney");
                            Wallet.Change(client, 100000);
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели $100 000", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Money2:
                        {
                            if (acc.Coins < 2500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 2500;
                            Loggings.Money(acc.Login, "server", 2500, "donateMoney");
                            Wallet.Change(client, 300000);
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели $300 000", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Money3:
                        {
                            if (acc.Coins < 2500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 2500;
                            Loggings.Money(acc.Login, "server", 2500, "donateMoney");
                            Wallet.Change(client, 500000);
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели $500 000", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Money4:
                        {
                            if (acc.Coins < 2500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 2500;
                            Loggings.Money(acc.Login, "server", 2500, "donateMoney");
                            Wallet.Change(client, 1000000);
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели $1 000 000", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Box1:
                        {
                            if (acc.Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }


                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateBox1");
                            Wallet.Change(client, 150000000);
                            Main.Players[client].Licenses[1] = true;
                            Main.Players[client].EXP += 10;
                            VehicleManager.Create(client.Name, CarNameS[1], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели Старт для начала набор", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Box2:
                        {
                            if (acc.Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateBox1");
                            Wallet.Change(client, 150000000);
                            Main.Players[client].Licenses[1] = true;
                            Main.Players[client].EXP += 15;
                            VehicleManager.Create(client.Name, CarNameS[2], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели Солидненько набор", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Box3:
                        {
                            if (acc.Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateBox1");
                            Wallet.Change(client, 150000000);
                            Main.Players[client].Licenses[1] = true;
                            Main.Players[client].EXP += 20;
                            VehicleManager.Create(client.Name, CarNameS[3], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели Солидненько набор", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Box4:
                        {
                            if (acc.Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateBox1");
                            Wallet.Change(client, 150000000);
                            Main.Players[client].Licenses[1] = true;
                            Main.Players[client].EXP += 25;
                            VehicleManager.Create(client.Name, CarNameS[4], new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели Золотые запасы набор", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Lic1:
                        {
                            if (acc.Coins < 500)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            if (Main.Players[client].Licenses[2])
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас уже есть легковая лицензия", 3000);
                                return;
                            }

                            Main.Accounts[client].Coins -= 500;
                            Loggings.Money(acc.Login, "server", 500, "donateBox1");
                            Main.Players[client].Licenses[1] = true;
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели легковую лицензию", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                    case Type.Lic2:
                        {
                            if (acc.Coins < 600)
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно Coins!", 3000);
                                return;
                            }
                            if (Main.Players[client].Licenses[2])
                            {
                                Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас уже есть грузовая лицензия", 3000);
                                return;
                            }
                            Main.Accounts[client].Coins -= 600;
                            Loggings.Money(acc.Login, "server", 600, "donateBox1");
                            Main.Players[client].Licenses[2] = true;
                            Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно приобрели грузовую лицензию", 3000);
                            Dashboard.sendStats(client);
                            break;
                        }
                }
                Database.Query($"update `accounts` set `coins`={Main.Accounts[client].Coins} where `login`='{Main.Accounts[client].Login}'");
                Trigger.ClientEvent(client, "redset", Main.Accounts[client].Coins);
            }
            catch (Exception e) { Log.Write("donate: " + e.Message, Nlogs.Type.Error); }
        }
        #endregion Действия в донат-меню
        public static long SaleEvent(long input)
        {
            if (input < 1000) return input;
            if (input < 3000) return input + (input / 100 * 20);
            if (input < 5000) return input + (input / 100 * 25);
            if (input < 10000) return input + (input / 100 * 30);
            if (input < 14000) return input + (input / 100 * 35);
            if (input >= 14000) return input + (input / 100 * 50);
            return input;
        }
        public static void Rename(string Old, string New)
        {
            toChange.Enqueue(
                new KeyValuePair<string, string>(Old, New));
        }
    }
}