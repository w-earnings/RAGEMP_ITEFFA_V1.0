using System.Collections.Generic;
using System.Data;
using System;
using GTANetworkAPI;
using iTeffa.Kernel;
using iTeffa.Settings;
using iTeffa.Interface;
using System.Linq;
using Newtonsoft.Json;

namespace iTeffa.Fractions
{
    class Stocks : Script
    {
        private static nLog Log = new nLog("Stocks");

        public static Dictionary<int, FractionStock> fracStocks = new Dictionary<int, FractionStock>();
        private static Dictionary<int, Vector3> stockCoords = new Dictionary<int, Vector3>()
        {
            {1, new Vector3(-25.72462, -1397.463, 23.55845)},     // The Families
            {2, new Vector3(114.8752, -1995.554, 11.48072)},      // The Ballas Gang
            {3, new Vector3(475.6863, -1899.641, 24.83773)},      // Los Santos Vagos
            {4, new Vector3(1435.88, -1490.218, 65.49928)},       // Marabunta Grande
            {5, new Vector3(973.9128, -1845.269, 25.28732)},      // Blood Street

            { 6, new Vector3()},
            { 7, new Vector3()},
            { 8, new Vector3()}, 
            { 9, new Vector3()},
            { 10, new Vector3(1404.115, 1131.676, 83.217)},
            { 11, new Vector3(-120.4249, 976.35, 54.5895)},
            { 12, new Vector3(-1556.957, -106.6655, -193.2461)},
            { 13, new Vector3(-1808.541, 459.8595, -189.8455)},
            { 14, new Vector3()},
            { 15, new Vector3()},
            { 16, new Vector3(976.768738, -103.651985, 73.725174)},
            { 17, new Vector3(2040.175, 3018.278, -73.82208)},
            { 18, new Vector3()},
        };
        private static Dictionary<int, Vector3> garageCoords = new Dictionary<int, Vector3>()
        {
            {1, new Vector3(-25.12974, -1411.033, 28.50709)},    // The Families
            {2, new Vector3(115.0507, -1993.898, 17.18044)},     // The Ballas Gang
            {3, new Vector3(472.6555, -1882.347, 24.97735)},     // Los Santos Vagos
            {4, new Vector3(1435.542, -1479.373, 62.10447)},     // Marabunta Grande
            {5, new Vector3(975.5194, -1841.575, 30.14339)},     // Blood Street
            {6, new Vector3(-580.291, -130.4515, 34.00952)},     // Cityhall
            {7, new Vector3(434.195, -1005.593, 26.21062)},      // Police Dept
            {8, new Vector3(322.0637, -1479.8123, 28.813513)},   // Medical Center

            { 9, new Vector3(128.3342, -703.3604, 32.00156)},
            { 10, new Vector3(1413.687, 1118.036, 112.838)},
            { 11, new Vector3(-128.7453, 1006.892, 234.6121)},
            { 12, new Vector3(-1579.679, -82.6765, 53.01449)},
            { 13, new Vector3(-1793.165, 404.6401, 111.1755)},
            { 14, new Vector3(-2455.718, 2984.414, 31.81033)},
            { 15, new Vector3()},
            { 16, new Vector3()},
            { 17, new Vector3()}, // TODO:
            { 18, new Vector3(-447.6798, 5993.688, 29.22054)}, // Sheriff 
        };
        public static Dictionary<string, int> maxMats = new Dictionary<string, int>()
        {
            { "", 300 },
            { "BARRACKS", 10000 },
            { "BURRITO", 1500 },
            { "YOUGA", 1500 },
            { "YOUGA2", 1500 },
        };

