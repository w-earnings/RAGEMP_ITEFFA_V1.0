using System.Collections.Generic;
using System;
using GTANetworkAPI;
using iTeffa.Interface;
using iTeffa.Settings;

namespace iTeffa.Kernel
{
    class DrivingSchool : Script
    {
        private static int minCarHe = 700;
        private static List<int> LicPrices = new List<int>() { 600, 1000, 3000, 6000, 10000, 10000 };
        private static Vector3 enterSchool = new Vector3(229.6592, 378.3658, 104.9942); // 228.572708, 373.805969, 104.994225
        private static List<Vector3> drivingCoords = new List<Vector3>()
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
        private static nLog Log = new nLog("Driving School");
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1, 2, 0);
                shape.OnEntityEnterColShape += onPlayerEnterSchool;
                shape.OnEntityExitColShape += onPlayerExitSchool;

                NAPI.Marker.CreateMarker(1, enterSchool - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~g~Купить лицензию"), new Vector3(enterSchool.X, enterSchool.Y, enterSchool.Z + 1), 5f, 0.3f, 0, new Color(255, 255, 255));
                var blip = NAPI.Blip.CreateBlip(enterSchool, 0);
                blip.ShortRange = true;
                blip.Name = Main.StringToU16("Авто Школа");
                blip.Sprite = 545;
                blip.Color = 4;
                for (int i = 0; i < drivingCoords.Count; i++)
                {
                    var colshape = NAPI.ColShape.CreateCylinderColShape(drivingCoords[i], 4, 5, 0);
                    colshape.OnEntityEnterColShape += onPlayerEnterDrive;
                    colshape.SetData("NUMBER", i);
                }
                int ii = 0;
                foreach (var Check in Checkpoints)
                {
                    var col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER", ii);
                    col.OnEntityEnterColShape += onPlayerEnterSchoolTest;
                    col.OnEntityExitColShape += onPlayerExitSchoolTest;
                    ii++;
                };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        #region Покупка прав
        #region ColShape меню:
        private void onPlayerEnterSchool(ColShape shape, Player player)
        {
            try
            {
                Trigger.ClientEvent(player, "JobsEinfo");
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 39);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), nLog.Type.Error); }
        }
        private void onPlayerExitSchool(ColShape shape, Player player)
        {
            Trigger.ClientEvent(player, "JobsEinfo2");
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        #endregion
        #region Меню Покупаем права:
        public static void OpenDriveSchoolMenu(Player player)
        {
            Trigger.ClientEvent(player, "JobsEinfo2");
            Trigger.ClientEvent(player, "OpenDrivingSchool",
                LicPrices[0],
                LicPrices[1],
                LicPrices[2],
                LicPrices[3],
                LicPrices[4],
                LicPrices[5]);
        }
        #endregion
        #region После покупки это:
        [RemoteEvent("selectSchool_ID")]
        public static void startDrivingCourse(Player player, int index)
        {
            if (player.HasData("IS_DRIVING") || player.GetData<bool>("ON_WORK"))
            {
                Trigger.ClientEvent(player, "CloseDrivingSchool");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это сейчас", 3000);
                return;
            }
            if (Main.Players[player].Licenses[index])
            {
                Trigger.ClientEvent(player, "CloseDrivingSchool");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть эта лицензия", 3000);
                return;
            }
            switch (index)
            {
                case 0:
                    if (Main.Players[player].Money < LicPrices[0])
                    {
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[0] = true;
                    Finance.Wallet.Change(player, -LicPrices[0]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[0];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[0], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию на мото транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 1:
                    if (Main.Players[player].Money < LicPrices[1])
                    {
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Finance.Wallet.Change(player, -LicPrices[1]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[1];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[1], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Подойдите к любому свободному столу в нашей авто школе и попробуйте сдать тест", 3000);
                    player.SetData("TestSchool", 1);

                    return;
                case 2:
                    if (Main.Players[player].Money < LicPrices[2])
                    {
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[2] = true;
                    Finance.Wallet.Change(player, -LicPrices[2]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[2];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[2], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию на водный транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 3:
                    if (Main.Players[player].Money < LicPrices[3])
                    {
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[3] = true;
                    Finance.Wallet.Change(player, -LicPrices[3]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[3];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[3], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию на водный транспорт", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 4:
                    if (Main.Players[player].Money < LicPrices[4])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"", 3000);
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[4] = true;
                    Finance.Wallet.Change(player, -LicPrices[4]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[4];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[4], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление вертолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
                case 5:
                    if (Main.Players[player].Money < LicPrices[5])
                    {
                        Trigger.ClientEvent(player, "CloseDrivingSchool");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег, чтобы купить эту лицензию", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[5] = true;
                    Finance.Wallet.Change(player, -LicPrices[5]);
                    Fractions.Stocks.fracStocks[6].Money += LicPrices[5];
                    GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", LicPrices[5], $"buyLic");
                    Trigger.ClientEvent(player, "CloseDrivingSchool");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно купили лицензию управление самолётами", 3000);
                    Dashboard.sendStats(player);
                    return;
            }
        }
        #endregion
        #endregion
        #region Сдача теста
        #region Координаты Столов
        private static List<Checkpoint> Checkpoints = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(227.5337, 371.517, 104.9942), 140.0437), // Сдать тест 0
            new Checkpoint(new Vector3(226.3162, 371.8558, 104.9942), 200.1652), // Собрать тест 1
            new Checkpoint(new Vector3(225.1053, 372.3355, 104.9942), 200.1652), // Собрать тест 2
            new Checkpoint(new Vector3(224.4268, 370.4944, 104.9942), 200.1652), // Собрать тест 3
            new Checkpoint(new Vector3(225.6595, 370.05, 104.9942), 200.1652),   // Собрать тест 4
            new Checkpoint(new Vector3(226.8794, 369.6078, 104.9942), 200.1652), // Собрать тест 5
            new Checkpoint(new Vector3(227.9955, 369.1587, 104.9942), 200.1652), // Собрать тест 6
        };
        internal class Checkpoint
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Checkpoint(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
        #endregion
        #region Cрабатывает - сдать тест
        private void onPlayerEnterSchoolTest(ColShape shape, Player player)
        {
            try
            {
                Trigger.ClientEvent(player, "JobsEinfo");
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 511);
            }
            catch (Exception e) { Log.Write("onPlayerEnterSchool: " + e.ToString(), nLog.Type.Error); }
        }
        private void onPlayerExitSchoolTest(ColShape shape, Player player)
        {
            Trigger.ClientEvent(player, "JobsEinfo2");
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        #endregion
        #region Проверка что вы купили права
        public static void OpenTestSchoolMenu(Player player)
        {
            try
            {
                if (player.HasData("IS_DRIVING") || player.GetData<bool>("ON_WORK"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это сейчас", 3000);
                    return;
                }
                if (!Main.Players[player].Licenses[1])
                {
                    if (player.GetData<int>("TestSchool") == 1)
                    {
                        Trigger.ClientEvent(player, "JobsEinfo2");
                        Trigger.ClientEvent(player, "DrivingSchoolTEST");
                    }
                    else
                    {
                        Trigger.ClientEvent(player, "JobsEinfo2");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Купите тест", 3000);
                    }
                }
                else { Trigger.ClientEvent(player, "JobsEinfo2"); Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже сдавали тест", 3000); }
            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpointTest: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region  Проверка
        [RemoteEvent("SelectSchoolOK")]
        public static void SELECTDrivingSchoolTEST(Player player, int ok)
        {
            if (ok == 1)
            {
                player.SetData("IS_DRIVING", true);
                player.SetData("LICENSE", 1);
                player.SetData("CHECK", 0);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ступайте во двор займите свободный транспорт", 3000);
                Trigger.ClientEvent(player, "CloseDrivingSchoolTEST");
            }
        }
        #endregion
        #endregion
        #region Управления Транспортом
        #region Спавн машины и респавн машины
        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void SchoolCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "SCHOOL");
                NAPI.Data.SetEntityData(veh, "WORK", 100);
                NAPI.Data.SetEntityData(veh, "TYPE", "RENTCAR");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, false);
            }
        }
        public static void respawnCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");
                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                NAPI.Data.SetEntityData(veh, "ACCESS", "SCHOOL");
                NAPI.Data.SetEntityData(veh, "WORK", 100);
                NAPI.Data.SetEntityData(veh, "TYPE", "RENTCAR");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("respawnCar: " + e.Message, nLog.Type.Error); }
        }
        public static void respawnCar2(Player player)
        {
            try
            {
                var a = player.GetData<int>("NUMBERRE");
                var veh2 = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh2, "ACCESS", "SCHOOL");
                NAPI.Data.SetEntityData(veh2, "WORK", 100);
                NAPI.Data.SetEntityData(veh2, "TYPE", "RENTCAR");
                NAPI.Data.SetEntityData(veh2, "NUMBER", a);
                NAPI.Data.SetEntityData(veh2, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh2, "DRIVER", null);
                veh2.SetSharedData("PETROL", VehicleManager.VehicleTank[veh2.Class]);
                VehicleStreaming.SetEngineState(veh2, false);
                VehicleStreaming.SetLockStatus(veh2, false);
            }
            catch (Exception e) { Log.Write("respawnCar: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Садигся в машину сраатывает:
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "RENTCAR" || player.VehicleSeat != -1) return;
                if (!Main.Players[player].Licenses[1])
                {
                    if (player.HasData("IS_DRIVING") == true)
                    {
                        if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") == null) { rentCar(player); return; }
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть машина", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        }
                        if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине есть водитель", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        }
                        NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                    else
                    {
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вам нужно сдать тест.", 3000);
                    }
                }
                else
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас есть лицензия на (На этот транспорт)", 3000);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Если всё верно то после срабатывает:
        public static void rentCar(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != -1 || player.Vehicle.GetData<string>("TYPE") != "RENTCAR") return;

            var vehicle = player.Vehicle;
            NAPI.Data.SetEntityData(player, "WORK", vehicle);
            NAPI.Data.SetEntityData(player, "ON_WORK", true);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(player, "NUMBERRE", player.Vehicle.GetData<int>("NUMBER"));
            NAPI.Data.SetEntityData(vehicle, "ON_WORK", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);

            Trigger.ClientEvent(player, "createCheckpoint", 12, 1, drivingCoords[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
            Trigger.ClientEvent(player, "createWaypoint", drivingCoords[0].X, drivingCoords[0].Y);

            float bodyHealth = NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
            float engineHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle);

            Trigger.ClientEvent(player, "OpenStatsDrivingSchool", minCarHe, Convert.ToInt32(bodyHealth), Convert.ToInt32(engineHealth));
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести транспорт, нажмите B", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Максимальная скорость у машины состовляемт 60 КМ/Ч", 3000);
            Trigger.ClientEvent(player, "SetMaxSpeedSchool");
        }
        #endregion
        #region Дамаг по машине
        [ServerEvent(Event.VehicleDamage)]
        public void OnVehicleDamage(Vehicle vehicle, float bodyHealthLoss, float engineHealthLoss)
        {

            var player = NAPI.Data.GetEntityData(vehicle, "DRIVER");
            if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "RENTCAR" &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
            {
                var vehHP = player.Vehicle.Health; // макс
                float bodyHealth = NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
                float engineHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle);
                if (bodyHealth < minCarHe || engineHealth < minCarHe)
                {
                    player.ResetData("IS_DRIVING");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(player, "ON_WORK", false);
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                            respawnCar2(player);
                        }
                        catch { }
                    });
                    Trigger.ClientEvent(player, "CloseStatsDrivingSchool");
                    player.ResetData("SCHOOLVEH");

                    NAPI.Data.SetEntityData(player, "WORK", null);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы провалили экзамен", 3000);
                    return;
                }
                  if (Convert.ToInt32(bodyHealthLoss) != 0)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Урон по корпусу составил: {Convert.ToInt32(bodyHealthLoss)} едениц!", 3000);
                }
                if (Convert.ToInt32(engineHealthLoss) != 0)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Урон по двигателю составил: {Convert.ToInt32(engineHealthLoss)} едениц!", 3000);
                }
                Trigger.ClientEvent(player, "OpenStatsDrivingSchool", minCarHe, Convert.ToInt32(bodyHealth), Convert.ToInt32(engineHealth));
            }
        }
        #endregion
        #region Выход из машины
        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "RENTCAR" &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если вы не сядете в машину в течение 60 секунд, то провалите экзамен", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }
        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 30)
                    {
                        respawnCar(vehicle);

                        Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                        player.ResetData("IS_DRIVING");
                        player.ResetData("SCHOOLVEH");
                        Trigger.ClientEvent(player, "CloseStatsDrivingSchool");
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы провалили экзмен", 3000);

                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                }
                catch (Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle_Collector:\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }
        #endregion
        #endregion
        #region Удачно сели в машину идёт сдача города
        private void onPlayerEnterDrive(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != -1) return;
                if (!player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "SCHOOL") return;
                if (!player.HasData("IS_DRIVING")) return;
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
                            NAPI.Data.SetEntityData(player, "ON_WORK", false);
                            NAPI.Entity.DeleteEntity(player.Vehicle);
                            respawnCar2(player);
                        }
                        catch { }
                    });
                    Trigger.ClientEvent(player, "CloseStatsDrivingSchool");
                    player.ResetData("SCHOOLVEH");
                    if (vehHP < minCarHe)
                    {
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы провалили экзамен", 3000);
                        return;
                    }
                    Main.Players[player].Licenses[player.GetData<int>("LICENSE")] = true;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сдали экзамен", 3000);
                    NAPI.Data.SetEntityData(player, "WORK", null);
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
                Log.Write("ENTERDRIVE:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        #region Игрок
        #region onPlayerDisconnected Если игрок вышел из игры
        public static void onPlayerDisconnected(Player player, DisconnectionType type, string reaso)
        {
            try
            {
                if (player.GetData<bool>("ON_WORK"))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    var vehicle = player.GetData<Vehicle>("WORK");
                    respawnCar(vehicle);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Event_PlayerDeath Если игрок умер
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");
                    respawnCar(vehicle);

                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы провалили экзмен", 3000);
                    Trigger.ClientEvent(player, "CloseStatsDrivingSchool");

                    NAPI.Data.SetEntityData(player, "ON_WORK", false);
                    NAPI.Data.SetEntityData(player, "WORK", null);
                    Customization.ApplyCharacter(player);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #endregion
    }
}
