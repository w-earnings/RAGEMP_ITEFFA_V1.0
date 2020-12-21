using System.Collections.Generic;
using System;
using GTANetworkAPI;
using iTeffa.Interface;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class DrivingSchool : Script
    {
        // мотоциклы, легковые машины, грузовые, водный, вертолёты, самолёты
        private static readonly List<int> LicPrices = new List<int>() { 600, 1000, 3000, 6000, 10000, 10000 };
        private static readonly Vector3 enterSchool = new Vector3(228.572708, 373.805969, 104.994225);
        private static readonly List<Vector3> startCourseCoord = new List<Vector3>()
        {
            new Vector3(213.8353, 389.4972, 106.6874),
        };
        private static readonly List<Vector3> startCourseRot = new List<Vector3>()
        {
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
            new Vector3(-0.08991995, -0.000970318, 51.23025),
        };
        private static readonly List<Vector3> drivingCoords = new List<Vector3>()
        {
            new Vector3(188.7008, 366.5621, 107.6869),
            new Vector3(140.3896, 362.6936, 111.3745),
            new Vector3(59.31527, 318.6287, 111.6048),
            new Vector3(25.11215, 249.0263, 109.4103),
            new Vector3(8.065119, 201.3615, 104.7364),
            new Vector3(-34.12751, 86.93284, 75.26752),
            new Vector3(-54.11743, 11.41459, 71.952),
            new Vector3(-71.31182, -59.41452, 59.85419),
            new Vector3(-94.23499, -131.5553, 57.34979),
            new Vector3(-116.3168, -223.1266, 44.65726),
            new Vector3(-160.103, -347.5135, 34.55911),
            new Vector3(-208.4418, -388.8625, 31.55594),
            new Vector3(-254.0737, -355.5688, 29.80524),
            new Vector3(-266.2953, -266.59, 31.72675),
            new Vector3(-269.8519, -201.0853, 38.45251),
            new Vector3(-261.4669, -116.0676, 45.96093),
            new Vector3(-246.2458, -74.34864, 49.06917),
            new Vector3(-191.4826, -80.41444, 51.67457),
            new Vector3(-129.636, -96.98338, 55.98493),
            new Vector3(-51.06133, -122.5222, 57.77376),
            new Vector3(13.30934, -141.3208, 55.99965),
            new Vector3(77.85547, -163.4343, 54.99051),
            new Vector3(106.8497, -85.77554, 62.53413),
            new Vector3(137.0112, -5.062579, 67.47971),
            new Vector3(163.5823, 67.71493, 81.28201),
            new Vector3(193.233, 147.0089, 102.3583),
            new Vector3(210.9522, 196.814, 105.4418),
            new Vector3(237.1179, 270.5571, 105.3869),
            new Vector3(252.6139, 341.5507, 105.4139),
            new Vector3(219.1046, 357.64, 105.7917),
            new Vector3(213.2493, 388.6108, 106.7043),
        };

        private static readonly Nlogs Log = new Nlogs("Driving School");

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1, 2, 0);
                shape.OnEntityEnterColShape += onPlayerEnterSchool;
                shape.OnEntityExitColShape += onPlayerExitSchool;

                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Driving School"), new Vector3(enterSchool.X, enterSchool.Y, enterSchool.Z + 1), 5f, 0.3f, 0, new Color(255, 255, 255));
                var blip = NAPI.Blip.CreateBlip(545, enterSchool, 0.75F, 29, Main.StringToU16("Driving School"), 255, 0, true, 0);

                for (int i = 0; i < drivingCoords.Count; i++)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(drivingCoords[i], 4, 5, 0);
                    colshape.OnEntityEnterColShape += onPlayerEnterDrive;
                    colshape.SetData("NUMBER", i);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("SCHOOLVEH") && player.GetData<Vehicle>("SCHOOLVEH") == vehicle)
                {
                    player.SetData("SCHOOL_TIMER", Timers.StartOnce(60000, () => timer_exitVehicle(player)));

                    Notify.Send(player, NotifyType.Warning, NotifyPosition.TopCenter, $"Если вы не сядете в машину в течение 60 секунд, то провалите экзамен", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, Nlogs.Type.Error); }
        }

        private void timer_exitVehicle(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    if (!player.HasData("SCHOOLVEH")) return;
                    if (player.IsInVehicle && player.Vehicle == player.GetData<Vehicle>("SCHOOLVEH")) return;
                    NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("SCHOOLVEH"));
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    Timers.Stop(player.GetData<string>("SCHOOL_TIMER"));
                    player.ResetData("SCHOOL_TIMER");
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.TopCenter, $"Вы провалили экзмен", 3000);
                }
                catch (Exception e) { Log.Write("TimerDrivingSchool: " + e.Message, Nlogs.Type.Error); }
            });
        }

        public static void onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player.HasData("SCHOOLVEH")) NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("SCHOOLVEH"));
                }
                catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
            }, 0);
        }
        public static void startDrivingCourse(Player player, int index)
        {
            if (player.HasData("IS_DRIVING") || player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете сделать это сейчас", 3000);
                return;
            }
            if (Main.Players[player].Licenses[index])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас уже есть эта лицензия", 3000);
                return;
            }
            switch (index)
            {
                case 0:
                    if (Main.Players[player].Money < LicPrices[0])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    var vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Bagger, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 0);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    Finance.Wallet.Change(player, -LicPrices[0]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[0];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[0], $"buyLic");
                    Globals.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 1:
                    if (Main.Players[player].Money < LicPrices[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Dilettante, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 1);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    Finance.Wallet.Change(player, -LicPrices[1]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[1];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[1], $"buyLic");
                    Globals.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 2:
                    if (Main.Players[player].Money < LicPrices[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    vehicle = NAPI.Vehicle.CreateVehicle(VehicleHash.Flatbed, startCourseCoord[0], startCourseRot[0], 30, 30);
                    player.SetIntoVehicle(vehicle, 0);
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.SetData("DRIVER", player);
                    player.SetData("IS_DRIVING", true);
                    player.SetData("LICENSE", 2);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);
                    player.SetData("CHECK", 0);
                    Finance.Wallet.Change(player, -LicPrices[2]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[2];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[2], $"buyLic");
                    Globals.VehicleStreaming.SetEngineState(vehicle, false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Чтобы завести транспорт, нажмите B", 3000);
                    return;
                case 3:
                    if (Main.Players[player].Money < LicPrices[3])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[3] = true;
                    Finance.Wallet.Change(player, -LicPrices[3]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[3];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[3], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно купили лицензию на водный транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 4:
                    if (Main.Players[player].Money < LicPrices[4])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"", 3000);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[4] = true;
                    Finance.Wallet.Change(player, -LicPrices[4]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[4];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[4], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно купили лицензию управление вертолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 5:
                    if (Main.Players[player].Money < LicPrices[5])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[5] = true;
                    Finance.Wallet.Change(player, -LicPrices[5]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[5];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[5], $"buyLic");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно купили лицензию управление самолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
            }
        }
        private void onPlayerEnterSchool(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 39);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), Nlogs.Type.Error); }
        }
        private void onPlayerExitSchool(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        private void onPlayerEnterDrive(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != 0) return;
                if (!player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "SCHOOL") return;
                if (!player.HasData("IS_DRIVING")) return;
                if (player.Vehicle != player.GetData<Vehicle>("SCHOOLVEH")) return;
                if (shape.GetData<int>("NUMBER") != player.GetData<int>("CHECK")) return;
                var check = player.GetData<int>("CHECK");
                if (check == drivingCoords.Count - 1)
                {
                    player.ResetData("IS_DRIVING");
                    var vehHP = player.Vehicle.Health;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                        }
                        catch { }
                    });
                    player.ResetData("SCHOOLVEH");
                    if (vehHP < 500)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы провалили экзамен", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[player.GetData<int>("LICENSE")] = true;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы успешно сдали экзамен", 3000);
                    Dashboard.sendStats(player);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    return;
                }

                player.SetData("CHECK", check + 1);
                if (check + 2 < drivingCoords.Count)
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0, drivingCoords[check + 2] - new Vector3(0, 0, 1.12));
                else
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[check + 1] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                Trigger.ClientEvent(player, "createWaypoint", drivingCoords[check + 1].X, drivingCoords[check + 1].Y);
            }
            catch (Exception e)
            {
                Log.Write("ENTERDRIVE:\n" + e.ToString(), Nlogs.Type.Error);
            }
        }

        #region menu
        public static void OpenDriveSchoolMenu(Player player)
        {
            Menu menu = new Menu("driveschool", false, false)
            {
                Callback = callback_driveschool
            };

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header)
            {
                Text = "Лицензии"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_0", Menu.MenuItem.Button)
            {
                Text = $"(A) Мотоциклы - {LicPrices[0]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_1", Menu.MenuItem.Button)
            {
                Text = $"(B) Легковые машины - {LicPrices[1]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_2", Menu.MenuItem.Button)
            {
                Text = $"(C) Грузовые машины - {LicPrices[2]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_3", Menu.MenuItem.Button)
            {
                Text = $"(V) Водный транспорт - {LicPrices[3]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_4", Menu.MenuItem.Button)
            {
                Text = $"(LV) Вертолёты - {LicPrices[4]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("lic_5", Menu.MenuItem.Button)
            {
                Text = $"(LS) Самолёты - {LicPrices[5]}$"
            };
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button)
            {
                Text = "Закрыть"
            };
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_driveschool(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(client);
            if (item.ID == "close") return;
            var id = item.ID.Split('_')[1];
            startDrivingCourse(client, Convert.ToInt32(id));
            return;
        }
        #endregion
    }
}