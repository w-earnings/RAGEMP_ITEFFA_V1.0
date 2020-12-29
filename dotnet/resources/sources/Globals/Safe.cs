using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using iTeffa.Interface;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    public class Safe
    {
        public int ID { get; private set; }
        public Vector3 Position { get; private set; }
        public float Rotation { get; private set; }
        public int MinAmount { get; private set; }
        public int MaxAmount { get; private set; }
        public string Address { get; private set; }

        [JsonIgnore]
        public bool IsOpen { get; private set; }

        [JsonIgnore]
        public List<int> LockAngles { get; private set; } = new List<int>();

        [JsonIgnore]
        public Player Occupier { get; set; }

        [JsonIgnore]
        public GTANetworkAPI.Object Object { get; private set; }

        [JsonIgnore]
        private GTANetworkAPI.Object DoorObject;

        [JsonIgnore]
        public TextLabel Label;

        [JsonIgnore]
        private ColShape colShape;

        [JsonIgnore]
        public int SafeLoot = 0;

        [JsonIgnore]
        private int RemainingSeconds;

        [JsonIgnore]
        private string Timer;

        [JsonIgnore]
        public Blip Blip { get; set; } = null;

        [JsonIgnore]
        public DateTime BlipSet { get; set; } = DateTime.Now;

        public Safe(int id, Vector3 position, float rotation, int minamount, int maxamount, string address)
        {
            ID = id;
            Position = position;
            Rotation = rotation;
            MinAmount = minamount;
            MaxAmount = maxamount;
            Address = address;
        }

        public void Create()
        {
            Object = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("v_ilev_gangsafe"), Position, new Vector3(0.0, 0.0, Rotation), 255, 0);
            DoorObject = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("v_ilev_gangsafedoor"), Position, new Vector3(0.0, 0.0, Rotation), 255, 0);
            colShape = NAPI.ColShape.CreateCylinderColShape(Position, 1.25f, 1.0f, 0);

            Label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Сейф"), Position + new Vector3(0, 0, 1.05), 5f, 0.65f, 0, new Color(255, 255, 255), false);

            for (int i = 0; i < 3; i++)
                LockAngles.Add(SafeMain.SafeRNG.Next(0, 361));

            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    player.SetData("temp_SafeID", ID);
                    player.SetData("INTERACTIONCHECK", 43);
                }
                catch (Exception e) { Console.WriteLine("colShape.OnEntityEnterColShape: " + e.ToString()); }
            };

            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    if (player == Occupier) Occupier = null;
                    player.SetData("INTERACTIONCHECK", 0);
                    Trigger.ClientEvent(player, "dial", "close");
                    player.ResetData("temp_SafeID");
                }
                catch (Exception e) { Console.WriteLine("colShape.OnEntityExitColShape: " + e.ToString()); }
            };
        }

        public void Loot(Player player)
        {

            if (player.HasData("HEIST_DRILL"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас уже есть сумка", 3000);
                return;
            }

            if (SafeLoot == 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В сейфе больше нет денег", 3000);
                return;
            }

            var money = (SafeLoot >= SafeMain.MaxMoneyInBag) ? SafeMain.MaxMoneyInBag : SafeLoot;
            if (player.HasData("HAND_MONEY"))
            {
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.BagWithMoney);
                var lefts = (item == null) ? 0 : Convert.ToInt32(item.Data.ToString());
                if (lefts == SafeMain.MaxMoneyInBag)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Ваша сумка полностью забита деньгами", 3000);
                    return;
                }
                if (money + lefts > SafeMain.MaxMoneyInBag)
                    money = (SafeMain.MaxMoneyInBag - lefts);
                lefts += money;
                item.Data = $"{lefts}";

                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Теперь в Вашей сумке {lefts}$", 3000);
            }
            else
            {
                var item = new nItem(ItemType.BagWithMoney, 1, $"{money}");
                nInventory.Items[Main.Players[player].UUID].Add(item);

                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы взяли сумку с {money}$", 3000);
            }
            Dashboard.sendItems(player);

            player.SetClothes(5, 45, 0);
            player.SetData("HAND_MONEY", true);

            SafeLoot -= money;
            return;
        }

        public void Countdown()
        {
            RemainingSeconds--;

            if (RemainingSeconds < 1)
            {
                Label.Text = "~b~Сейф";
                for (int i = 0; i < 3; i++)
                    LockAngles[i] = SafeMain.SafeRNG.Next(10, 351);
                SetDoorOpen(false);
            }
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(RemainingSeconds);
                Label.Text = string.Format("~r~Сейф ~n~~w~{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
                Label.Text += $"\n~b~{SafeLoot}$";
            }
        }

        public void SetDoorOpen(bool is_open)
        {
            IsOpen = is_open;
            DoorObject.Rotation = new Vector3(0.0, 0.0, (is_open) ? Rotation + 105.0 : Rotation);

            if (is_open)
            {
                RemainingSeconds = SafeMain.SafeRespawnTime;

                Timer = Timers.Start(1000, () => {
                    Countdown();
                });
            }
            else
            {
                SafeLoot = 0;

                if (Timer != null) Timers.Stop(Timer);
                Timer = null;
            }
        }

        public void Destroy(bool check_players = false)
        {
            if (check_players)
            {
                foreach (var player in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(player)) continue;
                    if (player.Position.DistanceTo(colShape.Position) > 1.5f) continue;

                    Trigger.ClientEvent(player, "SetSafeNearby", false);
                    player.ResetData("temp_SafeID");
                }
            }

            Object.Delete();
            DoorObject.Delete();
            Label.Delete();

            NAPI.ColShape.DeleteColShape(colShape);
            if (Timer != null) Timers.Stop(Timer);
        }
    }
}
