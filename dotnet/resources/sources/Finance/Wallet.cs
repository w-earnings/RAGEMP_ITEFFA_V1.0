using System;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Finance
{
    class Wallet : Script
    {
        public static bool Change(Player player, int Amount)
        {
            if (!Main.Players.ContainsKey(player)) return false;
            if (Main.Players[player] == null) return false;
            int temp = (int)Main.Players[player].Money + Amount;
            if (temp < 0) return false;
            Main.Players[player].Money = temp;
            Plugins.Trigger.ClientEvent(player, "UpdateMoney", temp, Convert.ToString(Amount));
            Globals.Database.Query($"UPDATE characters SET money={Main.Players[player].Money} WHERE uuid={Main.Players[player].UUID}");
            return true;
        }
        public static void Set(Player player, long Amount)
        {
            var data = Main.Players[player];
            if (data == null) return;
            data.Money = Amount;
            Plugins.Trigger.ClientEvent(player, "UpdateMoney", data.Money);
        }

        public static bool ChangeDonateBalance(Player player, int Amount)
        {
            if (!Main.Players.ContainsKey(player)) return false;
            if (Main.Players[player] == null) return false;
            int temp = Convert.ToInt32(Main.Accounts[player].Coins + Amount);
            if (temp < 0)
                return false;
            else
            {
                Main.Accounts[player].Coins = temp;
                Globals.Database.Query($"UPDATE `accounts` SET `coins`={temp} WHERE login='{Main.Accounts[player].Login}'");
                return true;
            }
        }
    }
}