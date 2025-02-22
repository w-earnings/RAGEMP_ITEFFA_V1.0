﻿using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Infodata;
using iTeffa.Interface;
using iTeffa.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace iTeffa.Houses
{
    class HouseManager : Script
    {
        public static Plugins.Logs Log = new Plugins.Logs("Manager House");
        public static List<House> Houses = new List<House>();
        public static List<HouseType> HouseTypeList = new List<HouseType>
        {
            new HouseType("Трейлер", new Vector3(1973.124, 3816.065, 32.30873), new Vector3(), 0.0f, "trevorstrailer"),
            new HouseType("Эконом", new Vector3(151.2052, -1008.007, -100.12), new Vector3(), 0.0f,"hei_hw1_blimp_interior_v_motel_mp_milo_"),
            new HouseType("Эконом+", new Vector3(265.9691, -1007.078, -102.0758), new Vector3(), 0.0f,"hei_hw1_blimp_interior_v_studio_lo_milo_"),
            new HouseType("Комфорт", new Vector3(346.6991, -1013.023, -100.3162), new Vector3(349.5223, -994.5601, -99.7562), 264.0f, "hei_hw1_blimp_interior_v_apart_midspaz_milo_"),
            new HouseType("Комфорт+", new Vector3(-31.35483, -594.9686, 78.9109),  new Vector3(-25.42115, -581.4933, 79.12776), 159.84f, "hei_hw1_blimp_interior_32_dlc_apart_high2_new_milo_"),
            new HouseType("Премиум", new Vector3(-17.85757, -589.0983, 88.99482), new Vector3(-38.84652, -578.466, 88.58952), 50.8f, "hei_hw1_blimp_interior_10_dlc_apart_high_new_milo_"),
            new HouseType("Премиум+", new Vector3(-173.9419, 497.8622, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
        };
        private static readonly List<int> MaxRoommates = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
        public static int GetUID()
        {
            int newUID = 0;
            while (Houses.FirstOrDefault(h => h.ID == newUID) != null) newUID++;
            return newUID;
        }
        public static int DimensionID = 10000;
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                foreach (HouseType house_type in HouseTypeList) house_type.Create();
                var result = Database.QueryRead($"SELECT * FROM `houses`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", Plugins.Logs.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    try
                    {
                        var id = Convert.ToInt32(Row["id"].ToString());
                        var owner = Convert.ToString(Row["owner"]);
                        var type = Convert.ToInt32(Row["type"]);
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var price = Convert.ToInt32(Row["price"]);
                        var locked = Convert.ToBoolean(Row["locked"]);
                        var garage = Convert.ToInt32(Row["garage"]);
                        var bank = Convert.ToInt32(Row["bank"]);
                        var roommates = JsonConvert.DeserializeObject<List<string>>(Row["roommates"].ToString());

                        House house = new House(id, owner, type, position, price, locked, garage, bank, roommates)
                        {
                            Dimension = DimensionID
                        };
                        house.CreateInterior();
                        FurnitureManager.Create(id);
                        house.CreateAllFurnitures();

                        Houses.Add(house);
                        DimensionID++;

                    }
                    catch (Exception e)
                    {
                        Log.Write(Row["id"].ToString() + e.ToString(), Plugins.Logs.Type.Error);
                    }

                }

                NAPI.Object.CreateObject(0x07e08443, new Vector3(1972.76892, 3815.36694, 33.6632576), new Vector3(0, 0, -109.999962), 255, NAPI.GlobalDimension);
                GarageManager.spawnCarsInGarage();
                Log.Write($"Loaded {Houses.Count} houses.", Plugins.Logs.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void Event_OnPlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                NAPI.Entity.SetEntityDimension(player, 0);
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void SavingHouses()
        {
            foreach (var h in Houses) h.Save();
            Log.Write("Houses has been saved to DB", Plugins.Logs.Type.Success);
        }
        [ServerEvent(Event.ResourceStop)]
        public void Event_OnResourceStop()
        {
            try
            {
                SavingHouses();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static House GetHouse(Player player, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == player.Name);
            if (house != null)
                return house;
            else if (!checkOwner)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(player.Name));
                return house;
            }
            else
                return null;
        }
        public static House GetHouse(string name, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == name);
            if (house != null)
                return house;
            else if (!checkOwner)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(name));
                return house;
            }
            else
                return null;
        }
        public static void RemovePlayerFromHouseList(Player player)
        {
            if (Main.Players[player].InsideHouseID != -1)
            {
                House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                if (house == null) return;
                house.RemoveFromList(player);
            }
        }
        public static void CheckAndKick(Player player)
        {
            var house = GetHouse(player);
            if (house == null) return;
            if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
        }
        public static void ChangeOwner(string oldName, string newName)
        {
            lock (Houses)
            {
                foreach (House h in Houses)
                {
                    if (h.Owner != oldName) continue;
                    Log.Write($"The house was found! [{h.ID}]");
                    h.ChangeOwner(newName);
                    h.Save();
                }
            }
        }
        public static void interactPressed(Player player, int id)
        {
            switch (id)
            {
                case 6:
                    {
                        if (player.IsInVehicle) return;
                        if (!player.HasData("HOUSEID")) return;

                        House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                        if (house == null) return;
                        if (string.IsNullOrEmpty(house.Owner))
                        {
                            OpenHouseBuyMenu(player, player.GetData<int>("HOUSEID"));
                            return;
                        }
                        else
                        {
                            if (house.Locked)
                            {
                                var playerHouse = GetHouse(player);
                                if (playerHouse != null && playerHouse.ID == house.ID) { OpenHouseMenuInform(player, player.GetData<int>("HOUSEID")); }
                                else if (player.HasData("InvitedHouse_ID") && player.GetData<int>("InvitedHouse_ID") == house.ID) { OpenHouseMenuInform(player, player.GetData<int>("HOUSEID")); }
                                else { OpenHouseMenuInform(player, player.GetData<int>("HOUSEID")); }
                            }
                            else
                            {
                                OpenHouseMenuInform(player, player.GetData<int>("HOUSEID"));
                            }
                        }
                        return;
                    }
                case 7:
                    {
                        if (Main.Players[player].InsideHouseID == -1) return;

                        House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                        if (house == null) return;

                        if (player.HasData("IS_EDITING"))
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны закончить редактирование", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        Plugins.Trigger.ClientEvent(player, "ExitHouseMenu");
                        return;
                    }
            }
        }
        [RemoteEvent("LockedHouseS")]
        public static void OpenHouseBuyMenu2(Player player, int id)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            house.SetLock(!house.Locked);
            if (house.Locked) Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы закрыли дом", 3000);
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы открыли дом", 3000);
            return;
        }
        [RemoteEvent("WarnHouseS")]
        public static void OpenHouseBuyMenu3(Player player, int id)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            house.RemoveAllPlayers(player);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выгнали всех из дома", 3000);
            return;

        }
        [RemoteEvent("CarHouseS")]
        public static void OpenHouseBuyMenu4(Player player, int id)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            OpenCarsMenu(player);
            return;
        }
        [RemoteEvent("SellHomeS")]
        public static void OpenHouseBuyMenu1(Player player, int id)
        {
            House house = GetHouse(player, true);

            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            int price = 0;
            switch (Main.Accounts[player].VipLvl)
            {
                case 0: // None
                    price = Convert.ToInt32(house.Price * 0.6);
                    break;
                case 1: // Bronze
                    price = Convert.ToInt32(house.Price * 0.65);
                    break;
                case 2: // Silver
                    price = Convert.ToInt32(house.Price * 0.7);
                    break;
                case 3: // Gold
                    price = Convert.ToInt32(house.Price * 0.75);
                    break;
                case 4: // Platinum
                    price = Convert.ToInt32(house.Price * 0.8);
                    break;
            }
            Plugins.Trigger.ClientEvent(player, "openSellHome", "HOUSE_SELL_TOGOV", $"${price}?");
            return;
        }
        [RemoteEvent("ExitHouseMenuE")]
        public static void ExitHouseAA(Player player)
        {
            House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
            house.RemovePlayer(player);
        }
        public static void OpenHouseBuyMenu(Player player, int id)
        {
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            Plugins.Trigger.ClientEvent(player, "HouseMenuBuy", id, house.Owner, HouseTypeList[house.Type].Name, house.Locked, house.Price, GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars, MaxRoommates[house.Type]);
        }
        public static void OpenHouseMenuInform(Player player, int id)
        {
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            Plugins.Trigger.ClientEvent(player, "HouseMenu", id, house.Owner, HouseTypeList[house.Type].Name, house.Locked, house.Price, GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars, MaxRoommates[house.Type]);

        }
        [RemoteEvent("GoHouseInterS")]
        public static void GoHouseMenuInformA(Player player, int act)
        {
            if (!player.HasData("HOUSEID")) return;

            House house = Houses.FirstOrDefault(h => h.ID == act);
            if (house == null) return;

            if (!string.IsNullOrEmpty(house.Owner))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В этом доме уже имеется хозяин", 3000);
                return;
            }
            house.SendPlayer(player);
            return;
        }
        [RemoteEvent("GoHouseMenuS")]
        public static void GoHouseMenuInform(Player player, int act)
        {
            House house = Houses.FirstOrDefault(h => h.ID == act);

            if (house.Locked)
            {
                var playerHouse = GetHouse(player);
                if (playerHouse != null && playerHouse.ID == house.ID) { house.SendPlayer(player); return; }
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Двери закрыты! Вас тут не ждали!", 3000);
                return;
            }
            house.SendPlayer(player);

        }
        [RemoteEvent("buyHouseMenuS")]
        private static void callback_housebuy(Player player, int act)
        {
            if (!player.HasData("HOUSEID")) return;

            House house = Houses.FirstOrDefault(h => h.ID == act);
            if (house == null) return;

            if (!string.IsNullOrEmpty(house.Owner))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В этом доме уже имеется хозяин", 3000);
                return;
            }

            if (house.Price > Main.Players[player].Money)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас не хватает средств для покупки дома", 3000);
                return;
            }

            if (Houses.Count(h => h.Owner == player.Name) >= 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не можете купить больше одного дома", 3000);
                return;
            }
            var vehicles = VehicleManager.getAllPlayerVehicles(player.Name).Count;
            var maxcars = GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars;
            if (vehicles > maxcars)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Дом, который Вы покупаете, имеет {maxcars} машиномест, продайте лишние машины", 3000);
                OpenCarsSellMenu(player);
                return;
            }
            CheckAndKick(player);
            house.SetLock(true);
            house.SetOwner(player);
            house.SendPlayer(player);
            Finance.Bank.Accounts[house.BankID].Balance = Convert.ToInt32(house.Price / 100 * 0.02) * 2;

            Modules.Wallet.Change(player, -house.Price);
            Loggings.Money($"player({Main.Players[player].UUID})", $"server", house.Price, $"houseBuy({house.ID})");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы купили этот дом, не забудьте внести налог за него в банкомате", 3000);
            return;
        }
        public static void OpenHouseManageMenu(Player player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            Plugins.Trigger.ClientEvent(player, "MyyHouseMenu");

            Menu menu = new Menu("housemanage", false, false)
            {
                Callback = callback_housemanage
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Управление домом"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("changestate", Menu.MenuItem.Button)
            {
                Text = "Открыть/закрыть"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("removeall", Menu.MenuItem.Button)
            {
                Text = "Выгнать всех"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("furniture", Menu.MenuItem.Button)
            {
                Text = "Мебель"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("cars", Menu.MenuItem.Button)
            {
                Text = "Машины"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("roommates", Menu.MenuItem.Button)
            {
                Text = "Сожители"
            };
            menu.Add(menuItem);
            /*
            menuItem = new Menu.Item("sell", Menu.MenuItem.Button)
            {
                Text = $"Продать гос-ву за {Convert.ToInt32(house.Price * 0.6)}$"
            };
            menu.Add(menuItem);
            */
            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_housemanage(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            switch (item.ID)
            {
                case "changestate":
                    house.SetLock(!house.Locked);
                    if (house.Locked) Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы закрыли дом", 3000);
                    else Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы открыли дом", 3000);
                    return;
                case "removeall":
                    house.RemoveAllPlayers(player);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выгнали всех из дома", 3000);
                    return;
                case "furniture":
                    MenuManager.Close(player);
                    OpenFurnitureMenu(player);
                    return;
                case "sell":
                    int price = 0;
                    switch (Main.Accounts[player].VipLvl)
                    {
                        case 0: // None
                            price = Convert.ToInt32(house.Price * 0.6);
                            break;
                        case 1: // Bronze
                            price = Convert.ToInt32(house.Price * 0.65);
                            break;
                        case 2: // Silver
                            price = Convert.ToInt32(house.Price * 0.7);
                            break;
                        case 3: // Gold
                            price = Convert.ToInt32(house.Price * 0.75);
                            break;
                        case 4: // Platinum
                            price = Convert.ToInt32(house.Price * 0.8);
                            break;
                    }
                    Plugins.Trigger.ClientEvent(player, "openSellHome", "HOUSE_SELL_TOGOV", $"Вы действительно хотите продать дом за ${price}?");
                    MenuManager.Close(player);
                    return;
                case "cars":
                    OpenCarsMenu(player);
                    return;
                case "roommates":
                    OpenRoommatesMenu(player);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }
        public static void acceptHouseSellToGov(Player player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                return;
            }

            if (Main.Players[player].InsideGarageID != -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны выйти из гаража", 3000);
                return;
            }
            house.RemoveAllPlayers();
            house.SetOwner(null);
            Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 333);
            Plugins.Trigger.ClientEvent(player, "deleteGarageBlip");
            int price = 0;
            switch (Main.Accounts[player].VipLvl)
            {
                case 0: // None
                    price = Convert.ToInt32(house.Price * 0.6);
                    break;
                case 1: // Bronze
                    price = Convert.ToInt32(house.Price * 0.65);
                    break;
                case 2: // Silver
                    price = Convert.ToInt32(house.Price * 0.7);
                    break;
                case 3: // Gold
                    price = Convert.ToInt32(house.Price * 0.75);
                    break;
                case 4: // Platinum
                    price = Convert.ToInt32(house.Price * 0.8);
                    break;
            }
            Modules.Wallet.Change(player, price);
            Loggings.Money($"server", $"player({Main.Players[player].UUID})", Convert.ToInt32(house.Price * 0.6), $"houseSell({house.ID})");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы продали свой дом государству за {price}$", 3000);
        }
        public static void OpenCarsSellMenu(Player player)
        {
            Menu menu = new Menu("carsell", false, false)
            {
                Callback = callback_carsell
            };



            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Продажа автомобилей"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("label", Menu.MenuItem.Card)
            {
                Text = "Выберите машину, которую хотите продать"
            };
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                var vData = VehicleManager.Vehicles[v];
                var price = (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) ? Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5) : 0;
                menuItem = new Menu.Item(v, Menu.MenuItem.Button)
                {
                    Text = $"{vData.Model} - {v} ({price}$)"
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
        private static void callback_carsell(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            var vData = VehicleManager.Vehicles[item.ID];
            var price = (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) ? Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5) : 0;
            Modules.Wallet.Change(player, price);
            Loggings.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы продали {vData.Model} ({item.ID}) за {price}$", 3000);
            VehicleManager.Remove(item.ID);
            MenuManager.Close(player);
        }
        public static void OpenFurnitureMenu(Player player)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("furnitures", false, false)
            {
                Callback = callback_furniture0
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Мебель"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("buyfurniture", Menu.MenuItem.Button)
            {
                Text = "Покупка мебели"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("tofurniture", Menu.MenuItem.Button)
            {
                Text = "Управление мебелью"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_furniture0(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if (house.ID != Main.Players[player].InsideHouseID)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if (item.ID == "tofurniture")
            {
                if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет мебели", 3000);
                    MenuManager.Close(player);
                    return;
                }
                Menu nmenu = new Menu("furnitures", false, false)
                {
                    Callback = callback_furniture
                };

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
                {
                    Text = "Управление мебелью"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("furniture", Menu.MenuItem.List)
                {
                    Text = "ID:"
                };
                var list = new List<string>();
                foreach (var f in FurnitureManager.HouseFurnitures[house.ID]) list.Add(f.Value.ID.ToString());
                menuItem.Elements = list;
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("sellit", Menu.MenuItem.Button)
                {
                    Text = "Продать (7500$)"
                };
                nmenu.Add(menuItem);

                var furn = FurnitureManager.HouseFurnitures[house.ID][Convert.ToInt32(list[0])];
                menuItem = new Menu.Item("type", Menu.MenuItem.Card)
                {
                    Text = $"Тип: {furn.Name}"
                };
                nmenu.Add(menuItem);

                var open = (furn.IsSet) ? "Да" : "Нет";
                menuItem = new Menu.Item("isSet", Menu.MenuItem.Card)
                {
                    Text = $"Установлено: {open}"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("change", Menu.MenuItem.Button)
                {
                    Text = "Установить/Убрать"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("close", Menu.MenuItem.Button)
                {
                    Text = "Закрыть"
                };
                nmenu.Add(menuItem);

                nmenu.Open(player);
                return;
            }
            else if (item.ID == "buyfurniture")
            {

                Menu nmenu = new Menu("furnitures", false, false)
                {
                    Callback = callback_furniture1
                };

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
                {
                    Text = "Покупка мебели"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy1", Menu.MenuItem.Button)
                {
                    Text = "Оружейный сейф (15000$)"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy2", Menu.MenuItem.Button)
                {
                    Text = "Шкаф с одеждой (15000$)"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("buy3", Menu.MenuItem.Button)
                {
                    Text = "Шкаф с предметами (15000$)"
                };
                nmenu.Add(menuItem);

                menuItem = new Menu.Item("close", Menu.MenuItem.Button)
                {
                    Text = "Закрыть"
                };
                nmenu.Add(menuItem);

                nmenu.Open(player);
            }
        }
        private static void callback_furniture1(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if (house.ID != Main.Players[player].InsideHouseID)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if (FurnitureManager.HouseFurnitures[house.ID].Count() >= 50)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "В Вашей квартире уже слишком много мебели, продайте что-то", 3000);
                return;
            }
            if (item.ID == "buy1")
            {
                if (Main.Players[player].Money < 15000)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                Modules.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Оружейный сейф");
                Loggings.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Оружейный сейф)");
            }
            else if (item.ID == "buy2")
            {
                if (Main.Players[player].Money < 15000)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                Modules.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Шкаф с одеждой");
                Loggings.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Шкаф с одеждой)");
            }
            else if (item.ID == "buy3")
            {
                if (Main.Players[player].Money < 15000)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                    return;
                }
                Modules.Wallet.Change(player, -15000);
                FurnitureManager.newFurniture(house.ID, "Шкаф с предметами");
                Loggings.Money("server", $"player({Main.Players[player].UUID})", 15000, $"buyFurn({house.ID} | Шкаф с предметами)");
            }
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Поздравляем с успешной покупкой мебели!", 3000);
            MenuManager.Close(player);
        }
        private static void callback_furniture(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if (house.ID != Main.Players[player].InsideHouseID)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                MenuManager.Close(player);
                return;
            }
            if (Main.Players[player].InsideHouseID == -1 || Main.Players[player].InsideHouseID != house.ID)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться дома", 3000);
                MenuManager.Close(player);
                return;
            }
            if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет мебели", 3000);
                MenuManager.Close(player);
                return;
            }
            int id = Convert.ToInt32(data["1"]["Value"].ToString());
            var f = FurnitureManager.HouseFurnitures[house.ID][id];
            if (item.ID == "sellit")
            {
                if (f.IsSet)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Уберите мебель перед продажей.", 3000);
                    return;
                }
                Loggings.Money($"player({Main.Players[player].UUID})", "server", 7500, $"sellFurn({house.ID} | {f.Name})");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы успешно продали {f.Name} за 7500$", 3000);
                house.DestroyFurniture(f.ID);
                FurnitureManager.HouseFurnitures[house.ID].Remove(id);
                FurnitureManager.FurnituresItems[house.ID].Remove(id);
                Modules.Wallet.Change(player, 7500);
                MenuManager.Close(player);
                return;
            }
            switch (eventName)
            {
                case "button":
                    switch (f.IsSet)
                    {
                        case true:
                            house.DestroyFurniture(f.ID);
                            f.IsSet = false;
                            menu.Items[4].Text = $"Установлено: Нет";
                            menu.Change(player, 4, menu.Items[4]);
                            return;
                        case false:
                            if (player.HasData("IS_EDITING"))
                            {
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны закончить редактирование", 3000);
                                MenuManager.Close(player);
                                return;
                            }
                            player.SetData("IS_EDITING", true);
                            player.SetData("EDIT_ID", f.ID);
                            Plugins.Trigger.ClientEvent(player, "startEditing", f.Model);
                            MenuManager.Close(player);
                            return;
                    }
                case "listChangeleft":
                case "listChangeright":
                    menu.Items[3].Text = $"Тип: {f.Name}";
                    menu.Change(player, 3, menu.Items[3]);
                    var open = (f.IsSet) ? "Да" : "Нет";
                    menu.Items[4].Text = $"Установлено: {open}";
                    menu.Change(player, 4, menu.Items[4]);
                    return;
            }
        }
        public static void OpenRoommatesMenu(Player player)
        {
            Menu menu = new Menu("roommates", false, false)
            {
                Callback = callback_roommates
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Сожители"
            };
            menu.Add(menuItem);

            var house = GetHouse(player, true);
            if (house.Roommates.Count > 0)
            {
                menuItem = new Menu.Item("label", Menu.MenuItem.Card)
                {
                    Text = "Нажмите на имя игрока, которого хотите выселить"
                };
                menu.Add(menuItem);

                foreach (var p in house.Roommates)
                {
                    menuItem = new Menu.Item(p, Menu.MenuItem.Button)
                    {
                        Text = $"{p.Replace('_', ' ')}"
                    };
                    menu.Add(menuItem);
                }
            }
            else
            {
                menuItem = new Menu.Item("label", Menu.MenuItem.Card)
                {
                    Text = "У Вас никто не подселен в дом"
                };
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.Button)
            {
                Text = "Назад"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_roommates(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(player);
                return;
            }

            var mName = item.ID;
            var roomMate = NAPI.Player.GetPlayerFromName(mName);

            var house = GetHouse(player);
            if (house.Roommates.Contains(mName)) house.Roommates.Remove(mName);

            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выселили {mName} из своего дома", 3000);
        }
        public static void OpenCarsMenu(Player player)
        {
            Menu menu = new Menu("cars", false, false)
            {
                Callback = callback_cars
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Машины"
            };
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(v, Menu.MenuItem.Button)
                {
                    Text = $"{VehicleManager.Vehicles[v].Model} - {v}"
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
        private static void callback_cars(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "close") return;
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_cars: " + e.Message + e.Message, Plugins.Logs.Type.Error); }
            });
        }
        public static void OpenSelectedCarMenu(Player player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false)
            {
                Callback = callback_selectedcar
            };

            var vData = VehicleManager.Vehicles[number];

            var house = GetHouse(player);
            var garage = GarageManager.Garages[house.GarageID];
            var check = garage.CheckCar(false, number);
            var check_pos = (string.IsNullOrEmpty(vData.Position)) ? false : true;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = number
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card)
            {
                Text = vData.Model
            };
            menu.Add(menuItem);

            var vClass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(vData.Model));

            menuItem = new Menu.Item("repair", Menu.MenuItem.Button)
            {
                Text = $"Восстановить {VehicleManager.VehicleRepairPrice[vClass]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("key", Menu.MenuItem.Button)
            {
                Text = $"Получить дубликат ключа"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("changekey", Menu.MenuItem.Button)
            {
                Text = $"Сменить замки"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("spawnmycar", Menu.MenuItem.Button)
            {
                Text = $"Вызвать авто (2500$)"
            };
            menu.Add(menuItem);

            if (check)
            {
                menuItem = new Menu.Item("evac", Menu.MenuItem.Button)
                {
                    Text = $"Эвакуировать машину"
                };
                menu.Add(menuItem);

                menuItem = new Menu.Item("gps", Menu.MenuItem.Button)
                {
                    Text = $"Отметить в GPS"
                };
                menu.Add(menuItem);
            }
            else if (check_pos)
            {
                menuItem = new Menu.Item("evac_pos", Menu.MenuItem.Button)
                {
                    Text = $"Эвакуировать машину"
                };
                menu.Add(menuItem);
            }

            int price = 0;
            if (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model))
            {
                price = Main.Accounts[player].VipLvl switch
                {
                    // None
                    0 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                    // Bronze
                    1 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6),
                    // Silver
                    2 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7),
                    // Gold
                    3 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8),
                    // Platinum
                    4 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9),
                    _ => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                };
            }
            menuItem = new Menu.Item("sell", Menu.MenuItem.Button)
            {
                Text = $"Продать ({price}$)"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_selectedcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "sell":
                    player.SetData("CARSELLGOV", menu.Items[0].Text);
                    VehicleManager.VehicleData vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    int price = 0;
                    if (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model))
                    {
                        price = Main.Accounts[player].VipLvl switch
                        {
                            // None
                            0 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                            // Bronze
                            1 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.6),
                            // Silver
                            2 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.7),
                            // Gold
                            3 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.8),
                            // Platinum
                            4 => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.9),
                            _ => Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5),
                        };
                    }
                    Plugins.Trigger.ClientEvent(player, "openDialog", "CAR_SELL_TOGOV", $"Вы действительно хотите продать государству {vData.Model} ({menu.Items[0].Text}) за ${price}?");
                    MenuManager.Close(player);
                    return;
                case "repair":
                    vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    if (vData.Health > 0)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Машина не нуждается в восстановлении", 3000);
                        return;
                    }
                    var vClass = NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(vData.Model));
                    if (!Modules.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "У Вас недостаточно средств", 3000);
                        return;
                    }
                    vData.Items = new List<nItem>();
                    Loggings.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                    vData.Health = 1000;
                    var garage = GarageManager.Garages[GetHouse(player).GarageID];
                    garage.SendVehicleIntoGarage(menu.Items[0].Text);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы восстановили {vData.Model} ({menu.Items[0].Text})", 3000);
                    return;
                case "evac":
                    if (!Main.Players.ContainsKey(player)) return;
                    var number = menu.Items[0].Text;
                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    var check = garage.CheckCar(false, number);

                    if (!check)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Эта машина стоит в гараже", 3000);
                        return;
                    }
                    if (Main.Players[player].Money < 200)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств (не хватает {200 - Main.Players[player].Money}$)", 3000);
                        return;
                    }

                    var veh = garage.GetOutsideCar(number);
                    if (veh == null) return;
                    VehicleManager.Vehicles[number].Fuel = (!NAPI.Data.HasEntityData(veh, "PETROL")) ? VehicleManager.VehicleTank[veh.Class] : NAPI.Data.GetEntityData(veh, "PETROL");
                    NAPI.Entity.DeleteEntity(veh);
                    garage.SendVehicleIntoGarage(number);

                    Modules.Wallet.Change(player, -200);
                    Loggings.Money($"player({Main.Players[player].UUID})", $"server", 200, $"carEvac");
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Ваша машина была отогнана в гараж", 3000);
                    return;
                case "spawnmycar":
                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    number = menu.Items[0].Text;
                    check = garage.CheckCar(false, number);
                    if (!check)
                    {
                        if (number != null)
                        {
                            var pricespawncar = 2500;
                            if (Main.Players[player].Money > pricespawncar)
                                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.BottomCenter, $"Ваша машина будет доставлена в течении 10-ти секунд", 3000);
                            Modules.Wallet.Change(player, -pricespawncar);
                            NAPI.Task.Run(() =>
                            {
                                garage.SpawnCarAtPosition(player, number, player.Position, player.Rotation);
                            }, delayTime: 10000);

                        }
                    }
                    else
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.BottomCenter, "Эта машина не стоит в гараже", 3000);
                        return;
                    }
                    return;
                case "evac_pos":
                    if (!Main.Players.ContainsKey(player)) return;

                    number = menu.Items[0].Text;
                    if (string.IsNullOrEmpty(VehicleManager.Vehicles[number].Position))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Машина не нуждается в эвакуации", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[number].Position = null;
                    VehicleManager.Save(number);

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    garage.SendVehicleIntoGarage(number);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Ваша машина была эвакуирована в гараж", 3000);
                    return;
                case "gps":
                    if (!Main.Players.ContainsKey(player)) return;

                    number = menu.Items[0].Text;
                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    check = garage.CheckCar(false, number);

                    if (!check)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Эта машина стоит в гараже", 3000);
                        return;
                    }

                    veh = garage.GetOutsideCar(number);
                    if (veh == null) return;

                    Plugins.Trigger.ClientEvent(player, "createWaypoint", veh.Position.X, veh.Position.Y);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "В GPS было отмечено расположение Вашей машины", 3000);
                    return;
                case "key":
                    if (!Main.Players.ContainsKey(player)) return;

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    if (garage.Type == -1)
                    {
                        if (player.Position.DistanceTo(garage.Position) > 4)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться около гаража", 3000);
                            return;
                        }
                    }
                    else
                    {
                        if (Main.Players[player].InsideGarageID == -1)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в гараже", 3000);
                            return;
                        }
                    }

                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{VehicleManager.Vehicles[menu.Items[0].Text].KeyNum}"));
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы получили ключ от машины с номером {menu.Items[0].Text}", 3000);
                    return;
                case "changekey":
                    if (!Main.Players.ContainsKey(player)) return;

                    garage = GarageManager.Garages[GetHouse(player).GarageID];
                    if (garage.Type == -1)
                    {
                        if (player.Position.DistanceTo(garage.Position) > 4)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться около гаража", 3000);
                            return;
                        }
                    }
                    else
                    {
                        if (Main.Players[player].InsideGarageID == -1)
                        {
                            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться в гараже", 3000);
                            return;
                        }
                    }

                    if (!Modules.Wallet.Change(player, -1000))
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Смена замков стоит $1000", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[menu.Items[0].Text].KeyNum++;
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы сменили замки на машине {menu.Items[0].Text}. Теперь старые ключи не могут быть использованы", 3000);
                    return;
            }
        }
        public static void InviteToRoom(Player player, Player guest)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет личного дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас в доме проживает максимальное кол-во игроков", 3000);
                return;
            }

            if (GetHouse(guest) != null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE", $"Игрок ({player.Value}) предложил Вам подселиться к нему");

            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили игроку ({guest.Value}) подселиться к Вам", 3000);
        }
        public static void acceptRoomInvite(Player player)
        {
            Player owner = player.GetData<Player>("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            House house = GetHouse(owner, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет личного дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В доме проживает максимальное кол-во игроков", 3000);
                return;
            }

            house.Roommates.Add(player.Name);
            Plugins.Trigger.ClientEvent(player, "createCheckpoint", 333, 1, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 1.12), 1, NAPI.GlobalDimension, 220, 220, 0);
            Plugins.Trigger.ClientEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);

            Plugins.Notice.Send(owner, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) подселился к Вам", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы подселились к игроку ({owner.Value})", 3000);
        }
        public static void InvitePlayerToHouse(Player player, Player guest)
        {
            House house = GetHouse(player);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                return;
            }
            guest.SetData("InvitedHouse_ID", house.ID);
            Plugins.Notice.Send(guest, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) пригласил Вас в свой дом", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы пригласили игрока ({guest.Value}) в свой дом", 3000);
        }
        public static void OfferHouseSell(Player player, Player target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetHouse(player, true);
            if (house == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (GetHouse(target, true) != null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока уже есть дом", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Plugins.Trigger.ClientEvent(target, "openDialog", "HOUSE_SELL", $"Игрок ({player.Value}) предложил Вам купить свой дом за ${price}");
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы предложили игроку ({target.Value}) купить Ваш дом за {price}$", 3000);
        }
        public static void acceptHouseSell(Player player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData<Player>("HOUSE_SELLER"))) return;
            Player seller = player.GetData<Player>("HOUSE_SELLER");

            if (GetHouse(player, true) != null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У Вас уже есть дом", 3000);
                return;
            }

            House house = GetHouse(seller, true);
            var price = player.GetData<int>("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!Modules.Wallet.Change(player, -price))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            Modules.Wallet.Change(seller, price);
            Loggings.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("deleteCheckpoint", 333);
            seller.TriggerEvent("deleteGarageBlip");
            house.SetOwner(player);
            house.Save();

            Plugins.Notice.Send(seller, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) купил у Вас дом", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы купили дом у игрока ({seller.Value})", 3000);
        }
    }
}