using GTANetworkAPI;
using iTeffa.Settings;
using System;

namespace iTeffa.Commands
{
    public class FractionCommands : Script
    {
        private static readonly Nlogs Log = new Nlogs("Fraction Commands");
        #region Шерифи
        [Command("sh1")]
        public static void CMD_sheriffAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Sheriff.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
    }
}
