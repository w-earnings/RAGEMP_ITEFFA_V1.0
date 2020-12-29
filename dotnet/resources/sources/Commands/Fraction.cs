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
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Realm.Sheriff.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
    }
}
