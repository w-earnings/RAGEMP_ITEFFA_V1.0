using GTANetworkAPI;

namespace iTeffa.Plugins
{
    public static class Notice
    {
        public static void Send(Player client, TypeNotice type, PositionNotice pos, string msg, int time)
        {
            Trigger.ClientEvent(client, "notify", type, pos, msg, time);
        }
    }
    public enum TypeNotice { Alert, Error, Success, Info, Warning }
    public enum PositionNotice { Top, TopLeft, TopCenter, TopRight, Center, CenterLeft, CenterRight, Bottom, BottomLeft, BottomCenter, BottomRight }
}
