using GTANetworkAPI;
using System.Collections.Generic;
using System;
using iTeffa.Globals;
using iTeffa.Settings;

namespace iTeffa.Working
{
    class Construction : Script
    {
        private static readonly int checkpointPayment = 100;
        private static readonly int JobWorkId = 12;
        private static readonly int JobsMinLVL = 1;
        private static readonly Nlogs Log = new Nlogs("L");

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(566, new Vector3(145.0445, -373.0724, 42.54742), 1, 46, Main.StringToU16("Строитель"), 255, 0, true, 0, 0);
                NAPI.TextLabel.CreateTextLabel("~w~Приму вас на работу", new Vector3(144.8581, -373.5612, 44.54737), 30f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                var col = NAPI.ColShape.CreateCylinderColShape(new Vector3(145.0445, -373.0724, 42.54742), 4, 2, 0);

                col.OnEntityEnterColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 509);
                        Trigger.ClientEvent(player, "JobsEinfo");
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
                };
                col.OnEntityExitColShape += (shape, player) => {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                        Trigger.ClientEvent(player, "JobsEinfo2");
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, Nlogs.Type.Error); }
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
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 4, 2, 0);
                    col.SetData("NUMBER3", ii);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    ii++;
                };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }

        #region Чекпоинты
        private static readonly List<Checkpoint> Checkpoints = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(22.8898, -402.6957, 44.43897), 19.49689), // Тоскает коробку
            new Checkpoint(new Vector3(28.69614, -452.2071, 44.43775), 243.5095), // Тоскает мусор цемент
            new Checkpoint(new Vector3(33.00998, -444.1138, 44.43775), 207.824), // Тоскает кирпич
            new Checkpoint(new Vector3(33.96332, -450.0271, 44.43776), 215.6413), // Тоскает мусор
            new Checkpoint(new Vector3(39.33658, -430.9125, 44.43895), 72.72858), // Электрик
            new Checkpoint(new Vector3(38.71178, -401.6262, 44.43897), 68.8477), // Стучит молотком
            new Checkpoint(new Vector3(38.71178, -401.6262, 44.43897), 341.7192), // Стучит молотком
        };
        private static readonly List<Checkpoint> Checkpoints2 = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(54.49169, -377.0713, 44.43923), 152.6219), // Тоскает коробку Конец
            new Checkpoint(new Vector3(60.10947, -398.5452, 38.80103), 65.90971), // Тоскает мусор Конец
            new Checkpoint(new Vector3(46.74149, -391.0762, 44.43874), 253.3996), // Тоскает кирпич Конец
            new Checkpoint(new Vector3(60.10947, -398.5452, 38.80103), 65.90971), // Тоскает мусор Конец
        };
        #endregion

        #region Меню которое нажимается на E
        public static void StartWorkDayConstruction(Player player)
        {
            if (Main.Players[player].LVL < JobsMinLVL)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Необходим как минимум {JobsMinLVL} уровень", 3000);
                return;
            }

            Trigger.ClientEvent(player, "JobsEinfo2");
            Trigger.ClientEvent(player, "OpenConstruction", checkpointPayment, Main.Players[player].LVL, Main.Players[player].WorkID, NAPI.Data.GetEntityData(player, "ON_WORK2"));

        }
        #endregion
        #region Устроться на работу
        [RemoteEvent("jobJoinConstruction")]
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
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, Nlogs.Type.Error); }
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
                //Dashboard.sendStats(player);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы уволились с работы", 3000);
                var jobsid = Main.Players[player].WorkID;
                Trigger.ClientEvent(player, "secusejobConstruction", jobsid);
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
            //Dashboard.sendStats(player);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы устроились на работу", 3000);
            var jobsid = Main.Players[player].WorkID;
            Trigger.ClientEvent(player, "secusejobConstruction", jobsid);
            return;
        }
        #endregion
        #region Начать рабочий день
        [RemoteEvent("enterJobConstruction")]
        public static void ClientEvent_Construction(Player client, int act)
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
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, Nlogs.Type.Error); }
        }
        public static void Layoff2(Player player)
        {
            if (player.GetData<int>("PACKAGES") == 1)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Сдайте коробку прежде чем закончить рабочий день", 3000);
                return;
            }
            if (NAPI.Data.GetEntityData(player, "ON_WORK") != false)
            {
                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK", false);
                player.SetData("ON_WORK2", 0);
                Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                Trigger.ClientEvent(player, "deleteWorkBlip");
                player.SetData("PACKAGES", 0);
                Finance.Wallet.Change(player, player.GetData<int>("PAYMENT"));

                Trigger.ClientEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"+ {player.GetData<int>("PAYMENT")}$", 3000);
                player.SetData("PAYMENT", 0);
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

            Customization.ClearClothes(player, Main.Players[player].Gender);
            if (Main.Players[player].Gender)
            {
                player.SetAccessories(0, 145, 0);
                player.SetClothes(3, 30, 0);
                player.SetClothes(8, 59, 0);
                player.SetClothes(11, 1, 0);
                player.SetClothes(4, 0, 5);
                player.SetClothes(6, 48, 0);
            }
            else
            {
                player.SetAccessories(0, 144, 0);
                player.SetClothes(3, 0, 0);
                player.SetClothes(8, 36, 0);
                player.SetClothes(11, 0, 0);
                player.SetClothes(4, 1, 5);
                player.SetClothes(6, 49, 0);
            }

            var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
            player.SetData("WORKCHECK", check);
            Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
            Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);
            player.SetData("PACKAGES", 0);

            player.SetData("ON_WORK", true);
            player.SetData("ON_WORK2", job);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы начали рабочий день", 3000);
            Trigger.ClientEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

        }
        #endregion
        #region Когда заходишь в чекпоинт
        private static void PlayerEnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != JobWorkId || !player.GetData<bool>("ON_WORK")) return;

                if (player.GetData<int>("PACKAGES") == 0)
                {
                    if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK"))
                    {
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") + 1);
                        player.SetData("WORKCHECK", -1);

                        NAPI.Entity.SetEntityPosition(player, Checkpoints[shape.GetData<int>("NUMBER2")].Position + new Vector3(0, 0, 1.2));
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checkpoints[shape.GetData<int>("NUMBER2")].Heading));
                        if (shape.GetData<int>("NUMBER2") < 4)
                        {
                            #region Таскает коробку 1
                            if (shape.GetData<int>("NUMBER2") == 0)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 47);

                                var check = shape.GetData<int>("NUMBER2");
                                player.SetData("WORKCHECK", check);
                                Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints2[check].Position, 5, 0, 255, 0, 0);
                                Trigger.ClientEvent(player, "createWorkBlip", Checkpoints2[check].Position);

                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
                                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("ng_proc_box_01a"), 18905, new Vector3(0.1, 0.1, 0.3), new Vector3(-10, -75, -40));
                                        }
                                    }
                                    catch { }
                                }, 250);
                            }
                            #endregion
                            #region Таскает мусор цемент
                            if (shape.GetData<int>("NUMBER2") == 1)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@prop_human_bum_bin@idle_a", "idle_a", 47);

                                var check = shape.GetData<int>("NUMBER2");
                                player.SetData("WORKCHECK", check);
                                Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints2[check].Position, 5, 0, 255, 0, 0);
                                Trigger.ClientEvent(player, "createWorkBlip", Checkpoints2[check].Position);

                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            player.PlayAnimation("amb@world_human_janitor@male@base", "base", 49);
                                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_rub_binbag_01"), 57005, new Vector3(0.1, 0.0, 0.0), new Vector3(-90, 0, 0));
                                        }
                                    }
                                    catch { }
                                }, 250);
                            }
                            #endregion
                            #region Таскает кирпичи 2
                            if (shape.GetData<int>("NUMBER2") == 2)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 47);

                                var check = shape.GetData<int>("NUMBER2");
                                player.SetData("WORKCHECK", check);
                                Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints2[check].Position, 5, 0, 255, 0, 0);
                                Trigger.ClientEvent(player, "createWorkBlip", Checkpoints2[check].Position);

                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            player.StopAnimation();
                                            Main.OffAntiAnim(player);
                                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("ng_proc_brick_01a"), 57005, new Vector3(0.1, 0.0, 0.0), new Vector3(-90, 0, 0));
                                        }
                                    }
                                    catch { }
                                }, 250);
                            }
                            #endregion
                            #region Таскает мусор
                            if (shape.GetData<int>("NUMBER2") == 3)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@prop_human_bum_bin@idle_a", "idle_a", 47);

                                var check = shape.GetData<int>("NUMBER2");
                                player.SetData("WORKCHECK", check);
                                Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints2[check].Position, 5, 0, 255, 0, 0);
                                Trigger.ClientEvent(player, "createWorkBlip", Checkpoints2[check].Position);

                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            player.PlayAnimation("amb@world_human_janitor@male@base", "base", 49);
                                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_rub_binbag_01"), 57005, new Vector3(0.1, 0.0, 0.0), new Vector3(-90, 0, 0));
                                        }
                                    }
                                    catch { }
                                }, 4000);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Электрик
                            if (shape.GetData<int>("NUMBER2") == 4)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 39);
                                Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                                Trigger.ClientEvent(player, "deleteWorkBlip");

                                NAPI.Task.Run(() => {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment);
                                            Trigger.ClientEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);

                                            player.StopAnimation();
                                            Main.OffAntiAnim(player);
                                            var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                                            player.SetData("WORKCHECK", check);
                                            Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
                                            Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);
                                        }
                                    }
                                    catch { }
                                }, 4000);
                            }
                            #endregion
                            #region Стучит молотком 1
                            if (shape.GetData<int>("NUMBER2") == 5)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@world_human_hammering@male@base", "base", 39);
                                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_tool_hammer"), 57005, new Vector3(0.1, 0.0, 0.0), new Vector3(-90, 0, 0));
                                Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                                Trigger.ClientEvent(player, "deleteWorkBlip");

                                NAPI.Task.Run(() => {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment);
                                            Trigger.ClientEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);

                                            BasicSync.DetachObject(player);
                                            player.StopAnimation();
                                            Main.OffAntiAnim(player);
                                            var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                                            player.SetData("WORKCHECK", check);
                                            Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
                                            Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);
                                        }
                                    }
                                    catch { }
                                }, 4000);
                            }
                            #endregion
                            #region Стучит молотком 2
                            if (shape.GetData<int>("NUMBER2") == 6)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@world_human_hammering@male@base", "base", 39);
                                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_tool_hammer"), 57005, new Vector3(0.1, 0.0, 0.0), new Vector3(-90, 0, 0));
                                Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                                Trigger.ClientEvent(player, "deleteWorkBlip");

                                NAPI.Task.Run(() => {
                                    try
                                    {
                                        if (player != null && Main.Players.ContainsKey(player))
                                        {
                                            var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment);
                                            Trigger.ClientEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                                            player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);

                                            BasicSync.DetachObject(player);
                                            player.StopAnimation();
                                            Main.OffAntiAnim(player);
                                            var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                                            player.SetData("WORKCHECK", check);
                                            Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
                                            Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);
                                        }
                                    }
                                    catch { }
                                }, 4000);
                            }
                            #endregion

                        }

                    }
                }
                else
                {
                    if (shape.GetData<int>("NUMBER3") == player.GetData<int>("WORKCHECK"))
                    {
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);
                        player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 47);

                        player.SetData("WORKCHECK", -1);
                        var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                        player.SetData("WORKCHECK", check);
                        Trigger.ClientEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
                        Trigger.ClientEvent(player, "createWorkBlip", Checkpoints[check].Position);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                                    player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment);
                                    Trigger.ClientEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                                    BasicSync.DetachObject(player);
                                    player.StopAnimation();
                                    Main.OffAntiAnim(player);
                                }
                            }
                            catch { }
                        }, 400);
                    }
                }

            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, Nlogs.Type.Error); }
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
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetData("ON_WORK2", 0);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                    Trigger.ClientEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                    Trigger.ClientEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"+ {player.GetData<int>("PAYMENT")}$", 3000);
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Nlogs.Type.Error); }
        }
        #endregion
        #region Если игрок вышел из игры или его кикнуло
        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetData("ON_WORK2", 0);
                    Trigger.ClientEvent(player, "deleteCheckpoint", 15);
                    Trigger.ClientEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, Nlogs.Type.Error); }
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
