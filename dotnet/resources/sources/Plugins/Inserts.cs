using System;
using GTANetworkAPI;
using iTeffa.Fractions;
using iTeffa.Settings;

namespace iTeffa.Plugins
{
    public class Blips : Script
    {
        private static nLog Log = new nLog("Blips");
        public static void onResourceStart()
        {
            try
            {

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MANAGER_BLIPS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

    }
}
