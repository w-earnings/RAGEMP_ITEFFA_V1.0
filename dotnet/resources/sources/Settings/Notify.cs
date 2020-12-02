using GTANetworkAPI;

namespace iTeffa.Settings
{
    public static class Notify
    {
        public static void Send(Player client, NotifyType type, NotifyPosition pos, string msg, int time)
        {
            Trigger.ClientEvent(client, "notify", type, pos, msg, time);
        }
    }
}
