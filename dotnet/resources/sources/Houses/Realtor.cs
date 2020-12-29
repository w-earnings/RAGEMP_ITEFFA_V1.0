using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Houses
{
    class Realtor : Script
    {
        private static readonly Nlogs RLog = new Nlogs("RealtorManager");
        private static readonly List<object> HouseList = new List<object>();
        private static ColShape shape;
        private static Marker intmarker;
        private static readonly Vector3 PositionRealtor = new Vector3(-1289.75, -574.48, 29.08);
        private static readonly int[] PriceToInfo = { 0, 100, 200, 500, 1000, 1500, 2000 };

        [ServerEvent(Event.ResourceStart)]
        public static void EnterShapeRealtor()
        {
            try
            {
                NAPI.TextLabel.CreateTextLabel("~r~Realtor", new Vector3(-1290.61, -574.38, 31.77), 3.5f, 0.3f, 0, new Color(255, 225, 64), true, 0);
                intmarker = NAPI.Marker.CreateMarker(27, PositionRealtor + new Vector3(0, 0, 0.1), new Vector3(), new Vector3(), 1.75f, new Color(0, 0, 0), false, 0);
                shape = NAPI.ColShape.CreateCylinderColShape(PositionRealtor, 1, 2, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 512);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shape.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                RLog.Write("Loaded", Nlogs.Type.Info);
            }
            catch (Exception e) { RLog.Write(e.ToString(), Nlogs.Type.Error); }
        }

        public static void OpenRealtorMenu(Player player)
        {
            Plugins.Trigger.ClientEvent(player, "openRealtorMenu");
        }

        [RemoteEvent("closeRealtorMenu")]
        public static void CloseRealtorMenu(Player player)
        {
            Plugins.Trigger.ClientEvent(player, "closeRealtorMenu");
        }

        [RemoteEvent("buyRealtorInfoHome")]
        public static void BuyInfoHome(Player player, int hclass, float x, float y)
        {
            RLog.Write($"BuyInfoHome {player} {hclass} {x} {y}");

            if (PriceToInfo[hclass] > Main.Players[player].Money)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас не хватает средств для покупки информации", 3000);
            }
            else
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Маршрут установлен", 3000);
                Plugins.Trigger.ClientEvent(player, "createWaypoint", x, y);
                Finance.Wallet.Change(player, -PriceToInfo[hclass]);
            }
            NAPI.Task.Run(() =>
            {
                try
                {
                    CloseRealtorMenu(player);
                }
                catch { }
            }, 200);
        }

        [RemoteEvent("LoadHouseToMenu")]
        public static void LoadHouseToMenu(Player player, int houseclass)
        {
            try
            {
                foreach (House house in HouseManager.Houses.FindAll(x => x.Type == houseclass))
                {
                    if (house.Owner == "")
                    {
                        var maxcars = GarageManager.Garages.ContainsKey(house.GarageID) ? GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars.ToString() : "нет";
                        List<object> data = new List<object>
                        {
                            house.ID,
                            house.Type,
                            house.Price,
                            house.Position,
                            maxcars,
                            PriceToInfo[houseclass]
                        };
                        HouseList.Add(data);
                    }
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(HouseList);
                Plugins.Trigger.ClientEvent(player, "LoadHouse", json);

                HouseList.Clear();
            }
            catch (Exception e)
            {
                RLog.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
    }
}
