using GTANetworkAPI;
using iTeffa.Globals;
using System;

namespace iTeffa.Commands
{
    public class PlayerCommands : Script
    {
        public static DateTime StartDate { get; } = DateTime.Now;

        [Command("build")]
        public static void CMD_BUILD(Player client)
        {
            try
            {
                client.SendChatMessage($"Сборка: !{{#00FFFF}}{Constants.GM_VERSION}!{{#FFF}} запущена !{{#f39c12}}{StartDate}");
            }
            catch { }
        }
    }
}
