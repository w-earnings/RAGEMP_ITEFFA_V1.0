﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class SafeZones : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("SafeZones");
        public static void CreateSafeZone(Vector3 position, int height, int width)
        {
            var colShape = NAPI.ColShape.Create2DColShape(position.X, position.Y, height, width, 0);
            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    Plugins.Trigger.ClientEvent(player, "safeZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", Plugins.Logs.Type.Error); }
                
            };
            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    Plugins.Trigger.ClientEvent(player, "safeZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", Plugins.Logs.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(445.07443, -983.2143, 29.569595), 70, 70); // полиция
            CreateSafeZone(new Vector3(240.7599, -1379.576, 32.74176), 70, 70); // ems safe zone
            //CreateSafeZone(new Vector3(-712.2147, -1298.926, 4.101922), 70, 70); // driving school safe zone
        }
    }
}
