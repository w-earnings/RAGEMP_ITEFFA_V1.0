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
            Trigger.ClientEvent(player, "UpdateMoney", temp, Convert.ToString(Amount));
            Connect.Query($"UPDATE characters SET money={Main.Players[player].Money} WHERE uuid={Main.Players[player].UUID}");
            return true;
        }
        public static void Set(Player player, long Amount)
        {
            var data = Main.Players[player];
            if (data == null) return;
            data.Money = Amount;
            Trigger.ClientEvent(player, "UpdateMoney", data.Money);
        }
    }
}
