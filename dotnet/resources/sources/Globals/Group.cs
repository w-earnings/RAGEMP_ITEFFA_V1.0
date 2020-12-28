using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using iTeffa.Settings;
using System.Linq;

namespace iTeffa.Globals
{
    public class Group
    {
        public static List<GroupCommand> GroupCommands = new List<GroupCommand>();
        public static void LoadCommandsConfigs()
        {
            DataTable result = Database.QueryRead($"SELECT * FROM adminaccess");
            if (result == null || result.Rows.Count == 0) return;
            List<GroupCommand> groupCmds = new List<GroupCommand>();
            foreach (DataRow Row in result.Rows)
            {
                string cmd = Convert.ToString(Row["command"]);
                bool isadmin = Convert.ToBoolean(Row["isadmin"]);
                int minrank = Convert.ToInt32(Row["minrank"]);

                groupCmds.Add(new GroupCommand(cmd, isadmin, minrank));
            }
            GroupCommands = groupCmds;
        }

        public static List<string> GroupNames = new List<string>()
        {
            "Игрок",
            "Bronze VIP",
            "Silver VIP",
            "Gold VIP",
            "Platinum VIP",
        };
        public static List<float> GroupPayAdd = new List<float>()
        {
            1.0f,
            1.0f,
            1.15f,
            1.25f,
            1.35f,
        };
        public static List<int> GroupAddPayment = new List<int>()
        {
            0,
            200,
            400,
            550,
            700
        };
        public static List<int> GroupMaxContacts = new List<int>()
        {
            50,
            60,
            70,
            80,
            100,
        };
        public static List<int> GroupMaxBusinesses = new List<int>()
        {
            1,
            1,
            1,
            1,
            1,
        };
        public static List<int> GroupEXP = new List<int>()
        {
            1,
            2,
            2,
            2,
            3,
        };

        public static bool CanUseCmd(Player player, string cmd, string args = "")
        {
            if (!Main.Players.ContainsKey(player)) return false;
            GroupCommand command = GroupCommands.FirstOrDefault(c => c.Command == cmd);
        check:
            if (command != null)
            {
                if (command.IsAdmin)
                {
                    if (Main.Players[player].AdminLVL >= command.MinLVL) return true;
                }
                else
                {
                    if (Main.Accounts[player].VipLvl >= command.MinLVL) return true;
                }
            }
            else
            {
                Database.Query($"INSERT INTO `adminaccess`(`command`, `isadmin`, `minrank`) VALUES ('{cmd}',1,7)");
                GroupCommand newGcmd = new GroupCommand(cmd, true, 7);
                command = newGcmd;
                GroupCommands.Add(newGcmd);
                goto check;
            }

            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно прав", 3000);
            return false;
        }

        public class GroupCommand
        {
            public GroupCommand(string command, bool isAdmin, int minlvl)
            {
                Command = command;
                IsAdmin = isAdmin;
                MinLVL = minlvl;
            }

            public string Command { get; }
            public bool IsAdmin { get; }
            public int MinLVL { get; }
        }
    }
}
