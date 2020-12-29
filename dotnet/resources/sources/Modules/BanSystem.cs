using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace iTeffa.Modules
{
    class BanSystem : Models.BanData
    {
        private static readonly List<BanSystem> Banned = new List<BanSystem>();
        private static readonly Nlogs Log = new Nlogs("BanSystem");
        public static void Sync()
        {
            lock (Banned)
            {
                Banned.Clear();
                DataTable result = Globals.Database.QueryRead("select * from banned");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow row in result.Rows)
                {
                    Banned.Add(new BanSystem()
                    {
                        UUID = Convert.ToInt32(row["uuid"]),
                        Name = Convert.ToString(row["name"]),
                        Account = Convert.ToString(row["account"]),
                        Time = Convert.ToDateTime(row["time"]),
                        Until = Convert.ToDateTime(row["until"]),
                        isHard = Convert.ToBoolean(row["ishard"]),
                        IP = Convert.ToString(row["ip"]),
                        SocialClub = Convert.ToString(row["socialclub"]),
                        HWID = Convert.ToString(row["hwid"]),
                        Reason = Convert.ToString(row["reason"]),
                        ByAdmin = Convert.ToString(row["byadmin"])
                    });
                }
            }
        }
        public static BanSystem Get1(Player client)
        {
            lock (Banned)
            {
                BanSystem ban = null;
                if (client.HasData("RealSocialClub"))
                {
                    ban = Banned.FindLast(x => x.SocialClub == client.GetData<string>("RealSocialClub"));
                    if (ban != null) return ban;
                }
                ban = Banned.FindLast(x => x.IP == client.Address);
                if (ban != null) return ban;
                if (client.HasData("RealHWID")) ban = Banned.FindLast(x => x.HWID == client.GetData<string>("RealHWID"));
                return ban;
            }
        }
        public static BanSystem Get2(int UUID)
        {
            lock (Banned)
            {
                BanSystem ban = null;
                ban = Banned.Find(x => x.UUID == UUID);
                return ban;
            }
        }
        public bool CheckDate()
        {
            if (DateTime.Now <= Until)
            {
                return true;
            }
            else
            {
                Globals.Database.Query($"DELETE FROM banned WHERE uuid={this.UUID}");
                lock (Banned)
                {
                    Banned.Remove(this);
                }
                return false;
            }
        }
        public static void Online(Player client, DateTime until, bool ishard, string reason, string admin)
        {
            var acc = Main.Players[client];
            if (acc == null)
            {
                Log.Write($"Can't ban player {client.Name}", Nlogs.Type.Error);
                return;
            }
            BanSystem ban = new BanSystem()
            {
                UUID = acc.UUID,
                Name = acc.FirstName + "_" + acc.LastName,
                Account = Main.Accounts[client].Login,
                Time = DateTime.Now,
                Until = until,
                isHard = ishard,
                IP = client.Address,
                SocialClub = client.GetData<string>("RealSocialClub"),
                HWID = client.GetData<string>("RealHWID"),
                Reason = reason,
                ByAdmin = admin
            };
            Globals.Database.Query("INSERT INTO `banned`(`uuid`,`name`,`account`,`time`,`until`,`ishard`,`ip`,`socialclub`,`hwid`,`reason`,`byadmin`) " +
                $"VALUES ({ban.UUID},'{ban.Name}','{ban.Account}','{Globals.Database.ConvertTime(ban.Time)}','{Globals.Database.ConvertTime(ban.Until)}',{ban.isHard},'{ban.IP}','{ban.SocialClub}','{ban.HWID}','{ban.Reason}','{ban.ByAdmin}')");
            Banned.Add(ban);
        }
        public static void UpdateBan(int uuid)
        {
            var ban = Banned.FirstOrDefault(b => b.UUID == uuid);
            if (ban == null) return;

            Globals.Database.Query($"UPDATE `banned` SET `account`='{ban.Account}',`hwid`='{ban.HWID}' WHERE `uuid`={uuid}");
        }
        public static bool Offline(string nickname, DateTime until, bool ishard, string reason, string admin)
        {
            if (Banned.FirstOrDefault(b => b.Name == nickname) != null) return false;
            if (!Main.PlayerUUIDs.ContainsKey(nickname)) return false;
            var uuid = Main.PlayerUUIDs[nickname];
            if (uuid == -1) return false;

            var ip = "";
            var socialclub = "";
            var account = "";
            var hwid = "";

            if (ishard)
            {
                DataTable result = Globals.Database.QueryRead($"SELECT `hwid`,`socialclub`,`ip`,`login` FROM `accounts` WHERE `character1`={uuid} OR `character2`={uuid} OR `character3`={uuid}");
                var row = result.Rows[0];
                if (result == null || result.Rows.Count == 0) return false;
                ip = row["ip"].ToString();
                socialclub = row["socialclub"].ToString();
                account = row["login"].ToString();
                hwid = row["hwid"].ToString();
            }

            BanSystem ban = new BanSystem()
            {
                UUID = uuid,
                Name = nickname,
                Account = account,
                Time = DateTime.Now,
                Until = until,
                isHard = ishard,
                IP = ip,
                SocialClub = socialclub,
                HWID = hwid,
                Reason = reason,
                ByAdmin = admin
            };
            Globals.Database.Query("INSERT INTO `banned`(`uuid`,`name`,`account`,`time`,`until`,`ishard`,`ip`,`socialclub`,`hwid`,`reason`,`byadmin`) " +
                $"VALUES ({ban.UUID},'{ban.Name}','{ban.Account}','{Globals.Database.ConvertTime(ban.Time)}','{Globals.Database.ConvertTime(ban.Until)}',{ban.isHard},'{ban.IP}','{ban.SocialClub}','{ban.HWID}','{ban.Reason}','{ban.ByAdmin}')");
            Banned.Add(ban);
            return true;
        }
        public static bool PardonHard(string nickname)
        {
            lock (Banned)
            {
                int index = Banned.FindIndex(x => x.Name == nickname);
                if (index < 1) return false;

                Banned[index].isHard = false;
                Globals.Database.Query($"UPDATE banned SET ishard={false} WHERE name='{nickname}'");
                return true;
            }
        }
        public static bool PardonHard(int uuid)
        {
            lock (Banned)
            {
                int index = Banned.FindIndex(x => x.UUID == uuid);
                if (index < 1) return false;

                Banned[index].isHard = false;
                Globals.Database.Query($"UPDATE banned SET ishard={false} WHERE uuid={uuid}");
                return true;
            }
        }
        public static bool Pardon(string nickname)
        {
            lock (Banned)
            {
                int index = Banned.FindIndex(x => x.Name == nickname);
                if (index < 0) return false;

                Banned.RemoveAt(index);
                Globals.Database.Query($"DELETE FROM banned WHERE name='{nickname}'");
                return true;
            }
        }
        public static bool Pardon(int uuid)
        {
            lock (Banned)
            {
                int index = Banned.FindIndex(x => x.UUID == uuid);
                if (index < 0) return false;
                Banned.RemoveAt(index);
                Globals.Database.Query($"DELETE FROM banned WHERE uuid={uuid}");
                return true;
            }
        }
    }
}
