using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Threading;

namespace iTeffa.Globals
{
    public class Loggings
    {
        private static Thread thread;
        private static readonly Nlogs Log = new Nlogs("iTeffa - Game Logs");
        private static readonly Queue<string> queue = new Queue<string>();
        private static readonly Dictionary<int, DateTime> OnlineQueue = new Dictionary<int, DateTime>();
        private static readonly string insert = "insert into {0}({1}) values ({2})";
        public static void Votes(uint ElectionId, string Login, string VoteFor)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_vote", "`election`,`login`,`votefor`,`time`", $"'{ElectionId}','{Login}','{VoteFor}','{DateTime.Now:s}'"));
        }
        public static void Stock(int Frac, int Uuid, string Type, int Amount, bool In)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_stock", "`time`,`frac`,`uuid`,`type`,`amount`,`in`", $"'{DateTime.Now:s}',{Frac},{Uuid},'{Type}',{Amount},{In}"));
        }
        public static void Admin(string Admin, string Action, string Player)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_admins", "`time`,`admin`,`action`,`player`", $"'{DateTime.Now:s}','{Admin}','{Action}','{Player}'"));
        }
        public static void Money(string From, string To, long Amount, string Comment)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_money", "`time`,`from`,`to`,`amount`,`comment`", $"'{DateTime.Now:s}','{From}','{To}',{Amount},'{Comment}'"));
        }
        public static void Items(string From, string To, int Type, int Amount, string Data)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_items", "`time`,`from`,`to`,`type`,`amount`,`data`", $"'{DateTime.Now:s}','{From}','{To}',{Type},{Amount},'{Data}'"));
        }
        public static void Name(int Uuid, string Old, string New)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_name", "`time`,`uuid`,`old`,`new`", $"'{DateTime.Now:s}',{Uuid},'{Old}','{New}'"));
        }

        public static void Ban(int Admin, int Player, DateTime Until, string Reason, bool isHard)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_ban", "`time`,`admin`,`player`,`until`,`reason`,`ishard`", $"'{DateTime.Now:s}',{Admin},{Player},'{Until:s}','{Reason}',{isHard}"));
        }
        public static void Ticket(int player, int target, int sum, string reason, string pnick, string tnick)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_ticket", "`time`,`player`,`target`,`sum`,`reason`,`pnick`,`tnick`", $"'{DateTime.Now:s}',{player},{target},{sum},'{reason}','{pnick}','{tnick}'"));
        }
        public static void Arrest(int player, int target, string reason, int stars, string pnick, string tnick)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_arrest", "`time`,`player`,`target`,`reason`,`stars`,`pnick`,`tnick`", $"'{DateTime.Now:s}',{player},{target},'{reason}',{stars},'{pnick}','{tnick}'"));
        }
        public static void Connected(string Name, int Uuid, string SClub, string Hwid, int Id, string ip)
        {
            if (thread == null || OnlineQueue.ContainsKey(Uuid)) return;
            DateTime now = DateTime.Now;
            if (ip.Equals("80.235.53.64")) ip = "31.13.190.88";
            queue.Enqueue(string.Format(insert, "logs_conn", "`in`,`out`,`uuid`,`sclub`,`hwid`,`ip`", $"'{now:s}',null,'{Uuid}','{SClub}','{Hwid}','{ip}'"));
            queue.Enqueue(string.Format(insert, "logs_id", "`in`,`out`,`uuid`,`id`,`name`", $"'{now:s}',null,'{Uuid}','{Id}','{Name}'"));
            OnlineQueue.Add(Uuid, now);
        }
        public static void Disconnected(int Uuid)
        {
            if (thread == null || !OnlineQueue.ContainsKey(Uuid)) return;
            DateTime conn = OnlineQueue[Uuid];
            if (conn == null) return;
            OnlineQueue.Remove(Uuid);
            queue.Enqueue($"update logs_conn set `out`='{DateTime.Now:s}' WHERE `in`='{conn:s}' and `uuid`={Uuid}");
        }
        public static void CharacterDelete(string name, int uuid, string account)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(
                insert, "logs_delete", "`time`,`uuid`,`name`,`account`", $"'{DateTime.Now:s}',{uuid},'{name}','{account}'"));
        }
        public static void EventLogAdd(string AdmName, string EventName, ushort MembersLimit, string Started)
        {
            if (thread == null) return;
            queue.Enqueue(string.Format(insert, "logs_events", "`AdminStarted`,`EventName`,`MembersLimit`,`Started`", $"'{AdmName}','{EventName}','{MembersLimit}','{Started}'"));
        }
        public static void EventLogUpdate(string AdmName, int MembCount, string WinName, uint Reward, string Time, uint RewardLimit, ushort MemLimit, string EvName)
        {
            if (thread == null) return;
            queue.Enqueue($"update logs_events set `AdminClosed`='{AdmName}',`Members`={MembCount},`Winner`='{WinName},`Reward`={Reward},`Ended`='{Time}',`RewardLimit`={RewardLimit} WHERE `Winner`='Undefined' AND `MembersLimit`={MemLimit} AND `EventName`='{EvName}'");
        }

        public static void Start()
        {
            thread = new Thread(Worker) { IsBackground = true };
            thread.Start();
        }

        private static void Worker()
        {
            string CMD = "";
            try
            {
                Log.Debug("Worker started");
                while (true)
                {
                    if (queue.Count < 1) continue;
                    else
                        Connect.Query(queue.Dequeue());
                }
            }
            catch (Exception e)
            {
                Log.Write($"{e}\n{CMD}", Nlogs.Type.Error);
            }
        }
        public static void Stop()
        {
            thread.Join();
        }
    }
}