        [ServerEvent(Event.ResourceStart)]
        public void fillStocks()
        {
            try
            {
                var result = Connect.QueryRead("SELECT * FROM fractions");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("Table 'fractions' returns null result", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    var data = new FractionStock();
                    data.Drugs = Convert.ToInt32(Row["drugs"]);
                    data.Money = Convert.ToInt32(Row["money"]);
                    data.Materials = Convert.ToInt32(Row["mats"]);
                    data.Medkits = Convert.ToInt32(Row["medkits"]);
                    data.Weapons = JsonConvert.DeserializeObject<List<nItem>>(Row["weapons"].ToString());
                    data.IsOpen = Convert.ToBoolean(Row["isopen"]);
                    data.FuelLimit = Convert.ToInt32(Row["fuellimit"]);
                    data.FuelLeft = Convert.ToInt32(Row["fuelleft"]);
                    var id = Convert.ToInt32(Row["id"]);
                    Weapons.FractionsLastSerial[id] = Convert.ToInt32(Row["lastserial"]);

                    #region label Creating
                    if (garageCoords.ContainsKey(id))
                    {
                        data.label = NAPI.TextLabel.CreateTextLabel("~b~", garageCoords[id] + new Vector3(0, 0, 1.5), 10f, 0.4f, 0, new Color(255, 255, 255), true);
                        if (id == 14) data.maxMats = 250000;
                        else data.maxMats = 50000;
                        data.UpdateLabel();
                    }

                    #endregion
                    fracStocks.Add(id, data);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(stockCoords[id], 1, 2, 0); // stock colshape
                    colshape.SetData("FRACID", id);
                    colshape.OnEntityEnterColShape += enterStockShape;
                    colshape.OnEntityExitColShape += exitStockShape;
                    NAPI.Marker.CreateMarker(1, stockCoords[id] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(227, 252, 252, 220));
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~Склад {Manager.getName(id)}"), new Vector3(stockCoords[id].X, stockCoords[id].Y, stockCoords[id].Z + 0.6), 5F, 0.5F, 0, new Color(227, 252, 252));

                    colshape = NAPI.ColShape.CreateCylinderColShape(garageCoords[id], 5, 8, 0); // garage colshape
                    colshape.SetData("FRACID", id);
                    colshape.OnEntityEnterColShape += enterGarageShape;
                    colshape.OnEntityExitColShape += exitGarageShape;
                    NAPI.Marker.CreateMarker(1, garageCoords[id], new Vector3(), new Vector3(), 3f, new Color(227, 252, 252));
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        #region stocks colshape
        private static void enterGarageShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 32);
                entity.SetData("ONFRACSTOCK", shape.GetData<object>("FRACID"));
                entity.TriggerEvent("interactHint", true);
            }
            catch (Exception ex) { Log.Write("enterGarageShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void exitGarageShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 0);
                entity.SetData("ONFRACSTOCK", 0);
                entity.TriggerEvent("interactHint", false);
            }
            catch (Exception ex) { Log.Write("exitGarageShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void enterStockShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 33);
                entity.SetData("ONFRACSTOCK", shape.GetData<object>("FRACID"));
                entity.TriggerEvent("interactHint", true);
            }
            catch (Exception ex) { Log.Write("enterStockShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void exitStockShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 0);
                entity.SetData("ONFRACSTOCK", 0);
                entity.TriggerEvent("interactHint", false);
            }
            catch (Exception ex) { Log.Write("exitStockShape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion

        public static int TryAdd(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            var tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
            {
                if (items.Count >= 200) return -1;
            }
            else
            {
                var count = 0;
                foreach (var i in items)
                    if (i.Type == item.Type) count += nInventory.ItemsStacks[i.Type] - i.Count;

                var slots = 200;
                var maxCapacity = (slots - items.Count) * nInventory.ItemsStacks[item.Type] + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }
        public static void Add(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
            {
                items.Add(item);
            }
            else
            {
                var count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.ItemsStacks[item.Type])
                    {
                        var temp = nInventory.ItemsStacks[item.Type] - items[i].Count;
                        if (count < temp) temp = count;
                        items[i].Count += temp;
                        count -= temp;
                    }
                }

                while (count > 0)
                {
                    if (count >= nInventory.ItemsStacks[item.Type])
                    {
                        items.Add(new nItem(item.Type, nInventory.ItemsStacks[item.Type], item.Data));
                        count -= nInventory.ItemsStacks[item.Type];
                    }
                    else
                    {
                        items.Add(new nItem(item.Type, count, item.Data));
                        count = 0;
                    }
                }
            }

            fracStocks[fraction].Weapons = items;
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 6 && p.HasData("ONFRACSTOCK") && p.GetData<int>("ONFRACSTOCK") == fraction) Dashboard.OpenOut(p, fracStocks[fraction].Weapons, "Склад оружия", 6);
            }
        }
        public static void Remove(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)
                || item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey)
            {
                items.Remove(item);
            }
            else
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (i >= items.Count) continue;
                    if (items[i].Type != item.Type) continue;
                    if (items[i].Count <= item.Count)
                    {
                        item.Count -= items[i].Count;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        items[i].Count -= item.Count;
                        item.Count = 0;
                        break;
                    }
                }
            }

