using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class RodManager : Script
    {
        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            BasicSync.DetachObject(player);
        }

        public static Dictionary<int, ItemType> FishItems1 = new Dictionary<int, ItemType>
        {
            {1, ItemType.Lococ },
            {2, ItemType.Okyn },
            {3, ItemType.Okyn },
            {4, ItemType.Okyn },
            {5, ItemType.Okyn },
            {6, ItemType.Ocetr },
            {7, ItemType.Ygol },
            {8, ItemType.Chyka },
            {9, ItemType.Chyka },
            {10, ItemType.Chyka },
        };

        public static Dictionary<int, ItemType> FishItems2 = new Dictionary<int, ItemType>
        {
            {1, ItemType.Koroska },
            {2, ItemType.Koroska },
            {4, ItemType.Lococ },
            {5, ItemType.Okyn },
            {6, ItemType.Okyn },
            {7, ItemType.Okyn },
            {8, ItemType.Ocetr },
            {9, ItemType.Skat },
            {10, ItemType.Skat },
            {12, ItemType.Ygol },
            {13, ItemType.Ygol },
            {15, ItemType.Chyka },
            {16, ItemType.Chyka },
            {17, ItemType.Chyka },
        };

        public static Dictionary<int, ItemType> FishItems3 = new Dictionary<int, ItemType>
        {
            {1, ItemType.Koroska },
            {2, ItemType.Kyndja },
            {3, ItemType.Lococ },
            {4, ItemType.Okyn },
            {5, ItemType.Okyn },
            {6, ItemType.Ocetr },
            {7, ItemType.Skat },
            {8, ItemType.Tunec },
            {9, ItemType.Ygol },
            {10, ItemType.Amyr },
            {11, ItemType.Chyka },
            {12, ItemType.Chyka },
        };

        public static Dictionary<int, ItemType> TypeRod = new Dictionary<int, ItemType>
        {
            {1, ItemType.Rod },
            {2, ItemType.RodUpgrade },
            {3, ItemType.RodMK2 },
        };

        public static ItemType GetSellingItemType(string name)
        {
            var type = ItemType.Naz;
            switch (name)
            {
                case "Корюшка":
                    type = ItemType.Koroska;
                    break;
                case "Кунджа":
                    type = ItemType.Kyndja;
                    break;
                case "Лосось":
                    type = ItemType.Lococ;
                    break;
                case "Окунь":
                    type = ItemType.Okyn;
                    break;
                case "Осётр":
                    type = ItemType.Ocetr;
                    break;
                case "Скат":
                    type = ItemType.Skat;
                    break;
                case "Тунец":
                    type = ItemType.Tunec;
                    break;
                case "Угорь":
                    type = ItemType.Ygol;
                    break;
                case "Чёрный амур":
                    type = ItemType.Amyr;
                    break;
                case "Щука":
                    type = ItemType.Chyka;
                    break;
            }
            return type;
        }

        public static string GetNameByItemType(ItemType tupe)
        {
            string type = "nope";
            switch (tupe)
            {
                case ItemType.Koroska:
                    type = "Корюшка";
                    break;
                case ItemType.Kyndja:
                    type = "Кунджа";
                    break;
                case ItemType.Lococ:
                    type = "Лосось";
                    break;
                case ItemType.Okyn:
                    type = "Окунь";
                    break;
                case ItemType.Ocetr:
                    type = "Осётр";
                    break;
                case ItemType.Skat:
                    type = "Скат";
                    break;
                case ItemType.Tunec:
                    type = "Тунец";
                    break;
                case ItemType.Ygol:
                    type = "Угорь";
                    break;
                case ItemType.Amyr:
                    type = "Чёрный амур";
                    break;
                case ItemType.Chyka:
                    type = "Щука";
                    break;
            }

            return type;
        }

        public static void OpenBizSellShopMenu(Player player)
        {
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            List<List<string>> items = new List<List<string>>();

            foreach (var p in biz.Products)
            {
                List<string> item = new List<string>
                {
                    p.Name,
                    $"{p.Price * Main.pluscost}$"
                };
                items.Add(item);
            }
            string json = JsonConvert.SerializeObject(items);
            Trigger.ClientEvent(player, "fishshop", json);
        }

        private static readonly Nlogs Log = new Nlogs("RodManager");

        private static int lastRodID = -1;

        [ServerEvent(Event.ResourceStart)]

        public void OnResourceStart()
        {
            try
            {
                var result = Database.QueryRead($"SELECT * FROM rodings");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB rod return null result.", Nlogs.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());

                    Roding data = new Roding(Convert.ToInt32(Row["id"]), pos, Convert.ToInt32(Row["radius"]));
                    int id = Convert.ToInt32(Row["id"]);
                    lastRodID = id;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"RODINGS\":\n" + e.ToString(), Nlogs.Type.Error);
            }
        }


        public static void createRodAreaCommand(Player player, float radius)
        {
            if (!Group.CanUseCmd(player, "createbusiness")) return;

            var pos = player.Position;
            pos.Z -= 1.12F;

            ++lastRodID;
            Roding biz = new Roding(lastRodID, pos, radius);

            Database.Query($"INSERT INTO rodings (id, pos, radius) " + $"VALUES ({lastRodID}, '{JsonConvert.SerializeObject(pos)}', {radius})");

        }

        public enum AnimationFlags
        {
            Loop = 1 << 0,
            StopOnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7
        }

        public static void setallow(Player player)
        {
            player.SetData("FISHING", true);
            Main.OnAntiAnim(player);
            player.PlayAnimation("amb@world_human_stand_fishing@base", "base", 31);
            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_fishing_rod_01"), 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null && Main.Players.ContainsKey(player))
                    {
                        allowfish(player);
                    }
                }
                catch { }
            }, 18000);
        }

        public static void allowfish(Player player)
        {
            player.PlayAnimation("amb@world_human_stand_fishing@idle_a", "idle_c", 31);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Что-то клюнуло", 1000);

            player.TriggerEvent("fishingBaitTaken");

        }

        public static void crashpros(Player player)
        {
            player.StopAnimation();
            Main.OffAntiAnim(player);
            BasicSync.DetachObject(player);
            player.SetData("FISHING", false);
        }

        [RemoteEvent("giveRandomFish")]
        public static void giveRandomFish(Player player)
        {
            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Ocetr));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно места в инвентаре", 3000);
                crashpros(player);
                return;
            }
            if (player.GetData<int>("FISHLEVEL") == 1)
            {
                var rnd = new Random();
                int fishco = rnd.Next(1, FishItems1.Count);
                nInventory.Add(player, new nItem(FishItems1[fishco], 1));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы поймали рыбу {GetNameByItemType(FishItems1[fishco])}", 3000);
            }
            if (player.GetData<int>("FISHLEVEL") == 2)
            {
                var rnd = new Random();
                int fishco = rnd.Next(1, FishItems2.Count);
                nInventory.Add(player, new nItem(FishItems2[fishco], 1));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы поймали рыбу {GetNameByItemType(FishItems2[fishco])}", 3000);
            }
            if (player.GetData<int>("FISHLEVEL") == 3)
            {
                var rnd = new Random();
                int fishco = rnd.Next(1, FishItems3.Count);
                nInventory.Add(player, new nItem(FishItems3[fishco], 1));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы поймали рыбу {GetNameByItemType(FishItems3[fishco])}", 3000);
            }
            crashpros(player);
        }

        [RemoteEvent("stopFishDrop")]
        public static void stopFishDrop(Player player)
        {
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Рыба сошла с крючка!", 3000);
            crashpros(player);
        }

        public static void useInventory(Player player, int level)
        {
            nInventory.Add(player, new nItem(TypeRod[level], 1));
            if (player.IsInVehicle)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не должны находится в машине!", 3000);
                Interface.Dashboard.Close(player);
                return;
            }
            if (player.GetData<bool>("FISHING") == true)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже рыбачите!", 3000);
                return;
            }
            var aItem = nInventory.Find(Main.Players[player].UUID, ItemType.Naz);
            if (aItem == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас нет наживки", 3000);
                return;
            }
            if (player.GetData<bool>("ALLOWFISHING") == false || player.GetData<bool>("ALLOWFISHING") == false)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "В данном месте нельзя рыбачить", 3000);
                return;
            }
            var rndf = new Random();
            nInventory.Remove(player, ItemType.Naz, 1);
            player.SetData("FISHLEVEL", level);
            setallow(player);
            Commands.Controller.RPChat("me", player, $"Начал(а) рыбачить");
        }

        public class Roding
        {
            public int ID { get; set; }
            public float Radius { get; set; }
            public Vector3 AreaPoint { get; set; }

            [JsonIgnore]
            private readonly Blip blip = null;
            [JsonIgnore]
            private readonly ColShape shape = null;
            public Roding(int id, Vector3 areapoint, float radius)
            {
                ID = id;
                AreaPoint = areapoint;
                Radius = radius;
                blip = NAPI.Blip.CreateBlip(68, AreaPoint, 0.75F, 67, "Место для рыбалки", 255, 0, true);
                shape = NAPI.ColShape.CreateCylinderColShape(AreaPoint, Radius, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("ALLOWFISHING", true);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("ALLOWFISHING", false);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
            }

        }
    }
}
