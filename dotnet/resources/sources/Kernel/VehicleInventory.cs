using GTANetworkAPI;
using System.Collections.Generic;
using iTeffa.Interface;
using System.Linq;
using iTeffa.Settings;

namespace iTeffa.Kernel
{
    class VehicleInventory : Script
    {
        public static void Add(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type)
                || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Add(item);
            }
            else
            {
                int count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.ItemsStacks[item.Type])
                    {
                        int temp = nInventory.ItemsStacks[item.Type] - items[i].Count;
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

            vehicle.SetData("ITEMS", items);

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static int TryAdd(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return -1;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            int tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                if (items.Count >= 25) return -1;
            }
            else
            {
                int count = 0;
                foreach (nItem i in items)
                    if (i.Type == item.Type) count += nInventory.ItemsStacks[i.Type] - i.Count;

                int slots = 25;
                int maxCapacity = (slots - items.Count) * nInventory.ItemsStacks[item.Type] + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }

        public static int GetCountOfType(Vehicle vehicle, ItemType type)
        {
            if (!vehicle.HasData("ITEMS")) return 0;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }

        public static void Remove(Vehicle vehicle, ItemType type, int amount)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (i >= items.Count) continue;
                if (items[i].Type != type) continue;
                if (items[i].Count <= amount)
                {
                    amount -= items[i].Count;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].Count -= amount;
                    amount = 0;
                    break;
                }
            }

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static void Remove(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
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

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }
    }
}
