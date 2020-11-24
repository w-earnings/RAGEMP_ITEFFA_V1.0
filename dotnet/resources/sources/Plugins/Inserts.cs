using System;
using GTANetworkAPI;
using iTeffa.Settings;

/*
 * Копирайт: www.iteffa.com
 */

namespace iTeffa.Plugins
{
    #region Дополнительные блипы на карте
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
    #endregion
    #region Добавляем транспорт витрын
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
    #endregion
}
