using GTANetworkAPI;
using System;
using System.Collections.Generic;
using iTeffa.Globals;
using iTeffa.Settings;
using System.Linq;
using Newtonsoft.Json;

namespace iTeffa.Finance
{
    class Branch : Script
    {
        private static readonly Nlogs Log = new Nlogs("Bank branches");
        public static Dictionary<int, ColShape> BRANCHCols = new Dictionary<int, ColShape>();
        #region Branches List
        public static List<Vector3> BRANCHs = new List<Vector3>
        {
            new Vector3(313.6169, -278.5236, 54.17078),
            new Vector3(149.3, -1040.227, 29.37408),
            new Vector3(-1213.453, -330.7448, 37.78702),
        };
        #endregion
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                Log.Write("Loading BRANCHs...");
                for (int i = 0; i < BRANCHs.Count; i++)
                {
                    if (i != 58) NAPI.Blip.CreateBlip(207, BRANCHs[i], 0.85f, 25, "iTeffa Банк", shortRange: true, dimension: 0);

                    BRANCHCols.Add(i, NAPI.ColShape.CreateCylinderColShape(BRANCHs[i], 1, 2, 0));
                    BRANCHCols[i].SetData("NUMBER", i);
                    BRANCHCols[i].OnEntityEnterColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 506);
                            Working.Collector.CollectorEnterATM(e, s);
                        }
                        catch (Exception ex) { Log.Write("BRANCHCols.OnEntityEnterColShape: " + ex.Message, Nlogs.Type.Error); }
                    };
                    BRANCHCols[i].OnEntityExitColShape += (s, e) => {
                        try
                        {
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("BRANCHCols.OnEntityExitrColShape: " + ex.Message, Nlogs.Type.Error); }
                    };
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Nlogs.Type.Error); }
        }
        public static Vector3 GetNearestBRANCH(Player player)
        {
            Vector3 nearesetBRANCH = BRANCHs[0];
            foreach (var v in BRANCHs)
            {
                if (v == new Vector3(237.3785, 217.7914, 106.2868)) continue;
                if (player.Position.DistanceTo(v) < player.Position.DistanceTo(nearesetBRANCH))
                    nearesetBRANCH = v;
            }
            return nearesetBRANCH;
        }
        public static void OpenBRANCH(Player player)
        {
            var acc = Main.Players[player];
            if (acc.Bank == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Зарегистрируйте счет в ближайшем отделении банка", 3000);
                return;
            }
            long balance = Bank.Accounts[acc.Bank].Balance;
            Trigger.ClientEvent(player, "setbranch", acc.Bank.ToString(), player.Name, balance.ToString(), "");
            Trigger.ClientEvent(player, "openbranch");
            return;
        }
        public static void BranchBizGen(Player player)
        {
            var acc = Main.Players[player];
            Log.Debug("Biz count : " + acc.BizIDs.Count);
            if (acc.BizIDs.Count > 0)
            {
                List<string> data = new List<string>();
                foreach (int key in acc.BizIDs)
                {
                    Business biz = BusinessManager.BizList[key];
                    string name = BusinessManager.BusinessTypeNames[biz.Type];
                    data.Add($"{name}");
                }
                Trigger.ClientEvent(player, "branchOpenBiz", JsonConvert.SerializeObject(data), "");
            }
            else
            {
                Trigger.ClientEvent(player, "branchOpen", "[1,0,0]");
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет бизнеса!", 3000);
            }
        }
        [RemoteEvent("branchVal")]
        public static void ClientEvent_BRANCHVAL(Player player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                var acc = Main.Players[player];
                int type = NAPI.Data.GetEntityData(player, "BRANCHTYPE");
                string data = Convert.ToString(args[0]);
                if (!int.TryParse(data, out int amount)) return;
                switch (type)
                {
                    case 0:
                        Trigger.ClientEvent(player, "branchClose");
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, +Math.Abs(amount));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"branchIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1:
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, +Math.Abs(amount));
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"branchOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2:
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"branchHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player,
                                "branchOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                        break;
                    case 3:
                        int bid = NAPI.Data.GetEntityData(player, "BRANCHBIZ");

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, +Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"branchBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Успешный перевод.", 3000);
                        Trigger.ClientEvent(player, "branchOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        break;
                    case 4:
                        if (!Bank.Accounts.ContainsKey(amount) || amount <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closebranch");
                            return;
                        }
                        NAPI.Data.SetEntityData(player, "BRANCH2ACC", amount);
                        Trigger.ClientEvent(player,
                                "branchOpen", "[2,0,'Сумма для перевода']");
                        NAPI.Data.SetEntityData(player, "BRANCHTYPE", 44);
                        break;
                    case 44:
                        if (Main.Players[player].LVL < 5)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Перевод денег доступен после первого уровня", 3000);
                            return;
                        }
                        if (player.HasData("NEXT_BANK_TRANSFER") && DateTime.Now < player.GetData<DateTime>("NEXT_BANK_TRANSFER"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Следующая транзакция будет возможна в течение минуты", 3000);
                            return;
                        }
                        int bank = NAPI.Data.GetEntityData(player, "BRANCH2ACC");
                        if (!Bank.Accounts.ContainsKey(bank) || bank <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closebranch");
                            return;
                        }
                        if (Bank.Accounts[bank].Type != 1 && Main.Players[player].AdminLVL == 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Счет не найден!", 3000);
                            Trigger.ClientEvent(player, "closebranch");
                            return;
                        }
                        if (acc.Bank == bank)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Операция отменена.", 3000);
                            Trigger.ClientEvent(player, "closebranch");
                            return;
                        }
                        Bank.Transfer(acc.Bank, bank, Math.Abs(amount));
                        Trigger.ClientEvent(player, "closebranch");
                        if (Main.Players[player].AdminLVL == 0) player.SetData("NEXT_BANK_TRANSFER", DateTime.Now.AddMinutes(1));
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Nlogs.Type.Error);
            }
        }
        [RemoteEvent("branchDP")]
        public static void ClientEvent_BRANCHDupe(Player client)
        {
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 3)
                {
                    p.SendChatMessage($"!{{#d35400}}[BRANCH-FLOOD] {client.Name} ({client.Value})");
                }
            }
        }
        [RemoteEvent("branchCB")]
        public static void ClientEvent_BRANCHCB(Player player, params object[] args)
        {
            try
            {
                var acc = Main.Players[player];
                int type = Convert.ToInt32(args[0]);
                int index = Convert.ToInt32(args[1]);
                if (index == -1)
                {
                    Trigger.ClientEvent(player, "closebranch");
                    return;
                }
                switch (type)
                {
                    case 1:
                        switch (index)
                        {
                            case 0:
                                Trigger.ClientEvent(player,
                                    "branchOpen", "[2,0,'Сумма внесения наличных']");
                                NAPI.Data.SetEntityData(player, "BRANCHTYPE", index);
                                break;
                            case 1:
                                Trigger.ClientEvent(player,
                                    "branchOpen", "[2,0,'Сумма для снятия']");
                                NAPI.Data.SetEntityData(player, "BRANCHTYPE", index);
                                break;
                            case 2:
                                if (Houses.HouseManager.GetHouse(player, true) == null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет дома!", 3000);
                                    return;
                                }
                                var house = Houses.HouseManager.GetHouse(player, true);
                                Trigger.ClientEvent(player,
                                    "branchOpen", $"[2,'{Bank.Accounts[house.BankID].Balance}/{Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7}$','Сумма внесения наличных']");
                                Trigger.ClientEvent(player, "setbranch", "Дом", $"Дом #{house.ID}", Bank.Accounts[house.BankID].Balance, "");
                                NAPI.Data.SetEntityData(player, "BRANCHTYPE", index);
                                break;
                            case 3:
                                BranchBizGen(player);
                                NAPI.Data.SetEntityData(player, "BRANCHTYPE", index);
                                break;
                            case 4:
                                Trigger.ClientEvent(player,
                                    "branchOpen", "[2,0,'Счет зачисления']");
                                NAPI.Data.SetEntityData(player, "BRANCHTYPE", index);
                                break;

                        }
                        break;
                    case 2:
                        Trigger.ClientEvent(player, "branchOpen", "[1,0,0]");
                        Trigger.ClientEvent(player, "setbranch", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                        break;
                    case 3:
                        if (index == -1)
                        {
                            Trigger.ClientEvent(player, "branchOpen", "[1,0,0]");
                            Trigger.ClientEvent(player, "setbranch", acc.Bank, player.Name, Bank.Accounts[acc.Bank].Balance, "");
                            return;
                        }
                        Business biz = BusinessManager.BizList[acc.BizIDs[index]];
                        NAPI.Data.SetEntityData(player, "BRANCHBIZ", index);
                        Trigger.ClientEvent(player, "branchOpen", $"[2,'{Bank.Accounts[biz.BankID].Balance}/{Convert.ToInt32(biz.SellPrice / 100 * 0.013) * 24 * 7}$','Сумма зачисления']");
                        Trigger.ClientEvent(player, "setbranch",
                            "Бизнес",
                            BusinessManager.BusinessTypeNames[biz.Type],
                            Bank.Accounts[biz.BankID].Balance, "");
                        break;

                }
            }
            catch (Exception e) { Log.Write("branchCB: " + e.Message, Nlogs.Type.Error); }
        }
        [RemoteEvent("branch")]
        public static void ClientEvent_BRANCH(Player player, params object[] args)
        {
            try
            {
                if (Admin.IsServerStoping)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Сервер сейчас не может принять это действие", 3000);
                    return;
                }
                int act = Convert.ToInt32(args[0]);
                string data1 = Convert.ToString(args[1]);
                var acc = Main.Players[player];
                if (!int.TryParse(data1, out int amount)) return;
                Log.Debug($"{player.Name} : {data1}");
                switch (act)
                {
                    case 0:
                        if (Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Bank.Change(acc.Bank, amount);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({acc.Bank})", Math.Abs(amount), $"branchIn");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 1:
                        if (Bank.Change(acc.Bank, -Math.Abs(amount)))
                        {
                            Wallet.Change(player, amount);
                            GameLog.Money($"bank({acc.Bank})", $"player({Main.Players[player].UUID})", Math.Abs(amount), $"branchOut");
                            Trigger.ClientEvent(player, "setbank", Bank.Accounts[acc.Bank].Balance.ToString(), "");
                        }
                        break;
                    case 2:
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null) return;

                        var maxMoney = Convert.ToInt32(house.Price / 100 * 0.013) * 24 * 7;
                        if (Bank.Accounts[house.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(house.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({house.BankID})", Math.Abs(amount), $"branchHouse");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Успешный перевод.", 3000);
                        break;
                    case 3:
                        var check = NAPI.Data.GetEntityData(player, "bizcheck");
                        if (check == null) return;
                        if (acc.BizIDs.Count != check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }
                        int bid = 0;
                        if (!int.TryParse(Convert.ToString(args[2]), out bid))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Возникла ошибка! Попробуйте еще раз.", 3000);
                            return;
                        }

                        Business biz = BusinessManager.BizList[acc.BizIDs[bid]];

                        maxMoney = Convert.ToInt32(biz.SellPrice / 100 * 0.01) * 24 * 7;
                        if (Bank.Accounts[biz.BankID].Balance + Math.Abs(amount) > maxMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                            return;
                        }
                        if (!Wallet.Change(player, -Math.Abs(amount)))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств.", 3000);
                            return;
                        }
                        Bank.Change(biz.BankID, Math.Abs(amount));
                        GameLog.Money($"player({Main.Players[player].UUID})", $"bank({biz.BankID})", Math.Abs(amount), $"branchBiz");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Успешный перевод.", 3000);
                        break;
                    case 4:
                        int num = 0;
                        if (!int.TryParse(Convert.ToString(args[2]), out num))
                            return;
                        Bank.Transfer(acc.Bank, num, +Math.Abs(amount));
                        break;
                }
            }
            catch (Exception e) { Log.Write("branch: " + e.Message, Nlogs.Type.Error); }
        }
    }
}
