﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using iTeffa.Kernel;
using iTeffa.Settings;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using MySqlConnector;

namespace iTeffa.Finance
{
    class Bank : Script
    {
        private static nLog Log = new nLog("BankSystem");
        private static Random Rnd = new Random();

        public static Dictionary<int, Data> Accounts = new Dictionary<int, Data>();
        public static ICollection<int> BankAccKeys = Accounts.Keys;

        public enum BankNotifyType
        {
            PaySuccess,
            PayIn,
            PayOut,
            PayError,
            InputError,
        }
        public Bank()
        {
            Log.Write("Loading Bank Accounts...");
            var result = Connect.QueryRead("SELECT * FROM `money`");
            if (result == null || result.Rows.Count == 0)
            {
                Log.Write("DB return null result.", nLog.Type.Warn);
                return;
            }
            foreach (DataRow Row in result.Rows)
            {
                Data data = new Data();
                data.ID = Convert.ToInt32(Row["id"]);
                data.Type = Convert.ToInt32(Row["type"]);
                data.Holder = Row["holder"].ToString();
                data.Balance = Convert.ToInt64(Row["balance"]);
                Accounts.Add(Convert.ToInt32(Row["id"]), data);
            }
        }

        #region Changing account balance
        public static bool Change(int accountID, long amount, bool notify = true)
        {
            lock (Accounts)
            {
                if (!Accounts.ContainsKey(accountID)) return false;
                if (Accounts[accountID].Balance + amount < 0) return false;
                Accounts[accountID].Balance += amount;
                if (Accounts[accountID].Type == 1 || amount >= 10000) Connect.Query($"UPDATE `money` SET balance={Accounts[accountID].Balance} WHERE id={accountID}");
                if (Accounts[accountID].Type == 1 && NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder) != null)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (notify)
                            {
                                if (amount > 0)
                                    BankNotify(NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder), BankNotifyType.PayIn, amount.ToString());
                                else
                                    BankNotify(NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder), BankNotifyType.PayOut, amount.ToString());
                            }
                            NAPI.Player.GetPlayerFromName(Accounts[accountID].Holder).TriggerEvent("UpdateBank", Accounts[accountID].Balance);
                        }
                        catch { }
                    });
                }
                return true;
            }
        }
        #endregion Changing account balance
        #region Transfer money from 1-Acc to 2-Acc
        public static bool Transfer(int firstAccID, int lastAccID, long amount)
        {
            if (firstAccID == 0 || lastAccID == 0)
            {
                Log.Write($"Account ID error [{firstAccID}->{lastAccID}]", nLog.Type.Error);
                return false;
            }
            Data firstAcc = Accounts[firstAccID];
            if (!Accounts.ContainsKey(lastAccID))
            {
                if (firstAcc.Type == 1)
                    BankNotify(NAPI.Player.GetPlayerFromName(firstAcc.Holder), BankNotifyType.InputError, "Такого счета не существует!");
                Log.Write($"Transfer with error. Account does not exist! [{firstAccID.ToString()}->{lastAccID.ToString()}:{amount.ToString()}]", nLog.Type.Warn);
                return false;
            }
            if (!Change(firstAccID, -amount))
            {
                if (firstAcc.Type == 1)
                    BankNotify(NAPI.Player.GetPlayerFromName(firstAcc.Holder), BankNotifyType.PayError, "Недостаточно средств!");
                Log.Write($"Transfer with error. Insufficient funds! [{firstAccID.ToString()}->{lastAccID.ToString()}:{amount.ToString()}]", nLog.Type.Warn);
                return false;
            }
            Change(lastAccID, amount);
            GameLog.Money($"bank({firstAccID})", $"bank({lastAccID})", amount, "bankTransfer");
            return true;
        }
        #endregion Transfer money from 1-Acc to 2-Acc
        #region Save Acc
        public static void Save(int AccID)
        {
            if (!Accounts.ContainsKey(AccID)) return;
            Data acc = Accounts[AccID];
            Connect.Query($"UPDATE `money` SET `balance`={acc.Balance}, `holder`='{acc.Holder}' WHERE id={AccID}");
        }
        #endregion Save Acc

        public static void BankNotify(Player player, BankNotifyType type, string info)
        {
            switch (type)
            {
                case BankNotifyType.InputError:
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка ввода", 3000);
                    return;
                case BankNotifyType.PayError:
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка списания средств", 3000);
                    return;
                case BankNotifyType.PayIn:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поступление средств ({info}$)", 3000);
                    return;
                case BankNotifyType.PayOut:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Списание средств ({info}$)", 3000);
                    return;
            }
        }

        public static int Create(string holder, int type = 1, long balance = 0)
        {
            int id = GenerateUUID();
            Data data = new Data();
            data.ID = id;
            data.Type = type;
            data.Holder = holder;
            data.Balance = balance;
            Accounts.Add(id, data);
            Connect.Query($"INSERT INTO `money`(`id`, `type`, `holder`, `balance`) VALUES ({id},{type},'{holder}',{balance})");
            Log.Write("Created new Bank Account! ID:" + id.ToString(), nLog.Type.Success);
            return id;
        }
        public static void Remove(int id, string holder)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM `money` WHERE holder=@pn";
            cmd.Parameters.AddWithValue("@pn",holder);
            Connect.Query(cmd);
            Log.Write("Bank account deleted! ID:" + id, nLog.Type.Warn);
        }
        public static void RemoveByID(int id)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM `money` WHERE id=@pn";
            cmd.Parameters.AddWithValue("@pn",id);
            Connect.Query(cmd);
            Log.Write("Bank account deleted! ID:" + id, nLog.Type.Warn);
        }
        public static bool isAccExist(int id)
        {
            return Accounts.ContainsKey(id);
        }

        public static Data Get(string holder)
        {
            return Accounts.FirstOrDefault(A => A.Value.Holder == holder).Value;
        }

        public static Data Get(int id)
        {
            return Accounts.FirstOrDefault(A => A.Value.ID == id).Value;
        }

        public static void Update(Player client)
        {
            NAPI.Task.Run(() =>
            {
                Trigger.ClientEvent(client, "UpdateBank", Get(client.Name).Balance);
            });
        }

        private static int GenerateUUID()
        {
            var result = 0;
            while (true)
            {
                result = Rnd.Next(000001, 999999);
                if (!BankAccKeys.Contains(result)) break;
            }
            return result;
        }

        public static void changeHolder(string oldName, string newName)
        {
            List<int> toChange = new List<int>();
            lock (Accounts)
            {
                foreach (KeyValuePair<int, Data> bank in Accounts)
                {
                    if (bank.Value.Holder != oldName) continue;
                    Log.Debug($"The bank was found! [{bank.Key}]");
                    toChange.Add(bank.Key);
                }
                foreach (int id in toChange)
                {
                    Accounts[id].Holder = newName;
                    Save(id);
                }
            }
        }

        internal class Data
        {
            public int ID { get; set; }
            public int Type { get; set; }
            public string Holder { get; set; }
            public long Balance { get; set; }
        }
    }

    class ATM : Script
    {
        private static nLog Log = new nLog("ATMs");

        public static Dictionary<int, ColShape> ATMCols = new Dictionary<int, ColShape>();



        #region Координаты банкоматов
        public static List<Vector3> ATMs = new List<Vector3>
        {
            #region Центр Вайнвуда
            new Vector3(285.53067, 143.57167, 104.17261),    // Бульвар Вайнвуда
            new Vector3(158.64749, 234.20576, 106.62715),    // Бульвар Вайнвуда
            #region Альта-Стрит: Публичный депозитный банк
            new Vector3(236.52216, 219.62317, 106.28677),    // Банкомат 1
            new Vector3(236.65250, 218.55400, 106.28677),    // Банкомат 2
            new Vector3(237.43347, 217.81444, 106.28677),    // Банкомат 3
            new Vector3(237.82574, 216.91905, 106.28677),    // Банкомат 4
            new Vector3(238.30281, 215.92021, 106.28677),    // Банкомат 5
            #endregion Альта-Стрит: Публичный депозитный банк
            new Vector3(380.77730, 323.45004, 103.56638),    // 24/7 Клинтон-Авеню
            new Vector3(356.88345, 173.33870, 103.06791),    // Пауэр-стрит
            new Vector3(89.622280, 2.4169147, 68.303470),    // Спениш-авеню
            #endregion Центр Вайнвуда
            #region Пиллбол-Хилл
            new Vector3(-203.87813, -861.40500, 30.24224),    // Бульвар Веспуччи
            new Vector3(-301.71370, -830.05206, 32.41723),    // Бульвар Веспуччи
            new Vector3(-303.29224, -829.81160, 32.41723),    // Бульвар Веспуччи
            new Vector3(147.560790, -1035.7654, 29.34303),    // Бульвар Веспуччи
            new Vector3(145.972980, -1035.0717, 29.34490),    // Бульвар Веспуччи
            new Vector3(-254.34350, -692.49550, 33.60964),    // Писфул-стрит
            new Vector3(-256.14893, -716.03110, 33.51834),    // Писфул-стрит
            new Vector3(-258.85010, -723.31290, 33.44657),    // Писфул-стрит
            new Vector3(112.671130, -819.33124, 31.32895),    // Сан-Андрефс-авеню
            new Vector3(111.193950, -775.32180, 31.42597),    // Сан-Андрефс-авеню
            #endregion Пиллбол-Хилл
            #region Палето-Бэй
            new Vector3(-386.77127, 6046.096, 31.50172),      // Бульвар Палето
            new Vector3(155.93127, 6642.8486, 31.60124),      // Бульвар Палето
            new Vector3(-95.47697, 6457.1343, 31.46048),      // Бульвар Палето
            new Vector3(-97.20478, 6455.4487, 31.46658),      // Бульвар Палето
            new Vector3(-282.9777, 6226.0070, 31.49306),      // Дулуоз-авеню
            #endregion Палето-Бэй
            #region Коньон Бенхэм
            new Vector3(-2975.013, 380.14206, 14.99330),      // Шоссе Грейт-Оушт
            new Vector3(-3044.035, 594.54770, 7.736011),      // Инесеню-роуд
            new Vector3(-2959.016, 487.74033, 15.463908),     // Шоссе Грейт-Оушн
            new Vector3(-2956.903, 487.63123, 15.463908),     // Шоссе Грейт-Оушн
            new Vector3(-3040.7854, 593.0608, 7.908930),      // 24/7 Инесено-роуд
            #endregion Коньон Бенхэм
            #region Рокфорд-Хилз
            new Vector3(-1205.8297, -324.696, 37.8614),       // Бульвар Дель-Перро
            new Vector3(-867.65436, -186.06395, 37.793724),   // Мэд-Узйн-Тандер-Драйв
            new Vector3(-866.6908, -187.71054, 37.842434),    // Мэд-Узйн-Тандер-Драйв
            new Vector3(-1410.357, -98.73505, 52.42969),      // Кугар-авеню
            new Vector3(-1409.749, -100.5033, 52.38365),      // Кугар-авеню
            new Vector3(-846.29785, -341.23773, 38.68024),    // Херитедж-вэй
            new Vector3(-846.80444, -340.1224, 38.68024),     // Херитедж-вэй
            #endregion Рокфорд-Хилз
            #region Ла-Луэрта
            new Vector3(-594.5639, -1161.282, 22.32425),      // Кале-авеню
            new Vector3(-596.0082, -1161.285, 22.32425),      // Кале-авеню
            #endregion 
            #region Мальнький Сеул
            new Vector3(-526.67114, -1222.95, 18.454979),     // Кале-Авеню
            new Vector3(-537.8721, -854.4221, 29.280344),     // Бульвар Веспуччи
            new Vector3(-611.8429, -704.7579, 31.214891),     // Паломино-авеню
            new Vector3(-614.578, -704.8344, 31.235937),      // Паломино-авеню
            new Vector3(-717.67883, -915.7043, 19.215591),    // 24/7 Джинджер-стрит
            #endregion Мальнький Сеул
            #region Пасифик-Блаффс
            new Vector3(-2072.3975, -317.25632, 13.315971),   // Шоссе День-Перро
            new Vector3(-2293.9229, 354.84770, 174.60162),    // Кортц-драйв
            new Vector3(-2294.7053, 356.46756, 174.60162),    // Кортц-драйв
            new Vector3(-2295.4675, 358.18607, 174.60162),    // Кортц-драйв
            #endregion Пасифик-Блаффс
            #region Чумаш
            new Vector3(-3240.647, 1008.6462, 12.83071),      // 24/7 Барбареню-роуд
            new Vector3(-3241.1804, 997.57855, 12.550388),    // Барбарено-роуд
            new Vector3(-3144.3506, 1127.6162, 20.855108),    // Грейт-Оушн
            #endregion Чумаш
            #region Строберри
            new Vector3(33.217453, -1348.2437, 29.49661),     // Бульвар Инносенс
            new Vector3(288.84875, -1282.2858, 29.630487),    // Бульвар Капитал
            new Vector3(289.04147, -1256.8461, 29.440756),    //  Шоссе Олимпик
            #endregion Строберри
            #region Пустыня Гранд-Сенора
            new Vector3(1171.4905, 2702.5488, 38.16955),      // Шоссе 68
            new Vector3(1172.5038, 2702.5803, 38.17475),      // Шоссе 68
            new Vector3(2683.0625, 3286.583, 55.122955),      // 24/7 Шоссе Сенора
            #endregion Пустыня Гранд-Сенора
            #region Сэнди-Щорс
            new Vector3(1822.6438, 3683.0999, 34.276554),     // Занкудо-авеню
            new Vector3(1968.1697, 3743.618, 32.335087),      // Альгамбра-драйв
            #endregion Сэнди-Щорс
            #region Западный Вайнвуд
            new Vector3(-165.16734, 232.73517, 94.92187),     // Бульвар Эклипс
            new Vector3(-165.10814, 234.78175, 94.92187),     // Бульвар Эклипс
            #endregion Западный Вайнвуд
            #region Пиллбокс-Хилл:
            new Vector3(-30.261902, -723.6828, 44.22823),     // Сан-Фндрефс-авеню
            new Vector3(-27.988794, -724.5387, 44.228912),    // Сан-Фндрефс-авеню
            #endregion Пиллбокс-Хилл:
            #region Миррор-Парк:
            new Vector3(1167.0012, -456.0833, 66.79836),      // Бульвар Миррор-Парк
            new Vector3(1138.2677, -468.9984, 66.729324),     // Бульвар Миррор-Парк
            #endregion Миррор-Парк:
            #region Татавиамские горы
            new Vector3(2558.747, 350.9753, 108.61537),       // Шоссе Паломино
            new Vector3(2558.478, 389.43405, 108.61779),      // 24/7 Шоссе Паломино
            #endregion Татавиамские горы
            #region Грейпсид
            new Vector3(1702.9719, 4933.574, 42.063435),      // 24/7 Грейпсид Мэйн-стрит
            new Vector3(1686.8506, 4815.7886, 42.008186),     // Грейпсид Мэйн-стрит
            #endregion Грейпсид
            #region Гора Чилиад
            new Vector3(1701.1953, 6426.514, 32.76405),       // Шоссе Сенора
            new Vector3(1735.249, 6410.5015, 35.03004),       // 24/7 Шоссе Сенора
            #endregion Гора Чилиад
            #region Морнингвуд
            new Vector3(-1430.1453, -211.12401, 46.499557),    // Кугар-авеню
            new Vector3(-1415.9736, -212.0589, 46.500378),     // Бульвар Морнингвуд
            #endregion Морнингвуд
            #region Дель-Перро
            new Vector3(-1285.7983, -572.6369, 29.454794), // Cityhall | Саут-Рокфорд-Драйв
            #endregion Дель-Перро 
            #region Без категории
            new Vector3(-57.71136, -92.68571, 57.78078),      // Бертон: Хавик-авеню
            new Vector3(527.27936, -160.7215, 57.08959),      // Хавик: Элгин-авеню
            new Vector3(-1091.497, 2708.6533, 18.95074),      // Река Занкудо: Шоссе 68
            new Vector3(540.33310, 2671.1143, 42.15650),      // Хармони: 24/7 Шоссе 68
            new Vector3(-56.85246, -1752.1791, 29.420998),    // Дэвис: Гроув-стрит
            new Vector3(2564.518, 2584.744, 38.07698),        // Дэвис-Кварц: Сенора-вэй
            new Vector3(-1109.824, -1690.783, 4.375022),      // Веспуччи-Бич: Бэй-Сити-авеню
            new Vector3(-821.6050, -1081.882, 11.13241),      // Каналы Веспуччи: Паломино-авеню
            #endregion Без категории 
        };
        #endregion Координаты банкоматов



        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Log.Write("Loading ATMs...");
                for (int i = 0; i < ATMs.Count; i++)
                {
                    ATMCols.Add(i, NAPI.ColShape.CreateCylinderColShape(ATMs[i], 1, 2, 0));
                    ATMCols[i].SetData("NUMBER", i);
                    ATMCols[i].OnEntityEnterColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 13);
                            Working.Collector.CollectorEnterATM(e, s);
                        }
                        catch (Exception ex) { Log.Write("ATMCols.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    ATMCols[i].OnEntityExitColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("ATMCols.OnEntityExitrColShape: " + ex.Message, nLog.Type.Error); }
                    };
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static Vector3 GetNearestATM(Player player)
        {
            Vector3 nearesetATM = ATMs[0];
            foreach (var v in ATMs)
            {
                if (v == new Vector3(237.3785, 217.7914, 106.2868)) continue;
                if (player.Position.DistanceTo(v) < player.Position.DistanceTo(nearesetATM))
                    nearesetATM = v;
            }
            return nearesetATM;
        }

        public static void OpenATM(Player player)
        {
            var acc = Main.Players[player];
            if (acc.Bank == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Зарегистрируйте счет в ближайшем отделении банка", 3000);
                return;
            }
            long balance = Bank.Accounts[acc.Bank].Balance;
            Trigger.ClientEvent(player, "setatm", acc.Bank.ToString(), player.Name, balance.ToString(), "");
            Trigger.ClientEvent(player, "openatm");
            return;
        }

        public static void AtmBizGen(Player player)
        {
            var acc = Main.Players[player];
            Log.Debug("Biz count : " + acc.BizIDs.Count);
            if (acc.BizIDs.Count > 0)
            {
                List<string> data = new List<string>();
                foreach (int key in acc.BizIDs)
                {
                    Business biz = BusinessManager.BizList[key];
                    string name = BusinessManager.BusinessTypeNames[biz.Type];
                    data.Add($"{name}");
                }
                Trigger.ClientEvent(player, "atmOpenBiz", JsonConvert.SerializeObject(data), "");
            }
            else
            {
                Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет бизнеса!", 3000);
            }
        }

        [RemoteEvent("atmVal")]
        public static void ClientEvent_ATMVAL(Player player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                var acc = Main.Players[player];
                int type = NAPI.Data.GetEntityData(player, "ATMTYPE");
                string data = Convert.ToString(args[0]);
                int amount;
                if (!Int32.TryParse(data, out amount))
                    return;
                switch (type)
                {
                    case 0:
                        Trigger.ClientEvent(player, "atmClose");
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, +Math.Abs(amount));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"atmIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1:
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, +Math.Abs(amount));
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"atmOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2:
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"atmHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player,
                                "atmOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                        break;
                    case 3:
                        int bid = NAPI.Data.GetEntityData(player, "ATMBIZ");

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"atmBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player, "atmOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        break;
                    case 4:
                        if (!Bank.Accounts.ContainsKey(amount) || amount <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        NAPI.Data.SetEntityData(player, "ATM2ACC", amount);
                        Trigger.ClientEvent(player,
                                "atmOpen", "[2,0,'Сумма для перевода']");
                        NAPI.Data.SetEntityData(player, "ATMTYPE", 44);
                        break;
                    case 44:
                        if (Main.Players[player].LVL < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перевод денег доступен после первого уровня", 3000);
                            return;
                        }
                        if (player.HasData("NEXT_BANK_TRANSFER") && DateTime.Now < player.GetData<DateTime>("NEXT_BANK_TRANSFER"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Следующая транзакция будет возможна в течение минуты", 3000);
                            return;
                        }
                        int bank = NAPI.Data.GetEntityData(player, "ATM2ACC");
                        if (!Bank.Accounts.ContainsKey(bank) || bank <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        if(Bank.Accounts[bank].Type != 1 && Main.Players[player].AdminLVL == 0) {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        if(acc.Bank == bank) {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Операция отменена.", 3000);
                            Trigger.ClientEvent(player, "closeatm");
                            return;
                        }
                        Bank.Transfer(acc.Bank, bank, Math.Abs(amount));
                        Trigger.ClientEvent(player, "closeatm");
                        if(Main.Players[player].AdminLVL == 0) player.SetData("NEXT_BANK_TRANSFER", DateTime.Now.AddMinutes(1));
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("atmDP")]
        public static void ClientEvent_ATMDupe(Player client)
        {
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 3)
                {
                    p.SendChatMessage($"!{{#d35400}}[ATM-FLOOD] {client.Name} ({client.Value})");
                }
            }
        }

        [RemoteEvent("atmCB")]
        public static void ClientEvent_ATMCB(Player player, params object[] args)
        {
            try
            {
                var acc = Main.Players[player];
                int type = Convert.ToInt32(args[0]);
                int index = Convert.ToInt32(args[1]);
                if (index == -1)
                {
                    Trigger.ClientEvent(player, "closeatm");
                    return;
                }
                switch (type)
                {
                    case 1:
                        switch (index)
                        {
                            case 0:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Сумма внесения наличных']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 1:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Сумма для снятия']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 2:
                                if (Houses.HouseManager.GetHouse(player, true) == null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет дома!", 3000);
                                    return;
                                }
                                var house = Houses.HouseManager.GetHouse(player, true);
                                Trigger.ClientEvent(player,
                                    "atmOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                                Trigger.ClientEvent(player, "setatm", "Дом", $"Дом #{house.ID}", Bank.Accounts[house.BankID].Balance, "");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 3:
                                AtmBizGen(player);
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;
                            case 4:
                                Trigger.ClientEvent(player,
                                    "atmOpen", "[2,0,'Счет зачисления']");
                                NAPI.Data.SetEntityData(player, "ATMTYPE", index);
                                break;

                        }
                        break;
                    case 2:
                        Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                        Trigger.ClientEvent(player, "setatm", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                        break;
                    case 3:
                        if (index == -1)
                        {
                            Trigger.ClientEvent(player, "atmOpen", "[1,0,0]");
                            Trigger.ClientEvent(player, "setatm", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                            return;
                        }
                        Business biz = BusinessManager.BizList[acc.BizIDs[index]];
                        NAPI.Data.SetEntityData(player, "ATMBIZ", index);
                        Trigger.ClientEvent(player, "atmOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        Trigger.ClientEvent(player, "setatm",
                            "Бизнес",
                            BusinessManager.BusinessTypeNames[biz.Type],
                            Bank.Accounts[biz.BankID].Balance, "");
                        break;

                }
            }
            catch (Exception e) { Log.Write("atmCB: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("atm")]
        public static void ClientEvent_ATM(Player player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                int act = Convert.ToInt32(args[0]);
                string data1 = Convert.ToString(args[1]);
                var acc = Main.Players[player];
                int amount;
                if (!Int32.TryParse(data1, out amount))
                    return;
                Log.Debug($"{player.Name} : {data1}");
                switch (act)
                {
                    case 0: //put money
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, amount);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"atmIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1: //take money
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, amount);
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"atmOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2: //put house tax
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"atmHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        break;
                    case 3: //put biz tax
                        var check = NAPI.Data.GetEntityData(player, "bizcheck");
                        if (check == null) return;
                        if (acc.BizIDs.Count != check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }
                        int bid = 0;
                        if (!Int32.TryParse(Convert.ToString(args[2]), out bid))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.01) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"atmBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        break;
                    case 4: //transfer to
                        int num = 0;
                        if (!Int32.TryParse(Convert.ToString(args[2]), out num))
                            return;
                        Bank.Transfer(acc.Bank, num, +Math.Abs(amount));
                        break;
                }
            }
            catch (Exception e) { Log.Write("atm: " + e.Message, nLog.Type.Error); }
        }
    }
}