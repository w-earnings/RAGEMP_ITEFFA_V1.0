using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Linq;

namespace iTeffa.Modules
{
    class Vitality : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Vitality");
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            Log.Write("Staring timers.", Plugins.Logs.Type.Info);
            Timers.StartTask("checkwater", 180000, () => CheckWater());
            Timers.StartTask("checkeat", 300000, () => CheckEat());
            Log.Write("Timers started.", Plugins.Logs.Type.Success);
        }
        public static void SetEat(Player player, int change)
        {
            Main.Players[player].Eat = change;
            Globals.Database.Query($"UPDATE characters SET eat={Main.Players[player].Eat} WHERE uuid={Main.Players[player].UUID}");
            Interface.Dashboard.sendStats(player);
            Plugins.Trigger.ClientEvent(player, "UpdateEat", Main.Players[player].Eat, Convert.ToString(change));
        }
        public static void AddEat(Player player, int change)
        {
            if (Main.Players[player].Eat + change > 100)
            {
                Main.Players[player].Eat = 100;
            }
            else
            {
                Main.Players[player].Eat += change;
            }
            Globals.Database.Query($"UPDATE characters SET eat={Main.Players[player].Eat} WHERE uuid={Main.Players[player].UUID}");
            Plugins.Trigger.ClientEvent(player, "UpdateEat", Main.Players[player].Eat, Convert.ToString(change));
            Interface.Dashboard.sendStats(player);
        }
        public static void SetWater(Player player, int change)
        {
            Main.Players[player].Water = change;
            Globals.Database.Query($"UPDATE characters SET water={Main.Players[player].Water} WHERE uuid={Main.Players[player].UUID}");
            Plugins.Trigger.ClientEvent(player, "UpdateWater", Main.Players[player].Water, Convert.ToString(change));
            Interface.Dashboard.sendStats(player);
        }
        public static void AddWater(Player player, int change)
        {
            if (Main.Players[player].Water + change > 100)
            {
                Main.Players[player].Water = 100;
            }
            else
            {
                Main.Players[player].Water += change;
            }
            Globals.Database.Query($"UPDATE characters SET water={Main.Players[player].Water} WHERE uuid={Main.Players[player].UUID}");
            Plugins.Trigger.ClientEvent(player, "UpdateWater", Main.Players[player].Water, Convert.ToString(change));
            Interface.Dashboard.sendStats(player);
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            SetEat(player, 40);
            SetWater(player, 40);
        }
        public static void CheckEat()
        {
            NAPI.Task.Run(() =>
            {
                Log.Write("Check Eat.", Plugins.Logs.Type.Info);
                foreach (Player player in Main.Players.Keys.ToList())
                {
                    try
                    {
                        if (player.Health > 0)
                        {
                            var rnd = new Random();
                            int intrnd = rnd.Next(2, 5);
                            if (Main.Players[player].Eat > 0 && Main.Players[player].Eat - intrnd > 0)
                            {
                                if (player.IsInVehicle)
                                {
                                    AddEat(player, -1);
                                }
                                else
                                {
                                    AddEat(player, -intrnd);
                                }
                            }
                            else if (Main.Players[player].Eat - intrnd < 0)
                            {
                                SetEat(player, 0);
                            }
                            if (Main.Players[player].Eat == 0 && player.Health >= 20)
                            {
                                player.Health -= 2;
                            }
                            else if (Main.Players[player].Water == 0 && Main.Players[player].Eat == 0)
                            {
                                player.Health -= 4;
                            }
                            if (Main.Players[player].Eat >= 80 && Main.Players[player].Water >= 80)
                            {
                                if (player.Health + 2 > 100)
                                {
                                    player.Health = 100;
                                }
                                else
                                {
                                    player.Health += 2;
                                }
                            }
                        }
                    }
                    catch { }
                }
            });
        }
        public static void CheckWater()
        {
            NAPI.Task.Run(() =>
            {
                Log.Write("Check Water.", Plugins.Logs.Type.Info);
                foreach (Player player in Main.Players.Keys.ToList())
                {
                    try
                    {
                        if (player.Health > 0)
                        {
                            if (Main.Players[player].Water > 0 && Main.Players[player].Water - 2 > 0)
                            {
                                if (player.IsInVehicle)
                                {
                                    AddWater(player, -1);
                                }
                                else
                                {
                                    AddWater(player, -2);
                                }
                            }
                            else if (Main.Players[player].Water - 2 < 0)
                            {
                                SetWater(player, 0);
                            }
                            if (Main.Players[player].Water == 0 && player.Health >= 20)
                            {
                                player.Health -= 2;
                            }
                        }
                    }
                    catch { }
                }
            });
        }

    }
}