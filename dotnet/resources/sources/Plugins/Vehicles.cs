using System;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Plugins
{
    public class Vehicles : Script
    {
        private static readonly Nlogs Log = new Nlogs("Blips");
        public static void OnResourceStart()
        {
            try
            {
                /* -- iTeffa -- */
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MANAGER_VEHICLE\":\n" + e.ToString(), Nlogs.Type.Error);
            }
        }

    }
}
