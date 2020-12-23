using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Working.FarmerJob
{
    class Market : Script
    {
        private static readonly Random rnd = new Random();
        private static readonly Nlogs Log = new Nlogs("Market");
        private static readonly List<FarmerProduct> Products = new List<FarmerProduct>();
        public static int marketmultiplier = rnd.Next(15, 30);
        private static readonly List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(2367.3, 4881.5, 41.4),
        };
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(501, new Vector3(2367.3, 4881.5, 35.3), 1, 81, "Скупщик урожая", 255, 0, true, 0, 0); // Блип на карте
                NAPI.TextLabel.CreateTextLabel("~w~Скупщик Мелог", new Vector3(2367.39, 4881.526, 43.2), 10f, 0.2f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); // Над головой у Ped

                var melogShape = NAPI.ColShape.CreateCylinderColShape(shape[0], 2f, 2, 0);
                melogShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 521);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), Nlogs.Type.Error);
                    }
                };
                melogShape.OnEntityExitColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), Nlogs.Type.Error);
                    }
                };
                Log.Write("Loaded", Nlogs.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        private static object MarketItem(FarmerProduct prod)
        {
            if (prod != null)
            {
                Products.Add(prod);
            }
            List<object> data = new List<object>()
            {
                prod.Price,
                prod.ID,
                prod.Name,
                prod.Ordered
            };
            return data;
        }
        private static List<object> SellItems = new List<object>()
        {
            MarketItem(new FarmerProduct(7, 234, "Урожай", true)),
            MarketItem(new FarmerProduct(2, 235, "Семена", false)),
        };

        private static List<object> BuyItems = new List<object>()
        {
            MarketItem(new FarmerProduct(5, 235, "Семена", false)),
        };
        [RemoteEvent("changePage")]
        public static void OpenMarketMenu(Player player, int page)
        {
            var hitem = nInventory.Find(Main.Players[player].UUID, ItemType.Hay);
            var shitem = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);
            int hayscount = hitem != null ? hitem.Count : 0;
            int seedscount = shitem != null ? shitem.Count : 0;
            List<object> data = new List<object>()
            {
                marketmultiplier,
                hayscount,
                seedscount,
            };
            LoadPage(player, page, data);
        }
        public static void LoadPage(Player player, int page, object data)
        {
            string json;
            switch (page)
            {
                case 0:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Trigger.ClientEvent(player, "loadPage", 0, json);
                    break;
                case 1:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(BuyItems);
                    Trigger.ClientEvent(player, "loadPage", 1, json);
                    break;
                case 2:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(SellItems);
                    Trigger.ClientEvent(player, "loadPage", 2, json);
                    break;
            }
        }
        [RemoteEvent("buyFarmerItem")]
        public static void ButFarmerItem(Player player, int id, int count)
        {
            nItem aItem = new nItem((ItemType)id);
            var tryAdd = nInventory.TryAdd(player, new nItem(aItem.Type, count));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 2000);
                return;
            }
            int price = 0;
            string name = null;
            foreach (var item in Products.FindAll(x => x.ID == id))
            {
                price = item.Ordered ? item.Price * marketmultiplier * count : item.Price * count;
                name = item.Name;
            }
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно денег", 2000);
                return;
            }
            Finance.Wallet.Change(player, -price);
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {count} {name} за ${price}", 2000);

        }
        [RemoteEvent("sellFarmerItem")]
        public static void SellFarmerItem(Player player, int id, int count)
        {
            var aItem = nInventory.Find(Main.Players[player].UUID, (ItemType)id);
            if (aItem == null || aItem.Count < count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно предмета в инвентаре", 2000);
                return;
            }
            int price = 0;
            string name = null;
            foreach (var item in Products.FindAll(x => x.ID == id))
            {
                price = item.Ordered ? item.Price * marketmultiplier * count : item.Price * count;
                name = item.Name;
            }
            Finance.Wallet.Change(player, price);
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {count} {name} за ${price}", 2000);

        }
        private class FarmerProduct
        {
            public FarmerProduct(int price, int id, string name, bool ordered)
            {
                Price = price;
                ID = id;
                Name = name;
                Ordered = ordered;
            }
            public int Price { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Ordered { get; set; }
        }
    }
}