            fracStocks[fraction].Weapons = items;
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 6 && p.HasData("ONFRACSTOCK") && p.GetData<int>("ONFRACSTOCK") == fraction) Dashboard.OpenOut(p, fracStocks[fraction].Weapons, "Склад оружия", 6);
            }
        }
        public static int GetCountOfType(int fraction, ItemType type)
        {
            List<nItem> items = fracStocks[fraction].Weapons;
            var count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }
        public static void inputStocks(Player player, int where, string action, int amount)
        {
            // where (0 - stock, 1 - garage)
            if (where == 0)
            {
                switch (action)
                {
                    case "put_stock":
                        var item = player.GetData<string>("selectedStock");
                        var data = fracStocks[Main.Players[player].FractionID];
                        int stockContains = 0;
                        int playerHave = 0;

                        if (item == "mats")
                        {
                            stockContains = data.Materials;
                            var maxstock = 50000;
                            if (Main.Players[player].FractionID == 14) maxstock = 250000;
                            if (stockContains + amount > maxstock)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад не может вместить такое кол-во материала", 3000);
                                return;
                            }
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                        }
                        else if (item == "drugs")
                        {
                            stockContains = data.Drugs;
                            if (stockContains + amount > 10000)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад не может вместить такое кол-во наркотиков", 3000);
                                return;
                            }
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                        }
                        else if (item == "money")
                        {
                            stockContains = data.Money;
                            playerHave = (int)Main.Players[player].Money;
                        }
                        else if (item == "medkits")
                        {
                            stockContains = data.Medkits;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }

                        if (playerHave < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет столько", 3000);
                            return;
                        }

                        if (item == "mats")
                        {
                            data.Materials += amount;
                            nInventory.Remove(player, ItemType.Material, amount);
                        }
                        else if (item == "drugs")
                        {
                            data.Drugs += amount;
                            nInventory.Remove(player, ItemType.Drugs, amount);
                        }
                        else if (item == "money")
                        {
                            data.Money += amount;
                            Finance.Wallet.Change(player, -amount);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"frac({Main.Players[player].FractionID})", amount, $"putStock");
                        }
                        else if (item == "medkits")
                        {
                            data.Medkits += amount;
                            nInventory.Remove(player, ItemType.HealthKit, amount);
                        }
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, item, amount, true);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"На складе осталось {stockContains + amount}, у Вас {playerHave - amount}", 3000);
                        data.UpdateLabel();
                        break;
                    case "take_stock":
                        item = player.GetData<string>("selectedStock");
                        if (!Manager.canUseCommand(player, $"take{item}")) return;

                        data = fracStocks[Main.Players[player].FractionID];
                        stockContains = 0;
                        playerHave = 0;
                        if (item == "mats")
                        {
                            stockContains = data.Materials;
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                            if (playerHave + amount > 300)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете вместить столько в свой инвентарь", 3000);
                                return;
                            }
                        }
                        else if (item == "drugs")
                        {
                            stockContains = data.Drugs;
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                            if (playerHave + amount > 50)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете вместить столько в свой инвентарь", 3000);
                                return;
                            }
                        }
                        else if (item == "money")
                        {
                            stockContains = data.Money;
                            playerHave = (int)Main.Players[player].Money;
                        }
                        else if (item == "medkits")
                        {
                            stockContains = data.Medkits;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }

                        if (stockContains < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе столько нет", 3000);
                            return;
                        }

                        if (item == "mats")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Material, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Materials -= amount;
                            nInventory.Add(player, new nItem(ItemType.Material, amount));
                        }
                        else if (item == "drugs")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Drugs, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Drugs -= amount;
                            nInventory.Add(player, new nItem(ItemType.Drugs, amount));
                        }
                        else if (item == "money")
                        {
                            data.Money -= amount;
                            Finance.Wallet.Change(player, amount);
                            GameLog.Money($"frac({Main.Players[player].FractionID})", $"player({Main.Players[player].UUID})", amount, $"takeStock");
                        }
                        else if (item == "medkits")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.HealthKit, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Medkits -= amount;
                            nInventory.Add(player, new nItem(ItemType.HealthKit, amount));
                        }
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, item, amount, false);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"На складе осталось {stockContains - amount}, у Вас {playerHave + amount}", 3000);
                        data.UpdateLabel();
                        break;
                }
            }
            else
            {
                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                    return;
                }
                var vehicle = player.Vehicle;
                if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Машина не может ничего перевозить", 3000);
                    return;
                }
                int onfrac = player.GetData<int>("ONFRACSTOCK");
                switch (action)
                {
                    case "load_mats":
                        if (onfrac != 14 && Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в {Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (onfrac != 14 && !Manager.canUseCommand(player, "takestock")) return;
                        if (fracStocks[onfrac].Materials < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе нет такого кол-ва матов", 3000);
                            return;
                        }
                        var maxMats = (Fractions.Stocks.maxMats.ContainsKey(vehicle.DisplayName)) ? Fractions.Stocks.maxMats[vehicle.DisplayName] : 600;
                        if (VehicleInventory.GetCountOfType(vehicle, ItemType.Material) + amount > maxMats)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                            return;
                        }
                        var tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.Material, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                            return;
                        }
                        var data = new nItem(ItemType.Material);
                        data.Count = amount;
                        VehicleInventory.Add(vehicle, data);
                        fracStocks[onfrac].Materials -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы загрузили материалы в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "mats", amount, false);
                        return;
                    case "unload_mats":
                        var count = VehicleInventory.GetCountOfType(vehicle, ItemType.Material);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В машине нет такого кол-ва матов", 3000);
                            return;
                        }
                        var maxstock = 50000;
                        if (onfrac == 14) maxstock = 250000;
                        if (fracStocks[onfrac].Materials + amount > maxstock)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад не может вместить такое кол-во материала", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.Material, amount);
                        fracStocks[onfrac].Materials += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы выгрузили материалы из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "mats", amount, true);
                        return;
                    case "load_drugs":
                        if (Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в ~y~{Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (!Manager.canUseCommand(player, "takestock")) return;
                        if (fracStocks[onfrac].Drugs < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе нет такого кол-ва наркотиков", 3000);
                            return;
                        }
                        tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.Drugs, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно загрузить такое кол-во наркотиков", 3000);
                            return;
                        }
                        data = new nItem(ItemType.Drugs);
                        data.Count = amount;
                        VehicleInventory.Add(vehicle, data);
                        fracStocks[onfrac].Drugs -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы загрузили наркотики в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "drugs", amount, false);
                        return;
                    case "unload_drugs":
                        count = VehicleInventory.GetCountOfType(vehicle, ItemType.Drugs);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В машине нет такого кол-ва наркотиков", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Drugs + amount > 10000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад не может вместить такое кол-во наркотиков", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.Drugs, amount);
                        fracStocks[onfrac].Drugs += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы выгрузили наркотики из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "drugs", amount, true);
                        return;
                    case "load_medkits":
                        if (Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в {Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (!player.GetData<bool>("ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Medkits < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На складе нет такого кол-ва аптечек", 3000);
                            return;
                        }
                        var maxMedkits = 100;
                        if (VehicleInventory.GetCountOfType(vehicle, ItemType.HealthKit) + amount > maxMedkits)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно загрузить такое кол-во аптечек", 3000);
                            return;
                        }
                        tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.HealthKit, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно загрузить такое кол-во аптечек", 3000);
                            return;
                        }
                        VehicleInventory.Add(vehicle, new nItem(ItemType.HealthKit, amount));
                        fracStocks[onfrac].Medkits -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы загрузили аптечки в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "medkits", amount, false);
                        return;
                    case "unload_medkits":
                        if (!player.GetData<bool>("ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }
                        count = VehicleInventory.GetCountOfType(vehicle, ItemType.HealthKit);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В машине нет такого кол-ва аптечек", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Medkits + amount > 1000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад не может вместить такое кол-во аптечек", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.HealthKit, amount);
                        fracStocks[onfrac].Medkits += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы выгрузили аптечки из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "medkits", amount, true);
                        return;
                }
            }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 32:
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    var vehicle = player.Vehicle;
                    if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Машина не может ничего перевозить", 3000);
                        return;
                    }

                    if (player.GetData<int>("ONFRACSTOCK") == 14 && (DateTime.Now.Hour < 13 && DateTime.Now.Hour > 1))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад намертво закрыт", 3000);
                        return;
                    }
                    else if (!fracStocks[(int)player.GetData<int>("ONFRACSTOCK")].IsOpen && player.GetData<int>("ONFRACSTOCK") != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад закрыт", 3000);
                        return;
                    }

                    OpenFracGarageMenu(player);
                    return;
                case 33:
                    if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    if (!fracStocks[Main.Players[player].FractionID].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    OpenFracStockMenu(player);
                    return;
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public static void onPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                /*var menu = MenuController.MenuManager.getMenu(player);
                if (menu != null && menu.Id == "fracGarage") MenuConstructor.CloseAll(player);*/
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, nLog.Type.Error); }
        }

        public static void saveStocksDic()
        {
            foreach (var key in fracStocks.Keys)
            {
                var data = fracStocks[key];
                Connect.Query($"UPDATE fractions SET drugs={data.Drugs},money={data.Money},mats={data.Materials},medkits={data.Medkits},lastserial={Weapons.FractionsLastSerial[key]}," +
                    $"weapons='{JsonConvert.SerializeObject(data.Weapons)}',isopen={data.IsOpen},fuellimit={data.FuelLimit},fuelleft={data.FuelLeft} WHERE id={key}");
            }
            Log.Write("Stocks has been saved to DB", nLog.Type.Success);
        }

        [RemoteEvent("openWeaponStock")]
        public static void Event_openWeaponsStock(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("ONFRACSTOCK") || player.GetData<int>("ONFRACSTOCK") == 0) return;
                if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK")) return;

                if (!Manager.canUseCommand(player, "openweaponstock")) return;

                Interface.Dashboard.OpenOut(player, fracStocks[(int)player.GetData<int>("ONFRACSTOCK")].Weapons, "Склад оружия", 6);
            }
            catch (Exception e) { Log.Write("Openweaponstock: " + e.Message, nLog.Type.Error); }
        }

        #region menus
        public static void OpenFracGarageMenu(Player player)
        {
            bool isArmy = !player.Vehicle.HasData("CANDRUGS");
            bool isMed = player.Vehicle.HasData("CANMEDKITS");
            Trigger.ClientEvent(player, "matsOpen", isArmy, isMed);
        }
        public static void fracgarage(Player player, string eventName, string data)
        {
            int amount = 0;
            if (!Int32.TryParse(data, out amount))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Введите корректные данные", 3000);
                return;
            }
            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Введите корректные данные", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                return;
            }
            var vehicle = player.Vehicle;
            if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Машина не может ничего перевозить", 3000);
                return;
            }
            switch (eventName)
            {
                case "loadmats":
                    if (player.GetData<int>("ONFRACSTOCK") != 14 && Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    Fractions.Stocks.inputStocks(player, 1, "load_mats", amount);
                    return;
                case "unloadmats":
                    Fractions.Stocks.inputStocks(player, 1, "unload_mats", amount);
                    return;
                case "loaddrugs":
                    if (!vehicle.HasData("CANDRUGS"))
                    {
                        MenuManager.Close(player);
                        return;
                    }
                    if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    Fractions.Stocks.inputStocks(player, 1, "load_drugs", amount);
                    return;
                case "unloaddrugs":
                    Fractions.Stocks.inputStocks(player, 1, "unload_drugs", amount);
                    return;
                case "loadmedkits":
                    Fractions.Stocks.inputStocks(player, 1, "load_medkits", amount);
                    return;
                case "unloadmedkits":
                    Fractions.Stocks.inputStocks(player, 1, "unload_medkits", amount);
                    return;
            }
        }

        public static void OpenFracStockMenu(Player player)
        {
            List<int> counter = new List<int>
            {
                fracStocks[Main.Players[player].FractionID].Money,
                fracStocks[Main.Players[player].FractionID].Medkits,
                fracStocks[Main.Players[player].FractionID].Drugs,
                fracStocks[Main.Players[player].FractionID].Materials,
                fracStocks[Main.Players[player].FractionID].Weapons.Count,
            };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(counter);
            Log.Debug(json);
            Trigger.ClientEvent(player, "openStock", json);
        }
        private static void callback_fracstock(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "mats":
                case "drugs":
                case "money":
                case "medkits":
                    MenuManager.Close(player);
                    OpenStockSelectMenu(player, item.ID);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }

        public static void OpenStockSelectMenu(Player player, string item)
        {
            player.SetData("selectedStock", item);
            string itemcount = "";
            string menuname = "";
            if (item == "mats")
            {
                var count = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                itemcount += count + " матов";
                menuname = "Материалы";
            }
            else if (item == "drugs")
            {
                var count = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                itemcount += count + "г";
                menuname = "Наркотики";
            }
            else if (item == "money")
            {
                itemcount += Main.Players[player].Money + "$";
                menuname = "Деньги";
            }
            else if (item == "medkits")
            {
                var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (invitem == null) itemcount += "0 шт";
                else itemcount += invitem.Count + " шт";
                menuname = "Аптечки";
            }
            Menu menu = new Menu("stockselect", false, false);
            menu.Callback = callback_stockselect;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = menuname;
            menu.Add(menuItem);

            menuItem = new Menu.Item("uhave", Menu.MenuItem.Card);
            menuItem.Text = $"У Вас есть {itemcount}";
            menu.Add(menuItem);

            menuItem = new Menu.Item("put", Menu.MenuItem.Button);
            menuItem.Text = "Положить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button);
            menuItem.Text = "Взять";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_stockselect(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "put":
                    MenuManager.Close(player);
                    Main.OpenInputMenu(player, "Введите кол-во", "put_stock");
                    return;
                case "take":
                    MenuManager.Close(player);
                    Main.OpenInputMenu(player, "Введите кол-во", "take_stock");
                    return;
                case "back":
                    MenuManager.Close(player);
                    OpenFracStockMenu(player);
                    return;
            }
        }
        #endregion

        public class FractionStock
        {
            public int Drugs { get; set; }
            public int Money { get; set; }
            public int Materials { get; set; }
            public int Medkits { get; set; }
            public List<nItem> Weapons { get; set; }
            public bool IsOpen { get; set; }
            [JsonIgnore]
            public int maxMats { get; set; }
            [JsonIgnore]
            public TextLabel label { get; set; }
            public int FuelLimit { get; set; }
            public int FuelLeft { get; set; }

            public void UpdateLabel()
            {
                if (label == null) return;
                var text = $"~b~";
                if (Drugs > 0) text += $"Наркотиков: {Drugs}/10000\n";
                if (Materials > 0) text += $"Материалов: {Materials}/{maxMats}\n";
                if (Medkits > 0) text += $"Аптечек: {Medkits}\n";
                label.Text = text;
            }
        }
    }
}
