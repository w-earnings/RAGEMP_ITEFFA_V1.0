using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Threading.Tasks;

namespace iTeffa.Commands
{
    public class СhatCommands : Script
    {
        private static readonly Nlogs Log = new Nlogs("Сhat Commands");
        [Command("global", GreedyArg = true)]
        public static void CMD_adminGlobalChat(Player player, string message)
        {
            try
            {
                Globals.Admin.adminGlobal(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("a", GreedyArg = true)]
        public static void CMD_adminChat(Player player, string message)
        {
            try
            {
                Globals.Admin.adminChat(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("me", GreedyArg = true)]
        public static async Task CMD_chatMe(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            msg = Controller.RainbowExploit(player, msg);
            await Controller.RPChatAsync("me", player, msg);
        }
        [Command("do", GreedyArg = true)]
        public static async Task CMD_chatDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            msg = Controller.RainbowExploit(player, msg);
            await Controller.RPChatAsync("do", player, msg);
        }
        [Command("todo", GreedyArg = true)]
        public static async Task CMD_chatToDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("todo", player, msg);
        }
        [Command("s", GreedyArg = true)]
        public static async Task CMD_chatS(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("s", player, msg);
        }
        [Command("b", GreedyArg = true)]
        public static async Task CMD_chatB(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("b", player, msg);
        }
        [Command("vh", GreedyArg = true)]
        public static async Task CMD_chatVh(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("vh", player, msg);
        }
        [Command("m", GreedyArg = true)]
        public static async Task CMD_chatM(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("m", player, msg);
        }
        [Command("t", GreedyArg = true)]
        public static async Task CMD_chatT(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await Controller.RPChatAsync("t", player, msg);
        }
        [Command("try", GreedyArg = true)]
        public static void CMD_chatTry(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            Controller.Try(player, msg);
        }
    }
}
