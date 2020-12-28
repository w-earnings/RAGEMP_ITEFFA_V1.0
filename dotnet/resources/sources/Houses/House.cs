using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iTeffa.Houses
{
    class House
    {
        public int ID { get; }
        public string Owner { get; private set; }
        public int Type { get; private set; }
        public Vector3 Position { get; }
        public int Price { get; set; }
        public bool Locked { get; private set; }
        public int GarageID { get; set; }
        public int BankID { get; set; }
        public List<string> Roommates { get; set; } = new List<string>();
        [JsonIgnore]
        public int Dimension { get; set; }
        [JsonIgnore]
        public Blip blip;
        [JsonIgnore]
        private readonly TextLabel label;
        [JsonIgnore]
        private readonly ColShape shape;
        [JsonIgnore]
        private ColShape intshape;
        [JsonIgnore]
        private Marker intmarker;
        [JsonIgnore]
        private List<GTANetworkAPI.Object> Objects = new List<GTANetworkAPI.Object>();
        [JsonIgnore]
        private readonly List<NetHandle> PlayersInside = new List<NetHandle>();
        public House(int id, string owner, int type, Vector3 position, int price, bool locked, int garageID, int bank, List<string> roommates)
        {
            ID = id;
            Owner = owner;
            Type = type;
            Position = position;
            Price = price;
            Locked = locked;
            GarageID = garageID;
            BankID = bank;
            Roommates = roommates;

            blip = NAPI.Blip.CreateBlip(Position);
            if (string.IsNullOrEmpty(Owner))
            {
                blip.Name = "Дом на продаже";
                blip.Sprite = 374;
                blip.Color = 25;
            }
            else
            {
                blip.Name = "Дом куплен";
                blip.Sprite = 374;
                blip.Color = 1;
            }
            blip.Scale = 0.6f;
            blip.ShortRange = true;

            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "HOUSEID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 6);
                    Working.Gopostal.GoPostal_onEntityEnterColShape(s, ent);
                    Trigger.ClientEvent(ent, "JobsEinfo");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "HOUSEID");
                    Trigger.ClientEvent(ent, "JobsEinfo2");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };
            label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"House {id}"), position + new Vector3(0, 0, 1.5), 10f, 0.4f, 0, new Color(255, 255, 255), false, 0);
            UpdateLabel();
        }
        public void UpdateLabel()
        {
            try
            {
                string text = $"Дом: #{ID}\n";
                if (!string.IsNullOrEmpty(Owner)) text += $"Дом куплен\n";
                else text += $"Дом продается\n";
                string text2 = "";
                text2 += Main.StringToU16((Locked) ? "~r~Закрыто\n" : "~b~Открыто\n");
                label.Text = Main.StringToU16(text);
            }
            catch (Exception e)
            {
                blip.Color = 48;
                Console.WriteLine(ID.ToString() + e.ToString());
            }
        }
        public void CreateAllFurnitures()
        {
            if (FurnitureManager.HouseFurnitures.ContainsKey(ID))
            {
                if (FurnitureManager.HouseFurnitures[ID].Count >= 1)
                {
                    foreach (var f in FurnitureManager.HouseFurnitures[ID].Values) if (f.IsSet) CreateFurniture(f);
                }
            }
        }
        public void CreateFurniture(HouseFurniture f)
        {
            try
            {
                var obj = f.Create((uint)Dimension);
                NAPI.Data.SetEntityData(obj, "HOUSE", ID);
                NAPI.Data.SetEntityData(obj, "ID", f.ID);
                NAPI.Entity.SetEntityDimension(obj, (uint)Dimension);
                if (f.Name == "Оружейный сейф") NAPI.Data.SetEntitySharedData(obj, "TYPE", "WeaponSafe");
                else if (f.Name == "Шкаф с одеждой") NAPI.Data.SetEntitySharedData(obj, "TYPE", "ClothesSafe");
                else if (f.Name == "Шкаф с предметами") NAPI.Data.SetEntitySharedData(obj, "TYPE", "SubjectSafe");
                Objects.Add(obj);
            }
            catch
            {
            }
        }
        public void DestroyFurnitures()
        {
            try
            {
                foreach (var obj in Objects) NAPI.Entity.DeleteEntity(obj);
                Objects = new List<GTANetworkAPI.Object>();
            }
            catch { }
        }
        public void DestroyFurniture(int id)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var obj in Objects)
                    {
                        if (obj.HasData("ID") && obj.GetData<int>("ID") == id)
                        {
                            NAPI.Entity.DeleteEntity(obj);
                            break;
                        }
                    }
                }
                catch { }
            });
        }
        public void UpdateBlip()
        {
            if (string.IsNullOrEmpty(Owner))
            {
                blip.Name = "Дом на продаже";
                blip.Sprite = 374;
                blip.Color = 52;
            }
            else
            {
                blip.Name = "Ваш дом";
                blip.Sprite = 40;
                blip.Color = 49;
            }
        }
        public void Create()
        {
            Database.Query($"INSERT INTO `houses`(`id`,`owner`,`type`,`position`,`price`,`locked`,`garage`,`bank`,`roommates`) " +
                $"VALUES ('{ID}','{Owner}',{Type},'{JsonConvert.SerializeObject(Position)}',{Price},{Locked},{GarageID},{BankID},'{JsonConvert.SerializeObject(Roommates)}')");
        }
        public void Save()
        {
            Finance.Bank.Save(BankID);
            Database.Query($"UPDATE `houses` SET `owner`='{Owner}',`type`={Type},`position`='{JsonConvert.SerializeObject(Position)}',`price`={Price}," +
                $"`locked`={Locked},`garage`={GarageID},`bank`={BankID},`roommates`='{JsonConvert.SerializeObject(Roommates)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            RemoveAllPlayers();
            blip.Delete();
            NAPI.ColShape.DeleteColShape(shape);
            NAPI.ColShape.DeleteColShape(intshape);
            label.Delete();
            intmarker.Delete();
            DestroyFurnitures();
        }
        public void SetLock(bool locked)
        {
            Locked = locked;

            UpdateLabel();
            Save();
        }
        public void SetOwner(Player player)
        {
            GarageManager.Garages[GarageID].DestroyCars();
            Owner = (player == null) ? string.Empty : player.Name;
            UpdateBlip();
            UpdateLabel();
            if (player != null)
            {
                Trigger.ClientEvent(player, "changeBlipColor", blip, 73);
                Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
                Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[GarageID].Position);
                Hotel.MoveOutPlayer(player);

                var vehicles = VehicleManager.getAllPlayerVehicles(Owner);
                if (GarageManager.Garages[GarageID].Type != -1)
                    NAPI.Task.Run(() => { try { GarageManager.Garages[GarageID].SpawnCars(vehicles); } catch { } });
            }
            foreach (var r in Roommates)
            {
                var roommate = NAPI.Player.GetPlayerFromName(r);
                if (roommate != null)
                {
                    Notify.Send(roommate, NotifyType.Warning, NotifyPosition.TopCenter, "Вы были выселены из дома", 3000);
                    roommate.TriggerEvent("deleteCheckpoint", 333);
                    roommate.TriggerEvent("deleteGarageBlip");
                }
            }
            Roommates = new List<string>();
            Save();
        }
        public string GaragePlayerExit(Player player)
        {
            var players = Main.Players.Keys.ToList();
            var online = players.FindAll(p => Roommates.Contains(p.Name) && p.Name != player.Name);

            var owner = NAPI.Player.GetPlayerFromName(Owner);
            if (Roommates.Contains(player.Name) && owner != null && Main.Players.ContainsKey(owner))
                online.Add(owner);

            var garage = GarageManager.Garages[GarageID];
            var number = garage.SendVehiclesInsteadNearest(online, player);

            return number;
        }
        public void SendPlayer(Player player)
        {
            NAPI.Entity.SetEntityPosition(player, HouseManager.HouseTypeList[Type].Position + new Vector3(0, 0, 1.12));
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            Main.Players[player].InsideHouseID = ID;
            DestroyFurnitures();
            CreateAllFurnitures();
            if (!PlayersInside.Contains(player)) PlayersInside.Add(player);
        }
        public void RemovePlayer(Player player, bool exit = true)
        {
            if (exit)
            {
                NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                NAPI.Entity.SetEntityDimension(player, 0);
            }
            player.ResetData("InvitedHouse_ID");
            Main.Players[player].InsideHouseID = -1;

            if (PlayersInside.Contains(player.Handle)) PlayersInside.Remove(player.Handle);
        }
        public void RemoveFromList(Player player)
        {
            if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
        }
        public void RemoveAllPlayers(Player requster = null)
        {
            for (int i = PlayersInside.Count - 1; i >= 0; i--)
            {
                Player player = NAPI.Entity.GetEntityFromHandle<Player>(PlayersInside[i]);
                if (requster != null && player == requster) continue;

                if (player != null)
                {
                    NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                    NAPI.Entity.SetEntityDimension(player, 0);

                    player.ResetData("InvitedHouse_ID");
                    Main.Players[player].InsideHouseID = -1;
                }

                PlayersInside.RemoveAt(i);
            }
        }
        public void CreateInterior()
        {
            intmarker = NAPI.Marker.CreateMarker(1, HouseManager.HouseTypeList[Type].Position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intshape = NAPI.ColShape.CreateCylinderColShape(HouseManager.HouseTypeList[Type].Position - new Vector3(0.0, 0.0, 1.0), 2f, 4f, (uint)Dimension);
            intshape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 7);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityEnterColShape: " + ex.Message); }
            };

            intshape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityExitColShape: " + ex.Message); }
            };
        }
        public void ChangeOwner(string newName)
        {
            Owner = newName;
            this.UpdateLabel();
            this.Save();
        }
    }
}