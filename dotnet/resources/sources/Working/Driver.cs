using System;
using System.Collections.Generic;
using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Settings;

namespace iTeffa.Working
{
    class Diver : Script
    {
        private static readonly int checkpointPayment = 100;
        private static readonly int JobWorkId = 13;
        private static readonly int JobsMinLVL = 3;
        private static readonly int ColObjects = 5;
        private static readonly Plugins.Logs Log = new Plugins.Logs("Diver");

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(new Vector3(1695.163, 42.85501, 160.6473), 1, 2, 0);

                col.OnEntityEnterColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 510);
                        Plugins.Trigger.ClientEvent(player, "JobsEinfo");
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                };
                col.OnEntityExitColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                        Plugins.Trigger.ClientEvent(player, "JobsEinfo2");
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, Plugins.Logs.Type.Error); }
                };

                int i = 0;
                foreach (var Check in Checkpoints)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER2", i);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    i++;
                };

                int ii = 0;
                foreach (var Check in Checkpoints2)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 2, 2, 0);
                    col.SetData("NUMBER3", ii);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    ii++;
                };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }

        private static readonly List<Checkpoint> Checkpoints = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(1762.287, -19.40464, 154.4776), 206.6532),
            new Checkpoint(new Vector3(1857.945, 1.099715, 152.0033), 206.6532),
            new Checkpoint(new Vector3(1876.625, 104.593, 149.4533), 206.6532),
            new Checkpoint(new Vector3(1958.733, 112.7229, 152.9555), 206.6532),
            new Checkpoint(new Vector3(1971.705, 190.3279, 148.1627), 206.6532),
        };

        private static readonly List<Checkpoint> Checkpoints2 = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(1695.163, 42.85501, 160.6473), 99.49088),
        };


        private static readonly List<string> Objects = new List<string>(){"apa_mp_h_acc_bottle_01", "bkr_prop_clubhouse_laptop_01b", "bkr_prop_coke_boxeddoll", "prop_roadcone02b", "prop_mr_rasberryclean"};


        #region Меню которое нажимается на E
        public static void StartWorkDayDiver(Player player)
        {
            if (Main.Players[player].LVL < JobsMinLVL)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Необходим как минимум {JobsMinLVL} уровень", 3000);
                return;
            }

            Plugins.Trigger.ClientEvent(player, "JobsEinfo2");
            Plugins.Trigger.ClientEvent(player, "OpenDiver", checkpointPayment, Main.Players[player].LVL, Main.Players[player].WorkID, NAPI.Data.GetEntityData(player, "ON_WORK2"));

        }
        #endregion

        #region Устроться на работу
        [RemoteEvent("jobJoinDiver")]
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
                        JobJoin(client);
                        return;
                }
            }
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void Layoff(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 0)
            {
                Main.Players[player].WorkID = 0;
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы уволились с работы", 3000);
                var jobsid = Main.Players[player].WorkID;
                Plugins.Trigger.ClientEvent(player, "secusejobDiver", jobsid);
                return;
            }
            else
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы никем не работаете", 3000);
        }
        public static void JobJoin(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 0)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже работаете: {WorkManager.JobStats[Main.Players[player].WorkID - 1]}", 3000);
                return;
            }
            Main.Players[player].WorkID = JobWorkId;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы устроились на работу", 3000);
            var jobsid = Main.Players[player].WorkID;
            Plugins.Trigger.ClientEvent(player, "secusejobDiver", jobsid);
            return;
        }
        #endregion
        #region Рандом Штаны и куртка Для мужиков
        private static readonly List<string> SetClothes4 = new List<string>()
        {
            "0",
            "1",
            "2",
            "3",
        };
        private static readonly List<string> SetClothes11 = new List<string>()
        {
            "0",
            "1",
            "2",
            "3",
        };
        #endregion
        #region Рандом Куртка и куртка Для девушек
        private static readonly List<string> SetClothes4_2 = new List<string>()
        {
            "0",
            "1",
            "2",
            "3",
        };
        private static readonly List<string> SetClothes11_2 = new List<string>()
        {
            "0",
            "1",
            "2",
            "3",
        };
        #endregion
        #region Начать рабочий день
        [RemoteEvent("enterJobDiver")]
        public static void ClientEvent_Diver(Player client, int act)
        {
            try
            {
                switch (act)
                {
                    case -1:
                        Layoff2(client);
                        return;
                    default:
                        JobJoin2(client, act);
                        return;
                }
            }
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static void Layoff2(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") != false)
            {
                player.SetData("WORKCHECK_0", 0);
                player.SetData("WORKCHECK_1", 0);
                player.SetData("WORKCHECK_2", 0);
                player.SetData("WORKCHECK_3", 0);
                player.SetData("WORKCHECK_4", 0);
                Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 0);
                Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 1);
                Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 2);
                Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 3);
                Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 4);
                Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 0);
                Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 1);
                Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 2);
                Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 3);
                Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 4);

                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK", false);
                player.SetData("ON_WORK2", 0);
                Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                player.SetData("PACKAGES", 0);

                Finance.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                Plugins.Trigger.ClientEvent(player, "CloseJobStatsInfoDiver");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"+ {player.GetData<int>("PAYMENT")}$", 3000);
                player.SetData("PAYMENT", 0);
                Plugins.Trigger.ClientEvent(player, "stopdiving");
            }
            else
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы уже не работаете", 3000);
            }
        }
        public static void JobJoin2(Player player, int job)
        {
            if (Main.Players[player].WorkID != JobWorkId)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не работаете на этой работе.", 3000);
                return;
            }
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            // Одежда
            var RandomClothes4 = WorkManager.rnd.Next(0, SetClothes4.Count);
            var RandomClothes11 = WorkManager.rnd.Next(0, SetClothes11.Count);
            var RandomClothes4_2 = WorkManager.rnd.Next(0, SetClothes4_2.Count);
            var RandomClothes11_2 = WorkManager.rnd.Next(0, SetClothes11_2.Count);
            Customization.ClearClothes(player, Main.Players[player].Gender);
            if (Main.Players[player].Gender)
            {
                player.SetClothes(8, 151, 0); 
                player.SetClothes(3, 17, 0);
                player.SetClothes(6, 67, 0);
                player.SetClothes(11, 178, RandomClothes11);
                player.SetClothes(4, 77, RandomClothes4);
            }
            else
            {
                player.SetClothes(8, 187, 0);
                player.SetClothes(3, 18, 0);
                player.SetClothes(6, 70, 0);
                player.SetClothes(11, 180, RandomClothes11_2);
                player.SetClothes(4, 79, RandomClothes11_2);
            }
            // Чекпоинты
            player.SetData("WORKCHECK_0", 0);
            player.SetData("WORKCHECK_1", 1);
            player.SetData("WORKCHECK_2", 2);
            player.SetData("WORKCHECK_3", 3);
            player.SetData("WORKCHECK_4", 4);
            Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 0, 66, Checkpoints[0].Position, "Мусор", 0);
            Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 1, 66, Checkpoints[1].Position, "Мусор", 0);
            Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 2, 66, Checkpoints[2].Position, "Мусор", 0);
            Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 3, 66, Checkpoints[3].Position, "Мусор", 0);
            Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 4, 66, Checkpoints[4].Position, "Мусор", 0);
            // Объекты
            Plugins.Trigger.ClientEvent(player, "createObjectJobs", 0, Objects[0], Checkpoints[0].Position.X, Checkpoints[0].Position.Y, Checkpoints[0].Position.Z);
            Plugins.Trigger.ClientEvent(player, "createObjectJobs", 1, Objects[1], Checkpoints[1].Position.X, Checkpoints[1].Position.Y, Checkpoints[1].Position.Z);
            Plugins.Trigger.ClientEvent(player, "createObjectJobs", 2, Objects[2], Checkpoints[2].Position.X, Checkpoints[2].Position.Y, Checkpoints[2].Position.Z);
            Plugins.Trigger.ClientEvent(player, "createObjectJobs", 3, Objects[3], Checkpoints[3].Position.X, Checkpoints[3].Position.Y, Checkpoints[3].Position.Z);
            Plugins.Trigger.ClientEvent(player, "createObjectJobs", 4, Objects[4], Checkpoints[4].Position.X, Checkpoints[4].Position.Y, Checkpoints[4].Position.Z);

            player.SetData("PACKAGES", ColObjects);
            player.SetData("OBJECTSJOB", 0);
            player.SetData("ON_WORK", true);
            player.SetData("ON_WORK2", job);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы начали рабочий день! На карте отмечены места с мусором. Соберите этот мусор.", 3000);
            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
            Plugins.Trigger.ClientEvent(player, "startdiving");
        }
        #endregion
        #region Когда заходишь в чекпоинт
        private static void PlayerEnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != JobWorkId || !player.GetData<bool>("ON_WORK")) return;

                if (player.GetData<int>("PACKAGES") != 0)
                {
                    if (player.GetData<int>("PACKAGES") != 1)
                    #region Если мусора больше чем 1
                    {
                        #region Нулевой чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_0"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_0")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_0")); // Удаляем блип
                            player.SetData("WORKCHECK_0", 10);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Первый чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_1"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_1")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_1")); // Удаляем блип
                            player.SetData("WORKCHECK_1", 11);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Второй чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_2"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_2")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_2")); // Удаляем блип
                            player.SetData("WORKCHECK_2", 12);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Третий чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_3"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_3")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_3")); // Удаляем блип
                            player.SetData("WORKCHECK_3", 13);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Четвёртый чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_4"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_4")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_4")); // Удаляем блип
                            player.SetData("WORKCHECK_4", 14);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                    }
                    #endregion
                    else
                    #region Если у вас 1 мусор срабатывает -1 и идём к 0
                    {
                        #region Нулевой чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_0"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_0")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_0")); // Удаляем блип
                            player.SetData("WORKCHECK_0", 10);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Первый чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_1"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_1")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_1")); // Удаляем блип
                            player.SetData("WORKCHECK_1", 11);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Второй чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_2"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_2")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_2")); // Удаляем блип
                            player.SetData("WORKCHECK_2", 12);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Третий чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_3"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_3")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_3")); // Удаляем блип
                            player.SetData("WORKCHECK_3", 13);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion
                        #region Четвёртый чекпоинт
                        if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK_4"))
                        {
                            Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", player.GetData<int>("WORKCHECK_4")); // Удаляем объект
                            Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", player.GetData<int>("WORKCHECK_4")); // Удаляем блип
                            player.SetData("WORKCHECK_4", 14);
                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1); // Минусуем Пакет
                            player.SetData("OBJECTSJOB", player.GetData<int>("OBJECTSJOB") + 1); // Прибавляем объект
                            Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);
                        }
                        #endregion

                        if (player.GetData<int>("PACKAGES") == 0)
                        {
                            player.SetData("WORKCHECK", 0);
                            Plugins.Trigger.ClientEvent(player, "createCheckpoint", 9, 1, Checkpoints2[0].Position, 5, 0, 255, 0, 0);
                            Plugins.Trigger.ClientEvent(player, "createWorkBlip", Checkpoints2[0].Position);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Если у вас 0 мусора
                    if (shape.GetData<int>("NUMBER3") == player.GetData<int>("WORKCHECK"))
                    {


                        Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 9);
                        Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                        player.SetData("PACKAGES", ColObjects);
                        player.SetData("OBJECTSJOB", 0);
                        var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                        player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment);
                        Plugins.Trigger.ClientEvent(player, "JobStatsInfoDiver", player.GetData<int>("PAYMENT"), player.GetData<int>("OBJECTSJOB"), ColObjects);

                        // Чекпоинты
                        player.SetData("WORKCHECK_0", 0);
                        player.SetData("WORKCHECK_1", 1);
                        player.SetData("WORKCHECK_2", 2);
                        player.SetData("WORKCHECK_3", 3);
                        player.SetData("WORKCHECK_4", 4);
                        Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 0, 66, Checkpoints[0].Position, "Мусор", 0);
                        Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 1, 66, Checkpoints[1].Position, "Мусор", 0);
                        Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 2, 66, Checkpoints[2].Position, "Мусор", 0);
                        Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 3, 66, Checkpoints[3].Position, "Мусор", 0);
                        Plugins.Trigger.ClientEvent(player, "JobMenusBlip", 4, 66, Checkpoints[4].Position, "Мусор", 0);
                        // Объекты
                        Plugins.Trigger.ClientEvent(player, "createObjectJobs", 0, Objects[0], Checkpoints[0].Position.X, Checkpoints[0].Position.Y, Checkpoints[0].Position.Z);
                        Plugins.Trigger.ClientEvent(player, "createObjectJobs", 1, Objects[1], Checkpoints[1].Position.X, Checkpoints[1].Position.Y, Checkpoints[1].Position.Z);
                        Plugins.Trigger.ClientEvent(player, "createObjectJobs", 2, Objects[2], Checkpoints[2].Position.X, Checkpoints[2].Position.Y, Checkpoints[2].Position.Z);
                        Plugins.Trigger.ClientEvent(player, "createObjectJobs", 3, Objects[3], Checkpoints[3].Position.X, Checkpoints[3].Position.Y, Checkpoints[3].Position.Z);
                        Plugins.Trigger.ClientEvent(player, "createObjectJobs", 4, Objects[4], Checkpoints[4].Position.X, Checkpoints[4].Position.Y, Checkpoints[4].Position.Z);
                    }
                    #endregion
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, Plugins.Logs.Type.Error); }
        }
        #endregion
        #region Если игрок умер
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {
                    player.SetData("WORKCHECK_0", 0);
                    player.SetData("WORKCHECK_1", 0);
                    player.SetData("WORKCHECK_2", 0);
                    player.SetData("WORKCHECK_3", 0);
                    player.SetData("WORKCHECK_4", 0);
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 0); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 1); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 2); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 3); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 4); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 0); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 1); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 2); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 3); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 4); // Удаляем объект

                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetData("ON_WORK2", 0);
                    Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                    Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                    Finance.Wallet.Change(player, player.GetData<int>("PAYMENT"));

                    Plugins.Trigger.ClientEvent(player, "CloseJobStatsInfoDiver");
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"+ {player.GetData<int>("PAYMENT")}$", 3000);
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Plugins.Logs.Type.Error); }
        }
        #endregion
        #region Если игрок вышел из игры или его кикнуло
        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {
                    player.SetData("WORKCHECK_0", 0);
                    player.SetData("WORKCHECK_1", 0);
                    player.SetData("WORKCHECK_2", 0);
                    player.SetData("WORKCHECK_3", 0);
                    player.SetData("WORKCHECK_4", 0);
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 0); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 1); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 2); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 3); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteJobMenusBlip", 4); // Удаляем блип
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 0); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 1); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 2); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 3); // Удаляем объект
                    Plugins.Trigger.ClientEvent(player, "deleteObjectJobs", 4); // Удаляем объект

                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetData("ON_WORK2", 0);
                    Plugins.Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                    Plugins.Trigger.ClientEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Plugins.Logs.Type.Error); }
        }
        #endregion
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
    }
}
