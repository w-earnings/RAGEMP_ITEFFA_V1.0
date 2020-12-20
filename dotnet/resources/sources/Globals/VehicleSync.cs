using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    public class VehicleStreaming : Script
    {
        private static readonly nLog Log = new nLog("VehicleStreaming");
        public class VehicleSyncData
        {
            public bool Locked { get; set; } = false;
            public bool Engine { get; set; } = false;
            public bool LeftIL { get; set; } = false;
            public bool RightIL { get; set; } = false;
            public float Dirt { get; set; } = 0.0f;
            public float BodyHealth { get; set; } = 1000.0f;
            public float EngineHealth { get; set; } = 1000.0f;
            public List<int> Door { get; set; } = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] Window { get; set; } = new int[4] { 0, 0, 0, 0 };
            public int[] Wheel { get; set; } = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        private static readonly Dictionary<Entity, VehicleSyncData> VehiclesSyncDatas = new Dictionary<Entity, VehicleSyncData>();

        [ServerEvent(Event.EntityDeleted)]
        public void Event_EntityDeleted(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Vehicle && VehiclesSyncDatas.ContainsKey(entity)) VehiclesSyncDatas.Remove(entity);
            }
            catch (Exception e) { Log.Write("Event_EntityDeleted: " + e.Message); }
        }

        [RemoteEvent("syncSirens")]
        public void VehSirenSync(Player c)
        {
            if (c.Vehicle.HasSharedData("silentMode") && c.Vehicle.GetSharedData<bool>("silentMode") == true)
            {
                c.Vehicle.SetSharedData("silentMode", false);
            }
            else
            {
                c.Vehicle.SetSharedData("silentMode", true);
            }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void VehStreamExitAttempt(Player player, Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEvent(player, "VehStream_PlayerExitVehicleAttempt", veh, data.Engine);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void VehStreamEnter(Player player, Vehicle veh, sbyte seat)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEvent(player, "VehStream_PlayerEnterVehicle", veh, seat, data.Engine);
        }

        [ServerEvent(Event.VehicleDamage)]
        public void OnVehicleDamageHandler(Vehicle entity, float bodyHealthLoss, float engineHealthLoss)
        {
            List<Vehicle> allVehicles = NAPI.Pools.GetAllVehicles();
            if (allVehicles.Count == 0) return;
            foreach (Vehicle veh in allVehicles)
            {
                var players = NAPI.Pools.GetAllPlayers();
                foreach (var player in players)
                {
                    if (veh.Health >= 800)
                    {
                        if (player.IsInVehicle)
                        {
                            if (player.Vehicle == veh)
                            {
                                if (player.VehicleSeat == 0)
                                {
                                    int damagged = (int)veh.Health;
                                    var rnd = new Random();
                                    var damage = rnd.Next(5, 10);
                                    veh.Health -= damage;
                                    veh.SetSharedData("HEALTHVEHICLE", damagged);
                                    player.TriggerEvent("createNewHeadNotificationAdvanced", "-" + damage + "Damaged" + "~r~Health=" + veh.Health);
                                }
                            }
                        }
                    }
                    else if (veh.Health < 800)
                    {
                        if (player.IsInVehicle)
                        {
                            if (player.Vehicle == veh)
                            {
                                if (player.VehicleSeat == 0)
                                {
                                    var rndm = new Random();
                                    var emkan = rndm.Next(1, 10);
                                    if (emkan > 6)
                                    {
                                        if (!NAPI.Data.HasEntityData(player.Vehicle, "vehicle_colision"))
                                        {
                                            player.TriggerEvent("createNewHeadNotificationAdvanced", "~r~Автомобиль поврежден, ~g~Повторите попытку");
                                            NAPI.Data.SetEntityData(veh, "vehicle_colision", true);
                                            NAPI.Vehicle.SetVehicleEngineStatus(veh, false);
                                            var damage = rndm.Next(5, 7);
                                            veh.Health -= damage;
                                            int random_timer = rndm.Next(15, 45);
                                            int damagged = (int)veh.Health;
                                            veh.SetSharedData("HEALTHVEHICLE", damagged);
                                            NAPI.Task.Run(() =>
                                            {
                                                NAPI.Data.ResetEntityData(veh, "vehicle_colision");
                                            }, random_timer * 1000);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /*
        [ServerEvent(Event.VehicleDamage)]
        public void VehDamage(Vehicle veh, float bodyHealthLoss, float engineHealthLoss)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.BodyHealth -= bodyHealthLoss;
            data.EngineHealth -= engineHealthLoss;

            UpdateVehicleSyncData(veh, data);

            if (NAPI.Vehicle.GetVehicleDriver(veh) != default(Player))
                NAPI.ClientEvent.TriggerClientEvent((Player)NAPI.Vehicle.GetVehicleDriver(veh), "VehStream_PlayerExitVehicleAttempt", veh);
        }
        */

        public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[(int)wheel] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus_Single", veh.Handle, (int)wheel, (int)state);
        }

        public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WheelState)data.Wheel[(int)wheel];
        }

        public static void SetVehicleWindowState(Vehicle veh, WindowID window, WindowState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Window[(int)window] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWindowStatus_Single", veh.Handle, (int)window, (int)state);
        }

        public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WindowState)data.Window[(int)window];
        }

        public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[(int)door] = (int)state;
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetVehicleDoorStatus_Single", veh, (int)door, (int)state);
        }

        public static DoorState GetDoorState(Vehicle veh, DoorID door)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (DoorState)data.Door[(int)door];
        }

        public static void SetEngineState(Vehicle veh, bool status)
        {
            NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Engine = status;
            data.RightIL = false;
            data.LeftIL = false;
            veh.SetSharedData("rightlight", false);
            veh.SetSharedData("leftlight", false);
            UpdateVehicleSyncData(veh, data);
            NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetEngineStatus", veh, status, true, data.LeftIL, data.RightIL);
        }

        public static bool GetEngineState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Engine;
        }

        public static void SetVehicleIndicatorLights(Vehicle veh, int light, bool state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if (data.Engine)
            {
                if (light == 0)
                {
                    veh.SetSharedData("rightlight", state);
                    data.RightIL = state;
                }
                else
                {
                    veh.SetSharedData("leftlight", state);
                    data.LeftIL = state;
                }
                UpdateVehicleSyncData(veh, data);
                NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleIndicatorLights_Single", veh.Handle, light, state);
            }
        }

        public static bool GetVehicleIndicatorLights(Vehicle veh, bool light)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            if (!light) return data.RightIL;
            else return data.LeftIL;
        }

        public static void SetLockStatus(Vehicle veh, bool status)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            veh.SetSharedData("LOCKED", status);
            data.Locked = status;
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetLockStatus", veh, status);
        }

        public static bool GetLockState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Locked;
        }


        [RemoteEvent("VehStream_RequestFixStreamIn")]
        public void VehicleFixStreamIn(Player player, Vehicle veh)
        {
            try
            {
                if (veh != null && NAPI.Entity.DoesEntityExist(veh))
                {
                    VehicleSyncData data = GetVehicleSyncData(veh);
                    if (data == default(VehicleSyncData)) data = new VehicleSyncData();
                    UpdateVehicleSyncData(veh, data);

                    List<object> vData = new List<object>()
                    {
                        veh.NumberPlate,
                        veh.PrimaryColor,
                        veh.SecondaryColor,
                    };
                    if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "PERSONAL")
                    {
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Components);
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Dirt);
                    }
                    Trigger.ClientEvent(player, "VehStream_FixStreamIn", veh.Handle, JsonConvert.SerializeObject(vData));
                }
                return;
            }
            catch (Exception e) { Log.Write("VehStream_RequestFixStreamIn: " + e.Message, nLog.Type.Error); return; }
        }

        [RemoteEvent("VehStream_SetVehicleDirt")]
        public void SetVehicleDirtLevel(Player player, Vehicle vehicle, float dirt)
        {
            try
            {
                if (vehicle != null) SetVehicleDirt(vehicle, dirt);
            }
            catch (Exception e) { Log.Write("VehStream_SetVehicleDirt: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("VehStream_SetDirtLevel")]
        public void VehStreamSetDirtLevel(Player player, Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();
                data.Dirt = dirt;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        [RemoteEvent("VehStream_SetDoorData")]
        public void VehStreamSetDoorData(Player player, Vehicle veh, int door1state, int door2state, int door3state, int door4state, int door5state, int door6state, int door7state, int door8state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[0] = door1state;
            data.Door[1] = door2state;
            data.Door[2] = door3state;
            data.Door[3] = door4state;
            data.Door[4] = door5state;
            data.Door[5] = door6state;
            data.Door[6] = door7state;
            data.Door[7] = door8state;

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, door1state, door2state, door3state, door4state, door5state, door6state, door7state, door8state);
        }

        [RemoteEvent("VehStream_SetWindowData")]
        public void VehStreamSetWindowData(Player player, Vehicle veh, int window1state, int window2state, int window3state, int window4state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Window[0] = window1state;
            data.Window[1] = window2state;
            data.Window[2] = window3state;
            data.Window[3] = window4state;

            UpdateVehicleSyncData(veh, data);

            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWindowStatus", veh.Handle, window1state, window2state, window3state, window4state);
        }

        [RemoteEvent("VehStream_SetWheelData")]
        public void VehStreamSetWheelData(Player player, Vehicle veh, int wheel1state, int wheel2state, int wheel3state, int wheel4state, int wheel5state, int wheel6state, int wheel7state, int wheel8state, int wheel9state, int wheel10state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[0] = wheel1state;
            data.Wheel[1] = wheel2state;
            data.Wheel[2] = wheel3state;
            data.Wheel[3] = wheel4state;
            data.Wheel[4] = wheel5state;
            data.Wheel[5] = wheel6state;
            data.Wheel[6] = wheel7state;
            data.Wheel[7] = wheel8state;
            data.Wheel[8] = wheel9state;
            data.Wheel[9] = wheel10state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus", veh.Handle, wheel1state, wheel2state, wheel3state, wheel4state, wheel5state, wheel6state, wheel7state, wheel8state, wheel9state, wheel10state);
        }

        [ServerEvent(Event.PlayerEnterVehicleAttempt)]
        public void VehStreamEnterAttempt(Player player, Vehicle veh, sbyte seat)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEvent(player, "VehStream_PlayerEnterVehicleAttempt", veh, seat);
        }

        [ServerEvent(Event.VehicleDoorBreak)]
        public void VehDoorBreak(Vehicle veh, int index)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[index] = 2;

            UpdateVehicleSyncData(veh, data);

            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, data.Door[0], data.Door[1], data.Door[2], data.Door[3], data.Door[4], data.Door[5], data.Door[6], data.Door[7]);
        }

        [RemoteEvent("VehStream_SetIndicatorLightsData")]
        public void VehStreamSetIndicatorLightsData(Player player, Vehicle veh, bool leftstate, bool rightstate)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if (data.Engine)
            {
                data.RightIL = rightstate;
                data.LeftIL = leftstate;
                veh.SetSharedData("rightlight", rightstate);
                veh.SetSharedData("leftlight", leftstate);
                UpdateVehicleSyncData(veh, data);
                Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetVehicleIndicatorLights", veh.Handle, leftstate, rightstate);
            }
        }

        [Command("setvehdirt")]
        public static void setvehdirt(Player player, float dirt)
        {
            if (!Group.CanUseCmd(player, "setvehdirt")) return;
            if (player.Vehicle != null)
            {
                SetVehicleDirt(player.Vehicle, dirt);
            }
        }

        public static void SetVehicleDirt(Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            data.Dirt = dirt;
            UpdateVehicleSyncData(veh, data);

            if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "PERSONAL")
            {
                if (!VehicleManager.Vehicles.ContainsKey(veh.NumberPlate)) return;

                VehicleManager.Vehicles[veh.NumberPlate].Dirt = dirt;
            }

            Trigger.ClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        public static float GetVehicleDirt(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Dirt;
        }
        private static VehicleSyncData GetVehicleSyncData(Vehicle veh)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (VehiclesSyncDatas.ContainsKey(veh))
                            return VehiclesSyncDatas[veh];
                        else
                        {
                            VehiclesSyncDatas.Add(veh, new VehicleSyncData());
                            return VehiclesSyncDatas[veh];
                        }
                    }
                }
            }
            catch { };

            return default;
        }

        public static bool UpdateVehicleSyncData(Vehicle veh, VehicleSyncData data)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (data != null)
                        {
                            if (VehiclesSyncDatas.ContainsKey(veh))
                                VehiclesSyncDatas[veh] = data;
                            else
                                VehiclesSyncDatas.Add(veh, data);
                            NAPI.Data.SetEntitySharedData(veh, "VehicleSyncData", JsonConvert.SerializeObject(data));
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e) { Log.Write("UpdateVehicleSyncData: " + e.Message, nLog.Type.Error); return false; }
        }
    }
}
