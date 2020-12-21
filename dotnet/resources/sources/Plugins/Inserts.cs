using System;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Plugins
{
    public class Blips : Script
    {
        private static readonly Nlogs Log = new Nlogs("Blips");
        public static void OnResourceStart()
        {
            try
            {
                
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MANAGER_BLIPS\":\n" + e.ToString(), Nlogs.Type.Error);
            }
        }

    }
}
