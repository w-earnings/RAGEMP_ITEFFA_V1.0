using GTANetworkAPI;
using iTeffa.Finance;
using iTeffa.Globals;
using iTeffa.Interface;
using iTeffa.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace iTeffa.Commands
{
    public class OtherCommands : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Other Commands");
        private static readonly Random rnd = new Random();
        [Command("getbonus")]
        public static void GetLastBonus(Player player, int id)
        {
            if (!Globals.Group.CanUseCmd(player, "getbonus")) return;

            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.Players[target].LastBonus)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Бонус игрока({id}): {hour} часов и {min} минут ({Main.Players[target].LastBonus})", 3000);
        }
        [Command("lastbonus")]
        public static void LastBonus(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "lastbonus")) return;
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.oldconfig.LastBonusMin)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Бонус составляет: {hour} часов и {min} минут", 2500);
        }
        [Command("setbonus")]
        public static void SetLastBonus(Player player, int id, int count)
        {
            if (!Globals.Group.CanUseCmd(player, "setbonus")) return;

            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            count = Convert.ToInt32(Math.Abs(count));
            if (count > Main.oldconfig.LastBonusMin)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Введенное число превышает значение максимума. Максимум: {Main.oldconfig.LastBonusMin}", 3000);
                return;
            }
            Main.Players[target].LastBonus = count;
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.Players[target].LastBonus)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Бонус для игрока({id}) установлен на {hour} часов и {min} минут ({Main.Players[target].LastBonus})", 3000);
        }
        [Command("vehchange")]
        public static void CMD_vehchage(Player client, string newmodel)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;

                if (!client.IsInVehicle) return;

                if (!client.Vehicle.HasData("ACCESS"))
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    VehicleManager.Vehicles[client.Vehicle.NumberPlate].Model = newmodel;
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Машина будет доступна после респавна", 3000);
                }
                else if (client.Vehicle.GetData<string>("ACCESS") == "WORK")
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || client.Vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    return;
            }
            catch { }
        }
        [Command("findtrailer")]
        public static void CMD_findTrailer(Player player)
        {
            try
            {
                if (player.HasData("TRAILER"))
                {
                    Vehicle trailer = player.GetData<Vehicle>("TRAILER");
                    Plugins.Trigger.ClientEvent(player, "createWaypoint", trailer.Position.X, trailer.Position.Y);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы успешно установили маркер на карту, там находится Ваш трейлер", 5000);
                }
            }
            catch { }
        }
        [Command("bankfix")]
        public static void CMD_bankfix(Player client, int bank)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;
                if (Bank.Accounts.ContainsKey(bank))
                {
                    Bank.RemoveByID(bank);
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы успешно удалили банковский счёт номер {bank}", 3000);
                }
                else Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Банковский счёт {bank} не найден", 3000);
            }
            catch { }
        }
        [Command("gethwid")]
        public static void CMD_gethwid(Player client, int ID)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок с таким ID не найден", 3000);
                    return;
                }
                client.SendChatMessage("Реальный HWID у " + target.Name + ": " + target.GetData<string>("RealHWID"));
            }
            catch { }
        }
        [Command("getsocialclub")]
        public static void CMD_getsc(Player client, int ID)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок с таким ID не найден", 3000);
                    return;
                }
                client.SendChatMessage("Реальный SocialClub у " + target.Name + ": " + target.GetData<string>("RealSocialClub"));
            }
            catch { }
        }
        [Command("loggedinfix")]
        public static void CMD_loggedinfix(Player player, string login)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL <= 6) return;
                if (Main.LoggedIn.ContainsKey(login))
                {
                    if (NAPI.Player.IsPlayerConnected(Main.LoggedIn[login]))
                    {
                        Main.LoggedIn[login].Kick();
                        Main.LoggedIn.Remove(login);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Вы кикнули персонажа с сервера, через минуту можно будет пытаться зайти в аккаунт.", 3000);
                    }
                    else
                    {
                        Main.LoggedIn.Remove(login);
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Персонаж не был в сети, аккаунт удалён из списка авторизовавшихся.", 3000);
                    }
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Аккаунта в сети с логином {login} не найдено", 3000);
            }
            catch { }
        }
        [Command("vconfigload")]
        public static void CMD_loadConfigVehicles(Player player, int type, int number)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "vconfigload")) return;
                if (type == 0)
                {
                    Fractions.Configs.FractionVehicles[number] = new Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>();
                    DataTable result = Database.QueryRead($"SELECT * FROM `fractionvehicles` WHERE `fraction`={number}");
                    if (result == null || result.Rows.Count == 0) return;
                    foreach (DataRow Row in result.Rows)
                    {
                        var fraction = Convert.ToInt32(Row["fraction"]);
                        var numberplate = Row["number"].ToString();
                        var model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var minrank = Convert.ToInt32(Row["rank"]);
                        var color1 = Convert.ToInt32(Row["colorprim"]);
                        var color2 = Convert.ToInt32(Row["colorsec"]);
                        VehicleManager.VehicleCustomization components = JsonConvert.DeserializeObject<VehicleManager.VehicleCustomization>(Row["components"].ToString());

                        Fractions.Configs.FractionVehicles[fraction].Add(numberplate, new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(model, position, rotation, minrank, color1, color2, new VehicleManager.VehicleCustomization()));
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {

                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && v.GetData<int>("FRACTION") == number)
                                    v.Delete();
                            }
                            Fractions.Configs.SpawnFractionCars(number);
                        }
                        catch { }
                    });
                }
                else // othervehicles
                {
                    var result = Database.QueryRead($"SELECT * FROM `othervehicles` WHERE `type`={number}");
                    if (result == null || result.Rows.Count == 0) return;

                    switch (number)
                    {
                        case 0:
                            Rentcar.CarInfos = new List<CarInfo>();
                            break;
                        case 3:
                            Working.Taxi.CarInfos = new List<CarInfo>();
                            break;
                        case 4:
                            Working.Bus.CarInfos = new List<CarInfo>();
                            break;
                        case 5:
                            Working.Lawnmower.CarInfos = new List<CarInfo>();
                            break;
                        case 6:
                            Working.Truckers.CarInfos = new List<CarInfo>();
                            break;
                        case 7:
                            Working.Collector.CarInfos = new List<CarInfo>();
                            break;
                        case 8:
                            Working.AutoMechanic.CarInfos = new List<CarInfo>();
                            break;
                    }

                    foreach (DataRow Row in result.Rows)
                    {
                        var numberPlate = Row["number"].ToString();
                        var model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var color1 = Convert.ToInt32(Row["color1"]);
                        var color2 = Convert.ToInt32(Row["color2"]);
                        var price = Convert.ToInt32(Row["price"]);
                        var data = new CarInfo(numberPlate, model, position, rotation, color1, color2, price);

                        switch (number)
                        {
                            case 0:
                                Rentcar.CarInfos.Add(data);
                                break;
                            case 3:
                                Working.Taxi.CarInfos.Add(data);
                                break;
                            case 4:
                                Working.Bus.CarInfos.Add(data);
                                break;
                            case 5:
                                Working.Lawnmower.CarInfos.Add(data);
                                break;
                            case 6:
                                Working.Truckers.CarInfos.Add(data);
                                break;
                            case 7:
                                Working.Collector.CarInfos.Add(data);
                                break;
                            case 8:
                                Working.AutoMechanic.CarInfos.Add(data);
                                break;
                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && ((v.GetData<string>("ACCESS") == "RENT" && number == 0) || (v.GetData<string>("ACCESS") == "WORK" && v.GetData<int>("WORK") == number)))
                                    v.Delete();
                            }
                            switch (number)
                            {
                                case 0:
                                    Rentcar.rentCarsSpawner();
                                    break;
                                case 3:
                                    Working.Taxi.taxiCarsSpawner();
                                    break;
                                case 4:
                                    Working.Bus.busCarsSpawner();
                                    break;
                                case 5:
                                    Working.Lawnmower.mowerCarsSpawner();
                                    break;
                                case 6:
                                    Working.Truckers.truckerCarsSpawner();
                                    break;
                                case 7:
                                    Working.Collector.collectorCarsSpawner();
                                    break;
                                case 8:
                                    Working.AutoMechanic.mechanicCarsSpawner();
                                    break;
                            }
                        }
                        catch { }
                    });
                }
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Машины зареспавнены", 3000);
            }
            catch (Exception e) { Log.Write("vconfigload: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("giveammo")]
        public static void CMD_ammo(Player client, int ID, int type, int amount = 1)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "giveammo")) return;

                var target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                List<ItemType> types = new List<ItemType>
                {
                    ItemType.PistolAmmo,
                    ItemType.RiflesAmmo,
                    ItemType.ShotgunsAmmo,
                    ItemType.SMGAmmo,
                    ItemType.SniperAmmo
                };

                if (type > types.Count || type < -1) return;

                var tryAdd = nInventory.TryAdd(target, new nItem(types[type], amount));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                nInventory.Add(target, new nItem(types[type], amount));
            }
            catch { }
        }
        [Command("newvnum")]
        public static void CMD_newVehicleNumber(Player player, string oldNum, string newNum)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "newvnum")) return;
                if (!VehicleManager.Vehicles.ContainsKey(oldNum))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Такой машины не существует", 3000);
                    return;
                }

                if (VehicleManager.Vehicles.ContainsKey(newNum))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Такой номер уже существует", 3000);
                    return;
                }

                var vData = VehicleManager.Vehicles[oldNum];
                VehicleManager.Vehicles.Remove(oldNum);
                VehicleManager.Vehicles.Add(newNum, vData);

                var house = Houses.HouseManager.GetHouse(vData.Holder, true);
                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(oldNum);
                    garage.SpawnCar(newNum);
                }

                Database.Query($"UPDATE vehicles SET number='{newNum}' WHERE number='{oldNum}'");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Новый номер для {oldNum} = {newNum}", 3000);
            }
            catch (Exception e) { Log.Write("newvnum: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("takecoins")]
        public static void CMD_offredbaks(Player client, string name, long amount)
        {
            if (!Globals.Group.CanUseCmd(client, "takecoins")) return;
            try
            {
                name = name.ToLower();
                KeyValuePair<Player, Globals.nAccount.Account> acc = Main.Accounts.FirstOrDefault(x => x.Value.Login == name);
                if (acc.Value != null)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок онлайн! {acc.Key.Name}:{acc.Key.Value}", 8000);
                    return;
                }
                Database.Query($"update `accounts` set `coins`=`coins`+{amount} where `login`='{name}'");
                Loggings.Admin(client.Name, $"takecoins({amount})", name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("checkprop")]
        public static void CMD_checkProperety(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "checkprop")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                var house = Houses.HouseManager.GetHouse(target);
                if (house != null)
                {
                    if (house.Owner == target.Name)
                    {
                        player.SendChatMessage($"Игрок имеет дом стоимостью ${house.Price} класса '{Houses.HouseManager.HouseTypeList[house.Type].Name}'");
                        var targetVehicles = VehicleManager.getAllPlayerVehicles(target.Name);
                        foreach (var num in targetVehicles)
                            player.SendChatMessage($"У игрока есть машина '{VehicleManager.Vehicles[num].Model}' с номером '{num}'");
                    }
                    else
                        player.SendChatMessage($"Игрок заселен в дом к {house.Owner} стоимостью ${house.Price}");
                }
                else
                    player.SendChatMessage("У игрока нет дома");
            }
            catch (Exception e)
            {
                Log.Write("checkprop: " + e.Message, Plugins.Logs.Type.Error);
            }
        }
        [Command("id", "~y~/id [имя/id]")]
        public static void CMD_checkId(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "id")) return;

                if (int.TryParse(target, out int id))
                {
                    foreach (var p in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Value == id)
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            return;
                        }
                    }
                    player.SendChatMessage("Игрок с таким ID не найден");
                }
                else
                {
                    var players = 0;
                    foreach (var p in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Name.ToUpper().Contains(target.ToUpper()))
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            players++;
                        }
                    }
                    if (players == 0)
                        player.SendChatMessage("Не найдено игрока с таким именем");
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\"/id/:\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("setdim")]
        public static void CMD_setDim(Player player, int id, int dim)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "setdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                target.Dimension = Convert.ToUInt32(dim);
                Loggings.Admin($"{player.Name}", $"setDim({dim})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write("setdim: " + e.Message, Plugins.Logs.Type.Error);
            }
        }
        [Command("checkdim")]
        public static void CMD_checkDim(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "checkdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                Loggings.Admin($"{player.Name}", $"checkDim", $"{target.Name}");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Измерение игрока - {target.Dimension}", 4000);
            }
            catch (Exception e)
            {
                Log.Write("checkdim: " + e.Message, Plugins.Logs.Type.Error);
            }
        }
        [Command("setbizmafia")]
        public static void CMD_setBizMafia(Player player, int mafia)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "setbizmafia")) return;
                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (mafia < 10 || mafia > 13) return;

                Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
                biz.Mafia = mafia;
                biz.UpdateLabel();
                Loggings.Admin($"{player.Name}", $"setBizMafia({biz.ID},{mafia})", $"");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"{mafia} мафия теперь владеет бизнесом №{biz.ID}", 3000);
            }
            catch (Exception e) { Log.Write("setbizmafia: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("newsimcard")]
        public static void CMD_newsimcard(Player player, int id, int newnumber)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "newsimcard")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.SimCards.ContainsKey(newnumber))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Такой номер уже существует", 3000);
                    return;
                }

                Main.SimCards.Remove(newnumber);
                Main.SimCards.Add(newnumber, Main.Players[target].UUID);
                Main.Players[target].Sim = newnumber;
                Dashboard.sendStats(target);
                Loggings.Admin($"{player.Name}", $"newsim({newnumber})", $"{target.Name}");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Новый номер для {target.Name} = {newnumber}", 3000);
            }
            catch (Exception e) { Log.Write("newsimcard: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("paydaymultiplier")]
        public static void CMD_paydaymultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "paydaymultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Возможно установить только от 1 до 5", 3000);
                    return;
                }

                Main.oldconfig.PaydayMultiplier = multi;
                Loggings.Admin($"{player.Name}", $"paydayMultiplier({multi})", $"");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"PaydayMultiplier изменен на {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("expmultiplier")]
        public static void CMD_expmultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "expmultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Возможно установить только от 1 до 5", 3000);
                    return;
                }

                Main.oldconfig.ExpMultiplier = multi;
                Loggings.Admin($"{player.Name}", $"expMultiplier({multi})", $"");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"ExpMultiplier изменен на {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("offdelfrac")]
        public static void CMD_offlineDelFraction(Player player, string name)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "offdelfrac")) return;

                var split = name.Split('_');
                Database.Query($"UPDATE `characters` SET fraction=0,fractionlvl=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы уволили игрока {name} из Вашей фракции", 3000);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                Loggings.Admin($"{player.Name}", $"delfrac", $"{name}");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы сняли фракцию с {name}", 3000);
            }
            catch (Exception e) { Log.Write("offdelfrac: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("removeobj")]
        public static void CMD_removeObject(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "removeobj")) return;

                player.SetData("isRemoveObject", true);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Следующий подобранный предмет будет в бане", 3000);
            }
            catch (Exception e) { Log.Write("removeobj: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("unwarn")]
        public static void CMD_unwarn(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "unwarn")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.Players[target].Warns <= 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет варнов", 3000);
                    return;
                }

                Main.Players[target].Warns--;
                Interface.Dashboard.sendStats(target);

                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы сняли варн у игрока {target.Name}, у него {Main.Players[target].Warns} варнов", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"У вас сняли варн, осталось {Main.Players[target].Warns} варнов", 3000);
                Loggings.Admin($"{player.Name}", $"unwarn", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("unwarn: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("offunwarn")]
        public static void CMD_offunwarn(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "unwarn")) return;

                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не найден", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок онлайн", 3000);
                    return;
                }

                var split = target.Split('_');
                var data = Database.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = 0;
                foreach (System.Data.DataRow Row in data.Rows)
                {
                    warns = Convert.ToInt32(Row["warns"]);
                }

                if (warns <= 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока нет варнов", 3000);
                    return;
                }

                warns--;
                Loggings.Admin($"{player.Name}", $"offUnwarn", $"{target}");
                Database.Query($"UPDATE characters SET warns={warns} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы сняли варн у игрока {target}, у него {warns} варнов", 3000);
            }
            catch (Exception e) { Log.Write("offunwarn: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("rescar")]
        public static void CMD_respawnCar(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "rescar")) return;
                if (!player.IsInVehicle) return;
                var vehicle = player.Vehicle;

                if (!vehicle.HasData("ACCESS"))
                    return;
                else if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "На данный момент функция восстановления личной машины игрока отключена", 3000);
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    Admin.RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    Admin.RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);

                Loggings.Admin($"{player.Name}", $"rescar", $"");
            }
            catch (Exception e) { Log.Write("ResCar: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("bansync")]
        public static void CMD_banlistSync(Player client)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "ban")) return;
                Plugins.Notice.Send(client, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Начинаю процедуру синхронизации...", 4000);
                Modules.BanSystem.Sync();
                Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Процедура завершена!", 3000);
            }
            catch (Exception e) { Log.Write("bansync: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("setcolour")]
        public static void CMD_setTerritoryColor(Player player, int gangid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "setcolour")) return;

                if (player.GetData<int>("GANGPOINT") == -1)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не находитесь ни на одном из регионов", 3000);
                    return;
                }
                var terrid = player.GetData<int>("GANGPOINT");

                if (!Fractions.GangsCapture.gangPointsColor.ContainsKey(gangid))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Банды с таким ID нет", 3000);
                    return;
                }

                Fractions.GangsCapture.gangPoints[terrid].GangOwner = gangid;
                Main.ClientEventToAll("setZoneColor", Fractions.GangsCapture.gangPoints[terrid].ID, Fractions.GangsCapture.gangPointsColor[gangid]);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Теперь территорией №{terrid} владеет {Fractions.Manager.FractionNames[gangid]}", 3000);
                Loggings.Admin($"{player.Name}", $"setColour({terrid},{gangid})", $"");
            }
            catch (Exception e) { Log.Write("CMD_SetColour: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("sc")]
        public static void CMD_setClothes(Player player, int id, int draw, int texture)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "allspawncar")) return;
                player.SetClothes(id, draw, texture);
                if (id == 11) player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][draw], 0);
                if (id == 1) Customization.SetMask(player, draw, texture);
            }
            catch { }
        }
        [Command("sac")]
        public static void CMD_setAccessories(Player player, int id, int draw, int texture)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Globals.Group.CanUseCmd(player, "allspawncar")) return;
            if (draw > -1)
                player.SetAccessories(id, draw, texture);
            else
                player.ClearAccessory(id);

        }
        [Command("checkwanted")]
        public static void CMD_checkwanted(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Globals.Group.CanUseCmd(player, "checkwanted")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Человек с таким ID не найден", 3000);
                return;
            }
            var stars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Количество звезд - {stars}", 3000);
        }
        [Command("fixcar")]
        public static void CMD_fixcar(Player player)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "fixcar")) return;
                if (!player.IsInVehicle) return;
                VehicleManager.RepairCar(player.Vehicle);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixcar\":" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("fixweaponsshops")]
        public static void CMD_fixweaponsshops(Player client)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "fixgovbizprices")) return;

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Type != 6) continue;
                    biz.Products = BusinessManager.fillProductList(6);

                    var result = Database.QueryRead($"SELECT * FROM `weapons` WHERE id={biz.ID}");
                    if (result != null) continue;
                    Database.Query($"INSERT INTO weapons (id,lastserial) VALUES ({biz.ID},0)");
                    Log.Debug($"Insert into weapons new business ({biz.ID})");
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixweaponsshops\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("fixgovbizprices")]
        public static void CMD_fixgovbizprices(Player client)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "fixgovbizprices")) return;

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    foreach (var p in biz.Products)
                    {
                        if (p.Name == "Расходники" || biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики" || p.Name == "Патроны") continue;
                        p.Price = BusinessManager.ProductsOrderPrice[p.Name];
                    }
                    biz.UpdateLabel();
                }

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Owner != "Государство") continue;
                    foreach (var p in biz.Products)
                    {
                        if (p.Name == "Расходники") continue;
                        double price = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики" || p.Name == "Патроны") ? 125 : (biz.Type == 1) ? 6 : BusinessManager.ProductsOrderPrice[p.Name] * 1.25;
                        p.Price = Convert.ToInt32(price);
                        p.Lefts = Convert.ToInt32(BusinessManager.ProductsCapacity[p.Name] * 0.1);
                    }
                    biz.UpdateLabel();
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixgovbizprices\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("setproductbyindex")]
        public static void CMD_setproductbyindex(Player client, int id, int index, int product)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "setproductbyindex")) return;

                var biz = BusinessManager.BizList[id];
                biz.Products[index].Lefts = product;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_setproductbyindex\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("deleteproducts")]
        public static void CMD_deleteproducts(Player client, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "deleteproducts")) return;

                var biz = BusinessManager.BizList[id];
                foreach (var p in biz.Products)
                    p.Lefts = 0;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_setproductbyindex\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("changebizprice")]
        public static void CMD_changeBusinessPrice(Player player, int newPrice)
        {
            if (!Globals.Group.CanUseCmd(player, "changebizprice")) return;
            if (player.GetData<int>("BIZ_ID") == -1)
            {
                player.SendChatMessage("~r~Вы должны находиться на одном из бизнесов");
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            biz.SellPrice = newPrice;
            biz.UpdateLabel();
        }
        [Command("pa")]
        public static void CMD_playAnimation(Player player, string dict, string anim, int flag)
        {
            if (!Globals.Group.CanUseCmd(player, "pa")) return;
            player.PlayAnimation(dict, anim, flag);
        }
        [Command("sa")]
        public static void CMD_stopAnimation(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "sa")) return;
            player.StopAnimation();
        }
        [Command("changestock")]
        public static void CMD_changeStock(Player player, int fracID, string item, int amount)
        {
            if (!Globals.Group.CanUseCmd(player, "changestock")) return;
            if (!Fractions.Stocks.fracStocks.ContainsKey(fracID))
            {
                player.SendChatMessage("~r~Склада такой фракции нет");
                return;
            }
            switch (item)
            {
                case "mats":
                    Fractions.Stocks.fracStocks[fracID].Materials += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "drugs":
                    Fractions.Stocks.fracStocks[fracID].Drugs += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "medkits":
                    Fractions.Stocks.fracStocks[fracID].Medkits += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "money":
                    Fractions.Stocks.fracStocks[fracID].Money += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
            }
            player.SendChatMessage("~r~mats - материалы");
            player.SendChatMessage("~r~drugs - наркотики");
            player.SendChatMessage("~r~medkits - мед. аптечки");
            player.SendChatMessage("~r~money - деньги");
            Loggings.Admin($"{player.Name}", $"changeStock({item},{amount})", $"");
        }
        [Command("tpc")]
        public static void CMD_tpCoord(Player player, double x, double y, double z)
        {
            if (!Globals.Group.CanUseCmd(player, "tpc")) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, z));
        }
        [Command("inv")]
        public static void CMD_ToogleInvisible(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Globals.Group.CanUseCmd(player, "inv")) return;

            BasicSync.SetInvisible(player, !BasicSync.GetInvisible(player));
        }
        [Command("delfrac")]
        public static void CMD_delFrac(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.GetPlayerByID(id) == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            Admin.delFrac(player, Main.GetPlayerByID(id));
        }
        [Command("sendcreator")]
        public static void CMD_SendToCreator(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Globals.Group.CanUseCmd(player, "sendcreator")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            Customization.SendToCreator(target);
            Loggings.Admin($"{player.Name}", $"sendCreator", $"{target.Name}");
        }
        [Command("afuel")]
        public static void CMD_setVehiclePetrol(Player player, int fuel)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "afuel")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetSharedData("PETROL", fuel);
                Loggings.Admin($"{player.Name}", $"afuel({fuel})", $"");
            }
            catch (Exception e) { Log.Write("afuel: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("changename", GreedyArg = true)]
        public static void CMD_changeName(Player client, string curient, string newName)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(client, "changename")) return;
                if (!Main.PlayerNames.ContainsValue(curient)) return;

                try
                {
                    string[] split = newName.Split("_");
                    Log.Debug($"SPLIT: {split[0]} {split[1]}");
                }
                catch (Exception e)
                {
                    Log.Write("ERROR ON CHANGENAME COMMAND\n" + e.ToString());
                }


                if (Main.PlayerNames.ContainsValue(newName))
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Такое имя уже существует!", 3000);
                    return;
                }

                Player target = NAPI.Player.GetPlayerFromName(curient);
                Globals.Character.Character.toChange.Add(curient, newName);

                if (target == null || target.IsNull)
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "Игрок оффлайн, меняем...", 3000);
                    Task changeTask = Globals.Character.Character.changeName(curient);
                }
                else
                {
                    Plugins.Notice.Send(client, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "Игрок онлайн, кикаем...", 3000);
                    NAPI.Player.KickPlayer(target);
                }

                Plugins.Notice.Send(client, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Ник изменен!", 3000);
                Loggings.Admin($"{client.Name}", $"changeName({newName})", $"{curient}");

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_CHANGENAME\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("startmatwars")]
        public static void CMD_startMatWars(Player player)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "startmatwars")) return;
                if (Fractions.MatsWar.isWar)
                {
                    player.SendChatMessage("~r~Война за маты уже идёт");
                    return;
                }
                Fractions.MatsWar.startMatWarTimer();
                player.SendChatMessage("~r~Начата война за маты");
                Loggings.Admin($"{player.Name}", $"startMatwars", $"");
            }
            catch (Exception e) { Log.Write("startmatwars: " + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("whitelistdel")]
        public static void CMD_whitelistdel(Player player, string socialClub)
        {
            try
            {
                if (CheckSocialClubInWhiteList(socialClub))
                {
                    Database.Query("DELETE FROM `whiteList` WHERE `socialclub` = '" + socialClub + "';");
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Social club успешно удален из white list!", 3000);
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данный social club не найден в white list!", 3000);
                }
                Loggings.Admin($"{player.Name}", $"whitelistdel", $"");
            }
            catch (Exception e) { Log.Write("whitelistdel: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static bool CheckSocialClubInWhiteList(string SocialClub)
        {
            DataTable data = Database.QueryRead($"SELECT * FROM `whiteList` WHERE 1");
            foreach (DataRow Row in data.Rows)
            {
                if (Row["socialclub"].ToString() == SocialClub)
                {
                    return true;
                }
            }
            return false;
        }
        [Command("whitelistadd")]
        public static void CMD_whitelistadd(Player player, string socialClub)
        {
            try
            {
                if (CheckSocialClubInAccounts(socialClub))
                {
                    if (!CheckSocialClubInWhiteList(socialClub))
                    {
                        Database.Query("INSERT INTO `whiteList` (`socialclub`) VALUES ('" + socialClub + "');");
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, "Social club успешно добавлен в white list!", 3000);
                    }
                    else
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данный social club уже состоит в white list!", 3000);
                    }
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Данный social club не найден!", 3000);
                }
                Loggings.Admin($"{player.Name}", $"whitelistadd", $"");
            }
            catch (Exception e) { Log.Write("whitelistadd: " + e.Message, Plugins.Logs.Type.Error); }
        }
        public static bool CheckSocialClubInAccounts(string SocialClub)
        {
            DataTable data = Database.QueryRead($"SELECT * FROM `accounts` WHERE 1");
            foreach (DataRow Row in data.Rows)
            {
                if (Row["socialclub"].ToString() == SocialClub)
                {
                    return true;
                }
            }
            return false;
        }
        [Command("giveexp")]
        public static void CMD_giveExp(Player player, int id, int exp)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "giveexp")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Main.Players[target].EXP += exp;
                if (Main.Players[target].EXP >= 3 + Main.Players[target].LVL * 3)
                {
                    Main.Players[target].EXP = Main.Players[target].EXP - (3 + Main.Players[target].LVL * 3);
                    Main.Players[target].LVL += 1;
                    if (Main.Players[target].LVL == 1)
                    {
                        NAPI.Task.Run(() => { try { Plugins.Trigger.ClientEvent(target, "disabledmg", false); } catch { } }, 5000);
                    }
                }
                Dashboard.sendStats(target);
                Loggings.Admin($"{player.Name}", $"giveExp({exp})", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("giveexp" + e.Message, Plugins.Logs.Type.Error); }
        }
        [Command("housetypeprice")]
        public static void CMD_replaceHousePrices(Player player, int type, int newPrice)
        {
            if (!Globals.Group.CanUseCmd(player, "housetypeprice")) return;
            foreach (var h in Houses.HouseManager.Houses)
            {
                if (h.Type != type) continue;
                h.Price = newPrice;
                h.UpdateLabel();
                h.Save();
            }
        }
        [Command("delhouseowner")]
        public static void CMD_deleteHouseOwner(Player player)
        {
            if (!Globals.Group.CanUseCmd(player, "delhouseowner")) return;
            if (!player.HasData("HOUSEID"))
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.SetOwner(null);
            house.UpdateLabel();
            house.Save();
            Loggings.Admin($"{player.Name}", $"delHouseOwner({house.ID})", $"");
        }
        [Command("stt")]
        public static void CMD_SetTurboTorque(Player player, float power, float torque)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "stt")) return;
                if (!player.IsInVehicle) return;
                Plugins.Trigger.ClientEvent(player, "svem", power, torque);
            }
            catch (Exception e)
            {
                Log.Write("Error at \"STT\":" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("svm")]
        public static void CMD_SetVehicleMod(Player player, int type, int index)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetMod(type, index);

            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVM\":" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("svn")]
        public static void CMD_SetVehicleNeon(Player player, byte r, byte g, byte b, byte alpha)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (alpha != 0)
                {
                    NAPI.Vehicle.SetVehicleNeonState(v, true);
                    NAPI.Vehicle.SetVehicleNeonColor(v, r, g, b);
                }
                else
                {
                    NAPI.Vehicle.SetVehicleNeonColor(v, 255, 255, 255);
                    NAPI.Vehicle.SetVehicleNeonState(v, false);
                }
            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("svhid")]
        public static void CMD_SetVehicleHeadlightColor(Player player, int hlcolor)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (hlcolor >= 0 && hlcolor <= 12)
                {
                    v.SetSharedData("hlcolor", hlcolor);
                    Plugins.Trigger.ClientEventInRange(v.Position, 250f, "VehStream_SetVehicleHeadLightColor", v.Handle, hlcolor);
                }
                else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Цвет фар может быть от 0 до 12.", 3000);
            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [Command("svh")]
        public static void CMD_SetVehicleHealth(Player player, int health = 100)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "svh")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                v.Repair();
                v.Health = health;

            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVH\":" + e.ToString(), Plugins.Logs.Type.Error);
            }

        }
        [Command("payday")]
        public static void payDay(Player player, string text = null)
        {
            if (!Globals.Group.CanUseCmd(player, "payday")) return;
            Loggings.Admin($"{player.Name}", $"payDay", "");
            Main.payDayTrigger();
        }
        [Command("giveitem")]
        public static void CMD_giveItem(Player player, int id, int itemType, int amount, string data)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "giveitem"))
                {
                    return;
                }

                var target = Main.GetPlayerByID(id);

                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден!", 3000);
                    return;
                }

                if (itemType == 12)
                {
                    int.TryParse(data, out int parsedData);

                    if (parsedData > 100)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Ты хочешь слишком многого", 3000);

                        return;
                    }
                }

                nInventory.Add(player, new nItem((ItemType)itemType, amount, data));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"У вас есть {amount} {nInventory.ItemsNames[itemType]} ", 3000);
            }
            catch { }
        }
        [Command("setleader")]
        public static void CMD_setLeader(Player player, int id, int fracid)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setFracLeader(player, Main.GetPlayerByID(id), fracid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }

        [Command("lsn", GreedyArg = true)]
        public static void CMD_adminLSnewsChat(Player player, string message)
        {
            try
            {
                Admin.adminLSnews(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("tpcar")]
        public static void CMD_teleportToMeWithCar(Player player, int id)
        {
            try
            {
                Player Target = Main.GetPlayerByID(id);

                if (Target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Target.IsInVehicle)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не в автомобиле", 3000);
                    return;
                }

                Admin.teleportTargetToPlayerWithCar(player, Target);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }

        [Command("guns")]
        public static void CMD_adminGuns(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetGun(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("giveclothes")]
        public static void CMD_adminClothes(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetClothes(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("setskin")]
        public static void CMD_adminSetSkin(Player player, int id, string pedModel)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetSkin(player, Main.GetPlayerByID(id), pedModel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("oguns")]
        public static void CMD_adminOGuns(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.takeTargetGun(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }

        [Command("deljob")]
        public static void CMD_deljob(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.delJob(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("pos")]
        public void HandlePos(Player c)
        {

            Vector3 pos = c.Position;
            Vector3 rot = c.Rotation;

            c.SendChatMessage("---------------");
            c.SendChatMessage("Position");
            c.SendChatMessage("Pos: " + pos.X + "| " + pos.Y + "| " + pos.Z);
            c.SendChatMessage("Rotation");
            c.SendChatMessage("Z: " + rot.Z);
        }
        [Command("dim")]
        public void HandleTp(Player c, uint d)
        {
            c.Dimension = d;
        }
        [Command("mtp2")]
        public void HandleTp(Player c, double x, double y, double z)
        {
            c.Position = new Vector3(x, y, z);
        }
        [Command("aclear")]
        public static void ACMD_aclear(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Globals.Group.CanUseCmd(player, "aclear")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок не найден", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Невозможно очистить персонажа, который находится в игре", 3000);
                    return;
                }
                string[] split = target.Split('_');
                int tuuid = 0;
                // CLEAR BIZ
                DataTable result = Database.QueryRead($"SELECT uuid,adminlvl,biz FROM `characters` WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRow row = result.Rows[0];
                    if (Convert.ToInt32(row["adminlvl"]) >= Main.Players[player].AdminLVL)
                    {
                        Controller.SendToAdmins(3, $"!{{#d35400}}[ACLEAR-DENIED] {player.Name} ({player.Value}) попытался очистить {target} (offline), который имеет выше уровень администратора.");
                        return;
                    }
                    tuuid = Convert.ToInt32(row["uuid"]);
                    List<int> TBiz = JsonConvert.DeserializeObject<List<int>>(row["biz"].ToString());
                    if (TBiz.Count >= 1 && TBiz[0] >= 1)
                    {
                        var biz = BusinessManager.BizList[TBiz[0]];
                        var owner = biz.Owner;
                        var ownerplayer = NAPI.Player.GetPlayerFromName(owner);

                        if (ownerplayer != null && Main.Players.ContainsKey(player))
                        {
                            Plugins.Notice.Send(ownerplayer, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Администратор отобрал у Вас бизнес", 3000);
                            Finance.Wallet.Change(ownerplayer, Convert.ToInt32(biz.SellPrice * 0.8));
                            Main.Players[ownerplayer].BizIDs.Remove(biz.ID);
                        }
                        else
                        {
                            var split1 = biz.Owner.Split('_');
                            var data = Database.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                            List<int> ownerBizs = new List<int>();
                            var money = 0;

                            foreach (DataRow Row in data.Rows)
                            {
                                ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                money = Convert.ToInt32(Row["money"]);
                            }

                            ownerBizs.Remove(biz.ID);
                            Database.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                        }

                        Finance.Bank.Accounts[biz.BankID].Balance = 0;
                        biz.Owner = "Государство";
                        biz.UpdateLabel();
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отобрали бизнес у {owner}", 3000);
                    }
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Не удалось найти персонажа в базе данных", 3000);
                    return;
                }
                // CLEAR HOUSE
                result = Database.QueryRead($"SELECT id FROM `houses` WHERE `owner`='{target}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRow row = result.Rows[0];
                    Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == Convert.ToInt32(row[0]));
                    if (house != null)
                    {
                        house.SetOwner(null);
                        house.UpdateLabel();
                        house.Save();
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отобрали дом у {target}", 3000);
                    }
                }
                // CLEAR VEHICLES
                result = Database.QueryRead($"SELECT `number` FROM `vehicles` WHERE `holder`='{target}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRowCollection rows = result.Rows;
                    foreach (DataRow row in rows)
                    {
                        VehicleManager.Remove(row[0].ToString());
                    }
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы отобрали у {target} все машины.", 3000);
                }

                // CLEAR MONEY, HOTEL, FRACTION, SIMCARD, PET
                Database.Query($"UPDATE `characters` SET `money`=0,`fraction`=0,`fractionlvl`=0,`hotel`=-1,`hotelleft`=0,`sim`=-1, WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                // CLEAR BANK MONEY
                Bank.Data bankAcc = Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target).Value;
                if (bankAcc != null)
                {
                    bankAcc.Balance = 0;
                    Database.Query($"UPDATE `money` SET `balance`=0 WHERE `holder`='{target}'");
                }
                // CLEAR ITEMS
                if (tuuid != 0) Database.Query($"UPDATE `inventory` SET `items`='[]' WHERE `uuid`={tuuid}");
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы забрали у игрока все вещи, деньги с рук и банковского счёта у {target}", 3000);
                Loggings.Admin($"{player.Name}", $"aClear", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT aclear\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("vehcustomscolor")]
        public static void CMD_ApplyCustomSColor(Player client, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;
                Color color = new Color(r, g, b);
                var number = client.Vehicle.NumberPlate;
                VehicleManager.Vehicles[number].Components.SecColor = color;
                VehicleManager.Vehicles[number].Components.SecModColor = mod;
                VehicleManager.ApplyCustomization(client.Vehicle);

            }
            catch { }
        }
        [Command("findbyveh")]
        public static void CMD_FindByVeh(Player player, string number)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Globals.Group.CanUseCmd(player, "findbyveh")) return;
            if (number.Length > 8)
            {
                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Количество символов в номерном знаке не может превышать 8.", 3000);
                return;
            }
            if (VehicleManager.Vehicles.ContainsKey(number)) Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Номер машины: {number} | Модель: {VehicleManager.Vehicles[number].Model} | Владелец: {VehicleManager.Vehicles[number].Holder}", 6000);
            else Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Не найдено машины с таким номерным знаком.", 3000);
        }
        [Command("vehcustom")]
        public static void CMD_ApplyCustom(Player client, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Globals.Group.CanUseCmd(client, "setvehdirt")) return;

                if (!client.IsInVehicle) return;

                if (cat < 0)
                {
                    Globals.VehicleManager.ApplyCustomization(client.Vehicle);
                    return;
                }

                var number = client.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        var turbo = (VehicleManager.Vehicles[number].Components.Turbo == 0);
                        client.Vehicle.SetSharedData("TURBO", turbo);
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.NeonColor.Red = id;
                        break;
                    case 20:
                        VehicleManager.Vehicles[number].Components.NeonColor.Green = id;
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.NeonColor.Blue = id;
                        break;
                    case 22:
                        VehicleManager.Vehicles[number].Components.NeonColor.Alpha = id;
                        break;
                    case 23:
                        VehicleManager.Vehicles[number].Components.WheelsType = id;
                        break;
                    case 24:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        break;
                    case 25:
                        VehicleManager.Vehicles[number].Components.WheelsColor = id;
                        break;
                }

                Globals.VehicleManager.ApplyCustomization(client.Vehicle);
            }
            catch { }
        }
        [Command("st")]
        public static void CMD_setTime(Player player, int hours, int minutes, int seconds)
        {
            if (!Globals.Group.CanUseCmd(player, "st")) return;
            NAPI.World.SetTime(hours, minutes, seconds);
        }
        [Command("tp")]
        public static void CMD_teleport(Player player, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "tp")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(player, target.Position + new Vector3(1, 0, 1.5));
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("goto")]
        public static void CMD_teleportveh(Player player, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "tp")) return;
                if (!player.IsInVehicle) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                NAPI.Entity.SetEntityDimension(player.Vehicle, NAPI.Entity.GetEntityDimension(target));
                NAPI.Entity.SetEntityPosition(player.Vehicle, target.Position + new Vector3(2, 2, 2));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("flip")]
        public static void CMD_flipveh(Player player, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "tp")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (!target.IsInVehicle)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не находится в машине", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(target.Vehicle, target.Vehicle.Position + new Vector3(0, 0, 2.5f));
                NAPI.Entity.SetEntityRotation(target.Vehicle, new Vector3(0, 0, target.Vehicle.Rotation.Z));
                Loggings.Admin($"{player.Name}", $"flipVeh", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("mtp")]
        public static void CMD_maskTeleport(Player player, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "mtp")) return;

                if (!Main.MaskIds.ContainsKey(id))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Маска с таким ID не найдена", 3000);
                    return;
                }
                var target = Main.MaskIds[id];

                NAPI.Entity.SetEntityPosition(player, target.Position);
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("removesafe")]
        public static void CMD_removeSafe(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    SafeMain.CMD_RemoveSafe(player);
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
            });
        }
        [Command("offjail", GreedyArg = true)]
        public static void CMD_offlineJailTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "offjail")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок не найден", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.sendPlayerToDemorgan(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрок был онлайн, поэтому offjail заменён на demorgan", 3000);
                    return;
                }

                var firstTime = time * 60;
                var deTimeMsg = "м";
                if (time > 60)
                {
                    deTimeMsg = "ч";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = "д";
                        time /= 24;
                    }
                }

                var split = target.Split('_');
                Database.QueryRead($"UPDATE `characters` SET `demorgan`={firstTime},`arrest`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} посадил игрока {target} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
                Loggings.Admin($"{player.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("offwarn", GreedyArg = true)]
        public static void CMD_offlineWarnTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "offwarn")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок не найден", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.warnPlayer(player, NAPI.Player.GetPlayerFromName(target), reason);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Игрок был онлайн, поэтому offwarn был заменён на warn", 3000);
                    return;
                }
                else
                {
                    string[] split1 = target.Split('_');
                    DataTable result = Database.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                    DataRow row = result.Rows[0];
                    int targetadminlvl = Convert.ToInt32(row[0]);
                    if (targetadminlvl >= Main.Players[player].AdminLVL)
                    {
                        Controller.SendToAdmins(3, $"!{{#d35400}}[OFFWARN-DENIED] {player.Name} ({player.Value}) попытался забанить {target} (offline), который имеет выше уровень администратора.");
                        return;
                    }
                }


                var split = target.Split('_');
                var data = Database.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = Convert.ToInt32(data.Rows[0]["warns"]);
                warns++;

                if (warns >= 3)
                {
                    Database.Query($"UPDATE `characters` SET `warns`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    Modules.BanSystem.Offline(target, DateTime.Now.AddMinutes(43200), false, "Warns 3/3", "Server_Serverniy");
                }
                else
                    Database.Query($"UPDATE `characters` SET `unwarn`='{Database.ConvertTime(DateTime.Now.AddDays(14))}',`warns`={warns},`fraction`=0,`fractionlvl`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");

                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал предупреждение игроку {target} ({warns}/3 | {reason})");
                Loggings.Admin($"{player.Name}", $"warn({time},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ban", GreedyArg = true)]
        public static void CMD_banTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("hardban", GreedyArg = true)]
        public static void CMD_hardbanTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.hardbanPlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("offban", GreedyArg = true)]
        public static void CMD_offlineBanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offBanPlayer(player, name, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("offhardban", GreedyArg = true)]
        public static void CMD_offlineHardbanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offHardBanPlayer(player, name, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("unban", GreedyArg = true)]
        public static void CMD_unbanTarget(Player player, string name)
        {
            if (!Globals.Group.CanUseCmd(player, "ban")) return;
            try
            {
                Admin.unbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("unhardban", GreedyArg = true)]
        public static void CMD_unhardbanTarget(Player player, string name)
        {
            if (!Globals.Group.CanUseCmd(player, "ban")) return;
            try
            {
                Admin.unhardbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("mute", GreedyArg = true)]
        public static void CMD_muteTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.mutePlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("offmute", GreedyArg = true)]
        public static void CMD_offlineMuteTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок не найден", 3000);
                    return;
                }
                Admin.OffMutePlayer(player, target, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("unmute")]
        public static void CMD_muteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.unmutePlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("vmute", GreedyArg = true)]
        public static void CMD_voiceMuteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Globals.Group.CanUseCmd(player, "mute")) return;
                player.SetSharedData("voice.muted", true);
                Plugins.Trigger.ClientEvent(player, "voice.mute");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("vunmute")]
        public static void CMD_voiceUnMuteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Globals.Group.CanUseCmd(player, "unmute")) return;
                player.SetSharedData("voice.muted", false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("sban", GreedyArg = true)]
        public static void CMD_silenceBan(Player player, int id, int time)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, "", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("kick", GreedyArg = true)]
        public static void CMD_kick(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("skick")]
        public static void CMD_silenceKick(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), "Silence kick", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("gm")]
        public static void CMD_checkGamemode(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkGamemode(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("agm")]
        public static void CMD_enableGodmode(Player player)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "agm")) return;
                if (!player.HasSharedData("AGM"))
                {
                    Plugins.Trigger.ClientEvent(player, "AGM", true);
                    player.SetSharedData("AGM", true);
                }
                else
                {
                    Plugins.Trigger.ClientEvent(player, "AGM", false);
                    player.ResetSharedData("AGM");
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("warn", GreedyArg = true)]
        public static void CMD_warnTarget(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.warnPlayer(player, Main.GetPlayerByID(id), reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("asms", GreedyArg = true)]
        public static void CMD_adminSMS(Player player, int id, string msg)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.adminSMS(player, Main.GetPlayerByID(id), msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ans", GreedyArg = true)]
        public static void CMD_answer(Player player, int id, string answer)
        {
            try
            {
                var sender = Main.GetPlayerByID(id);
                if (sender == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.answerReport(player, sender, answer);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("setvip")]
        public static void CMD_setVip(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setPlayerVipLvl(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("checkmoney")]
        public static void CMD_checkMoney(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkMoney(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("leave")]
        public static void CMD_leaveFraction(Player player)
        {
            try
            {
                if (Main.Accounts[player].VipLvl == 0) return;

                Fractions.Manager.UNLoad(player);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == player.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                Main.Players[player].FractionID = 0;
                Main.Players[player].FractionLVL = 0;

                Customization.ApplyCharacter(player);
                if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                player.SetData("ON_DUTY", false);
                NAPI.Player.RemoveAllPlayerWeapons(player);

                Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Вы покинули организацию", 3000);
                return;
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("testnotify", GreedyArg = true)]
        public static void CMD_testnotify(Player player, int id, int sum, string reason)
        {
            Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Уведомление Success", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Уведомление Error", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"Уведомление Alert", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Уведомление Info", 3000);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Уведомление Info", 3000);
        }
        [Command("ticket", GreedyArg = true)]
        public static void CMD_govTicket(Player player, int id, int sum, string reason)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (sum < 1) return;
                if (target == null || !Main.Players.ContainsKey(target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.ticketToTarget(player, target, sum, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("respawn")]
        public static void CMD_respawnFracCars(Player player)
        {
            try
            {
                Fractions.FractionCommands.respawnFractionCars(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("givemedlic")]
        public static void CMD_givemedlic(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.giveMedicalLic(player, target);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("sellbiz")]
        public static void CMD_sellBiz(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                BusinessManager.sellBusinessCommand(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("password")]
        public static void CMD_ResetPassword(Player player, string new_password)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Main.Accounts[player].changePassword(new_password);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, "Вы сменили пароль! Перезайдите с новым.", 3000);
        }
        [Command("time")]
        public static void CMD_checkPrisonTime(Player player)
        {
            try
            {
                if (Main.Players[player].ArrestTime != 0)
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[player].DemorganTime != 0)
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ptime")]
        public static void CMD_pcheckPrisonTime(Player player, int id)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "a")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[target].DemorganTime != 0)
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("sellcars")]
        public static void CMD_sellCars(Player player)
        {
            try
            {
                Houses.HouseManager.OpenCarsSellMenu(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("dep", GreedyArg = true)]
        public static void CMD_govFracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.govFractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("gov", GreedyArg = true)]
        public static void CMD_gov(Player player, string msg)
        {
            try
            {
                if (!Fractions.Manager.canUseCommand(player, "gov")) return;
                int frac = Main.Players[player].FractionID;
                int lvl = Main.Players[player].FractionLVL;
                string[] split = player.Name.Split('_');

                NAPI.Chat.SendChatMessageToAll($"~y~[{Fractions.Manager.GovTags[frac]} | {split[0]} {split[1]}] {msg}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("call", GreedyArg = true)]
        public static void CMD_gov(Player player, int number, string msg)
        {
            try
            {
                if (number == 112)
                    Fractions.Realm.Police.callPolice(player, msg);
                else if (number == 911)
                    Fractions.Realm.Ems.callEms(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("q")]
        public static void CMD_disconnect(Player player)
        {
            Plugins.Trigger.ClientEvent(player, "quitcmd");
        }
        [Command("report", GreedyArg = true)]
        public static void CMD_report(Player player, string message)
        {
            try
            {
                if (message.Length > 150)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Слишком длинное сообщение", 3000);
                    return;
                }
                if (Main.Accounts[player].VipLvl == 0 && player.HasData("NEXT_REPORT"))
                {
                    DateTime nextReport = player.GetData<DateTime>("NEXT_REPORT");
                    if (DateTime.Now < nextReport)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Попробуйте отправить жалобу через некоторое время", 3000);
                        return;
                    }
                }
                if (player.HasData("MUTE_TIMER"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Пока вы заблокированы/отключены, вы не можете подавать жалобу.", 3000);
                    return;
                }
                ReportSys.AddReport(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("givearmylic")]
        public static void CMD_GiveArmyLicense(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (!Fractions.Manager.canUseCommand(player, "givearmylic")) return;

                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко от Вас", 3000);
                    return;
                }

                if (Main.Players[target].Licenses[8])
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"У игрока уже есть военный билет", 3000);
                    return;
                }

                Main.Players[target].Licenses[8] = true;
                Dashboard.sendStats(target);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выдали игроку ({target.Value}) военный билет", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) выдал вам военный билет", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("takegunlic")]
        public static void CMD_takegunlic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.takeGunLic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("givegunlic")]
        public static void CMD_givegunlic(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.giveGunLic(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("pd")]
        public static void CMD_policeAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Realm.Police.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("eject")]
        public static void CMD_ejectTarget(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (!player.IsInVehicle || player.VehicleSeat != -1)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы не находитесь в машине или не на пассажирском месте", 3000);
                    return;
                }
                if (!target.IsInVehicle || player.Vehicle != target.Vehicle) return;
                VehicleManager.WarpPlayerOutOfVehicle(target);

                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выкинули игрока ({target.Value}) из машины", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({player.Value}) выкинул Вас из машины", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ems")]
        public static void CMD_emsAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Realm.Ems.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("pocket")]
        public static void CMD_pocketTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (player.Position.DistanceTo(Main.GetPlayerByID(id).Position) > 2)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок слишком далеко", 3000);
                    return;
                }

                Fractions.FractionCommands.playerChangePocket(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("buybiz")]
        public static void CMD_buyBiz(Player player)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;

                BusinessManager.buyBusinessCommand(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("setrank")]
        public static void CMD_setRank(Player player, int id, int newrank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрока с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.SetFracRank(player, Main.GetPlayerByID(id), newrank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("invite")]
        public static void CMD_inviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.InviteToFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("uninvite")]
        public static void CMD_uninviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.UnInviteFromFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("f", GreedyArg = true)]
        public static void CMD_fracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.fractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("arrest")]
        public static void CMD_arrest(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.arrestTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("rfp")]
        public static void CMD_rfp(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.releasePlayerFromPrison(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("follow")]
        public static void CMD_follow(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.targetFollowPlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("unfollow")]
        public static void CMD_unfollow(Player player)
        {
            try
            {
                Fractions.FractionCommands.targetUnFollowPlayer(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("su", GreedyArg = true)]
        public static void CMD_suByPassport(Player player, int pass, int stars, string reason)
        {
            try
            {
                Fractions.FractionCommands.suPlayer(player, pass, stars, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("c")]
        public static void CMD_getCoords(Player player)
        {
            try
            {
                if (!Globals.Group.CanUseCmd(player, "a")) return;
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords", NAPI.Entity.GetEntityPosition(player).ToString());
                NAPI.Chat.SendChatMessageToPlayer(player, "Rot", NAPI.Entity.GetEntityRotation(player).ToString());
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("incar")]
        public static void CMD_inCar(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerInCar(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("pull")]
        public static void CMD_pullOut(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerOutCar(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("warg")]
        public static void CMD_warg(Player player)
        {
            try
            {
                Fractions.FractionCommands.setWargPoliceMode(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("medkit")]
        public static void CMD_medkit(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.sellMedKitToTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("accept")]
        public static void CMD_accept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.acceptEMScall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("heal")]
        public static void CMD_heal(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.healTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("capture")]
        public static void CMD_capture(Player player)
        {
            try
            {
                Fractions.GangsCapture.CMD_startCapture(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("repair")]
        public static void CMD_mechanicRepair(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Working.AutoMechanic.mechanicRepair(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("sellfuel")]
        public static void CMD_mechanicSellFuel(Player player, int id, int fuel, int pricePerLitr)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Working.AutoMechanic.mechanicFuel(player, Main.GetPlayerByID(id), fuel, pricePerLitr);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("buyfuel")]
        public static void CMD_mechanicBuyFuel(Player player, int fuel)
        {
            try
            {
                Working.AutoMechanic.buyFuel(player, fuel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ma")]
        public static void CMD_acceptMechanic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Working.AutoMechanic.acceptMechanic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("cmechanic")]
        public static void CMD_cancelMechanic(Player player)
        {
            try
            {
                Working.AutoMechanic.cancelMechanic(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("tprice")]
        public static void CMD_tprice(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Working.Taxi.offerTaxiPay(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ta")]
        public static void CMD_taxiAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Working.Taxi.acceptTaxi(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("ctaxi")]
        public static void CMD_cancelTaxi(Player player)
        {
            try
            {
                Working.Taxi.cancelTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("taxi")]
        public static void CMD_callTaxi(Player player)
        {
            try
            {
                Working.Taxi.callTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("orders")]
        public static void CMD_orders(Player player)
        {
            try
            {
                Working.Truckers.truckerOrders(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
        [Command("addfence")]
        public static void CMD_bc(Player player)
        {
            try
            {
                if (Main.Players[player].FractionID != 7) return;
                if (Main.Players[player].FractionLVL < 5) return;
                if (player.Dimension != 0) return;

                if (player.HasData("PDOBJECT"))
                {
                    var beacon = NAPI.Data.GetEntityData(player, "PDOBJECT");
                    try
                    {
                        NAPI.Entity.DeleteEntity(beacon);
                        NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("PDOBJECTSHAPE"));
                        NAPI.Data.ResetEntityData(player, "PDOBJECT");
                    }
                    catch
                    {

                    }
                }
                else
                {
                    var beacon = NAPI.Object.CreateObject(10928689, player.Position - new Vector3(0, 0, 1.0), new Vector3(0, 0, 0), 255, 0);
                    var beaconShape = NAPI.ColShape.CreateCylinderColShape(player.Position - new Vector3(0, 0, 1.0), 1, 2, 0);
                    beaconShape.OnEntityEnterColShape += (s, e) =>
                    {
                        if (!Main.Players.ContainsKey(e)) return;
                        e.SetData("PDOBJECT", beacon);
                        e.SetData("PDOBJECTSHAPE", beaconShape);
                    };
                    beaconShape.OnEntityExitColShape += (s, e) =>
                    {
                        if (!Main.Players.ContainsKey(e)) return;
                        e.ResetData("PDOBJECT");
                    };
                }
            }
            catch
            {

            }
        }
        [Command("roll")]
        public static void rollDice(Player player, int id, int money)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Человек с таким ID не найден", 3000);
                    return;
                }

                if (money <= 0)
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Денежная ценность должна быть выше 0", 3000);
                    return;
                }


                // Send request for a game
                Player target = Main.GetPlayerByID(id);
                target.SetData("DICE_PLAYER", player);
                target.SetData("DICE_VALUE", money);
                Plugins.Trigger.ClientEvent(target, "openDialog", "DICE", $"Шпилер ({player.Value}) хочет сыграть с вами в бросок костей на {money}$. Вы принимаете?");

                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игровой запрос на кости был отправлен на ({target.Value}) за ${money}$.", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Plugins.Logs.Type.Error); }
        }
    }
}
