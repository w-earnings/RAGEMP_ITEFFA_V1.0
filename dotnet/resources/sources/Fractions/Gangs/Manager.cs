﻿using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Fractions.Gangs
{
    class Gangs : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Gangs");
        public static List<Vector3> DrugPoints = new List<Vector3>()
        {
            new Vector3(8.621573, 3701.914, 39.51624),
            new Vector3(3804.169, 4444.753, 3.977164),
        };
        private static readonly int PricePerDrug = 60;
        [ServerEvent(Event.ResourceStart)]
        public void Event_OnResourceStart()
        {
            try
            {
                foreach (var pos in DrugPoints)
                {
                    NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 4, new Color(255, 0, 0), false, 0);
                    NAPI.TextLabel.CreateTextLabel($"~g~Купить наркотики({PricePerDrug}$/g)", pos + new Vector3(0, 0, 0.7), 5f, 0.3f, 0, new Color(255, 255, 255), true, 0);
                    NAPI.Blip.CreateBlip(140, pos, 0.75F, 4, "Drugs", 255, 0, true, 0, 0);

                    var col = NAPI.ColShape.CreateCylinderColShape(pos - new Vector3(0, 0, 1.12), 4, 5, 0);
                    col.OnEntityEnterColShape += (s, e) =>
                    {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 47);
                        }
                        catch (Exception ex) { Log.Write("OnEntityEnterColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                    };
                    col.OnEntityExitColShape += (s, e) =>
                    {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", -1);
                        }
                        catch (Exception ex) { Log.Write("OnEntityExitColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                    };
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void InteractPressed(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!player.IsInVehicle || !player.Vehicle.HasData("CANDRUGS"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы должны находиться в машине, которая может перевозить наркотики", 3000);
                return;
            }
            if (Manager.FractionTypes[Main.Players[player].FractionID] != 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы не можете закупать наркотики", 3000);
                return;
            }
            if (!Manager.canUseCommand(player, "buydrugs")) return;
            Plugins.Trigger.ClientEvent(player, "openInput", "Закупить наркотики", $"Введите кол-во:", 4, "buy_drugs");
        }
        public static void BuyDrugs(Player player, int amount)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!player.IsInVehicle || !player.Vehicle.HasData("CANDRUGS"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы должны находиться в машине, которая может перевозить наркотики", 3000);
                return;
            }
            if (Manager.FractionTypes[Main.Players[player].FractionID] != 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Вы не можете закупать наркотики", 3000);
                return;
            }
            if (!Manager.canUseCommand(player, "buydrugs")) return;
            var tryAdd = VehicleInventory.TryAdd(player.Vehicle, new nItem(ItemType.Drugs, amount));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в машине", 3000);
                return;
            }
            if (Stocks.fracStocks[Main.Players[player].FractionID].Money < amount * PricePerDrug)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно средств на складе банды", 3000);
                return;
            }
            VehicleInventory.Add(player.Vehicle, new nItem(ItemType.Drugs, amount));
            Stocks.fracStocks[Main.Players[player].FractionID].Money -= amount * PricePerDrug;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы закупили {amount}г наркотиков", 3000);
        }
    }
}

