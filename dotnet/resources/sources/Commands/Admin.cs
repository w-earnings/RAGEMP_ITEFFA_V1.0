using GTANetworkAPI;
using iTeffa.Finance;
using iTeffa.Houses;
using iTeffa.Interface;
using iTeffa.Settings;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace iTeffa.Commands
{
    public class AdminCommands : Script
    {
        private static readonly Nlogs Log = new Nlogs("Admin Commands");
        [Command("admins")]
        public static void CMD_AllAdmins(Player client)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "admins")) return;
                client.SendChatMessage("=== ITEFFA ADMINS ONLINE ===");
                foreach (var p in Main.Players)
                {
                    if (p.Value.AdminLVL < 1) continue;
                    client.SendChatMessage($"[{p.Key.Value}] {p.Key.Name} - {p.Value.AdminLVL}");
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_AllAdmins\":" + e.ToString(), Nlogs.Type.Error);
            }
        }
        [Command("givelic")]
        public static void CMD_giveLicense(Player player, int id, int lic)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "givelic")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (lic < 0 || lic >= Main.Players[target].Licenses.Count)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"lic = от 0 до {Main.Players[target].Licenses.Count - 1}", 3000);
                    return;
                }
                Main.Players[target].Licenses[lic] = true;
                Dashboard.sendStats(target);
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Успешно выдано", 3000);
            }
            catch { }
        }
        [Command("resurrection")]
        public static void CMD_revive(Player client, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "resurrection")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Игрок с таким ID не найден", 3000);
                    return;
                }
                target.StopAnimation();
                NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 0.5));
                target.SetSharedData("InDeath", false);
                Trigger.ClientEvent(target, "DeathTimer", false);
                target.Health = 100;
                target.ResetData("IS_DYING");
                Main.Players[target].IsAlive = true;
                Main.OffAntiAnim(target);
                if (target.HasData("DYING_TIMER"))
                {
                    Timers.Stop(target.GetData<string>("DYING_TIMER"));
                    target.ResetData("DYING_TIMER");
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.TopCenter, $"Игрок ({client.Value}) реанимировал Вас", 3000);
                Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы реанимировали игрока ({target.Value})", 3000);

                if (target.HasData("CALLEMS_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
                }
                if (target.HasData("CALLEMS_COL"))
                {
                    NAPI.ColShape.DeleteColShape(target.GetData<ColShape>("CALLEMS_COL"));
                }
            }
            catch (Exception e) { Log.Write("Resurrection AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("addpromo")]
        public static void CMD_addPromo(Player player, int uuid, string promocode)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "addpromo")) return;
                promocode = promocode.ToLower();
                Main.PromoCodes.Add(promocode, new Tuple<int, int, int>(1, 0, uuid));
                MySqlCommand queryCommand = new MySqlCommand(@"INSERT INTO `promocodes` (`name`, `type`, `count`, `owner`) VALUES (@NAME, @TYPE, @COUNT, @OWNER)");
                queryCommand.Parameters.AddWithValue("@NAME", promocode);
                queryCommand.Parameters.AddWithValue("@TYPE", 1);
                queryCommand.Parameters.AddWithValue("@COUNT", 0);
                queryCommand.Parameters.AddWithValue("@OWNER", uuid);
                Database.Query(queryCommand);
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Промокод успешно создан...", 3000);
            }
            catch { }
        }
        [Command("createsafe", GreedyArg = true)]
        public static void CMD_createSafe(Player player, int id, float distance, int min, int max, string address)
        {
            try
            {
                Globals.SafeMain.CMD_CreateSafe(player, id, distance, min, max, address);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("allspawncar")]
        public static void CMD_allSpawnCar(Player player)
        {
            Globals.Admin.respawnAllCars(player);
        }
        [Command("save")]
        public static void CMD_saveCoord(Player player, string name)
        {
            Globals.Admin.saveCoords(player, name);
        }
        #region Администратор
        [Command("setadmin")]
        public static void CMD_setAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.setPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("deladmin")]
        public static void CMD_delAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.delPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("setadminrank")]
        public static void CMD_setAdminRank(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.setPlayerAdminRank(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Лидерка
        [Command("delleader")]
        public static void CMD_delleader(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.delFracLeader(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Бизнесы
        [Command("createbusiness")]
        public static void CMD_createBiz(Player player, int govPrice, int type)
        {
            try
            {
                Globals.BusinessManager.createBusinessCommand(player, govPrice, type);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error);
            }
        }
        [Command("deletebusiness")]
        public static void CMD_deleteBiz(Player player, int bizid)
        {
            try
            {
                Globals.BusinessManager.deleteBusinessCommand(player, bizid);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error);
            }

        }
        [Command("takeoffbiz")]
        public static void CMD_takeOffBusiness(Player admin, int bizid, bool byaclear = false)
        {
            try
            {
                if (!Main.Players.ContainsKey(admin)) return;
                if (!Globals.Group.CanUseCmd(admin, "takeoffbiz")) return;

                var biz = Globals.BusinessManager.BizList[bizid];
                var owner = biz.Owner;
                var player = NAPI.Player.GetPlayerFromName(owner);

                if (player != null && Main.Players.ContainsKey(player))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.TopCenter, $"Администратор отобрал у Вас бизнес", 3000);
                    Wallet.Change(player, Convert.ToInt32(biz.SellPrice * 0.8));
                    Main.Players[player].BizIDs.Remove(biz.ID);
                }
                else
                {
                    var split = biz.Owner.Split('_');
                    var data = Database.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    List<int> ownerBizs = new List<int>();
                    var money = 0;

                    foreach (DataRow Row in data.Rows)
                    {
                        ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                        money = Convert.ToInt32(Row["money"]);
                    }

                    ownerBizs.Remove(biz.ID);
                    Database.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                }

                Finance.Bank.Accounts[biz.BankID].Balance = 0;
                biz.Owner = "Государство";
                biz.UpdateLabel();
                Globals.Loggings.Money($"server", $"player({Main.PlayerUUIDs[owner]})", Convert.ToInt32(biz.SellPrice * 0.8), $"takeoffBiz({biz.ID})");
                Notify.Send(admin, NotifyType.Info, NotifyPosition.TopCenter, $"Вы отобрали бизнес у {owner}", 3000);
                if (!byaclear) Globals.Loggings.Admin($"{player.Name}", $"takeoffBiz({biz.ID})", $"");
            }
            catch (Exception e) { Log.Write("takeoffbiz: " + e.Message, Nlogs.Type.Error); }
        }
        [Command("createrod")]
        public static void CMD_createRod(Player player, float radius)
        {
            try
            {
                Globals.RodManager.createRodAreaCommand(player, radius);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("createunloadpoint")]
        public static void CMD_createUnloadPoint(Player player, int bizid)
        {
            try
            {
                Globals.BusinessManager.createBusinessUnloadpoint(player, bizid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Финансы
        [Command("givemoney")]
        public static void CMD_adminGiveMoney(Player player, int id, int money)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.giveMoney(player, Main.GetPlayerByID(id), money);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("givecoins")]
        public static void CMD_givecoins(Player player, int id, int amount)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.sendCoins(player, target, amount);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Фракции
        [Command("setfracveh")]
        public static void ACMD_setfracveh(Player player, string vehname, int rank, int c1, int c2)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setfracveh")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
                    return;
                }
                if (rank <= 0 || c1 < 0 || c1 >= 160 || c2 < 0 || c2 >= 160) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehname);
                if (vh == 0) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[vehicle.GetData<int>("FRACTION")].ContainsKey(vehicle.NumberPlate)) return;

                    var canmats = (vh == VehicleHash.Barracks || vh == VehicleHash.Youga || vh == VehicleHash.Burrito3);
                    var candrugs = (vh == VehicleHash.Youga || vh == VehicleHash.Burrito3);
                    var canmeds = (vh == VehicleHash.Ambulance);
                    int fractionid = vehicle.GetData<int>("FRACTION");
                    NAPI.Data.SetEntityData(vehicle, "CANMATS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANDRUGS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", false);
                    if (canmats) NAPI.Data.SetEntityData(vehicle, "CANMATS", true);
                    if (candrugs) NAPI.Data.SetEntityData(vehicle, "CANDRUGS", true);
                    if (canmeds) NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", true);
                    NAPI.Data.SetEntityData(vehicle, "MINRANK", rank);
                    Vector3 pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                    Vector3 rot = NAPI.Entity.GetEntityRotation(vehicle);
                    Globals.VehicleManager.VehicleCustomization data = Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item7;
                    if (Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item1 != vh) data = new Globals.VehicleManager.VehicleCustomization();
                    Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate] = new Tuple<VehicleHash, Vector3, Vector3, int, int, int, Globals.VehicleManager.VehicleCustomization>(vh, pos, rot, rank, c1, c2, data);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `model`=@mod,`position`=@pos,`rotation`=@rot,`rank`=@ra,`colorprim`=@col,`colorsec`=@sec,`components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@mod", vehname);
                    cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(pos));
                    cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(rot));
                    cmd.Parameters.AddWithValue("@ra", rank);
                    cmd.Parameters.AddWithValue("@col", c1);
                    cmd.Parameters.AddWithValue("@sec", c2);
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(data));
                    cmd.Parameters.AddWithValue("@num", vehicle.NumberPlate);
                    Database.Query(cmd);
                    vehicle.PrimaryColor = c1;
                    vehicle.SecondaryColor = c2;
                    NAPI.Entity.SetEntityModel(vehicle, (uint)vh);
                    Globals.VehicleManager.FracApplyCustomization(vehicle, fractionid);
                    player.SendChatMessage("Вы изменили данные этой машины для фракции.");
                }
                else player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"ACMD_setfracveh\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("createstock")]
        public static void CMD_createStock(Player player, int frac, int drugs, int mats, int medkits, int money)
        {
            try
            {
                Database.Query($"INSERT INTO fractions (id,drugs,mats,medkits,money) VALUES ({frac},{drugs},{mats},{medkits},{money})");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("newfracveh")]
        public static void ACMD_newfracveh(Player player, string model, int fracid, string number, int c1, int c2)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "newfracveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                Globals.VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `fractionvehicles`(`fraction`, `number`, `components`, `model`, `position`, `rotation`, `rank`, `colorprim`, `colorsec`) VALUES (@idfrac, @number, '{}', @model, @pos, @rot, '1', @c1, @c2);"
                };
                cmd.Parameters.AddWithValue("@idfrac", fracid);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                Database.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                Globals.VehicleManager.FracApplyCustomization(veh, fracid);
                player.SendChatMessage("Вы добавили машину фракции.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"ACMD_newfracveh\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("setfractun")]
        public static void ACMD_setfractun(Player player, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
                    return;
                }
                if (player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[player.Vehicle.GetData<int>("FRACTION")].ContainsKey(player.Vehicle.NumberPlate)) return;
                    int fractionid = player.Vehicle.GetData<int>("FRACTION");
                    if (cat < 0)
                    {
                        Globals.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                        return;
                    }

                    string number = player.Vehicle.NumberPlate;
                    Tuple<VehicleHash, Vector3, Vector3, int, int, int, Globals.VehicleManager.VehicleCustomization> oldtuple = Fractions.Configs.FractionVehicles[fractionid][number];
                    VehicleHash oldvehhash = oldtuple.Item1;
                    Vector3 oldvehpos = oldtuple.Item2;
                    Vector3 oldvehrot = oldtuple.Item3;
                    int oldvehrank = oldtuple.Item4;
                    int oldvehc1 = oldtuple.Item5;
                    int oldvehc2 = oldtuple.Item6;
                    Globals.VehicleManager.VehicleCustomization oldvehdata = oldtuple.Item7;
                    switch (cat)
                    {
                        case 0:
                            oldvehdata.Spoiler = id;
                            break;
                        case 1:
                            oldvehdata.FrontBumper = id;
                            break;
                        case 2:
                            oldvehdata.RearBumper = id;
                            break;
                        case 3:
                            oldvehdata.SideSkirt = id;
                            break;
                        case 4:
                            oldvehdata.Muffler = id;
                            break;
                        case 5:
                            oldvehdata.Wings = id;
                            break;
                        case 6:
                            oldvehdata.Roof = id;
                            break;
                        case 7:
                            oldvehdata.Hood = id;
                            break;
                        case 8:
                            oldvehdata.Vinyls = id;
                            break;
                        case 9:
                            oldvehdata.Lattice = id;
                            break;
                        case 10:
                            oldvehdata.Engine = id;
                            break;
                        case 11:
                            oldvehdata.Turbo = id;
                            var turbo = (oldvehdata.Turbo == 0);
                            player.Vehicle.SetSharedData("TURBO", turbo);
                            break;
                        case 12:
                            oldvehdata.Horn = id;
                            break;
                        case 13:
                            oldvehdata.Transmission = id;
                            break;
                        case 14:
                            oldvehdata.WindowTint = id;
                            break;
                        case 15:
                            oldvehdata.Suspension = id;
                            break;
                        case 16:
                            oldvehdata.Brakes = id;
                            break;
                        case 17:
                            oldvehdata.Headlights = id;
                            break;
                        case 18:
                            oldvehdata.NumberPlate = id;
                            break;
                        case 19:
                            oldvehdata.NeonColor.Red = id;
                            break;
                        case 20:
                            oldvehdata.NeonColor.Green = id;
                            break;
                        case 21:
                            oldvehdata.NeonColor.Blue = id;
                            break;
                        case 22:
                            oldvehdata.NeonColor.Alpha = id;
                            break;
                        case 23:
                            oldvehdata.WheelsType = id;
                            break;
                        case 24:
                            oldvehdata.Wheels = id;
                            break;
                        case 25:
                            oldvehdata.WheelsColor = id;
                            break;
                    }
                    Fractions.Configs.FractionVehicles[fractionid][number] = new Tuple<VehicleHash, Vector3, Vector3, int, int, int, Globals.VehicleManager.VehicleCustomization>(oldvehhash, oldvehpos, oldvehrot, oldvehrank, oldvehc1, oldvehc2, oldvehdata);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(oldvehdata));
                    cmd.Parameters.AddWithValue("@num", player.Vehicle.NumberPlate);
                    Database.Query(cmd);
                    Globals.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                    player.SendChatMessage("Вы изменили тюнинг этой машины для фракции.");
                }
                else player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
            }
            catch { }
        }
        #endregion
        #region Действия с игроком
        [Command("demorgan", GreedyArg = true)]
        public static void CMD_sendTargetToDemorgan(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.sendPlayerToDemorgan(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("udemorgan")]
        public static void CMD_releaseTargetFromDemorgan(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.releasePlayerFromDemorgan(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("redname")]
        public static void CMD_redname(Player player)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "redname")) return;

                if (!player.HasSharedData("REDNAME") || !player.GetSharedData<bool>("REDNAME"))
                {
                    player.SendChatMessage("~r~Redname ON");
                    player.SetSharedData("REDNAME", true);
                }
                else
                {
                    player.SendChatMessage("~r~Redname OFF");
                    player.SetSharedData("REDNAME", false);
                }

            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("hidenick")]
        public static void CMD_hidenick(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
            if (!player.HasSharedData("HideNick") || !player.GetSharedData<bool>("HideNick"))
            {
                player.SendChatMessage("~g~HideNick ON");
                player.SetSharedData("HideNick", true);
            }
            else
            {
                player.SendChatMessage("~g~HideNick OFF");
                player.SetSharedData("HideNick", false);
            }

        }
        [Command("addhp")]
        public static void CMD_adminHeal(Player player, int id, int hp)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.healTarget(player, Main.GetPlayerByID(id), hp);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("addarmor")]
        public static void CMD_adminArmor(Player player, int id, int ar)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.armorTarget(player, Main.GetPlayerByID(id), ar);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("usp")]
        public static void CMD_unspectateMode(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "sp")) return;
            try
            {
                Globals.AdminSP.UnSpectate(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("sp")]
        public static void CMD_spectateMode(Player player, int id)
        {
            if (!Globals.Group.CanUseCmd(player, "sp")) return;
            try
            {
                Globals.AdminSP.Spectate(player, id);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("metp")]
        public static void CMD_teleportToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("gethere")]
        public static void CMD_teleportVehToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("kill")]
        public static void CMD_kill(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.killTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("fz")]
        public static void CMD_adminFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.freezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("ufz")]
        public static void CMD_adminUnFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Globals.Admin.unFreezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Транспорт
        [Command("vehc")]
        public static void CMD_createVehicleCustom(Player player, string name, int r, int g, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "ADMIN";
                veh.CustomPrimaryColor = new Color(r, g, b);
                veh.CustomSecondaryColor = new Color(r, g, b);
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                Globals.VehicleStreaming.SetEngineState(veh, true);
                Log.Debug($"vehc {name} {r} {g} {b}");
                Globals.Loggings.Admin($"{player.Name}", $"vehCreate({name})", $"");
            }
            catch { }
        }
        [Command("veh")]
        public static void CMD_createVehicle(Player player, string name, int a, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                player.SendChatMessage("vh " + vh);
                if (vh == 0)
                {
                    player.SendChatMessage("vh return");
                    return;
                }
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "ADMIN";
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                Globals.VehicleStreaming.SetEngineState(veh, true);
                player.SetIntoVehicle(veh, 0);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD_veh\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("vehhash")]
        public static void CMD_createVehicleHash(Player player, string name, int a, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                var veh = NAPI.Vehicle.CreateVehicle(Convert.ToInt32(name, 16), player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "PROJECT";
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                Globals.VehicleStreaming.SetEngineState(veh, true);
            }
            catch { }
        }
        [Command("vehs")]
        public static void CMD_createVehicles(Player player, string name, int a, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for (int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.PrimaryColor = a;
                    veh.SecondaryColor = b;
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    Globals.VehicleStreaming.SetEngineState(veh, true);
                }
                Globals.Loggings.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        [Command("vehcs")]
        public static void CMD_createVehicleCustoms(Player player, string name, int r, int g, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for (int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.CustomPrimaryColor = new Color(r, g, b);
                    veh.CustomSecondaryColor = new Color(r, g, b);
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    Globals.VehicleStreaming.SetEngineState(veh, true);
                    Log.Debug($"vehc {name} {r} {g} {b}");
                }
                Globals.Loggings.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        [Command("vehcustompcolor")]
        public static void CMD_ApplyCustomPColor(Player client, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;
                Color color = new Color(r, g, b);

                var number = client.Vehicle.NumberPlate;

                Globals.VehicleManager.Vehicles[number].Components.PrimColor = color;
                Globals.VehicleManager.Vehicles[number].Components.PrimModColor = mod;

                Globals.VehicleManager.ApplyCustomization(client.Vehicle);

            }
            catch { }
        }
        [Command("delacars")]
        public static void CMD_deleteAdminCars(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Globals.Group.CanUseCmd(player, "delacars")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                                v.Delete();
                        }
                        Globals.Loggings.Admin($"{player.Name}", $"delacars", $"");
                    }
                    catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, Nlogs.Type.Error); }
        }
        [Command("delacar")]
        public static void CMD_deleteThisAdminCar(Player client)
        {
            if (!Globals.Group.CanUseCmd(client, "delacar")) return;
            if (!client.IsInVehicle) return;
            Vehicle veh = client.Vehicle;
            if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "ADMIN")
                veh.Delete();
            Globals.Loggings.Admin($"{client.Name}", $"delacar", $"");
        }
        [Command("delmycars", "dmcs")]
        public static void CMD_delMyCars(Player client)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Globals.Group.CanUseCmd(client, "vehc")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                            {
                                if (v.GetData<string>("BY") == client.Name)
                                    v.Delete();
                            }
                        }
                        Globals.Loggings.Admin($"{client.Name}", $"delmycars", $"");
                    }
                    catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, Nlogs.Type.Error); }
        }
        #endregion
        #region Аренда
        [Command("newrentveh")]
        public static void newrentveh(Player player, string model, string number, int price, int c1, int c2)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "newrentveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                Globals.VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `othervehicles`(`type`, `number`, `model`, `position`, `rotation`, `color1`, `color2`, `price`) VALUES (@type, @number, @model, @pos, @rot, @c1, @c2, @price);"
                };
                cmd.Parameters.AddWithValue("@type", 0);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                Database.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                player.SendChatMessage("Вы добавили машину для аренды.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"newrentveh\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        [Command("newjobveh")]
        public static void newjobveh(Player player, string typejob, string model, string number, int c1, int c2)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "newjobveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) throw null;
                int typeIdJob = 999;
                switch (typejob)
                {
                    case "Taxi":
                        typeIdJob = 3;
                        break;
                    case "Bus":
                        typeIdJob = 4;
                        break;
                    case "Lawnmower":
                        typeIdJob = 5;
                        break;
                    case "Truckers":
                        typeIdJob = 6;
                        break;
                    case "Collector":
                        typeIdJob = 7;
                        break;
                    case "AutoMechanic":
                        typeIdJob = 8;
                        break;
                    case "Scourge":
                        typeIdJob = 10;
                        break;
                    case "Driving":
                        typeIdJob = 100;
                        break;
                }
                if (typeIdJob == 999)
                {
                    player.SendChatMessage("Выберите один тип работы из: Taxi, Bus, Lawnmower, Truckers, Collector, AutoMechanic, Scourge, Driving");
                    throw null;
                }
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                Globals.VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `othervehicles`(`type`, `number`, `model`, `position`, `rotation`, `color1`, `color2`, `price`) VALUES (@type, @number, @model, @pos, @rot, @c1, @c2, '0');"
                };
                cmd.Parameters.AddWithValue("@type", typeIdJob);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                Database.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                player.SendChatMessage("Вы добавили рабочую машину.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"newjobveh\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region Дом и гараж
        [Command("cleargarages")]
        public static void CMD_CreateHouse(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;

            var list = new List<int>();
            lock (GarageManager.Garages)
            {
                foreach (var g in GarageManager.Garages)
                {
                    var house = HouseManager.Houses.FirstOrDefault(h => h.GarageID == g.Key);
                    if (house == null) list.Add(g.Key);
                }
            }

            foreach (var id in list)
            {
                GarageManager.Garages.Remove(id);
                Database.Query($"DELETE FROM `garages` WHERE `id`={id}");
            }
        }
        [Command("createhouse")]
        public static void CMD_CreateHouse(Player player, int type, int price)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;
            if (type < 0 || type >= HouseManager.HouseTypeList.Count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Неправильный тип", 3000);
                return;
            }

            var bankId = Bank.Create(string.Empty, 2, 0);
            House new_house = new House(HouseManager.GetUID(), string.Empty, type, player.Position - new Vector3(0, 0, 1.12), price, false, 0, bankId, new List<string>());
            HouseManager.DimensionID++;
            new_house.Dimension = HouseManager.DimensionID;
            new_house.Create();
            FurnitureManager.Create(new_house.ID);
            new_house.CreateInterior();

            HouseManager.Houses.Add(new_house);
        }
        [Command("removehouse")]
        public static void CMD_RemoveHouse(Player player, int id)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;

            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == id);
            if (house == null) return;

            house.Destroy();
            HouseManager.Houses.Remove(house);
            Database.Query($"DELETE FROM `houses` WHERE `id`='{house.ID}'");
        }
        [Command("houseis")]
        public static void CMD_HouseIs(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            NAPI.Chat.SendChatMessageToPlayer(player, $"{player.GetData<int>("HOUSEID")}");
        }
        [Command("housechange")]
        public static void CMD_HouseOwner(Player player, string newOwner)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.ChangeOwner(newOwner);
            HouseManager.SavingHouses();
        }
        [Command("housenewprice")]
        public static void CMD_setHouseNewPrice(Player player, int price)
        {
            if (!Globals.Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;
            house.Price = price;
            house.UpdateLabel();
            house.Save();
        }
        [Command("myguest")]
        public static void CMD_InvitePlayerToHouse(Player player, int id)
        {
            var guest = Main.GetPlayerByID(id);
            if (guest == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок не найден", 3000);
                return;
            }
            if (player.Position.DistanceTo(guest.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы находитесь слишком далеко", 3000);
                return;
            }
            HouseManager.InvitePlayerToHouse(player, guest);
        }
        [Command("sellhouse")]
        public static void CMD_sellHouse(Player player, int id, int price)
        {
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок не найден", 3000);
                return;
            }
            HouseManager.OfferHouseSell(player, target, price);
        }
        [Command("setgarage")]
        public static void CMD_SetGarage(Player player, int ID)
        {
            if (!Globals.Group.CanUseCmd(player, "ban")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Sie müssen auf der Hausmarkierung stehen", 3000);
                return;
            }

            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            if (!GarageManager.Garages.ContainsKey(ID)) return;
            house.GarageID = ID;
            house.Save();
        }
        [Command("creategarage")]
        public static void CMD_CreateGarage(Player player, int type)
        {
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Du musst im Auto sitzen!", 3000);
                return;
            }

            if (!Globals.Group.CanUseCmd(player, "allspawncar")) return;
            if (!GarageManager.GarageTypes.ContainsKey(type)) return;
            int id = 0;
            do
            {
                id++;

            } while (GarageManager.Garages.ContainsKey(id));

            Garage garage = new Garage(id, type, player.Vehicle.Position, player.Vehicle.Rotation)
            {
                Dimension = GarageManager.DimensionID
            };
            garage.Create();
            if (type != -1) garage.CreateInterior();

            GarageManager.Garages.Add(garage.ID, garage);
            NAPI.Chat.SendChatMessageToPlayer(player, garage.ID.ToString());
        }
        [Command("removegarage")]
        public static void CMD_RemoveGarage(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "allspawncar")) return;
            if (!player.HasData("GARAGEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны быть на гаражном маркере", 3000);
                return;
            }
            if (!GarageManager.Garages.ContainsKey(player.GetData<int>("GARAGEID"))) return;
            Garage garage = GarageManager.Garages[player.GetData<int>("GARAGEID")];
            garage.Destroy();
            GarageManager.Garages.Remove(player.GetData<int>("GARAGEID"));
            Database.Query($"DELETE FROM `garages` WHERE `id`='{garage.ID}'");
        }
        #endregion
        #region Сервер
        [Command("stop")]
        public static void CMD_stopServer(Player player, string text = null)
        {
            Globals.Admin.stopServer(player, text);
        }
        [Command("restart")]
        public void HandleShutDown(Player restart, int second)
        {
            if (second < 5 || second > 900)
            {
                restart.SendNotification("Минимум 5 секунд и максимум 9 минут!");
                return;
            }
            Controller.SendToAdmins(8, $"!{{#d35400}} {restart.Name}, пытается перезагрузить сервер.");
            if (Main.Players[restart].AdminLVL < 8)
            {
                Notify.Send(restart, NotifyType.Error, NotifyPosition.TopCenter, $"У игрока нет админ. прав", 3000);
                return;
            }
            foreach (Player c in NAPI.Pools.GetAllPlayers())
            {
                Main.saveDatabase();
            }
            NAPI.Chat.SendChatMessageToAll("[~r~SERVER~w~]: Перезагрузка сервера через " + second + " Секунды. [ИСПРАВЛЕНИЕ ОШИБКИ] Пожалуйста, выйдите из системы заранее, чтобы ваши вещи были сохранены!");
            Task.Run(() =>
            {
                Task.Delay(1000 * second * 1).Wait();
                Environment.Exit(0);
            });
        }
        #endregion
        #region Карта
        [Command("loadipl")]
        public static void CMD_LoadIPL(Player player, string ipl)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                NAPI.World.RequestIpl(ipl);
                player.SendChatMessage("Вы подгрузили IPL: " + ipl);
            }
            catch
            {
            }
        }
        [Command("unloadipl")]
        public static void CMD_UnLoadIPL(Player player, string ipl)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                NAPI.World.RemoveIpl(ipl);
                player.SendChatMessage("Вы выгрузили IPL: " + ipl);
            }
            catch
            {
            }
        }
        [Command("loadprop")]
        public static void CMD_LoadProp(Player player, double x, double y, double z, string prop)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.ClientEvent(player, "loadProp", x, y, z, prop);
                player.SendChatMessage("Вы подгрузили Interior Prop: " + prop);
            }
            catch
            {
            }
        }
        [Command("unloadprop")]
        public static void CMD_UnLoadProp(Player player, double x, double y, double z, string prop)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.ClientEvent(player, "UnloadProp", x, y, z, prop);
                player.SendChatMessage("Вы выгрузили Interior Prop: " + prop);
            }
            catch
            {
            }
        }
        #endregion
        #region Прочее
        [Command("sw")]
        public static void CMD_setWeatherID(Player player, byte weather)
        {
            if (!Globals.Group.CanUseCmd(player, "sw")) return;
            Main.changeWeather(weather);
            Globals.Loggings.Admin($"{player.Name}", $"setWeather({weather})", $"");
        }
        [Command("starteffect")]
        public static void CMD_StartEffect(Player player, string effect, int dur = 0, bool loop = false)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.ClientEvent(player, "startScreenEffect", effect, dur, loop);
                player.SendChatMessage("Вы включили Effect: " + effect);
            }
            catch
            {
            }
        }
        [Command("stopeffect")]
        public static void CMD_StopEffect(Player player, string effect)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.ClientEvent(player, "stopScreenEffect", effect);
                player.SendChatMessage("Вы выключили Effect: " + effect);
            }
            catch
            {
            }
        }
        #endregion
    }
}


