using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Infodata;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;

namespace iTeffa.Working.FarmerJob
{
    public class Farmer : Script
    {
        private static readonly Nlogs Log = new Nlogs("Working Farmer");
        private static readonly List<CharacterData> Farmers = new List<CharacterData>();
        private static ColShape checkpoint;
        private static readonly Random rnd = new Random();
        private static readonly int minsec = 40;
        private static readonly int maxsec = 100;
        private static readonly int maxlvl = 25;
        [ServerEvent(Event.ResourceStart)]
        public void Event_FarmerStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(677, new Vector3(438.3, 6510.9, 22.4), 1, 24, "Ферма", 255, 0, true, 0, 0);
                NAPI.TextLabel.CreateTextLabel("~g~Фермер Рикардо", new Vector3(438.3554, 6510.949, 29.6), 10f, 0.1f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                List<Vector3> shapes = new List<Vector3>()
                {
                    new Vector3(438.35, 6510.94, 28),
                };

                var iTeffaShape = NAPI.ColShape.CreateCylinderColShape(shapes[0], 2.1f, 3, 0);
                iTeffaShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 520);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), Nlogs.Type.Error);
                    }
                };
                iTeffaShape.OnEntityExitColShape += (shape, player) =>
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
                for (int i = 0; i < Checkpoints.Count; i++)
                {
                    checkpoint = NAPI.ColShape.CreateCylinderColShape(Checkpoints[i], 1, 2, 0);
                    checkpoint.SetData($"plantID", i);
                    checkpoint.OnEntityEnterColShape += PlayerEnterCheckpoint;
                }
                Log.Write("Loaded", Nlogs.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        public static void OpenFarmerMenu(Player player)
        {
            try
            {
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);
                int itemcount = item != null ? item.Count : 0;
                LoadLvl(player, "farmer");
                int[] jobinfo = player.GetData<int[]>("job_farmer");
                List<object> data = new List<object>()
                {
                    jobinfo[0],
                    jobinfo[1],
                    jobinfo[2],
                    itemcount,
                    Farmers.Count,
                    player.GetData<bool>("ON_WORK"),
                    maxlvl
                };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Trigger.ClientEvent(player, "openJobsMenu", json);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        [RemoteEvent("workstate")]
        public static void StartWork(Player player, bool state, string workname = null)
        {
            if (state)
            {
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);
                if (item == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет семян", 2000);
                    return;
                }
                for (int i = 0; i < Checkpoints.Count; i++)
                {
                    Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{i}"), "Плантация", 1, Checkpoints[i], 1, 0, 255, 0, 0);
                    player.SetData($"seedplant{i}", false);
                    player.ResetData($"regenplant{i}");
                }
                Farmers.Add(Main.Players[player]);
                SetFarmerClothes(player);
                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_trowel"), 57005, new Vector3(0.1, 0, 0), new Vector3(90, 50, -30));
                player.SetData("jobname", "farmer");
                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы устроились работать фермером", 2000);
            }
            else
            {
                try
                {
                    Farmers.Remove(Main.Players[player]);
                    for (int i = 0; i < Checkpoints.Count; i++)
                    {
                        Trigger.ClientEvent(player, "deletePlant", Convert.ToInt32($"10{i}"));
                        Timers.Stop($"{player.Name}farmer{i}");
                        player.SetData($"seedplant{i}", false);
                        player.ResetData($"regenplant{i}");
                    }
                    SaveLvl(player, "farmer");
                    Customization.ApplyCharacter(player);
                    BasicSync.DetachObject(player);
                    player.ResetData("job_farmer");
                    player.ResetData("jobname");
                    player.SetData("ON_WORK", false);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы уволились с работы фермера", 2000);
                }
                catch (Exception e)
                {
                    Log.Write(e.ToString(), Nlogs.Type.Error);
                }
            }
        }
        private static void PlayerEnterCheckpoint(ColShape colShape, Player player)
        {
            try
            {
                var colID = colShape.GetData<int>("plantID");
                if (player.IsInVehicle) return;
                if (player.GetData<string>("jobname") != "farmer") return;
                int[] jobinfo = player.GetData<int[]>("job_farmer");
                int lvl = jobinfo[0], exp = jobinfo[1], allpoints = jobinfo[2], sec = Convert.ToInt32(rnd.Next(minsec, maxsec) - lvl * 2);
                if (player.HasData($"regenplant{colID}"))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.TopCenter, $"{player.GetData<bool>($"regenplant{colID}")}", 2000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);

                if (!player.GetData<bool>($"seedplant{colID}"))
                {
                    if (item == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет семян", 2000);
                        return;
                    }

                    player.SetData($"seedplant{colID}", true);
                    nInventory.Remove(player, item.Type, 1);
                    NAPI.Task.Run(() => { Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Семена посажены", 2000); }, 5000);
                }
                else if (player.GetData<bool>($"seedplant{colID}"))
                {
                    player.ResetData("job_farmer");

                    if (exp == 100 && lvl < maxlvl)
                    {
                        player.SetData("job_farmer", new int[] { ++lvl, ++exp - 100, ++allpoints });
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Поздравляем с новым уровнем фермера: {lvl}", 2000);
                    }
                    else
                    {
                        if (lvl == maxlvl) exp = -1;
                        player.SetData("job_farmer", new int[] { lvl, ++exp, ++allpoints });
                    }

                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Hay));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 2000);
                        return;
                    }
                    player.SetData($"seedplant{colID}", false);
                    NAPI.Task.Run(() =>
                    {
                        nInventory.Add(player, new nItem(ItemType.Hay, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Урожай собран", 2000);
                    }, 5000);
                }
                Trigger.ClientEvent(player, "deletePlant", Convert.ToInt32($"10{colID}"));
                PlayFarmerAnimation(player);
                NAPI.Task.Run(() => { player.SetData($"regenplant{colID}", sec); UpdateCheckpointState(colShape, player); }, 5000);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        private static void UpdateCheckpointState(ColShape colShape, Player player)
        {
            var colID = colShape.GetData<int>("plantID");
            if (player.HasData($"regenplant{colID}"))
            {
                Timers.Start($"{player.Name}farmer{colID}", 5000, () =>
                {
                    if (player.HasData($"regenplant{colID}")) player.SetData($"regenplant{colID}", player.GetData<int>($"regenplant{colID}") - 5);
                    if (player.GetData<int>($"regenplant{colID}") < 1)
                    {
                        if (!player.GetData<bool>($"seedplant{colID}"))
                        {
                            player.ResetData($"regenplant{colID}");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Плантация готова для посадки", 2000);
                            Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{colID}"), "Плантация", 1, Checkpoints[colID], 1, 0, 255, 0, 0);
                            Timers.Stop($"{player.Name}farmer{colID}");
                        }
                        else
                        {
                            player.ResetData($"regenplant{colID}");
                            player.SetData($"seedplant{colID}", true);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Соберите урожай", 2000);
                            Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{colID}"), "Плантация", 1, Checkpoints[colID], 1, 0, 0, 255, 0);
                            Timers.Stop($"{player.Name}farmer{colID}");
                        }
                    }
                });
            }
        }
        private static void PlayFarmerAnimation(Player player)
        {
            Main.OnAntiAnim(player);
            player.PlayAnimation("amb@world_human_gardener_plant@male@enter", "enter", 39);
            NAPI.Task.Run(() =>
            {
                player.PlayAnimation("amb@world_human_gardener_plant@male@base", "base", 39);

                NAPI.Task.Run(() =>
                {
                    player.PlayAnimation("amb@world_human_gardener_plant@male@exit", "exit", 39);

                    NAPI.Task.Run(() =>
                    {
                        player.StopAnimation();
                        Main.OffAntiAnim(player);

                    }, 6000);

                }, 3000);

            }, 3000);
        }
        public static void LoadLvl(Player player, string workname)
        {
            try
            {
                if (player.HasData($"job_{workname}")) return;
                int lvl = 1, exp = 0, allpoints = 0;
                CharacterData acc = Main.Players[player];
                DataTable result = Database.QueryRead($"SELECT * FROM `{workname}` WHERE uuid={acc.UUID}");
                if (result == null || result.Rows.Count == 0)
                {
                    Database.Query($"INSERT INTO `{workname}`(`uuid`, `level`, `exp`, `allpoints`) VALUES({acc.UUID}, {lvl}, {exp}, {allpoints})");
                    Log.Write($"Я зарегал игрока {player.Name}", Nlogs.Type.Warn);
                }
                else
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        lvl = Convert.ToInt32(Row["level"]);
                        exp = Convert.ToInt32(Row["exp"]);
                        allpoints = Convert.ToInt32(Row["allpoints"]);
                    }
                    Log.Write($"Я загрузил игрока {player.Name}", Nlogs.Type.Warn);
                }
                player.SetData($"job_{workname}", new int[] { lvl, exp, allpoints });
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        public static void SaveLvl(Player player, string workname)
        {
            try
            {
                int[] data = player.GetData<int[]>($"job_{workname}");
                if (data == null) return;
                CharacterData acc = Main.Players[player];
                DataTable result = Database.QueryRead($"SELECT * FROM `{workname}` WHERE uuid={acc.UUID}");
                if (result == null || result.Rows.Count == 0)
                {
                    Database.Query($"INSERT INTO `{workname}`(`uuid`, `level`, `exp`, `allpoints`) VALUES({acc.UUID}, {data[0]}, {data[1]}, {data[2]})");
                    Log.Write("Пользователь внесен в базу", Nlogs.Type.Warn);
                }
                else
                {
                    Database.Query($"UPDATE `{workname}` SET `level`={data[0]}, `exp`={data[1]}, `allpoints`={data[2]} WHERE uuid={acc.UUID}");
                    Log.Write($"Я обновил данные игрока {player.Name}", Nlogs.Type.Warn);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        private static void SetFarmerClothes(Player player)
        {
            Customization.ClearClothes(player, Main.Players[player].Gender);
            player.SetClothes(3, 64, 0);
            player.SetClothes(4, 36, 0);
            player.SetClothes(6, 66, 5);
            player.SetClothes(11, 117, 0);
        }
        private static List<Vector3> Checkpoints = new List<Vector3>()
        {
            new Vector3(461.2964, 6468.779, 28.844),
            new Vector3(461.3177, 6474.811, 28.800),
            new Vector3(461.2937, 6480.455, 28.678),
            new Vector3(461.3493, 6486.196, 28.376),
            new Vector3(461.2838, 6492.039, 28.355),
            new Vector3(461.2453, 6497.739, 28.671),

            new Vector3(467.657, 6468.898, 28.867),
            new Vector3(467.587, 6473.910, 28.765),
            new Vector3(467.626, 6480.006, 28.753),
            new Vector3(467.651, 6486.813, 28.443),
            new Vector3(467.630, 6493.296, 28.345),
            new Vector3(467.616, 6499.664, 28.666),
        };
    }
}
