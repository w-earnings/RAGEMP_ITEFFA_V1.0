﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using iTeffa.Interface;
using Newtonsoft.Json;
using iTeffa.Settings;
using MySqlConnector;

namespace iTeffa.Globals
{
    class VehicleManager : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Vehicle");
        private static readonly Random Rnd = new Random();
        public static SortedDictionary<string, VehicleData> Vehicles = new SortedDictionary<string, VehicleData>();
        public static SortedDictionary<int, int> VehicleTank = new SortedDictionary<int, int>()
        {
            { -1, 100 },
            { 0, 120 }, // compacts
            { 1, 150 }, // Sedans
            { 2, 200 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 130 }, // Muscle
            { 5, 150 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 150 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 200 }, // Off-Road
            { 10, 150 }, // Industrial
            { 11, 150 }, // Utility
            { 12, 150 }, // Vans
            { 13, 1   }, // cycles
            { 14, 300 }, // Boats
            { 15, 400 }, // Helicopters
            { 16, 500 }, // Planes
            { 17, 130 }, // Service
            { 18, 200 }, // Emergency
            { 19, 150 }, // Military
            { 20, 150 }, // Commercial
            // 21 trains
        };
        public static SortedDictionary<int, int> VehicleRepairPrice = new SortedDictionary<int, int>()
        {
            { -1, 100 }, // compacts
            { 0, 100 }, // compacts
            { 1, 100 }, // Sedans
            { 2, 100 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 100 }, // Muscle
            { 5, 100 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 100 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 100 }, // Off-Road
            { 10, 100 }, // Industrial
            { 11, 100 }, // Utility
            { 12, 100 }, // Vans
            { 13, 100 }, // 13 cycles
            { 14, 100 }, // Boats
            { 15, 100 }, // Helicopters
            { 16, 100 }, // Planes
            { 17, 100 }, // Service
            { 18, 100 }, // Emergency
            { 19, 100 }, // Military
            { 20, 100 }, // Commercial
            // 21 trains
        };
        private static readonly SortedDictionary<int, int> PetrolRate = new SortedDictionary<int, int>()
        {
            { -1, 0 },
            { 0, 1 }, // compacts
            { 1, 1 }, // Sedans
            { 2, 1 }, // SUVs
            { 3, 1 }, // Coupes
            { 4, 1 }, // Muscle
            { 5, 1 }, // Sports
            { 6, 1 }, // Sports (classic?)
            { 7, 1 }, // Super
            { 8, 1 }, // Motorcycles
            { 9, 1 }, // Off-Road
            { 10, 1 }, // Industrial
            { 11, 1 }, // Utility
            { 12, 1 }, // Vans
            { 13, 0 }, // Cycles
            { 14, 1 }, // Boats
            { 15, 1 }, // Helicopters
            { 16, 1 }, // Planes
            { 17, 1 }, // Service
            { 18, 1 }, // Emergency
            { 19, 1 }, // Military
            { 20, 1 }, // Commercial
            // 21 trains
        };

