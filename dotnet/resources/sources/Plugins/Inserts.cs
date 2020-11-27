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
                NAPI.Blip.CreateBlip(419, Manager.FractionSpawns[6], 1.0F, 14, Main.StringToU16("CityHall"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(419, new Vector3(-1304.6462, -560.2332, 33.25491), 1.0F, 14, Main.StringToU16("CityHall"), 255, 0, true, 0);
                NAPI.Blip.CreateBlip(184, LSNews.LSNewsCoords[0], 0.75F, 1, Main.StringToU16("Новости"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(61, Ems.emsCheckpoints[0], 0.75F, 49, Main.StringToU16("Госпиталь"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(88, Fbi.EnterFBI, 0.75F, 58, Main.StringToU16("FIB"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(526, Police.policeCheckpoints[1], 0.75F, 38, Main.StringToU16("Полиция"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(85, Army.ArmyCheckpoints[2], 0.75F, 28, Main.StringToU16("Доки"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[1], 0.75F, 52, Main.StringToU16("The Families"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[2], 0.75F, 58, Main.StringToU16("The Ballas"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[3], 0.75f, 28, Main.StringToU16("Los Santos Vagos"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[4], 0.75F, 74, Main.StringToU16("Marabunta Grande"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[5], 0.75F, 49, Main.StringToU16("Blood Street"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[10], 0.75F, 5, Main.StringToU16("La Cosa Nostra"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[11], 0.75F, 4, Main.StringToU16("Русская мафия"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[12], 0.75F, 76, Main.StringToU16("Якудза"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[13], 0.75F, 40, Main.StringToU16("Армянская мафия"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(640, Manager.FractionSpawns[14], 0.75F, 52, Main.StringToU16("Army"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(526, Manager.FractionSpawns[18], 0.75F, 47, Main.StringToU16("Sheriff"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(668, new Vector3(-1123.202, 4929.628, 217.7096), 0.75F, 75, Main.StringToU16("Redneck"), 255, 0, true, 0);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MANAGER_BLIPS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

    }
}
