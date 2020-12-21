using GTANetworkAPI;
using Newtonsoft.Json;
using iTeffa.Globals;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iTeffa.Houses
{
    class Garage
    {
        public int ID { get; }
        public int Type { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        private readonly ColShape shape;

        [JsonIgnore]
        private ColShape intShape;
        [JsonIgnore]
        private Marker intMarker;

        [JsonIgnore]
        private Dictionary<string, Tuple<int, Entity>> entityVehicles = new Dictionary<string, Tuple<int, Entity>>();
        [JsonIgnore]
        private Dictionary<string, Entity> vehiclesOut = new Dictionary<string, Entity>();
        private readonly Nlogs Log = new Nlogs("Garage");

        public Garage(int id, int type, Vector3 position, Vector3 rotation)
        {
            ID = id;
            Type = type;
            Position = position;
            Rotation = rotation;

            shape = NAPI.ColShape.CreateCylinderColShape(position - new Vector3(0, 0, 1), 1, 3, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "GARAGEID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 40);
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    if (NAPI.Entity.GetEntityType(ent) != EntityType.Player) return;
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "GARAGEID");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };
        }
        public bool CheckCar(bool checkin, string number)
        {
            if (checkin)
            {
                if (entityVehicles.ContainsKey(number)) return true;
                else return false;
            }
            else
            {
                if (vehiclesOut.ContainsKey(number)) return true;
                else return false;
            }
        }
        public Vehicle GetOutsideCar(string number)
        {
            if (!vehiclesOut.ContainsKey(number)) return null;
            return NAPI.Entity.GetEntityFromHandle<Vehicle>(vehiclesOut[number]);
        }
        public void DeleteCar(string number, bool resetPosition = true)
        {
            if (entityVehicles.ContainsKey(number))
            {
                NAPI.Task.Run(() => {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(entityVehicles[number].Item2, "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntityData(entityVehicles[number].Item2, "PETROL")) ? VehicleManager.VehicleTank[vclass] : NAPI.Data.GetEntityData(entityVehicles[number].Item2, "PETROL");
                        }
                        NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                        entityVehicles.Remove(number);
                    }
                    catch { }
                });
            }

            if (vehiclesOut.ContainsKey(number))
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(vehiclesOut[number], "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntityData(vehiclesOut[number], "PETROL")) ? VehicleManager.VehicleTank[vclass] : NAPI.Data.GetEntityData(vehiclesOut[number], "PETROL");
                        }
                        NAPI.Entity.DeleteEntity(vehiclesOut[number]);
                        vehiclesOut.Remove(number);
                    }
                    catch { }
                });
                if (resetPosition) VehicleManager.Vehicles[number].Position = null;
            }
        }
        public void Create()
        {
            Connect.Query($"INSERT INTO `garages`(`id`,`type`,`position`,`rotation`) VALUES ({ID},{Type},'{JsonConvert.SerializeObject(Position)}','{JsonConvert.SerializeObject(Rotation)}')");
        }
        public void Save()
        {
            //MySQL.Query($"UPDATE `garages` SET `data`='{JsonConvert.SerializeObject(this)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            shape.Delete();
            intShape.Delete();
            intMarker.Delete();
        }
        public void SpawnCar(string number)
        {
            if (entityVehicles.ContainsKey(number)) return;
            int i = 0;
            for (i = 0; i < 10; i++)
            {
                if (entityVehicles.Values.FirstOrDefault(t => t.Item1 == i) == null)
                    break;
            }

            if (i >= GarageManager.GarageTypes[Type].VehiclesPositions.Count) return;

            var vehData = VehicleManager.Vehicles[number];
            if (vehData.Health < 1) return;
            //            var car = NAPI.Util.GetHashKey(vehData.Model);
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), GarageManager.GarageTypes[Type].VehiclesPositions[i] + new Vector3(0, 0, 0.25), GarageManager.GarageTypes[Type].VehiclesRotations[i], 0, 0);
            veh.NumberPlate = number;
            NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            veh.SetData("ACCESS", "GARAGE");
            veh.SetData("ITEMS", vehData.Items);
            veh.SetSharedData("PETROL", vehData.Fuel);
            VehicleManager.ApplyCustomization(veh);
            entityVehicles.Add(number, new Tuple<int, Entity>(i, veh));
        }
        public void SpawnCars(List<string> vehicles)
        {
            int i = 0;
            foreach (var number in vehicles)
            {
                if (i >= GarageManager.GarageTypes[Type].VehiclesPositions.Count) continue;
                var vehData = VehicleManager.Vehicles[number];
                if (vehData.Health < 1) continue;
                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), GarageManager.GarageTypes[Type].VehiclesPositions[i] + new Vector3(0, 0, 0.25), GarageManager.GarageTypes[Type].VehiclesRotations[i], 0, 0);
                veh.NumberPlate = number;
                NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, true);
                veh.SetData("ACCESS", "GARAGE");
                veh.SetData("ITEMS", vehData.Items);
                veh.SetSharedData("PETROL", vehData.Fuel);
                VehicleManager.ApplyCustomization(veh);
                entityVehicles.Add(number, new Tuple<int, Entity>(i, veh));
                i++;
            }
        }
        public void DestroyCars()
        {
            Log.Write("Destroy cars");
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var veh in entityVehicles)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value.Item2, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value.Item2);
                    }
                    entityVehicles = new Dictionary<string, Tuple<int, Entity>>();

                    foreach (var veh in vehiclesOut)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value);
                        VehicleManager.Vehicles[veh.Key].Position = null;
                    }
                    vehiclesOut = new Dictionary<string, Entity>();
                }
                catch { }
            });
        }
        public void RespawnCars()
        {
            try
            {
                List<string> vehicles = entityVehicles.Keys.ToList();

                /*foreach (var veh in entityVehicles)
                    NAPI.Entity.DeleteEntity(veh.Value.Item2);
                entityVehicles = new Dictionary<string, Tuple<int, NetHandle>>();*/

                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "GARAGE" && vehicles.Contains(v.NumberPlate))
                    {
                        if (VehicleManager.Vehicles.ContainsKey(v.NumberPlate) && v.HasData("ITEMS"))
                            VehicleManager.Vehicles[v.NumberPlate].Items = v.GetData<List<nItem>>("ITEMS");
                        v.Delete();
                    }
                }
                entityVehicles.Clear();

                SpawnCars(vehicles);
            }
            catch { }
        }
        public void SpawnCarAtPosition(Player player, string number, Vector3 position, Vector3 rotation)
        {
            if (vehiclesOut.ContainsKey(number))
            {
                Main.Players[player].LastVeh = "";
                return;
            }

            var vData = VehicleManager.Vehicles[number];
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), position, rotation, 0, 0, number);
            vehiclesOut.Add(number, veh);

            veh.SetSharedData("PETROL", vData.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", player);
            veh.SetData("ITEMS", vData.Items);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            VehicleManager.ApplyCustomization(veh);
        }

        public void GetVehicleFromGarage(Player player, string number)
        {
            NAPI.Task.Run(() => {

                var vData = VehicleManager.Vehicles[number];
                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), player.Position + new Vector3(0, 0, 0.3), Rotation, 0, 0, number);

                player.SetIntoVehicle(veh, 0);
                vehiclesOut.Add(number, veh);
                veh.SetSharedData("PETROL", vData.Fuel);
                veh.SetData("ACCESS", "PERSONAL");
                veh.SetData("OWNER", player);
                veh.SetData("ITEMS", vData.Items);
                veh.Position = Position;

                NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

                if (Type == -1)
                {
                    VehicleStreaming.SetEngineState(veh, false);
                    VehicleStreaming.SetLockStatus(veh, true);
                }
                else
                {
                    if (vData.Fuel > 0)
                        VehicleStreaming.SetEngineState(veh, true);
                    else
                        VehicleStreaming.SetEngineState(veh, false);
                }

                if (Type != -1)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                            entityVehicles.Remove(number);
                        }
                        catch { }
                    });
                }

                VehicleManager.ApplyCustomization(veh);
            }, 100);
        }
        public void SendVehicleIntoGarage(string number)
        {
            vehiclesOut.Remove(number);
            VehicleManager.Vehicles[number].Position = null;
            if (Type != -1) SpawnCar(number);
        }
        public void SendPlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            NAPI.Entity.SetEntityPosition(player, GarageManager.GarageTypes[Type].Position);
            Main.Players[player].InsideGarageID = ID;
            //Костыль
            RespawnCars();
        }
        public void RemovePlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, 0);
            NAPI.Entity.SetEntityPosition(player, Position);
            Main.Players[player].InsideGarageID = -1;
        }
        public void SendAllVehiclesToGarage()
        {
            try
            {
                var toSend = new List<string>();
                foreach (var v in vehiclesOut)
                {
                    toSend.Add(v.Key);
                    VehicleManager.Vehicles[v.Key].Items = NAPI.Data.GetEntityData(v.Value, "ITEMS");
                    var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[v.Key].Model));
                    VehicleManager.Vehicles[v.Key].Fuel = (!NAPI.Data.HasEntityData(v.Value, "PETROL")) ? VehicleManager.VehicleTank[vclass] : NAPI.Data.GetEntityData(v.Value, "PETROL");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(v.Value);
                        }
                        catch { }
                    });
                }
                foreach (var v in toSend)
                {
                    SendVehicleIntoGarage(v);
                }
            }
            catch { }
        }
        public string SendVehiclesInsteadNearest(List<Player> Roommates, Player player)
        {
            var number = "";
            var nearPlayerVehicles = new List<Vehicle>();
            var toSend = new List<string>();
            foreach (var v in vehiclesOut)
            {
                var veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(v.Value);
                var someNear = false;
                foreach (var p in Roommates)
                {
                    if (p.Position.DistanceTo(veh.Position) < 100)
                    {
                        someNear = true;
                        break;
                    }
                }

                if (!someNear)
                {
                    if (player.Position.DistanceTo(veh.Position) < 300) nearPlayerVehicles.Add(veh);
                    toSend.Add(v.Key);
                }
            }

            Vehicle nearestVehicle = null;
            foreach (var v in nearPlayerVehicles)
            {
                if (nearestVehicle == null)
                {
                    nearestVehicle = v;
                    continue;
                }
                if (player.Position.DistanceTo(v.Position) < nearestVehicle.Position.DistanceTo(v.Position)) nearestVehicle = v;
            }

            if (nearestVehicle != null)
            {
                toSend.Remove(nearestVehicle.NumberPlate);
                number = nearestVehicle.NumberPlate;
                VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(nearestVehicle.Position);
                VehicleManager.Vehicles[number].Rotation = JsonConvert.SerializeObject(nearestVehicle.Rotation);
                VehicleManager.Save(number);
                NAPI.Util.ConsoleOutput("delete " + number);
                DeleteCar(number, false);
            }

            try
            {
                foreach (var v in toSend)
                {
                    if (vehiclesOut.ContainsKey(v))
                    {
                        VehicleManager.Vehicles[v].Items = NAPI.Data.GetEntityData(vehiclesOut[v], "ITEMS");
                        var vclass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(VehicleManager.Vehicles[v].Model));
                        VehicleManager.Vehicles[v].Fuel = (!NAPI.Data.HasEntityData(vehiclesOut[v], "PETROL")) ? VehicleManager.VehicleTank[vclass] : NAPI.Data.GetEntityData(vehiclesOut[v], "PETROL");
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                NAPI.Entity.DeleteEntity(vehiclesOut[v]);
                            }
                            catch { };
                            SendVehicleIntoGarage(v);
                        });
                    }
                }
            }
            catch (Exception e) { Log.Write($"SendVehiclesInsteadNearest: " + e.Message, Nlogs.Type.Error); }

            return number;
        }
        public void CreateInterior()
        {
            #region Creating Interior ColShape
            intMarker = NAPI.Marker.CreateMarker(1, GarageManager.GarageTypes[Type].Position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intShape = NAPI.ColShape.CreateCylinderColShape(GarageManager.GarageTypes[Type].Position - new Vector3(0, 0, 1.12), 1f, 4f, (uint)Dimension);
            intShape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 41);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityEnterColShape: " + ex.Message); }
            };
            intShape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion
        }
    }
}