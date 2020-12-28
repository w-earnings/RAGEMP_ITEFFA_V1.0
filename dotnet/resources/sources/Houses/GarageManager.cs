using GTANetworkAPI;
using Newtonsoft.Json;
using iTeffa.Globals;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;

namespace iTeffa.Houses
{
    class GarageManager : Script
    {
        private static readonly Nlogs Log = new Nlogs("Garages");
        public static Dictionary<int, Garage> Garages = new Dictionary<int, Garage>();
        public static Dictionary<int, GarageType> GarageTypes = new Dictionary<int, GarageType>()
        {
            { -1, new GarageType(new Vector3(), new List<Vector3>(), new List<Vector3>(), 1) },
            { 0, new GarageType(new Vector3(178.9925, -1005.661, -98.9995),
                new List<Vector3>(){
                    new Vector3(170.6935, -1004.269, -99.41191),
                    new Vector3(174.3777, -1003.795, -99.41129),
                },
                new List<Vector3>(){
                    new Vector3(-0.1147747, 0.02747092, 183.3471),
                    new Vector3(-0.1562817, 0.01328733, 175.7529),
                }, 2)},
            { 1, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                },
                new List<Vector3>(){
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                }, 3)},
            { 2, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                },
                new List<Vector3>(){
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                }, 4)},
            { 3, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(204.1544, -997.7147, -99.41058),
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                },
                new List<Vector3>(){
                    new Vector3(-0.115809, -0.04190827, 166.4086),
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                }, 5)},
            { 4, new GarageType(new Vector3(206.9094, -999.0917, -100),
                new List<Vector3>(){
                    new Vector3(204.1544, -997.7147, -99.41058),
                    new Vector3(200.7814, -997.5886, -99.41073),
                    new Vector3(197.3544, -997.4301, -99.41062),
                    new Vector3(193.8947, -997.2777, -99.41056),
                    new Vector3(201.9032, -1004.244, -99.41065),
                    new Vector3(196.0699, -1003.287, -99.41054),
                },
                new List<Vector3>(){
                    new Vector3(-0.115809, -0.04190827, 166.4086),
                    new Vector3(-0.1146501, -0.03047129, 165.095),
                    new Vector3(-0.1124166, -0.03466159, 163.7391),
                    new Vector3(-0.1131818, -0.03073582, 163.4609),
                    new Vector3(-0.1150091, -0.03728109, 163.4917),
                    new Vector3(-0.1143998, -0.02649088, 161.4624),
                }, 6)},
            { 5, new GarageType(new Vector3(240.411, -1004.753, -100),
                new List<Vector3>(){
                    new Vector3(223.2661, -978.6877, -99.41358),
                    new Vector3(223.1918, -982.4593, -99.41795),
                    new Vector3(222.8921, -985.879, -99.41821),
                    new Vector3(222.8588, -989.4495, -99.41826),
                    new Vector3(223.0551, -993.4521, -99.41066),
                    new Vector3(233.6587, -983.3923, -99.41045),
                    new Vector3(234.0298, -987.5615, -99.41094),
                    new Vector3(234.0298, -991.406, -99.4104),
                    new Vector3(234.2386, -995.7032, -99.41273),
                    new Vector3(234.3856, -999.8402, -99.41091),
                },
                new List<Vector3>(){
                    new Vector3(-0.03247262, -0.08614436, 251.3986),
                    new Vector3(-0.8253403, 0.03646085, 246.0103),
                    new Vector3(-0.8608215, 0.004363943, 251.0875),
                    new Vector3(-0.8236036, 0.02502611, 248.026),
                    new Vector3(-0.1083736, -0.1425103, 240.252),
                    new Vector3(-0.1053052, 0.02684846, 130.5622),
                    new Vector3(-0.09362753, 0.1056001, 130.4442),
                    new Vector3(-0.09778301, 0.03327406, 129.4973),
                    new Vector3(-0.05343597, 0.06972831, 129.157),
                    new Vector3(-0.08984898, 0.1096697, 128.8663),
                }, 10)},
        };
        public static int DimensionID = 1000;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                var result = Database.QueryRead($"SELECT * FROM `garages`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", Nlogs.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    var id = Convert.ToInt32(Row["id"]);
                    var type = Convert.ToInt32(Row["type"]);
                    var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                    var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());

                    var garage = new Garage(id, type, position, rotation)
                    {
                        Dimension = DimensionID
                    };
                    if (garage.Type != -1) garage.CreateInterior();

                    Garages.Add(id, garage);
                    DimensionID++;
                }
                Log.Write($"Loaded {Garages.Count} garages.", Nlogs.Type.Success);
            }
            catch (Exception e) { Log.Write($"ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        public static void spawnCarsInGarage()
        {
            Log.Write($"Loading garage cars.", Nlogs.Type.Info);
            var count = 0;
            lock (Garages)
            {
                foreach (var garage in Garages)
                {
                    try
                    {
                        if (garage.Value.Type == -1) continue;
                        var house = HouseManager.Houses.Find(h => h.GarageID == garage.Key);
                        if (house == null) continue;
                        if (string.IsNullOrEmpty(house.Owner)) continue;

                        var vehicles = VehicleManager.getAllPlayerVehicles(house.Owner);
                        vehicles.RemoveAll(v => !string.IsNullOrEmpty(VehicleManager.Vehicles[v].Position));
                        garage.Value.SpawnCars(vehicles);

                        count += vehicles.Count;
                    }
                    catch (Exception e) { Log.Write($"garage load vehicles {e}", Nlogs.Type.Error); }
                }
            }
            Log.Write($"{count} vehicles were spawned in garages.", Nlogs.Type.Success);
        }

        public static void interactionPressed(Player player, int id)
        {
            try
            {
                switch (id)
                {
                    case 40:
                        if (!player.HasData("GARAGEID") || Houses.HouseManager.GetHouse(player) == null) return;
                        Garage garage = Garages[player.GetData<int>("GARAGEID")];
                        if (garage == null) return;
                        var house = HouseManager.GetHouse(player);
                        if (house == null || house.GarageID != garage.ID) return;

                        var vehicles = VehicleManager.getAllPlayerVehicles(house.Owner);
                        if (player.IsInVehicle && !vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Du kannst mit diesem Auto nicht in die Garage fahren", 3000);
                            return;
                        }
                        else if (player.IsInVehicle && vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            var vehicle = player.Vehicle;
                            var number = vehicle.NumberPlate;
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntityData(player.Vehicle, "PETROL")) ? VehicleManager.VehicleTank[player.Vehicle.Class] : NAPI.Data.GetEntityData(player.Vehicle, "PETROL");
                            VehicleManager.Vehicles[number].Items = player.Vehicle.GetData<List<nItem>>("ITEMS");
                            VehicleManager.Vehicles[number].Position = null;
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(vehicle); } catch { } });

                            garage.SendVehicleIntoGarage(number);
                        }

                        if (garage.Type == -1)
                        {
                            if (vehicles.Count == 0) return;
                            if (garage.CheckCar(false, vehicles[0]))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Ihr Auto ist jetzt irgendwo im Staat, Sie können evakuieren", 3000);
                                return;
                            }
                            if (player.IsInVehicle) return;

                            if (VehicleManager.Vehicles[vehicles[0]].Health < 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Sie müssen das Auto wiederherstellen", 3000);
                                return;
                            }
                            garage.GetVehicleFromGarage(player, vehicles[0]);
                        }
                        else
                        {
                            garage.SendPlayer(player);
                        }
                        return;
                    case 41:
                        if (Main.Players[player].InsideGarageID == -1) return;
                        garage = Garages[Main.Players[player].InsideGarageID];
                        garage.RemovePlayer(player);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"GARAGE_INTERACTION\":\n" + e.ToString(), Nlogs.Type.Error);
            }
        }

        public static void Event_PlayerDisconnected(Player player) { }
    }
}