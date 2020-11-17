using GTANetworkAPI;

namespace iTeffa.Kernel
{
    public class SmoothThrottleAntiReverse : Script
    {
        [ServerEvent(Event.PlayerExitVehicle)]
        public void SmoothThrottleExitEvent(Player player, Vehicle veh)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_PlayerExitVehicle", veh);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void SmoothThrottleEnterEvent(Player player, Vehicle veh, sbyte seat)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_PlayerEnterVehicle", veh, seat);
        }

        public static void SetSmoothThrottle(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetSmoothThrottle", turnedOn);
        }

        public static void SetAntiReverse(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetAntiReverse", turnedOn);
        }

        public static void SetSmoothThrottleAntiReverse(Player player, bool turnedOn)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "SmoothThrottle_SetGlobal", turnedOn);
        }
    }
}