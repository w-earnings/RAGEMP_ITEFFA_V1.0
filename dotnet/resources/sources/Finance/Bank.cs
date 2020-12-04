using GTANetworkAPI;
using System;
using System.Collections.Generic;
using iTeffa.Globals;
using iTeffa.Settings;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace iTeffa.Finance
{
    class Bank : Script
    {
        private static readonly nLog Log = new nLog("BankSystem");
        private static readonly Random Rnd = new Random();
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
                Data data = new Data
                {
                    ID = Convert.ToInt32(Row["id"]),
                    Type = Convert.ToInt32(Row["type"]),
                    Holder = Row["holder"].ToString(),
                    Balance = Convert.ToInt64(Row["balance"])
                };
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
                Log.Write($"Transfer with error. Account does not exist! [{firstAccID}->{lastAccID}:{amount}]", nLog.Type.Warn);
                return false;
            }
            if (!Change(firstAccID, -amount))
            {
                if (firstAcc.Type == 1)
                    BankNotify(NAPI.Player.GetPlayerFromName(firstAcc.Holder), BankNotifyType.PayError, "Недостаточно средств!");
                Log.Write($"Transfer with error. Insufficient funds! [{firstAccID}->{lastAccID}:{amount}]", nLog.Type.Warn);
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
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Ошибка ввода", 3000);
                    return;
                case BankNotifyType.PayError:
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Ошибка списания средств", 3000);
                    return;
                case BankNotifyType.PayIn:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Поступление средств ({info}$)", 3000);
                    return;
                case BankNotifyType.PayOut:
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Списание средств ({info}$)", 3000);
                    return;
            }
        }
        public static int Create(string holder, int type = 1, long balance = 0)
        {
            int id = GenerateUUID();
            Data data = new Data
            {
                ID = id,
                Type = type,
                Holder = holder,
                Balance = balance
            };
            Accounts.Add(id, data);
            Connect.Query($"INSERT INTO `money`(`id`, `type`, `holder`, `balance`) VALUES ({id},{type},'{holder}',{balance})");
            Log.Write("Created new Bank Account! ID:" + id.ToString(), nLog.Type.Success);
            return id;
        }
        public static void Remove(int id, string holder)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "DELETE FROM `money` WHERE holder=@pn"
            };
            cmd.Parameters.AddWithValue("@pn", holder);
            Connect.Query(cmd);
            Log.Write("Bank account deleted! ID:" + id, nLog.Type.Warn);
        }
        public static void RemoveByID(int id)
        {
            if (!Accounts.ContainsKey(id)) return;
            Accounts.Remove(id);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "DELETE FROM `money` WHERE id=@pn";
            cmd.Parameters.AddWithValue("@pn", id);
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
            int result;
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
}
