using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Globals
{
    class PublicGarage : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("PublicGarage");

        [RemoteEvent("publicGarage:spawnCars")]
        public static void spawnCars(Player player, params object[] arguments)
        {
            List<string> vehicleNumbers = VehicleManager.getAllPlayerVehicles(player.Name);
            foreach(string vNumber in vehicleNumbers)
            {
                Vector3 spawnPosition = player.Position;
                spawnPosition.X += 90;
                VehicleManager.Spawn(vNumber, player.Position, 90, player);
            }
        }



        #region Кординаты парковок
        public static Vector3[] vehstore = new Vector3[]
        {
            new Vector3(-1186.033, -742.2707, 19.11804),
            new Vector3(237.3155, -792.5198, 29.51836),
            new Vector3(-1685.067, 49.37582, 63.02029),
            new Vector3(614.9812, 2724.071, 40.86776),
            new Vector3(1708.349, 3775.306, 33.51489),
            new Vector3(10.59172, 6322.342, 30.23178),
            new Vector3(-3036.624, 105.1893, 10.59305),
            new Vector3(-85.10564, -2004.633, 17.01696),
            new Vector3(116.7458, -1949.892, 19.748),
            new Vector3(611.4971, 111.1495, 91.91084),
            new Vector3(-905.5421, -161.0625, 41.87945),
            new Vector3(-68.31268, 897.8906, 234.5641)
        };
        #endregion
        #region Парковка № 1
        public static List<Vector3> ParkingPlacesOne = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 2
        public static List<Vector3> ParkingPlacesTwo = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 3
        public static List<Vector3> ParkingPlacesThree = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 4
        public static List<Vector3> ParkingPlacesFour = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 5
        public static List<Vector3> ParkingPlacesFive = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 6
        public static List<Vector3> ParkingPlacesSix = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 7
        public static List<Vector3> ParkingPlacesSeven = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 8
        public static List<Vector3> ParkingPlacesEight = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 9
        public static List<Vector3> ParkingPlacesNine = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 10
        public static List<Vector3> ParkingPlacesTen = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 11
        public static List<Vector3> ParkingPlacesEleven = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion
        #region Парковка № 12
        public static List<Vector3> ParkingPlacesTwelve = new List<Vector3>()
        {
            new Vector3(0, 0, 0), // Место 1
            new Vector3(0, 0, 0), // Место 2
            new Vector3(0, 0, 0), // Место 3
            new Vector3(0, 0, 0), // Место 4
            new Vector3(0, 0, 0), // Место 5
        };
        #endregion



                [RemoteEvent("IsInNearVehStore")]
        public void IsInNearVehStore(Player c)
        {
            foreach (Vector3 garagePosition in vehstore)
            {
                if (c.Position.DistanceTo2D(garagePosition) < 5)
                {
                    c.TriggerEvent("OpenVehStore");
                    
                    if (c.IsInVehicle)
                    {
                        if (c.Vehicle.GetData<Player>("OWNER") == c)
                        {
                            if (spawnedVehiclesNumber.Contains(c.Vehicle.NumberPlate))
                            {
                                spawnedVehiclesNumber.Remove(c.Vehicle.NumberPlate);
                                c.Vehicle.Delete();
                            }
                            else
                            {
                                Plugins.Notice.Send(c, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter,
                                    $"У тебя есть дом. иди и припаркуйся в своем гараже!",
                                3000);
                            }
                        }
                        else
                        {
                            Log.Write("Idk why but c.Vehicle.GetData('OWNER') == c  is False ");
                        }
                    }

                    return; 
                }
            }
        }

        public static List<string> spawnedVehiclesNumber = new List<string> {};

        [RemoteEvent("spawnVehicle")]
        public static void SpawnVehicle(Player c, string vNumber)
        {
            try
            {
                if (spawnedVehiclesNumber.Contains(vNumber))
                {
                    Plugins.Notice.Send(c, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter,
                        $"Машины нет в гараже!",
                    3000);
                    return;
                }


                var house = Houses.HouseManager.GetHouse(c, true);
                if (house == null || house.GarageID == 0)
                {
                    if (! VehicleManager.getAllPlayerVehicles(c.Name).Contains(vNumber))
                    {
                        Log.Write("Кто-то пытался создать не его транспортное средство!");
                        Commands.Controller.SendToAdmins(3, $"!{{#d35400}}[CAR-GARAGE-EXPLOIT] {c.Name} ({c.Value})");
                        Plugins.Notice.Send(c, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter,
                            $"Это не твоя машина!",
                        3000);
                        return;
                    }
                    var access = VehicleManager.canAccessByNumber(c, vNumber);
                    if (! access)
                    {
                        Plugins.Notice.Send(c, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter,
                            $"Кажется, вы потеряли ключи от машины. Возьми новую!",
                        3000);
                        nInventory.Add(c, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));
                    }
                    VehicleManager.Spawn(vNumber, c.Position, 90, c);
                    spawnedVehiclesNumber.Add(vNumber);
                    Log.Write("Spawn vehicle" + vNumber);
                }
                else
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    if (!garage.CheckCar(false, vNumber) && !garage.CheckCar(true, vNumber))
                    {
                        Vector3 spawnPosition = c.Position;
                        VehicleManager.Spawn(vNumber, c.Position, 90, c);
                        spawnedVehiclesNumber.Add(vNumber);
                    }
                    else
                    {
                        Plugins.Notice.Send(c, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter,
                            $"Машины нет в открытом гараже!",
                        3000);
                        return;
                    }
                }
            } catch(Exception e) { Log.Write("Ошибка при создании машины из общественного гаража " + e.Message); }
          
        }

        public struct GarageVehicle
        {
            public string id;
            public string name;
            public float fuel;
            public Color color;
            public byte insurance;
            public float km;
            public float fuelConsumption;
            public DateTime buyDate;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            // Create blips
            foreach (Vector3 position in vehstore)
            {
                if (position != new Vector3(116.7458, -1949.892, 19.748))
                {
                    NAPI.Blip.CreateBlip(50, position, 0.75F, 4, "Общественная парковка", 255, 0, true);
                }

                NAPI.Marker.CreateMarker(1, position, new Vector3(), new Vector3(), 3, new Color(255, 255, 255, 220), false, 0);
                ColShape shape = NAPI.ColShape.CreateCylinderColShape(position, 3, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        Plugins.Notice.Send(entity, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter,
                            $"Нажмите E, чтобы открыть меню!",
                        3000);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
            }
        }


        [RemoteEvent("getVehicles")]
        public static void GetVehicles(Player c)
        {
            Console.WriteLine("getVehicles: ");
            List<GarageVehicle> vehicles = new List<GarageVehicle> { };
            List<string> vehicleNumbers = VehicleManager.getAllPlayerVehicles(c.Name);
            foreach (string vNumber in vehicleNumbers)
            {
                var vehicle = VehicleManager.Vehicles[vNumber];

                Color color = new Color(0, 0, 0);
                vehicles.Add(new GarageVehicle
                {
                    id = vNumber,
                    name = vehicle.Model,
                    fuel = vehicle.Fuel,
                    color = color, // TODO
                    insurance = 0, // TODO
                    km = vehicle.Health,
                    fuelConsumption = 0, // TODO
                    buyDate = new DateTime() // TODO
                });
            }
            
            
            c.TriggerEvent("receiveVehicles", vehicles);
        }
    }
}
