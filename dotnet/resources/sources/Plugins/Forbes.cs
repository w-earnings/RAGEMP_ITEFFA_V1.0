using GTANetworkAPI;
using iTeffa.Globals;
using iTeffa.Interface;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace iTeffa.Plugins
{
    class Forbes : Script
    {
        private static readonly Nlogs Log = new Nlogs("Forbes");
        private static readonly bool SeeAdmins = true;
        private static readonly int AdminLVLToSee = 0;
        private static readonly int MaxForbes = 30;

        private static Dictionary<string, int> Majors = new Dictionary<string, int>();

        public static void SyncMajors()
        {
            NAPI.Task.Run(() =>
            {
                try
                {

                    var database = Database.QueryRead($"SELECT * FROM `characters`");
                    Dictionary<string, int> nosync = new Dictionary<string, int> { };
                    Majors = new Dictionary<string, int> { };
                    foreach (DataRow Row in database.Rows)
                    {
                        if (!SeeAdmins && Convert.ToInt32(Row["adminlvl"]) >= AdminLVLToSee) continue;

                        string nick = Row["firstname"].ToString() + "_" + Row["lastname"].ToString();
                        int money = GetPlayerAllMoney(nick, Convert.ToInt32(Finance.Bank.Accounts[Convert.ToInt32(Row["bank"])].Balance) + Convert.ToInt32(Row["money"]));
                        nosync.Add(nick, money);
                    }

                    nosync = nosync.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    int max = MaxForbes;
                    if (nosync.Count < MaxForbes)
                        max = nosync.Count;
                    int i = 0;
                    foreach (KeyValuePair<string, int> pair in nosync.Reverse().ToDictionary(x => x.Key, x => x.Value))
                    {
                        if (i > max - 1) break;
                        Majors.Add(pair.Key, pair.Value); i++;
                    }
                }
                catch (Exception e) { Log.Write("MAJORS: " + e.ToString(), Nlogs.Type.Error); }
            }, 2000);
        }

        public static int GetPlayerAllMoney(string Name, int add)
        {
            try
            {
                int result = add;
                foreach (Houses.House house in Houses.HouseManager.Houses)
                    if (house.Owner == Name)
                    {
                        result += house.Price;
                        break;
                    }
                foreach (string number in VehicleManager.getAllPlayerVehicles(Name))
                {
                    if (BusinessManager.ProductsOrderPrice.ContainsKey(VehicleManager.Vehicles[number].Model))
                        result += BusinessManager.ProductsOrderPrice[VehicleManager.Vehicles[number].Model];
                }


                foreach (Business biz in BusinessManager.BizList.Values)
                    if (biz.Owner == Name)
                    {
                        result += biz.SellPrice;
                        break;
                    }

                return result;
            }
            catch (Exception e) { Log.Write("MAJORS: " + e.ToString(), Nlogs.Type.Error); return 0; }
        }


        public static void OpenForbes(Player player)
        {
            try
            {
                Menu menu = new Menu("forbes", false, false);
                menu.Callback = callback_forbes;

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Богачи штата";
                menu.Add(menuItem);

                foreach (KeyValuePair<string, int> obj in Majors)
                {
                    menuItem = new Menu.Item("forb", Menu.MenuItem.Button)
                    {
                        Text = $"{obj.Key}<br>{obj.Value}$<br>"
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
            catch (Exception e) { Log.Write(e.ToString()); }
        }

        private static void callback_forbes(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (item.ID == "back")
                {
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                }
            }
            catch { }
        }
    }
}
