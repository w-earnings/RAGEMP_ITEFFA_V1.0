using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using iTeffa.Globals;
using iTeffa.Settings;
using iTeffa.Interface;

namespace iTeffa.Working
{
    class WorkManager : Script
    {
        private static readonly Nlogs Log = new Nlogs("WorkManager");
        public static Random rnd = new Random();

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Points[0], 1, 2, 0)); // job placement
                Cols[0].OnEntityEnterColShape += JobMenu_onEntityEnterColShape; // job placement point handler
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Работа"), new Vector3(Points[0].X, Points[0].Y, Points[0].Z + 0.5), 10F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.TextLabel.CreateTextLabel("~w~Дайвер", new Vector3(1695.806, 43.05446, 162.9473), 30f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                NAPI.Marker.CreateMarker(1, Points[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));

                
                
                // blips
                NAPI.Blip.CreateBlip(729, new Vector3(1695.163, 42.85501, 160.6473), 0.75f, 46, Main.StringToU16("Дайвер"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(354, new Vector3(724.9625, 133.9959, 79.83643), 0.75F, 46, Main.StringToU16("Электростанция"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(408, new Vector3(105.4633, -1568.843, 28.60269), 0.75F, 3, Main.StringToU16("Почта"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(198, new Vector3(903.3215, -191.7, 73.40494), 0.75F, 46, Main.StringToU16("Такси"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(198, new Vector3(1956.65015, 3769.12817, 31.0833454), 0.75F, 46, Main.StringToU16("Такси"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(198, new Vector3(1791.82837, 4586.595, 36.2361145), 0.75F, 46, Main.StringToU16("Такси"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(513, new Vector3(462.6476, -605.5295, 27.49518), 0.75F, 46, Main.StringToU16("Автобусная станция"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(512, new Vector3(-1331.475, 53.58579, 53.53268), 0.75F, 2, Main.StringToU16("Газонокосилка"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(477, new Vector3(588.2037, -3037.641, 6.303829), 0.75F, 3, Main.StringToU16("Дальнобойщики"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(477, new Vector3(338.9279, 3417.426, 35.38838), 0.75F, 3, Main.StringToU16("Дальнобойщики"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(477, new Vector3(-2212.77, 4249.193, 46.17959), 0.75F, 3, Main.StringToU16("Дальнобойщики"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(915.9069, -1265.255, 25.52912), 0.75F, 25, Main.StringToU16("Инкассаторы"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(-1481.75537, -508.08847, 31.6868382), 0.75F, 25, Main.StringToU16("Инкассаторы"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(-144.374817, 6354.90869, 30.3706112), 0.75F, 25, Main.StringToU16("Инкассаторы"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(544, new Vector3(473.9508, -1275.597, 29.60513), 0.75F, 40, Main.StringToU16("Автомеханики"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(225, new Vector3(-530.78833, 59.4290543, 52.57218), 0.75F, 1, Main.StringToU16("Аренда машин"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(478, Truckers.getProduct[0], 0.75F, 84, Main.StringToU16("Склад продуктов"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(478, Truckers.getProduct[1], 0.75F, 36, Main.StringToU16("Склад бензина"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(478, Truckers.getProduct[2], 0.75F, 15, Main.StringToU16("Автосклад"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(478, Truckers.getProduct[3], 0.75F, 62, Main.StringToU16("Склад оружия"), 255, 0, true, 0, 0);

                // markers
                NAPI.Marker.CreateMarker(1, new Vector3(105.4633, -1568.843, 28.60269) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, new Vector3(106.2007, -1563.748, 28.60272) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, new Vector3(-0.51, -436.71, 38.74) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        public void JobMenu_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                openJobsSelecting(entity);
            }
            catch (Exception ex) { Log.Write("JobMenu_onEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
        }

        private static readonly SortedDictionary<int, ColShape> Cols = new SortedDictionary<int, ColShape>();
        public static List<string> JobStats = new List<string>
        {
            "Электрик",
            "Почтальон",
            "Таксист",
            "Водитель автобуса",
            "Газонокосильщик",
            "Дальнобойщик",
            "Инкассатор",
            "Автомеханик",
            "Водолазом",
            "Строителем",
        };
        public static SortedList<int, Vector3> Points = new SortedList<int, Vector3>
        {
            {0, new Vector3(247.6266, 219.5235, 105.2868) },  // Employment center
            {1, new Vector3(724.9625, 133.9959, 79.83643) },  // Electrician job
            {2, new Vector3(105.4633, -1568.843, 28.60269) },  // Postal job
            {3, new Vector3(903.3215,-191.7,73.40494) },      // Taxi job
            {4, new Vector3(406.2858, -649.6152, 28.49641) }, // Bus driver job
            {5, new Vector3(-1331.475, 53.58579, 53.53268) },  // Lawnmower job
            {6, new Vector3(588.2037, -3037.641, 6.303829) },  // Trucker job
            {7, new Vector3(915.9069, -1265.255, 25.52912) },  // Collector job
            {8, new Vector3(473.9508, -1275.597, 29.60513) },  // AutoMechanic job
            {9, new Vector3(1695.163, 42.85501, 160.6473) },  // Водолаз
        };
        private static readonly SortedList<int, string> JobList = new SortedList<int, string>
        {
            {1, "электриком" },
            {2, "почтальоном" },
            {3, "таксистом" },
            {4, "водителем автобуса" },
            {5, "газонокосильщиком" },
            {6, "дальнобойщиком" },
            {7, "инкассатором" },
            {8, "автомехаником" },
            {9, "Водолазом" },
        };
        private static readonly SortedList<int, int> JobsMinLVL = new SortedList<int, int>()
        {
            { 1, 0 },
            { 2, 1 },
            { 3, 2 },
            { 4, 2 },
            { 5, 0 },
            { 6, 5 },
            { 7, 8 },
            { 8, 4 },
            { 9, 3 },
        };

        public static void Layoff(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 0)
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы уволились с работы", 3000);
                Main.Players[player].WorkID = 0;
                Dashboard.sendStats(player);
                Trigger.ClientEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
            else
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы никем не работаете", 3000);
        }
        public static void JobJoin(Player player, int job)
        {
            if (Main.Players[player].WorkID != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Для начала увольтесь с предыдущей работы.", 3000);
                return;
            }
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }

            if (Main.Players[player].WorkID == job)
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы уже работаете {JobList[job]}", 3000);
            else
            {
                if (Main.Players[player].LVL < JobsMinLVL[job])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Необходим как минимум {JobsMinLVL[job]} уровень", 3000);
                    return;
                }
                if ((job == 3 || job == 8) && !Main.Players[player].Licenses[1])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет лицензии категории B", 3000);
                    return;
                }
                if ((job == 4 || job == 6 || job == 7) && !Main.Players[player].Licenses[2])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет лицензии категории C", 3000);
                    return;
                }
                Main.Players[player].WorkID = job;
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы устроились работать {JobList[job]}. Доберитесь до точки начала работы", 3000);
                Trigger.ClientEvent(player, "createWaypoint", Points[job].X, Points[job].Y);
                Dashboard.sendStats(player);
                Trigger.ClientEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
        }
        // REQUIRED REFACTOR //
        public static void load(Player player)
        {
            NAPI.Data.SetEntityData(player, "ON_WORK", false);
            NAPI.Data.SetEntityData(player, "PAYMENT", 0);
            NAPI.Data.SetEntityData(player, "BUS_ONSTOP", false);
            NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", false);
            NAPI.Data.SetEntityData(player, "IS_REQUESTED", false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
            player.SetData("PACKAGES", 0);
            NAPI.Data.SetEntityData(player, "WORK", null);
            NAPI.Data.SetEntityData(player, "WORKWAY", -1);
            NAPI.Data.SetEntityData(player, "IS_PRICED", false);
            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
            NAPI.Data.SetEntityData(player, "WANTED", 0);
            NAPI.Data.SetEntityData(player, "REQUEST", "null");
            player.SetData("IS_IN_ARREST_AREA", false);
            player.SetData("PAYMENT", 0);
            player.SetData("INTERACTIONCHECK", 0);
            player.SetData("IN_HOSPITAL", false);
            player.SetData("MEDKITS", 0);
            player.SetData("GANGPOINT", -1);
            player.SetData("CUFFED_BY_COP", false);
            player.SetData("CUFFED_BY_MAFIA", false);
            player.SetData("IS_CALL_MECHANIC", false);
            NAPI.Data.SetEntityData(player, "CARROOM_CAR", null);
        }

        #region Jobs
        #region Jobs Selecting
        public static void openJobsSelecting(Player player)
        {
            Trigger.ClientEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
        }
        [RemoteEvent("jobjoin")]
        public static void callback_jobsSelecting(Player client, int act)
        {
            try
            {
                switch (act)
                {
                    case -1:
                        Layoff(client);
                        return;
                    default:
                        JobJoin(client, act);
                        return;
                }
            }
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, Nlogs.Type.Error); }
        }
        #endregion
        #region GoPostal Job
        public static void openGoPostalStart(Player player)
        {
            Menu menu = new Menu("gopostal", false, false)
            {
                Callback = callback_gpStartMenu
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Склад"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("start", Menu.MenuItem.Button)
            {
                Text = "Начать работу"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("get", Menu.MenuItem.Button)
            {
                Text = "Взять посылки"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("finish", Menu.MenuItem.Button)
            {
                Text = "Закончить работу"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_gpStartMenu(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (!Main.Players.ContainsKey(client) || client.Position.DistanceTo(Gopostal.Coords[0]) > 15)
            {
                MenuManager.Close(client);
                return;
            }
            switch (item.ID)
            {
                case "start":
                    if (Main.Players[client].WorkID == 2)
                    {
                        if (!NAPI.Data.GetEntityData(client, "ON_WORK"))
                        {
                            if (Houses.HouseManager.Houses.Count == 0) return;
                            client.SetData("PACKAGES", 10);
                            Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы получили 10 посылок, развезите их по домам", 3000);
                            client.SetData("ON_WORK", true);

                            client.SetData("W_LASTPOS", client.Position);
                            client.SetData("W_LASTTIME", DateTime.Now);
                            var next = rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                            while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(client.Position) < 200)
                                next = rnd.Next(0, Houses.HouseManager.Houses.Count - 1);

                            client.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);
                            Trigger.ClientEvent(client, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                            Trigger.ClientEvent(client, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                            Trigger.ClientEvent(client, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                            var gender = Main.Players[client].Gender;
                            Customization.ClearClothes(client, gender);
                            if (gender)
                            {
                                Customization.SetHat(client, 76, 10);
                                client.SetClothes(11, 38, 3);
                                client.SetClothes(4, 17, 0);
                                client.SetClothes(6, 1, 7);
                                client.SetClothes(3, Customization.CorrectTorso[gender][38], 0);
                            }
                            else
                            {
                                Customization.SetHat(client, 75, 10);
                                client.SetClothes(11, 0, 6);
                                client.SetClothes(4, 25, 2);
                                client.SetClothes(6, 1, 2);
                                client.SetClothes(3, Customization.CorrectTorso[gender][0], 0);
                            }

                            int x = rnd.Next(0, Gopostal.GoPostalObjects.Count);
                            BasicSync.AttachObjectToPlayer(client, Gopostal.GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        }
                        else Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы уже начали рабочий день", 3000);
                    }
                    else Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не работаете курьером. Устроиться можно в мэрии", 3000);
                    return;
                case "get":
                    {
                        if (Main.Players[client].WorkID != 2)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не работаете курьером", 3000);
                            return;
                        }
                        if (!client.GetData<bool>("ON_WORK"))
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не начали рабочий день", 3000);
                            return;
                        }
                        if (client.GetData<int>("PACKAGES") != 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не раздали все посылки. У Вас осталось ещё {client.GetData<int>("PACKAGES")} штук", 3000);
                            return;
                        }
                        if (Houses.HouseManager.Houses.Count == 0) return;
                        client.SetData("PACKAGES", 10);
                        Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы получили 10 посылок. Развезите их по домам", 3000);

                        client.SetData("W_LASTPOS", client.Position);
                        client.SetData("W_LASTTIME", DateTime.Now);
                        var next = rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(client.Position) < 200)
                            next = Working.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                        client.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);

                        Trigger.ClientEvent(client, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                        Trigger.ClientEvent(client, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                        Trigger.ClientEvent(client, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                        int y = Working.WorkManager.rnd.Next(0, Working.Gopostal.GoPostalObjects.Count);
                        BasicSync.AttachObjectToPlayer(client, Working.Gopostal.GoPostalObjects[y], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        return;
                    }
                case "finish":
                    if (Main.Players[client].WorkID == 2)
                    {
                        if (NAPI.Data.GetEntityData(client, "ON_WORK"))
                        {
                            Trigger.ClientEvent(client, "deleteCheckpoint", 1, 0);
                            BasicSync.DetachObject(client);

                            Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы закончили рабочий день", 3000);
                            Trigger.ClientEvent(client, "deleteWorkBlip");

                            client.SetData("PAYMENT", 0);
                            Customization.ApplyCharacter(client);
                            if (client.HasData("HAND_MONEY")) client.SetClothes(5, 45, 0);
                            else if (client.HasData("HEIST_DRILL")) client.SetClothes(5, 41, 0);

                            client.SetData("PACKAGES", 0);
                            client.SetData("ON_WORK", false);

                            if (client.GetData<Vehicle>("WORK") != null)
                            {
                                NAPI.Entity.DeleteEntity(client.GetData<Vehicle>("WORK"));
                                client.SetData<Vehicle>("WORK", null);
                            }
                        }
                        else Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не работаете", 3000);

                    }
                    else Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не работаете курьером", 3000);
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }
        #endregion
        #region Truckers Job
        public static void OpenTruckersOrders(Player player)
        {
            Menu menu = new Menu("truckersorders", false, false);
            menu.Callback += callback_truckersorders;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Заказы"
            };
            menu.Add(menuItem);

            Order order = null;
            List<string> ordersIDs = new List<string>();
            foreach (var o in BusinessManager.Orders)
            {
                var biz = BusinessManager.BizList[o.Value];
                var temp_order = biz.Orders.FirstOrDefault(or => or.UID == o.Key);
                if (temp_order == null || temp_order.Taked) continue;
                if (order == null) order = temp_order;
                ordersIDs.Add(o.Key.ToString());
            }

            if (ordersIDs.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Нет свободных заказов", 3000);
                return;
            }

            menuItem = new Menu.Item("products", Menu.MenuItem.List)
            {
                Elements = ordersIDs
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("Name", Menu.MenuItem.Card)
            {
                Text = $"Продукт: {order.Name}"
            };
            menu.Add(menuItem);

            var youGet = Convert.ToInt32(order.Amount * BusinessManager.ProductsOrderPrice[order.Name] * 0.1);
            var max = Convert.ToInt32(2000 * Group.GroupPayAdd[Main.Accounts[player].VipLvl]);
            var min = Convert.ToInt32(500 * Group.GroupPayAdd[Main.Accounts[player].VipLvl]);
            if (youGet > max) youGet = max;
            else if (youGet < min) youGet = min;
            menuItem = new Menu.Item("youget", Menu.MenuItem.Card)
            {
                Text = $"Вы получите: {youGet}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button)
            {
                Text = "Взять заказ"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }

        public static List<Vector3> getProduct = new List<Vector3>()
        {
            new Vector3(95.82169, 6363.628, 30.37586), // 24/7 products
            new Vector3(2786.021, 1575.39, 23.50065), // petrol products
            new Vector3(148.6672, 6362.376, 30.52923), // autos
            new Vector3(148.6672, 6362.376, 30.52923),
            new Vector3(148.6672, 6362.376, 30.52923),
            new Vector3(148.6672, 6362.376, 30.52923),
            new Vector3(2710.076, 3454.989, 55.31736), // gun products
            new Vector3(95.82169, 6363.628, 30.37586), // clothes
            new Vector3(95.82169, 6363.628, 30.37586), // burgershot
            new Vector3(95.82169, 6363.628, 30.37586), // tattoo-salon
            new Vector3(95.82169, 6363.628, 30.37586), // barber-shop
            new Vector3(95.82169, 6363.628, 30.37586), // mask-shop
            new Vector3(95.82169, 6363.628, 30.37586), // ls customs
            new Vector3(95.82169, 6363.628, 30.37586), // car wash
        };

        private static void callback_truckersorders(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            List<Order> orders = client.GetData<List<Order>>("TRUCKERORDERLIST");
            switch (eventName)
            {
                case "listChangeright":
                case "listChangeleft":
                    {
                        var uid = Convert.ToInt32(data["1"]["Value"].ToString());
                        if (!BusinessManager.Orders.ContainsKey(uid)) return;

                        Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
                        var order = biz.Orders.FirstOrDefault(o => o.UID == uid);

                        menu.Items[2].Text = $"Продукт: {order.Name}";
                        menu.Change(client, 2, menu.Items[2]);

                        var youGet = Convert.ToInt32(order.Amount * BusinessManager.ProductsOrderPrice[order.Name] * 0.1);
                        var max = Convert.ToInt32(2000 * Group.GroupPayAdd[Main.Accounts[client].VipLvl]);
                        var min = Convert.ToInt32(500 * Group.GroupPayAdd[Main.Accounts[client].VipLvl]);
                        if (youGet > max) youGet = max;
                        else if (youGet < min) youGet = min;
                        menu.Items[3].Text = $"Вы получите: {youGet}$";
                        menu.Change(client, 3, menu.Items[3]);
                        return;
                    }
                case "button":
                    {
                        if (item.ID == "close")
                            MenuManager.Close(client);
                        else
                        {
                            if (client.HasData("ORDER"))
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы уже взяли заказ", 3000);
                                return;
                            }
                            var uid = Convert.ToInt32(data["1"]["Value"].ToString());
                            if (!BusinessManager.Orders.ContainsKey(uid))
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Такого заказа больше не существует", 3000);
                                return;
                            };

                            Business biz = BusinessManager.BizList[BusinessManager.Orders[uid]];
                            var order = biz.Orders.FirstOrDefault(o => o.UID == uid);
                            if (order == null || order.Taked)
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Этот заказ уже взял кто-то другой", 3000);
                                return;
                            }

                            order.Taked = true;

                            client.SetData("ORDERDATE", DateTime.Now.AddMinutes(6));

                            Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы взяли заказ по доставке {order.Name} в {BusinessManager.BusinessTypeNames[biz.Type]}. Сначала закупите товар", 3000);
                            var pos = getProduct[biz.Type];
                            Trigger.ClientEvent(client, "createWaypoint", pos.X, pos.Y);
                            client.SetData("ORDER", uid);
                            MenuManager.Close(client);
                        }
                        return;
                    }
            }
        }
        #endregion
        #endregion
    }
}