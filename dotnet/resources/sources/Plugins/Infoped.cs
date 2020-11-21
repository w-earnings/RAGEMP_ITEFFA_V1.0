using System;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Plugins
{
    class InfoPed : Script
    {
        private static nLog Log = new nLog("InfoPed");
        public static Vector3 NPCPoint1 = new Vector3(-1030.60, -2744.5, 13.85);

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            ColShape shape = NAPI.ColShape.CreateCylinderColShape(NPCPoint1, 3f, 10f);
            NAPI.Blip.CreateBlip(197, NPCPoint1, 0.75F, 37, Main.StringToU16("Newbie spawn"), 255, 0, true, 0, 0);
            NAPI.TextLabel.CreateTextLabel("~w~Информация", NPCPoint1 + new Vector3(0, 0, 1.2f), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
            shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    Trigger.ClientEvent(entity, "JobsEinfo");
                    entity.SetData("INTERACTIONCHECK", 571);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    Trigger.ClientEvent(entity, "JobsEinfo2");
                    entity.SetData("INTERACTIONCHECK", 0);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
        }
        public static void Interact1(Player player)
        {
            Trigger.ClientEvent(player, "openInfoMenu");
        }
    }
}
