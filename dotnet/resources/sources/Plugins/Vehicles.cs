using System;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Plugins
{
    public class Vehicles : Script
    {
        private static nLog Log = new nLog("Blips");
        public static void onResourceStart()
        {
            try
            {
                /* -- iTeffa -- */
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MANAGER_VEHICLE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

    }
}
