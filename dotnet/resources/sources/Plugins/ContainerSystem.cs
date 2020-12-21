using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using iTeffa.Settings;
using iTeffa.Globals;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

namespace iTeffa.Plugins
{
    class ContainerSystem : Script
    {
        private static readonly Nlogs Log = new Nlogs("SysContainers");

        public static List<Container> containers = new List<Container>();

        #region #TX1 MainLoad
        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                Blip portblip = NAPI.Blip.CreateBlip(50, new Vector3(1210, -2987, 0), 0.91f, 6, Main.StringToU16("Система контейнеров"), 254, 0, true, 0, 0);

                var table = Connect.QueryRead($"SELECT * FROM `containers`");
                if (table == null || table.Rows.Count == 0)
                {
                    Log.Write("Containers return null result.", Nlogs.Type.Warn);
                    return;
                }
                foreach (DataRow Row in table.Rows)
                {
                    Container data = new Container(
                        Convert.ToInt32(Row["id"]),
                        Convert.ToString(Row["name"]),
                        JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString()),
                        JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString()),
                        Convert.ToInt32(Row["price"]),
                        Convert.ToBoolean(Row["donate"])
                        );

                    Dictionary<string, int> autos = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["loot"].ToString());

                    foreach (var caritem in autos)
                    {
                        for (int i = 0; i < caritem.Value; i++)
                        {
                            data.Loots.Add(caritem.Key);
                        }
                    }
                    containers.Add(data);
                }

                Log.Write($"Загружены {containers.Count} контейнера");
            }
            catch (Exception e)
            {
                Log.Write($"Контейнеры: {e.Message}", Nlogs.Type.Error);
            }
        }
        #endregion

        #region #TX1 Change state containers
        [Command("boxstate")] //команда для вручной активации контейнеров
        public void ChangeStateContainers(Player player, bool state)
        {
            if (!Globals.Group.CanUseCmd(player, "statebox")) return;
            foreach (var item in containers)
            {
                item.Visible(state);
            }
            if (state)
                NAPI.Chat.SendChatMessageToAll("!{#fc4122} [Порт]: !{#ffffff}" + "В штат привезли новую партию контейнеров!");
        }
        #endregion

        #region #TX1 Open menu container
        public static void OpenMenuContainer(Player player)
        {
            if (!player.HasData("ContainerID")) return;
            Container container = containers[player.GetData<int>("ContainerID")];
            if (!container.State) return;
            Trigger.ClientEvent(player, "openContainerMenu", container);
        }
        #endregion

        #region #TX1 Open container
        [RemoteEvent("openContainer")]
        public static void OpenContainer(Player player)
        {
            try
            {
                if (!player.HasData("ContainerID")) return;
                Container container = containers[player.GetData<int>("ContainerID")];
                if (!container.State) return;
                if (container.DoorState) return;
                if (container.Donate)
                {
                    if (Main.Accounts[player].Coins < container.Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 2500);
                        return;
                    }
                    Finance.Wallet.ChangeDonateBalance(player, -container.Price);
                }
                else
                {
                    if (Main.Players[player].Money < container.Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 2500);
                        return;
                    }
                    Finance.Wallet.Change(player, -container.Price);
                }
                container.OpenDoor();
                container.GenerateLoot(player);
                container.Visible(false);
            }
            catch (Exception e)
            {
                Log.Write(e.Message, Nlogs.Type.Error);
            }
        }
        #endregion
    }

    public class Container
    {
        public static Random rnd = new Random();
        public int ID { get; set; } //номер контейнера
        public string Name { get; set; }
        public int Price { get; set; } //цена
        public bool Donate { get; set; } //за донат или нет
        public bool State { get; set; } = false; //активен или нет
        public bool DoorState { get; set; } = false; //открыты или закрыты
        public List<string> Loots = new List<string>();

        public GTANetworkAPI.Object Model;
        public GTANetworkAPI.Object Door_l;
        public GTANetworkAPI.Object Door_R;
        public GTANetworkAPI.Object Fence; //стена

        public GTANetworkAPI.ColShape shape;
        public GTANetworkAPI.Marker marker;
        public GTANetworkAPI.TextLabel label;

        public Container(int id, string name, Vector3 pos, Vector3 rot, int price, bool donate = false, string model = "prop_container_02a", string door_l = "prop_cntrdoor_ld_l", string door_r = "prop_cntrdoor_ld_r", string fence = "prop_fncsec_01b")
        {
            ID = id;
            Name = name;
            Price = price;
            Donate = donate;
            Model = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(model), pos, rot, 255, 0);
            Door_l = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(door_l), pos + new Vector3(1.3, 6.08, 1.4), rot, 255, 0);
            Door_R = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(door_r), pos + new Vector3(-1.3, 6.08, 1.4), rot, 255, 0);
            Fence = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(fence), pos + new Vector3(-1.25, 6.05, 0.5), rot, 0, 0);

            label = NAPI.TextLabel.CreateTextLabel("Неактивен", pos + new Vector3(-2, 6.7, 1), 10f, 0.2f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            marker = NAPI.Marker.CreateMarker(1, pos + new Vector3(-2, 6.7, 0), new Vector3(), new Vector3(), 1f, new Color(28, 90, 19, 0));
            shape = NAPI.ColShape.CreateCylinderColShape(pos + new Vector3(-2, 6.7, 0), 1f, 2f, 0);

            shape.OnEntityEnterColShape += (s, player) =>
            {
                if (!State) return;
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 814);
                NAPI.Data.SetEntityData(player, "ContainerID", ID);
            };
            shape.OnEntityExitColShape += (s, player) =>
            {
                if (!State) return;
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
                NAPI.Data.ResetEntityData(player, "ContainerID");
            };
        }

        #region #TX1 active or diActive
        public void Visible(bool state)
        {
            if (state)
            {
                label.Text = $"{Name} ID:~r~{ID} \nЦена: ~g~{Price}{(Donate ? "Coins" : "$")}";
                marker.Color = new Color(21, 90, 11, 170);
            }
            else
            {
                label.Text = "Неактивен";
                marker.Color = new Color(22, 99, 11, 0);
            }
            State = state;
        }
        #endregion

        #region #TX1 Open door
        public void OpenDoor()
        {
            int i = 0;
            DoorState = true;
            Timers.Start($"openDoorContainer{ID}", 1, () =>
            {
                ++i;
                if (i >= 121)
                {
                    Timers.Stop($"openDoorContainer{ID}"); //timerstop
                }
                Door_l.Rotation -= new Vector3(0, 0, 1);
                Door_R.Rotation -= new Vector3(0, 0, -1);
            });
        }
        #endregion

        #region #TX1 Close door
        public void CloseDoor(Vehicle veh)
        {
            int i = 0;
            Timers.Start($"closeDoorContainer{ID}", 1, () =>
            {
                ++i;
                if (i >= 121)
                {
                    DoorState = false;
                    NAPI.Task.Run(() => { veh.Delete(); });
                    Timers.Stop($"closeDoorContainer{ID}"); //timerstop
                }
                Door_l.Rotation += new Vector3(0, 0, 1);
                Door_R.Rotation += new Vector3(0, 0, -1);
            });
        }
        #endregion

        #region #TX1 Moving items
        public void Moveloots()
        {
            for (int i = 0; i < Loots.Count; i++)
            {
                Random rnd = new Random();
                int lastindex = rnd.Next(0, Loots.Count);
                string item = Loots[lastindex];
                Loots.RemoveAt(lastindex);
                Loots.Add(item);
            }
        }
        #endregion

        #region #TX1 Generate loot
        public void GenerateLoot(Player player)
        {
            Moveloots();
            string vName = Loots[rnd.Next(0, Loots.Count)];
            Vehicle veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vName), Model.Position, Model.Rotation.Z + 180, 0, 0);
            veh.Dimension = 0;
            veh.NumberPlate = "AUCPRIZE";
            veh.PrimaryColor = 0;
            veh.SecondaryColor = 0;
            veh.Health = 1000;
            veh.Locked = true;
            VehicleStreaming.SetEngineState(veh, false);

            string vNumber = "no";
            vNumber = VehicleManager.Create(player.Name, vName, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
            var house = Houses.HouseManager.GetHouse(player, true);
            if (house == null || house.GarageID == 0)
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш приз - {vName}", 2500);
            else
            {
                var garage = Houses.GarageManager.Garages[house.GarageID];
                if (vNumber != "no")
                {
                    garage.SpawnCar(vNumber);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш приз - {vName} будет доставлен в гараж", 2500);
                }
            }
            NAPI.Task.Run(() => { CloseDoor(veh); }, 12600);
        }
        #endregion
    }
}
