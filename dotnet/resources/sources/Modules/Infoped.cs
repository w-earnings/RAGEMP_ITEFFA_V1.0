using GTANetworkAPI;
using System;

namespace iTeffa.Modules
{
    class InfoPed : Script
    {
        public static Vector3 NPCPoint1 = new Vector3(-1030.60, -2744.5, 13.85);

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            ColShape shape = NAPI.ColShape.CreateCylinderColShape(NPCPoint1, 3f, 10f);
            NAPI.Blip.CreateBlip(197, NPCPoint1, 0.75F, 37, Main.StringToU16("Информация о сервере"), 255, 0, true, 0, 0);
            NAPI.TextLabel.CreateTextLabel("~w~Информация", NPCPoint1 + new Vector3(0, 0, 1.2f), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
            shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    Plugins.Trigger.ClientEvent(entity, "JobsEinfo");
                    entity.SetData("INTERACTIONCHECK", 571);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    Plugins.Trigger.ClientEvent(entity, "JobsEinfo2");
                    entity.SetData("INTERACTIONCHECK", 0);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
        }
        public static void Interact1(Player player)
        {
            Plugins.Trigger.ClientEvent(player, "openInfoMenu");
        }
    }
}