        public VehicleManager()
        {
            try
            {
                Timers.StartTask("fuel", 30000, () => FuelControl());

                Log.Write("Loading Vehicles...");
                DataTable result = Database.QueryRead("SELECT * FROM `vehicles`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", Plugins.Logs.Type.Warn);
                    return;
                }
                int count = 0;
                foreach (DataRow Row in result.Rows)
                {
                    count++;
                    VehicleData data = new VehicleData
                    {
                        Holder = Convert.ToString(Row["holder"]),
                        Model = Convert.ToString(Row["model"]),
                        Health = Convert.ToInt32(Row["health"]),
                        Fuel = Convert.ToInt32(Row["fuel"]),
                        Price = Convert.ToInt32(Row["price"]),
                        Components = JsonConvert.DeserializeObject<VehicleCustomization>(Row["components"].ToString()),
                        Items = JsonConvert.DeserializeObject<List<nItem>>(Row["items"].ToString()),
                        Position = Convert.ToString(Row["position"]),
                        Rotation = Convert.ToString(Row["rotation"]),
                        KeyNum = Convert.ToInt32(Row["keynum"]),
                        Dirt = (float)Row["dirt"]
                    };
                    Vehicles.Add(Convert.ToString(Row["number"]), data);
                }
                Log.Write($"Vehicles are loaded ({count})", Plugins.Logs.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }

        private static void FuelControl()
        {
            NAPI.Task.Run(() =>
            {
                List<Vehicle> allVehicles = NAPI.Pools.GetAllVehicles();
                if (allVehicles.Count == 0) return;
                foreach (Vehicle veh in allVehicles)
                {
                    object f = null;
                    try
                    {
                        if (!veh.HasSharedData("PETROL")) continue;
                        if (!VehicleStreaming.GetEngineState(veh)) continue;

                        f = veh.GetSharedData<int>("PETROL");
                        int fuel = (int)f;

                        if (fuel == 0) continue;
                        if (fuel - PetrolRate[veh.Class] <= 0)
                        {
                            fuel = 0;
                            VehicleStreaming.SetEngineState(veh, false);
                        }
                        else fuel -= PetrolRate[veh.Class];
                        veh.SetSharedData("PETROL", fuel);
                    }
                    catch (Exception e)
                    {
                        Log.Write($"FUELCONTROL_TIMER: {veh.NumberPlate} {f.ToString()}\n{e.Message}", Plugins.Logs.Type.Error);
                    }
                }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>
                    {
                        player
                    };
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (!vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Add(player);
                }

                if (player.VehicleSeat == 0)
                {
                    if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION")
                    {
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 14 && vehicle.DisplayName == "BARRACKS")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if ((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13))
                            {
                                if (DateTime.Now.Hour < 10)
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_IMPOSSIBLE_SIT, 3000);
                                    return;
                                }
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_START_ENGINE, 3000);
                                return;
                            }
                            else if (fracid == 14)
                            {
                                if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                                {
                                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NOT_HAVE_ACCESS, 3000);
                                    WarpPlayerOutOfVehicle(player);
                                    return;
                                }
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_START_ENGINE, 3000);
                                return;
                            }
                            else
                                WarpPlayerOutOfVehicle(player);
                        }
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == Main.Players[player].FractionID)
                        {
                            if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NOT_HAVE_ACCESS, 3000);
                                WarpPlayerOutOfVehicle(player);
                                return;
                            }
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_START_ENGINE, 3000);
                        }
                        else
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NOT_HAVE_ACCESS, 3000);
                            WarpPlayerOutOfVehicle(player);
                            return;
                        }
                    }
                    else if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" && player.GetData<Vehicle>("WORK") == vehicle)
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_START_ENGINE, 3000);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>();
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicleAttempt: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void API_onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.IsInVehicle)
                {
                    Vehicle vehicle = player.Vehicle;
                    if (!vehicle.HasData("OCCUPANTS"))
                    {
                        List<Player> occupantsList = new List<Player>();
                        vehicle.SetData("OCCUPANTS", occupantsList);
                    }
                    else
                    {
                        if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                    }
                }

                if (NAPI.Data.HasEntityData(player, "WORK_CAR_EXIT_TIMER"))
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void WarpPlayerOutOfVehicle(Player player)
        {
            Vehicle vehicle = player.Vehicle;
            if (vehicle == null) return;

            if (!vehicle.HasData("OCCUPANTS"))
            {
                List<Player> occupantsList = new List<Player>();
                vehicle.SetData("OCCUPANTS", occupantsList);
            }
            else
            {
                if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
            }
            player.WarpOutOfVehicle();
        }

        public static List<Player> GetVehicleOccupants(Vehicle vehicle)
        {
            if (!vehicle.HasData("OCCUPANTS"))
                return new List<Player>();
            else
                return vehicle.GetData<List<Player>>("OCCUPANTS");
        }

        public static void RepairCar(Vehicle vehicle)
        {
            vehicle.Repair();
            VehicleStreaming.UpdateVehicleSyncData(vehicle, new VehicleStreaming.VehicleSyncData());
        }

        public static string Create(string Holder, string Model, Color Color1, Color Color2, Color Color3, int Health = 1000, int Fuel = 100, int Price = 0)
        {
            VehicleData data = new VehicleData
            {
                Holder = Holder,
                Model = Model,
                Health = Health,
                Fuel = Fuel,
                Price = Price,
                Components = new VehicleCustomization()
            };
            data.Components.PrimColor = Color1;
            data.Components.SecColor = Color2;
            data.Components.NeonColor = Color3;
            data.Items = new List<nItem>();
            data.Dirt = 0.0F;

            string Number = GenerateNumber();
            Vehicles.Add(Number, data);
            Database.Query("INSERT INTO `vehicles`(`number`, `holder`, `model`, `health`, `fuel`, `price`, `components`, `items`)" +
                $" VALUES ('{Number}','{Holder}','{Model}',{Health},{Fuel},{Price},'{JsonConvert.SerializeObject(data.Components)}','{JsonConvert.SerializeObject(data.Items)}')");
            Log.Write("Created new vehicle with number: " + Number);
            return Number;
        }
        public static void Remove(string Number, Player player = null)
        {
            if (!Vehicles.ContainsKey(Number)) return;
            try
            {
                Houses.House house = Houses.HouseManager.GetHouse(Vehicles[Number].Holder, true);
                if (house != null)
                {
                    Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(Number);
                }
            }
            catch { }
            Vehicles.Remove(Number);
            Database.Query($"DELETE FROM `vehicles` WHERE number='{Number}'");
        }
        public static void Spawn(string Number, Vector3 Pos, float Rot, Player owner)
        {
            if (!Vehicles.ContainsKey(Number))
            {
                Log.Write(owner.Name + " failed to spawn vehicle " + Number);
                return;
            }

            VehicleData data = Vehicles[Number];
            VehicleHash model = (VehicleHash)NAPI.Util.GetHashKey(data.Model);
            Vehicle veh = NAPI.Vehicle.CreateVehicle(model, Pos, Rot, 0, 0);

            veh.Health = data.Health;
            veh.NumberPlate = Number;
            veh.SetSharedData("PETROL", data.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", owner);
            veh.SetData("ITEMS", data.Items);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, Number);
            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            ApplyCustomization(veh);
            owner.SetIntoVehicle(veh, 0);
        }
        public static bool Save(string Number)
        {
            if (!Vehicles.ContainsKey(Number)) return false;
            VehicleData data = Vehicles[Number];
            string items = JsonConvert.SerializeObject(data.Items);
            if (string.IsNullOrEmpty(items) || items == null) items = "[]";
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = "UPDATE `vehicles` SET holder=@hold, model=@model, health=@hp, fuel=@fuel, components=@comp, items=@it,position=@pos,rotation=@rot,keynum=@keyn,dirt=@dirt WHERE number=@numb"
            };
            cmd.Parameters.AddWithValue("@hold", data.Holder);
            cmd.Parameters.AddWithValue("@model", data.Model);
            cmd.Parameters.AddWithValue("@hp", data.Health);
            cmd.Parameters.AddWithValue("@fuel", data.Fuel);
            cmd.Parameters.AddWithValue("@comp", JsonConvert.SerializeObject(data.Components));
            cmd.Parameters.AddWithValue("@it", items);
            cmd.Parameters.AddWithValue("@pos", data.Position);
            cmd.Parameters.AddWithValue("@rot", data.Rotation);
            cmd.Parameters.AddWithValue("@keyn", data.KeyNum);
            cmd.Parameters.AddWithValue("@dirt", (byte)data.Dirt);
            cmd.Parameters.AddWithValue("@numb", Number);
            Database.Query(cmd);

            return true;
        }
        public static bool isHaveAccess(Player Player, Vehicle Vehicle)
        {
            if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "WORK")
            {
                if (NAPI.Data.GetEntityData(Player, "WORK") != Vehicle)
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "FRACTION")
            {
                if (Main.Players[Player].FractionID != NAPI.Data.GetEntityData(Vehicle, "FRACTION"))
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "PERSONAL")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "GARAGE")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "HOTEL")
            {
                if (NAPI.Data.HasEntityData(Player, "HOTELCAR") && NAPI.Data.GetEntityData(Player, "HOTELCAR") == Vehicle)
                {
                    return true;
                }
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "RENT")
            {
                if (NAPI.Data.GetEntityData(Vehicle, "DRIVER") == Player)
                {
                    return true;
                }
                else
                    return false;
            }
            return true;
        }
        public static Vehicle getNearestVehicle(Player player, int radius)
        {
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();
            Vehicle nearest_vehicle = null;
            foreach (Vehicle v in all_vehicles)
            {
                if (v.Dimension != player.Dimension) continue;
                if (nearest_vehicle == null && player.Position.DistanceTo(v.Position) < radius)
                {
                    nearest_vehicle = v;
                    continue;
                }
                else if (nearest_vehicle != null)
                {
                    if (player.Position.DistanceTo(v.Position) < player.Position.DistanceTo(nearest_vehicle.Position))
                    {
                        nearest_vehicle = v;
                        continue;
                    }
                }
            }
            return nearest_vehicle;
        }
        public static List<string> getAllPlayerVehicles(string playername)
        {
            List<string> all_number = new List<string>();
            foreach (KeyValuePair<string, VehicleData> accVehicle in Vehicles)
                if (accVehicle.Value.Holder == playername)
                {
                    all_number.Add(accVehicle.Key);
                }
            return all_number;
        }

        public static void sellCar(Player player, Player target)
        {
            player.SetData("SELLCARFOR", target);
            OpenSellCarMenu(player);
        }

        #region Selling Menu
        public static void OpenSellCarMenu(Player player)
        {
            Menu menu = new Menu("sellcar", false, true)
            {
                Callback = callback_sellcar
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Продажа машины"
            };
            menu.Add(menuItem);

            foreach (string number in getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(number, Menu.MenuItem.Button)
                {
                    Text = Vehicles[number].Model + " - " + number
                };
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_sellcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            if (item.ID == "close") return;
            player.SetData("SELLCARNUMBER", item.ID);
            Plugins.Trigger.ClientEvent(player, "openInput", "Продать машину", "Введите цену", 7, "sellcar");
        }
        #endregion

        public static void FracApplyCustomization(Vehicle veh, int fraction)
        {
            try
            {
                if (veh != null)
                {
                    if (!Fractions.Configs.FractionVehicles[fraction].ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Fractions.Configs.FractionVehicles[fraction][veh.NumberPlate].Item7;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);

                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if (data.Headlights >= 0)
                    {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Plugins.Trigger.ClientEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    }
                    else
                    {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }

                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void ApplyCustomization(Vehicle veh)
        {
            try
            {
                if (veh != null)
                {
                    if (!Vehicles.ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Vehicles[veh.NumberPlate].Components;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);
                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if (data.Headlights >= 0)
                    {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Plugins.Trigger.ClientEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    }
                    else
                    {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }

                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, data.PrimColor.Red, data.PrimColor.Green, data.PrimColor.Blue);
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, data.SecColor.Red, data.SecColor.Green, data.SecColor.Blue);

                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);

                    VehicleStreaming.SetVehicleDirt(veh, Vehicles[veh.NumberPlate].Dirt);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void ChangeVehicleDoors(Player player, Vehicle vehicle)
        {
            switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
            {
                case "HOTEL":
                    if (NAPI.Data.GetEntityData(vehicle, "OWNER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }
                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                    }
                    break;
                case "RENT":
                    if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }
                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                    }
                    break;
                case "WORK":
                    if (NAPI.Data.GetEntityData(player, "WORK") != vehicle && Main.Players[player].AdminLVL < 3)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }
                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                    }
                    break;
                case "PERSONAL":

                    bool access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }

                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                        return;
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                        return;
                    }
                case "GARAGE":

                    access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }

                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                        return;
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                        return;
                    }
                case "ADMIN":
                    if (Main.Players[player].AdminLVL == 0)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                        return;
                    }

                    if (VehicleStreaming.GetLockState(vehicle))
                    {
                        VehicleStreaming.SetLockStatus(vehicle, false);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_OPENED_DOORS, 3000);
                        return;
                    }
                    else
                    {
                        VehicleStreaming.SetLockStatus(vehicle, true);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_CLOSED_DOORS, 3000);
                        return;
                    }
                default:
                    return;
            }
            return;
        }
        public static bool canAccessByNumber(Player player, string number)
        {
            List<nItem> items = nInventory.Items[Main.Players[player].UUID];
            string needData = $"{number}_{Vehicles[number].KeyNum}";
            bool access = (items.FindIndex(i => i.Type == ItemType.CarKey && i.Data == needData) != -1);

            if (!access)
            {
                int index = items.FindIndex(i => i.Type == ItemType.KeyRing && new List<string>(Convert.ToString(i.Data).Split('/')).Contains(needData));
                if (index != -1) access = true;
            }

            return access;
        }
        // ///// need refactoring //// //
        public static void onClientEvent(Player sender, string eventName, params object[] args)
        {
            switch (eventName)
            {

                case "engineCarPressed":
                    #region Engine button
                    if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                    if (sender.VehicleSeat != 0)
                    {
                        Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_DRIVERS_SEAT, 3000);
                        return;
                    }
                    Vehicle vehicle = sender.Vehicle;
                    if (vehicle.Class == 13 && Main.Players[sender].InsideGarageID == -1) return;

                    int fuel = vehicle.GetSharedData<int>("PETROL");
                    if (fuel <= 0)
                    {
                        Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_FUEL_TANK_EMPTY, 3000);
                        return;
                    }
                    switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
                    {
                        case "HOTEL":
                            if (NAPI.Data.GetEntityData(vehicle, "OWNER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "SCHOOL":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "beltCarPressed":
                            if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                            bool beltstate = Convert.ToBoolean(args[0]);
                            if (!beltstate) Commands.Controller.RPChat("me", sender, Messages.GEN_VEHICLE_FASTENED_THE_BELT);
                            else Commands.Controller.RPChat("me", sender, Messages.GEN_VEHICLE_UNFASTENED_THE_BELT);
                            break;
                        case "RENT":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "WORK":
                            if (NAPI.Data.GetEntityData(sender, "WORK") != vehicle && Main.Players[sender].AdminLVL < 3)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "FRACTION":
                            if (Main.Players[sender].FractionID != NAPI.Data.GetEntityData(vehicle, "FRACTION"))
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "PERSONAL":

                            bool access = canAccessByNumber(sender, vehicle.NumberPlate);
                            if (!access && Main.Players[sender].AdminLVL < 3)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }

                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "GARAGE":
                            if (Main.Players[sender].InsideGarageID == -1) return;
                            string number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);

                            Houses.Garage garage = Houses.GarageManager.Garages[Main.Players[sender].InsideGarageID];
                            garage.RemovePlayer(sender);

                            garage.GetVehicleFromGarage(sender, number);
                            break;
                        case "QUEST":
                        case "MAFIADELIVERY":
                        case "GANGDELIVERY":
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                        case "ADMIN":
                            if (Main.Players[sender].AdminLVL == 0)
                            {
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_NO_KEY, 3000);
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(vehicle))
                            {
                                VehicleStreaming.SetEngineState(vehicle, false);
                                Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_OFF_ENGINE, 3000);
                            }
                            else
                            {
                                if (NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_low") || NAPI.Data.HasEntityData(sender.Vehicle, "vehicle_colision_high"))
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_ATTEMPT_TO_START, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health < 350)
                                {
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_BADLY_DAMAGED, 1000);
                                    return;
                                }
                                else if (sender.Vehicle.Health > 350)
                                {
                                    VehicleStreaming.SetEngineState(vehicle, true);
                                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_TURNED_ONN_ENGINE, 3000);
                                }
                            }
                            break;
                    }
                    if (VehicleStreaming.GetEngineState(vehicle)) Commands.Controller.RPChat("me", sender, "завел(а) транспортное средство");
                    else Commands.Controller.RPChat("me", sender, "заглушил(а) транспортное средство");
                    return;
                #endregion Engine button
                case "lockCarPressed":
                    #region inVehicle
                    if (NAPI.Player.IsPlayerInAnyVehicle(sender) && sender.VehicleSeat == 0)
                    {
                        vehicle = sender.Vehicle;
                        ChangeVehicleDoors(sender, vehicle);
                        return;
                    }
                    #endregion
                    #region outVehicle
                    vehicle = getNearestVehicle(sender, 10);
                    if (vehicle != null)
                        vehicle.Locked = !vehicle.Locked;
                    if (vehicle != null)
                        ChangeVehicleDoors(sender, vehicle);
                    #endregion
                    break;
            }
        }

        [ServerEvent(Event.VehicleDeath)]
        public void Event_vehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") == "ADMIN") return;
                string access = vehicle.GetData<string>("ACCESS");
                switch (access)
                {
                    case "PERSONAL":
                        {
                            Player owner = vehicle.GetData<Player>("OWNER");
                            string number = vehicle.NumberPlate;

                            Plugins.Notice.Send(owner, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "Ваша машина уничтожена", 3000);

                            VehicleData vData = Vehicles[number];
                            vData.Items = new List<nItem>();
                            vData.Health = 0;

                            vehicle.Delete();
                        }
                        return;
                    case "WORK":
                        Player player = vehicle.GetData<Player>("DRIVER");
                        if (player != null)
                        {
                            string got_salary = $"Вы получили зарплату в {player.GetData<int>("PAYMENT")}$";
                            string paymentMsg = (player.GetData<int>("PAYMENT") == 0) ? "" : got_salary;
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, Messages.GEN_VEHICLE_WAS_DESTROYED + paymentMsg, 3000);
                            player.SetData("ON_WORK", false);
                            Customization.ApplyCharacter(player);
                        }
                        string work = vehicle.GetData<string>("TYPE");
                        switch (work)
                        {
                            case "BUS":
                                Working.Bus.respawnBusCar(vehicle);
                                return;
                            case "MOWER":
                                Working.Lawnmower.respawnCar(vehicle);
                                return;
                            case "TAXI":
                                Working.Lawnmower.respawnCar(vehicle);
                                return;
                            case "TRUCKER":
                                if (player != null) Working.Truckers.cancelOrder(player);
                                Working.Truckers.respawnCar(vehicle);
                                return;
                            case "COLLECTOR":
                                Working.Collector.respawnCar(vehicle);
                                return;
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, Plugins.Logs.Type.Error); }
        }

        private static string GenerateNumber()
        {
            string number;
            do
            {
                number = "";
                number += (char)Rnd.Next(0x0041, 0x005A);
                for (int i = 0; i < 3; i++)
                    number += (char)Rnd.Next(0x0030, 0x0039);
                number += (char)Rnd.Next(0x0041, 0x005A);

            } while (Vehicles.ContainsKey(number));
            return number;
        }

        internal class VehicleData
        {
            public string Holder { get; set; }
            public string Model { get; set; }
            public int Health { get; set; }
            public int Fuel { get; set; }
            public int Price { get; set; }
            public VehicleCustomization Components { get; set; }
            public List<nItem> Items { get; set; }
            public string Position { get; set; }
            public string Rotation { get; set; }
            public int KeyNum { get; set; }
            public float Dirt { get; set; }
        }

        internal class VehicleCustomization
        {
            public Color PrimColor = new Color(0, 0, 0);
            public Color SecColor = new Color(0, 0, 0);
            public Color NeonColor = new Color(0, 0, 0, 0); // NeonTest

            public int PrimModColor = -1;
            public int SecModColor = -1;
            public int Muffler = -1;
            public int SideSkirt = -1;
            public int Hood = -1;
            public int Spoiler = -1;
            public int Lattice = -1;
            public int Wings = -1;
            public int Roof = -1;
            public int Vinyls = -1;
            public int FrontBumper = -1;
            public int RearBumper = -1;
            public int Engine = -1;
            public int Turbo = -1;
            public int Horn = -1;
            public int Transmission = -1;
            public int WindowTint = 0;
            public int Suspension = -1;
            public int Brakes = -1;
            public int Headlights = -1;
            public int NumberPlate = 0;
            public int Wheels = -1;
            public int WheelsType = 0;
            public int WheelsColor = 0;
            public int Armor = -1;
        }

        public static void changeOwner(string oldName, string newName)
        {
            List<string> toChange = new List<string>();
            lock (Vehicles)
            {
                foreach (KeyValuePair<string, VehicleData> vd in Vehicles)
                {
                    if (vd.Value.Holder != oldName) continue;
                    Log.Write($"The car was found! [{vd.Key}]");
                    toChange.Add(vd.Key);
                }
                foreach (string num in toChange)
                {
                    if (Vehicles.ContainsKey(num)) Vehicles[num].Holder = newName;
                }

                Database.Query($"UPDATE `vehicles` SET `holder`='{newName}' WHERE `holder`='{oldName}'");
            }
        }
    }
}
