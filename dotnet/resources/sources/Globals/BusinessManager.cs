﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using iTeffa.Interface;
using iTeffa.Finance;
using iTeffa.Settings;
using iTeffa.Infodata;
using iTeffa.Plugins;
using iTeffa.Models;
using iTeffa.Globals;

namespace iTeffa.Globals
{
    class BusinessManager : Script
    {
        private static nLog Log = new nLog("BusinessManager");
        private static int lastBizID = -1;
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = Connect.QueryRead($"SELECT * FROM businesses");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB biz return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 enterpoint = JsonConvert.DeserializeObject<Vector3>(Row["enterpoint"].ToString());
                    Vector3 unloadpoint = JsonConvert.DeserializeObject<Vector3>(Row["unloadpoint"].ToString());

                    Business data = new Business(Convert.ToInt32(Row["id"]), Row["owner"].ToString(), Convert.ToInt32(Row["sellprice"]), Convert.ToInt32(Row["type"]), JsonConvert.DeserializeObject<List<Product>>(Row["products"].ToString()), enterpoint, unloadpoint, Convert.ToInt32(Row["money"]),
                        Convert.ToInt32(Row["mafia"]), JsonConvert.DeserializeObject<List<Order>>(Row["orders"].ToString()));
                    var id = Convert.ToInt32(Row["id"]);
                    lastBizID = id;

                    if (data.Type == 0)
                    {
                        if (data.Products.Find(p => p.Name == "Связка ключей") == null)
                        {
                            Product product = new Product(ProductsOrderPrice["Связка ключей"], 0, 0, "Связка ключей", false);
                            data.Products.Add(product);
                            Log.Write($"product Связка ключей was added to {data.ID} biz");
                        }
                        data.Save();
                    }
                    BizList.Add(id, data);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"BUSINESSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            try
            {
                SavingBusiness();
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void SavingBusiness()
        {
            foreach (var b in BizList)
            {
                var biz = BizList[b.Key];
                biz.Save();
            }
            Log.Write("Businesses has been saved to DB", nLog.Type.Success);
        }

        public static Dictionary<int, Business> BizList = new Dictionary<int, Business>();
        public static Dictionary<int, int> Orders = new Dictionary<int, int>();

        #region Карта названия блипа
        public static List<string> BusinessTypeNames = new List<string>()
        {
            "24/7",               // 0
            "Заправка",           // 1
            "Автосалон",          // 2
            "Автосалон",          // 3
            "Автосалон",          // 4
            "Автосалон",          // 5
            "Автосалон",          // 6
            "Оружейная",          // 7
            "Магазин одежды",     // 8
            "Тату-салон",         // 9
            "Парикмахерская",     // 10
            "Магазин масок",      // 11
            "Тюнинг",             // 12
            "Автомойка",          // 13
            "FishShop",           // 14
            "SellShop",           // 15
        };
        #endregion
        #region Карта Тип блипа
        public static List<int> BlipByType = new List<int>()
        {
            52,                   // 0
            361,                  // 1
            530,                  // 2
            530,                  // 3
            530,                  // 4
            530,                  // 5
            530,                  // 6
            110,                  // 7
            73,                   // 8
            75,                   // 9
            71,                   // 10
            362,                  // 11
            446,                   // 12
            569,                  // 13
            371,                  // 14
            628,                  // 15
        };
        #endregion
        #region Карта цвет блипа
        public static List<int> BlipColorByType = new List<int>()
        {
            45,                    // 0
            45,                    // 1
            45,                    // 2
            45,                    // 3
            45,                    // 4
            45,                    // 5
            45,                    // 6
            45,                    // 7
            45,                    // 8
            45,                    // 9
            45,                    // 10
            45,                    // 11
            45,                    // 12
            45,                    // 13
            45,                    // 14
            45,                    // 15
        };
        #endregion
        #region 24/7 Маркет
        private static List<string> MarketProducts = new List<string>()
        {
            "Бургер",
            "Хот-Дог",
            "Сэндвич",
            "eCola",
            "Sprunk",
            "Монтировка",
            "Фонарик",
            "Молоток",
            "Гаечный ключ",
            "Канистра бензина",
            "Чипсы",
            "Пицца",
            "Сим-карта",
            "Связка ключей",
        };
        #endregion
        #region FishShop
        private static List<string> FishProducts = new List<string>()
        {
            "Удочка",
            "Улучшенная удочка",
            "Удочка MK2",
            "Наживка",
        };
        #endregion
        #region SellShop
        private static List<string> SellProducts = new List<string>()
        {
            "Корюшка",
            "Кунджа",
            "Лосось",
            "Окунь",
            "Осётр",
            "Скат",
            "Тунец",
            "Угорь",
            "Чёрный амур",
            "Щука",
        };
        #endregion
        #region Автосалон
        public static List<List<string>> CarsNames = new List<List<string>>()
        {
            new List<string>(){"Sultan","Kuruma","Jackal","Surano","Dubsta","Rocoto"},
            new List<string>(){"Sultan","Kuruma","Jackal","Surano","Dubsta","Rocoto"},
            new List<string>(){"Sultan","Kuruma","Jackal","Surano","Dubsta","Rocoto"},
            new List<string>(){"Sultan","Kuruma","Jackal","Surano","Dubsta","Rocoto"},
            new List<string>(){"Sultan","Kuruma","Jackal","Surano","Dubsta","Rocoto"},
        };
        #endregion
        #region Оружейная
        private static List<string> GunNames = new List<string>()
        {
            "Pistol",
            "CombatPistol",
            "Revolver",
            "HeavyPistol",
            "BullpupShotgun",
            "CombatPDW",
            "MachinePistol",
        };
        #endregion
        #region Татуировки
        public static List<List<BusinessTattoo>> BusinessTattoos = new List<List<BusinessTattoo>>()
        {
            #region Торс
            new List<BusinessTattoo>()
            {
                // iTeffa => 0 - Левый сосок | 1 - Правый сосок | 2 - Живот | 3 - Левый низ спины | 4 - Правый низ спины | 5 - Левый верх спины | 6 - Правый верх спины | 7 - Левый бок | 8 - Правый бок| ERROR: Skull of Suits
                new BusinessTattoo(new List<int>(){0,1},"In the Pocket", "mpvinewood_overlays", "MP_Vinewood_Tat_000_M", "MP_Vinewood_Tat_000_F",1500),
                new BusinessTattoo(new List<int>(){5,6}, "Jackpot", "mpvinewood_overlays", "MP_Vinewood_Tat_001_M", "MP_Vinewood_Tat_001_F",1350),
                new BusinessTattoo(new List<int>(){0}, "Royal Flush", "mpvinewood_overlays", "MP_Vinewood_Tat_003_M", "MP_Vinewood_Tat_003_F",1700),
                new BusinessTattoo(new List<int>(){5,6}, "Wheel of Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_006_M", "MP_Vinewood_Tat_006_F",2750),
                new BusinessTattoo(new List<int>(){5,6}, "777", "mpvinewood_overlays", "MP_Vinewood_Tat_007_M", "MP_Vinewood_Tat_007_F",7777),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Snake Eyes", "mpvinewood_overlays", "MP_Vinewood_Tat_008_M", "MP_Vinewood_Tat_008_F",3500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Till Death Do Us Part", "mpvinewood_overlays", "MP_Vinewood_Tat_009_M", "MP_Vinewood_Tat_009_F",2000),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Photo Finish", "mpvinewood_overlays", "MP_Vinewood_Tat_010_M", "MP_Vinewood_Tat_010_F",2550),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Life's a Gamble", "mpvinewood_overlays", "MP_Vinewood_Tat_011_M", "MP_Vinewood_Tat_011_F",4500),
                new BusinessTattoo(new List<int>(){2}, "Skull of Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_012_M", "MP_Vinewood_Tat_012_F",2750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "The Jolly Joker", "mpvinewood_overlays", "MP_Vinewood_Tat_015_M", "MP_Vinewood_Tat_015_F",2000),
                new BusinessTattoo(new List<int>(){2}, "Rose & Aces", "mpvinewood_overlays", "MP_Vinewood_Tat_016_M", "MP_Vinewood_Tat_016_F",3000),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Roll the Dice", "mpvinewood_overlays", "MP_Vinewood_Tat_017_M", "MP_Vinewood_Tat_017_F",1550),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Show Your Hand", "mpvinewood_overlays", "MP_Vinewood_Tat_021_M", "MP_Vinewood_Tat_021_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Blood Money", "mpvinewood_overlays", "MP_Vinewood_Tat_022_M", "MP_Vinewood_Tat_022_F",2550),
                new BusinessTattoo(new List<int>(){0,1}, "Lucky 7s", "mpvinewood_overlays", "MP_Vinewood_Tat_023_M", "MP_Vinewood_Tat_023_F",3550),
                new BusinessTattoo(new List<int>(){2}, "Cash Mouth", "mpvinewood_overlays", "MP_Vinewood_Tat_024_M", "MP_Vinewood_Tat_024_F",5000),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "The Table", "mpvinewood_overlays", "MP_Vinewood_Tat_029_M", "MP_Vinewood_Tat_029_F",3550),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "The Royals", "mpvinewood_overlays", "MP_Vinewood_Tat_030_M", "MP_Vinewood_Tat_030_F",2550),
                new BusinessTattoo(new List<int>(){2}, "Gambling Royalty", "mpvinewood_overlays", "MP_Vinewood_Tat_031_M", "MP_Vinewood_Tat_031_F",4750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Play Your Ace", "mpvinewood_overlays", "MP_Vinewood_Tat_032_M", "MP_Vinewood_Tat_032_F",6000),
                new BusinessTattoo(new List<int>(){2}, "Refined Hustler", "mpbusiness_overlays", "MP_Buis_M_Stomach_000", "",3000),
                new BusinessTattoo(new List<int>(){1}, "Rich", "mpbusiness_overlays", "MP_Buis_M_Chest_000", "",1750),
                new BusinessTattoo(new List<int>(){0}, "$$$", "mpbusiness_overlays", "MP_Buis_M_Chest_001", "",1750),
                new BusinessTattoo(new List<int>(){3,4}, "Makin' Paper", "mpbusiness_overlays", "MP_Buis_M_Back_000", "",2000),
                new BusinessTattoo(new List<int>(){0,1}, "High Roller", "mpbusiness_overlays", "", "MP_Buis_F_Chest_000",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Makin' Money", "mpbusiness_overlays", "", "MP_Buis_F_Chest_001",2500),
                new BusinessTattoo(new List<int>(){1}, "Love Money", "mpbusiness_overlays", "", "MP_Buis_F_Chest_002",1750),
                new BusinessTattoo(new List<int>(){2}, "Diamond Back", "mpbusiness_overlays", "", "MP_Buis_F_Stom_000",3000),
                new BusinessTattoo(new List<int>(){8}, "Santo Capra Logo", "mpbusiness_overlays", "", "MP_Buis_F_Stom_001",2000),
                new BusinessTattoo(new List<int>(){8}, "Money Bag", "mpbusiness_overlays", "", "MP_Buis_F_Stom_002",2000),
                new BusinessTattoo(new List<int>(){3,4}, "Respect", "mpbusiness_overlays", "", "MP_Buis_F_Back_000",2000),
                new BusinessTattoo(new List<int>(){3,4}, "Gold Digger", "mpbusiness_overlays", "", "MP_Buis_F_Back_001",2500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Carp Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_005", "MP_Xmas2_F_Tat_005",6250),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Carp Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_006", "MP_Xmas2_F_Tat_006",6250),
                new BusinessTattoo(new List<int>(){1}, "Time To Die", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_009", "MP_Xmas2_F_Tat_009",1250),
                new BusinessTattoo(new List<int>(){5,6}, "Roaring Tiger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_011", "MP_Xmas2_F_Tat_011",2250),
                new BusinessTattoo(new List<int>(){7}, "Lizard", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_013", "MP_Xmas2_F_Tat_013",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Japanese Warrior", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_015", "MP_Xmas2_F_Tat_015",2100),
                new BusinessTattoo(new List<int>(){0}, "Loose Lips Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_016", "MP_Xmas2_F_Tat_016",1750),
                new BusinessTattoo(new List<int>(){0}, "Loose Lips Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_017", "MP_Xmas2_F_Tat_017",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Royal Dagger Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_018", "MP_Xmas2_F_Tat_018",2500),
                new BusinessTattoo(new List<int>(){0,1}, "Royal Dagger Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_019", "MP_Xmas2_F_Tat_019",2500),
                new BusinessTattoo(new List<int>(){2,8}, "Executioner", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_028", "MP_Xmas2_F_Tat_028",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Bullet Proof", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_000_M", "MP_Gunrunning_Tattoo_000_F",2000),
                new BusinessTattoo(new List<int>(){3,4}, "Crossed Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_001_M", "MP_Gunrunning_Tattoo_001_F",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Butterfly Knife", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_009_M", "MP_Gunrunning_Tattoo_009_F",2250),
                new BusinessTattoo(new List<int>(){2}, "Cash Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_010_M", "MP_Gunrunning_Tattoo_010_F",3000),
                new BusinessTattoo(new List<int>(){1}, "Dollar Daggers", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_012_M", "MP_Gunrunning_Tattoo_012_F",1750),
                new BusinessTattoo(new List<int>(){5,6}, "Wolf Insignia", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_013_M", "MP_Gunrunning_Tattoo_013_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Backstabber", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_014_M", "MP_Gunrunning_Tattoo_014_F",2250),
                new BusinessTattoo(new List<int>(){0,1}, "Dog Tags", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_017_M", "MP_Gunrunning_Tattoo_017_F",2500),
                new BusinessTattoo(new List<int>(){3,4}, "Dual Wield Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_018_M", "MP_Gunrunning_Tattoo_018_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Pistol Wings", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_019_M", "MP_Gunrunning_Tattoo_019_F",2250),
                new BusinessTattoo(new List<int>(){0,1}, "Crowned Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_020_M", "MP_Gunrunning_Tattoo_020_F",2500),
                new BusinessTattoo(new List<int>(){5}, "Explosive Heart", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_022_M", "MP_Gunrunning_Tattoo_022_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Micro SMG Chain", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_028_M", "MP_Gunrunning_Tattoo_028_F",2500),
                new BusinessTattoo(new List<int>(){2}, "Win Some Lose Some", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_029_M", "MP_Gunrunning_Tattoo_029_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Crossed Arrows", "mphipster_overlays", "FM_Hip_M_Tat_000", "FM_Hip_F_Tat_000",2250),
                new BusinessTattoo(new List<int>(){1}, "Chemistry", "mphipster_overlays", "FM_Hip_M_Tat_002", "FM_Hip_F_Tat_002",1750),
                new BusinessTattoo(new List<int>(){7}, "Feather Birds", "mphipster_overlays", "FM_Hip_M_Tat_006", "FM_Hip_F_Tat_006",200),
                new BusinessTattoo(new List<int>(){5,6}, "Infinity", "mphipster_overlays", "FM_Hip_M_Tat_011", "FM_Hip_F_Tat_011",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Antlers", "mphipster_overlays", "FM_Hip_M_Tat_012", "FM_Hip_F_Tat_012",2250),
                new BusinessTattoo(new List<int>(){0,1}, "Boombox", "mphipster_overlays", "FM_Hip_M_Tat_013", "FM_Hip_F_Tat_013",2500),
                new BusinessTattoo(new List<int>(){6}, "Pyramid", "mphipster_overlays", "FM_Hip_M_Tat_024", "FM_Hip_F_Tat_024",1750),
                new BusinessTattoo(new List<int>(){5}, "Watch Your Step", "mphipster_overlays", "FM_Hip_M_Tat_025", "FM_Hip_F_Tat_025",1750),
                new BusinessTattoo(new List<int>(){2,8}, "Sad", "mphipster_overlays", "FM_Hip_M_Tat_029", "FM_Hip_F_Tat_029",3750),
                new BusinessTattoo(new List<int>(){3,4}, "Shark Fin", "mphipster_overlays", "FM_Hip_M_Tat_030", "FM_Hip_F_Tat_030",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Skateboard", "mphipster_overlays", "FM_Hip_M_Tat_031", "FM_Hip_F_Tat_031",2250),
                new BusinessTattoo(new List<int>(){6}, "Paper Plane", "mphipster_overlays", "FM_Hip_M_Tat_032", "FM_Hip_F_Tat_032",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Stag", "mphipster_overlays", "FM_Hip_M_Tat_033", "FM_Hip_F_Tat_033",2500),
                new BusinessTattoo(new List<int>(){2,8}, "Sewn Heart", "mphipster_overlays", "FM_Hip_M_Tat_035", "FM_Hip_F_Tat_035",3750),
                new BusinessTattoo(new List<int>(){3}, "Tooth", "mphipster_overlays", "FM_Hip_M_Tat_041", "FM_Hip_F_Tat_041",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Triangles", "mphipster_overlays", "FM_Hip_M_Tat_046", "FM_Hip_F_Tat_046",2250),
                new BusinessTattoo(new List<int>(){1}, "Cassette", "mphipster_overlays", "FM_Hip_M_Tat_047", "FM_Hip_F_Tat_047",1750),
                new BusinessTattoo(new List<int>(){5,6}, "Block Back", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_000_M", "MP_MP_ImportExport_Tat_000_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Power Plant", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_001_M", "MP_MP_ImportExport_Tat_001_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Tuned to Death", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_002_M", "MP_MP_ImportExport_Tat_002_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Serpents of Destruction", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_009_M", "MP_MP_ImportExport_Tat_009_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Take the Wheel", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_010_M", "MP_MP_ImportExport_Tat_010_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Talk Shit Get Hit", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_011_M", "MP_MP_ImportExport_Tat_011_F",2250),
                new BusinessTattoo(new List<int>(){0}, "King Fight", "mplowrider_overlays", "MP_LR_Tat_001_M", "MP_LR_Tat_001_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Holy Mary", "mplowrider_overlays", "MP_LR_Tat_002_M", "MP_LR_Tat_002_F",2500),
                new BusinessTattoo(new List<int>(){7}, "Gun Mic", "mplowrider_overlays", "MP_LR_Tat_004_M", "MP_LR_Tat_004_F",2000),
                new BusinessTattoo(new List<int>(){6}, "Amazon", "mplowrider_overlays", "MP_LR_Tat_009_M", "MP_LR_Tat_009_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Bad Angel", "mplowrider_overlays", "MP_LR_Tat_010_M", "MP_LR_Tat_010_F",6000),
                new BusinessTattoo(new List<int>(){1}, "Love Gamble", "mplowrider_overlays", "MP_LR_Tat_013_M", "MP_LR_Tat_013_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Love is Blind", "mplowrider_overlays", "MP_LR_Tat_014_M", "MP_LR_Tat_014_F",1250),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Sad Angel", "mplowrider_overlays", "MP_LR_Tat_021_M", "MP_LR_Tat_021_F",5500),
                new BusinessTattoo(new List<int>(){1}, "Royal Takeover", "mplowrider_overlays", "MP_LR_Tat_026_M", "MP_LR_Tat_026_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Turbulence", "mpairraces_overlays", "MP_Airraces_Tattoo_000_M", "MP_Airraces_Tattoo_000_F",1750),
                new BusinessTattoo(new List<int>(){5,6}, "Pilot Skull", "mpairraces_overlays", "MP_Airraces_Tattoo_001_M", "MP_Airraces_Tattoo_001_F",2250),
                new BusinessTattoo(new List<int>(){5,6}, "Winged Bombshell", "mpairraces_overlays", "MP_Airraces_Tattoo_002_M", "MP_Airraces_Tattoo_002_F",2250),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Balloon Pioneer", "mpairraces_overlays", "MP_Airraces_Tattoo_004_M", "MP_Airraces_Tattoo_004_F",5000),
                new BusinessTattoo(new List<int>(){5,6}, "Parachute Belle", "mpairraces_overlays", "MP_Airraces_Tattoo_005_M", "MP_Airraces_Tattoo_005_F",2250),
                new BusinessTattoo(new List<int>(){2}, "Bombs Away", "mpairraces_overlays", "MP_Airraces_Tattoo_006_M", "MP_Airraces_Tattoo_006_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Eagle Eyes", "mpairraces_overlays", "MP_Airraces_Tattoo_007_M", "MP_Airraces_Tattoo_007_F",2250),
                new BusinessTattoo(new List<int>(){0}, "Demon Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_000_M", "MP_MP_Biker_Tat_000_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Both Barrels", "mpbiker_overlays", "MP_MP_Biker_Tat_001_M", "MP_MP_Biker_Tat_001_F",2500),
                new BusinessTattoo(new List<int>(){2}, "Web Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_003_M", "MP_MP_Biker_Tat_003_F",3000),
                new BusinessTattoo(new List<int>(){0,1}, "Made In America", "mpbiker_overlays", "MP_MP_Biker_Tat_005_M", "MP_MP_Biker_Tat_005_F",2500),
                new BusinessTattoo(new List<int>(){3,4}, "Chopper Freedom", "mpbiker_overlays", "MP_MP_Biker_Tat_006_M", "MP_MP_Biker_Tat_006_F",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Freedom Wheels", "mpbiker_overlays", "MP_MP_Biker_Tat_008_M", "MP_MP_Biker_Tat_008_F",2250),
                new BusinessTattoo(new List<int>(){2}, "Skull Of Taurus", "mpbiker_overlays", "MP_MP_Biker_Tat_010_M", "MP_MP_Biker_Tat_010_F",3250),
                new BusinessTattoo(new List<int>(){5,6}, "R.I.P. My Brothers", "mpbiker_overlays", "MP_MP_Biker_Tat_011_M", "MP_MP_Biker_Tat_011_F",2250),
                new BusinessTattoo(new List<int>(){0,1}, "Demon Crossbones", "mpbiker_overlays", "MP_MP_Biker_Tat_013_M", "MP_MP_Biker_Tat_013_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Clawed Beast", "mpbiker_overlays", "MP_MP_Biker_Tat_017_M", "MP_MP_Biker_Tat_017_F",2250),
                new BusinessTattoo(new List<int>(){1}, "Skeletal Chopper", "mpbiker_overlays", "MP_MP_Biker_Tat_018_M", "MP_MP_Biker_Tat_018_F",1800),
                new BusinessTattoo(new List<int>(){0,1}, "Gruesome Talons", "mpbiker_overlays", "MP_MP_Biker_Tat_019_M", "MP_MP_Biker_Tat_019_F",2750),
                new BusinessTattoo(new List<int>(){5,6}, "Flaming Reaper", "mpbiker_overlays", "MP_MP_Biker_Tat_021_M", "MP_MP_Biker_Tat_021_F",2250),
                new BusinessTattoo(new List<int>(){0,1}, "Western MC", "mpbiker_overlays", "MP_MP_Biker_Tat_023_M", "MP_MP_Biker_Tat_023_F",2750),
                new BusinessTattoo(new List<int>(){0,1}, "American Dream", "mpbiker_overlays", "MP_MP_Biker_Tat_026_M", "MP_MP_Biker_Tat_026_F",2650),
                new BusinessTattoo(new List<int>(){0}, "Bone Wrench", "mpbiker_overlays", "MP_MP_Biker_Tat_029_M", "MP_MP_Biker_Tat_029_F",1650),
                new BusinessTattoo(new List<int>(){5,6}, "Brothers For Life", "mpbiker_overlays", "MP_MP_Biker_Tat_030_M", "MP_MP_Biker_Tat_030_F",2300),
                new BusinessTattoo(new List<int>(){2}, "Gear Head", "mpbiker_overlays", "MP_MP_Biker_Tat_031_M", "MP_MP_Biker_Tat_031_F",3000),
                new BusinessTattoo(new List<int>(){0}, "Western Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_032_M", "MP_MP_Biker_Tat_032_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Brotherhood of Bikes", "mpbiker_overlays", "MP_MP_Biker_Tat_034_M", "MP_MP_Biker_Tat_034_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Gas Guzzler", "mpbiker_overlays", "MP_MP_Biker_Tat_039_M", "MP_MP_Biker_Tat_039_F",2850),
                new BusinessTattoo(new List<int>(){0,1}, "No Regrets", "mpbiker_overlays", "MP_MP_Biker_Tat_041_M", "MP_MP_Biker_Tat_041_F",2500),
                new BusinessTattoo(new List<int>(){3,4}, "Ride Forever", "mpbiker_overlays", "MP_MP_Biker_Tat_043_M", "MP_MP_Biker_Tat_043_F",2100),
                new BusinessTattoo(new List<int>(){0,1}, "Unforgiven", "mpbiker_overlays", "MP_MP_Biker_Tat_050_M", "MP_MP_Biker_Tat_050_F",3000),
                new BusinessTattoo(new List<int>(){2}, "Biker Mount", "mpbiker_overlays", "MP_MP_Biker_Tat_052_M", "MP_MP_Biker_Tat_052_F",2500),
                new BusinessTattoo(new List<int>(){1}, "Reaper Vulture", "mpbiker_overlays", "MP_MP_Biker_Tat_058_M", "MP_MP_Biker_Tat_058_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Faggio", "mpbiker_overlays", "MP_MP_Biker_Tat_059_M", "MP_MP_Biker_Tat_059_F",1750),
                new BusinessTattoo(new List<int>(){0}, "We Are The Mods!", "mpbiker_overlays", "MP_MP_Biker_Tat_060_M", "MP_MP_Biker_Tat_060_F",1850),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "SA Assault", "mplowrider2_overlays", "MP_LR_Tat_000_M", "MP_LR_Tat_000_F",5500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Love the Game", "mplowrider2_overlays", "MP_LR_Tat_008_M", "MP_LR_Tat_008_F",5250),
                new BusinessTattoo(new List<int>(){7}, "Lady Liberty", "mplowrider2_overlays", "MP_LR_Tat_011_M", "MP_LR_Tat_011_F",2100),
                new BusinessTattoo(new List<int>(){0}, "Royal Kiss", "mplowrider2_overlays", "MP_LR_Tat_012_M", "MP_LR_Tat_012_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Two Face", "mplowrider2_overlays", "MP_LR_Tat_016_M", "MP_LR_Tat_016_F",3100),
                new BusinessTattoo(new List<int>(){1}, "Death Behind", "mplowrider2_overlays", "MP_LR_Tat_019_M", "MP_LR_Tat_019_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Dead Pretty", "mplowrider2_overlays", "MP_LR_Tat_031_M", "MP_LR_Tat_031_F",5250),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Reign Over", "mplowrider2_overlays", "MP_LR_Tat_032_M", "MP_LR_Tat_032_F",5600),
                new BusinessTattoo(new List<int>(){2}, "Abstract Skull", "mpluxe_overlays", "MP_LUXE_TAT_003_M", "MP_LUXE_TAT_003_F",2750),
                new BusinessTattoo(new List<int>(){1}, "Eye of the Griffin", "mpluxe_overlays", "MP_LUXE_TAT_007_M", "MP_LUXE_TAT_007_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Flying Eye", "mpluxe_overlays", "MP_LUXE_TAT_008_M", "MP_LUXE_TAT_008_F",1800),
                new BusinessTattoo(new List<int>(){0,1}, "Ancient Queen", "mpluxe_overlays", "MP_LUXE_TAT_014_M", "MP_LUXE_TAT_014_F",2600),
                new BusinessTattoo(new List<int>(){0}, "Smoking Sisters", "mpluxe_overlays", "MP_LUXE_TAT_015_M", "MP_LUXE_TAT_015_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Feather Mural", "mpluxe_overlays", "MP_LUXE_TAT_024_M", "MP_LUXE_TAT_024_F",6250),
                new BusinessTattoo(new List<int>(){0}, "The Howler", "mpluxe2_overlays", "MP_LUXE_TAT_002_M", "MP_LUXE_TAT_002_F",1750),
                new BusinessTattoo(new List<int>(){0,1,2,8}, "Geometric Galaxy", "mpluxe2_overlays", "MP_LUXE_TAT_012_M", "MP_LUXE_TAT_012_F",7000),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Cloaked Angel", "mpluxe2_overlays", "MP_LUXE_TAT_022_M", "MP_LUXE_TAT_022_F",6000),
                new BusinessTattoo(new List<int>(){0}, "Reaper Sway", "mpluxe2_overlays", "MP_LUXE_TAT_025_M", "MP_LUXE_TAT_025_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Cobra Dawn", "mpluxe2_overlays", "MP_LUXE_TAT_027_M", "MP_LUXE_TAT_027_F",1800),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Geometric Design T", "mpluxe2_overlays", "MP_LUXE_TAT_029_M", "MP_LUXE_TAT_029_F",5500),
                new BusinessTattoo(new List<int>(){1}, "Bless The Dead", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_000_M", "MP_Smuggler_Tattoo_000_F",1000),
                new BusinessTattoo(new List<int>(){2}, "Dead Lies", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_002_M", "MP_Smuggler_Tattoo_002_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Give Nothing Back", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_003_M", "MP_Smuggler_Tattoo_003_F",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Never Surrender", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_006_M", "MP_Smuggler_Tattoo_006_F",2100),
                new BusinessTattoo(new List<int>(){0,1}, "No Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_007_M", "MP_Smuggler_Tattoo_007_F",2500),
                new BusinessTattoo(new List<int>(){5,6}, "Tall Ship Conflict", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_009_M", "MP_Smuggler_Tattoo_009_F",2000),
                new BusinessTattoo(new List<int>(){2}, "See You In Hell", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_010_M", "MP_Smuggler_Tattoo_010_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Torn Wings", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_013_M", "MP_Smuggler_Tattoo_013_F",2100),
                new BusinessTattoo(new List<int>(){2}, "Jolly Roger", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_015_M", "MP_Smuggler_Tattoo_015_F",3000),
                new BusinessTattoo(new List<int>(){5,6}, "Skull Compass", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_016_M", "MP_Smuggler_Tattoo_016_F",2000),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Framed Tall Ship", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_017_M", "MP_Smuggler_Tattoo_017_F",5500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Finders Keepers", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_018_M", "MP_Smuggler_Tattoo_018_F",6000),
                new BusinessTattoo(new List<int>(){0}, "Lost At Sea", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_019_M", "MP_Smuggler_Tattoo_019_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Dead Tales", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_021_M", "MP_Smuggler_Tattoo_021_F",2000),
                new BusinessTattoo(new List<int>(){5}, "X Marks The Spot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_022_M", "MP_Smuggler_Tattoo_022_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Pirate Captain", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_024_M", "MP_Smuggler_Tattoo_024_F",5500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Claimed By The Beast", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_025_M", "MP_Smuggler_Tattoo_025_F",5500),
                new BusinessTattoo(new List<int>(){0,1}, "Wheels of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_011_M", "MP_MP_Stunt_Tat_011_F",2000),
                new BusinessTattoo(new List<int>(){7}, "Punk Biker", "mpstunt_overlays", "MP_MP_Stunt_Tat_012_M", "MP_MP_Stunt_Tat_012_F",2000),
                new BusinessTattoo(new List<int>(){2}, "Bat Cat of Spades", "mpstunt_overlays", "MP_MP_Stunt_Tat_014_M", "MP_MP_Stunt_Tat_014_F",3100),
                new BusinessTattoo(new List<int>(){0}, "Vintage Bully", "mpstunt_overlays", "MP_MP_Stunt_Tat_018_M", "MP_MP_Stunt_Tat_018_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Engine Heart", "mpstunt_overlays", "MP_MP_Stunt_Tat_019_M", "MP_MP_Stunt_Tat_019_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_024_M", "MP_MP_Stunt_Tat_024_F",5000),
                new BusinessTattoo(new List<int>(){5,6}, "Winged Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_026_M", "MP_MP_Stunt_Tat_026_F",2000),
                new BusinessTattoo(new List<int>(){0}, "Punk Road Hog", "mpstunt_overlays", "MP_MP_Stunt_Tat_027_M", "MP_MP_Stunt_Tat_027_F",1750),
                new BusinessTattoo(new List<int>(){3,4}, "Majestic Finish", "mpstunt_overlays", "MP_MP_Stunt_Tat_029_M", "MP_MP_Stunt_Tat_029_F",2000),
                new BusinessTattoo(new List<int>(){6}, "Man's Ruin", "mpstunt_overlays", "MP_MP_Stunt_Tat_030_M", "MP_MP_Stunt_Tat_030_F",2100),
                new BusinessTattoo(new List<int>(){1}, "Sugar Skull Trucker", "mpstunt_overlays", "MP_MP_Stunt_Tat_033_M", "MP_MP_Stunt_Tat_033_F",1750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Feather Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_034_M", "MP_MP_Stunt_Tat_034_F",1250),
                new BusinessTattoo(new List<int>(){5}, "Big Grills", "mpstunt_overlays", "MP_MP_Stunt_Tat_037_M", "MP_MP_Stunt_Tat_037_F",1750),
                new BusinessTattoo(new List<int>(){5,6}, "Monkey Chopper", "mpstunt_overlays", "MP_MP_Stunt_Tat_040_M", "MP_MP_Stunt_Tat_040_F",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Brapp", "mpstunt_overlays", "MP_MP_Stunt_Tat_041_M", "MP_MP_Stunt_Tat_041_F",2000),
                new BusinessTattoo(new List<int>(){0,1}, "Ram Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_044_M", "MP_MP_Stunt_Tat_044_F",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Full Throttle", "mpstunt_overlays", "MP_MP_Stunt_Tat_046_M", "MP_MP_Stunt_Tat_046_F",2100),
                new BusinessTattoo(new List<int>(){5,6}, "Racing Doll", "mpstunt_overlays", "MP_MP_Stunt_Tat_048_M", "MP_MP_Stunt_Tat_048_F",2100),
                new BusinessTattoo(new List<int>(){0}, "Blackjack", "multiplayer_overlays", "FM_Tat_Award_M_003", "FM_Tat_Award_F_003",1800),
                new BusinessTattoo(new List<int>(){2}, "Hustler", "multiplayer_overlays", "FM_Tat_Award_M_004", "FM_Tat_Award_F_004",3250),
                new BusinessTattoo(new List<int>(){5,6}, "Angel", "multiplayer_overlays", "FM_Tat_Award_M_005", "FM_Tat_Award_F_005",2100),
                new BusinessTattoo(new List<int>(){3,4}, "Los Santos Customs", "multiplayer_overlays", "FM_Tat_Award_M_008", "FM_Tat_Award_F_008",8400),
                new BusinessTattoo(new List<int>(){1}, "Blank Scroll", "multiplayer_overlays", "FM_Tat_Award_M_011", "FM_Tat_Award_F_011",1800),
                new BusinessTattoo(new List<int>(){1}, "Embellished Scroll", "multiplayer_overlays", "FM_Tat_Award_M_012", "FM_Tat_Award_F_012",1800),
                new BusinessTattoo(new List<int>(){1}, "Seven Deadly Sins", "multiplayer_overlays", "FM_Tat_Award_M_013", "FM_Tat_Award_F_013",1800),
                new BusinessTattoo(new List<int>(){3,4}, "Trust No One", "multiplayer_overlays", "FM_Tat_Award_M_014", "FM_Tat_Award_F_014",2100),
                new BusinessTattoo(new List<int>(){5,6}, "Clown", "multiplayer_overlays", "FM_Tat_Award_M_016", "FM_Tat_Award_F_016",2000),
                new BusinessTattoo(new List<int>(){5,6}, "Clown and Gun", "multiplayer_overlays", "FM_Tat_Award_M_017", "FM_Tat_Award_F_017",2100),
                new BusinessTattoo(new List<int>(){5,6}, "Clown Dual Wield", "multiplayer_overlays", "FM_Tat_Award_M_018", "FM_Tat_Award_F_018",2000),
                new BusinessTattoo(new List<int>(){6,6}, "Clown Dual Wield Dollars", "multiplayer_overlays", "FM_Tat_Award_M_019", "FM_Tat_Award_F_019",2100),
                new BusinessTattoo(new List<int>(){2}, "Faith T", "multiplayer_overlays", "FM_Tat_M_004", "FM_Tat_F_004",3100),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Skull on the Cross", "multiplayer_overlays", "FM_Tat_M_009", "FM_Tat_F_009",6000),
                new BusinessTattoo(new List<int>(){1}, "LS Flames", "multiplayer_overlays", "FM_Tat_M_010", "FM_Tat_F_010",1800),
                new BusinessTattoo(new List<int>(){5}, "LS Script", "multiplayer_overlays", "FM_Tat_M_011", "FM_Tat_F_011",2100),
                new BusinessTattoo(new List<int>(){2}, "Los Santos Bills", "multiplayer_overlays", "FM_Tat_M_012", "FM_Tat_F_012",3000),
                new BusinessTattoo(new List<int>(){6}, "Eagle and Serpent", "multiplayer_overlays", "FM_Tat_M_013", "FM_Tat_F_013",2100),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Evil Clown", "multiplayer_overlays", "FM_Tat_M_016", "FM_Tat_F_016",5750),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "The Wages of Sin", "multiplayer_overlays", "FM_Tat_M_019", "FM_Tat_F_019",5500),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Dragon T", "multiplayer_overlays", "FM_Tat_M_020", "FM_Tat_F_020",5000),
                new BusinessTattoo(new List<int>(){0,1,2,8}, "Flaming Cross", "multiplayer_overlays", "FM_Tat_M_024", "FM_Tat_F_024",6750),
                new BusinessTattoo(new List<int>(){0}, "LS Bold", "multiplayer_overlays", "FM_Tat_M_025", "FM_Tat_F_025",1800),
                new BusinessTattoo(new List<int>(){2,8}, "Trinity Knot", "multiplayer_overlays", "FM_Tat_M_029", "FM_Tat_F_029",4100),
                new BusinessTattoo(new List<int>(){5,6}, "Lucky Celtic Dogs", "multiplayer_overlays", "FM_Tat_M_030", "FM_Tat_F_030",2100),
                new BusinessTattoo(new List<int>(){1}, "Flaming Shamrock", "multiplayer_overlays", "FM_Tat_M_034", "FM_Tat_F_034",1700),
                new BusinessTattoo(new List<int>(){2}, "Way of the Gun", "multiplayer_overlays", "FM_Tat_M_036", "FM_Tat_F_036",3000),
                new BusinessTattoo(new List<int>(){0,1}, "Stone Cross", "multiplayer_overlays", "FM_Tat_M_044", "FM_Tat_F_044",2100),
                new BusinessTattoo(new List<int>(){3,4,5,6}, "Skulls and Rose", "multiplayer_overlays", "FM_Tat_M_045", "FM_Tat_F_045",5500)
            },
            #endregion Торс
            #region Голова
            new List<BusinessTattoo>(){
                // iTeffa => 0 - Передняя шея | 1 - Левая шея | 2 - Правая шея | 3 - Задняя шея | 4 - Левая щека | 5 - Правая щека
                new BusinessTattoo(new List<int>(){0},"Cash is King", "mpbusiness_overlays", "MP_Buis_M_Neck_000", "",1750),
                new BusinessTattoo(new List<int>(){1}, "Bold Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_001", "",1750),
                new BusinessTattoo(new List<int>(){1}, "Val-de-Grace Logo", "mpbusiness_overlays", "", "MP_Buis_F_Neck_000",1750),
                new BusinessTattoo(new List<int>(){1}, "Snake Head Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_024", "MP_Xmas2_F_Tat_024",1750),
                new BusinessTattoo(new List<int>(){1}, "Snake Head Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_025", "MP_Xmas2_F_Tat_025",1750),
                new BusinessTattoo(new List<int>(){1}, "Lock & Load", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_003_M", "MP_Gunrunning_Tattoo_003_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Western Stylized", "mpbiker_overlays", "MP_MP_Biker_Tat_051_M", "MP_MP_Biker_Tat_051_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Sinner", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_011_M", "MP_Smuggler_Tattoo_011_F",1750),
                new BusinessTattoo(new List<int>(){1}, "Geo Fox", "mphipster_overlays", "FM_Hip_M_Tat_021", "FM_Hip_F_Tat_021",1750),
                new BusinessTattoo(new List<int>(){1}, "Stunt Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_000_M", "MP_MP_Stunt_Tat_000_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Script Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_002", "",1750),
                new BusinessTattoo(new List<int>(){2}, "Toxic Spider", "mpstunt_overlays", "MP_MP_Stunt_Tat_006_M", "MP_MP_Stunt_Tat_006_F",200),
                new BusinessTattoo(new List<int>(){2}, "Bat Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_017_M", "MP_MP_Stunt_Tat_017_F",200),
                new BusinessTattoo(new List<int>(){2}, "Flaming Quad", "mpstunt_overlays", "MP_MP_Stunt_Tat_042_M", "MP_MP_Stunt_Tat_042_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Money Rose", "mpbusiness_overlays", "", "MP_Buis_F_Neck_001",1750),
                new BusinessTattoo(new List<int>(){2}, "Los Muertos", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_007", "MP_Xmas2_F_Tat_007",1750),
                new BusinessTattoo(new List<int>(){2}, "Beautiful Death", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_029", "MP_Xmas2_F_Tat_029",1750),
                new BusinessTattoo(new List<int>(){2}, "Beautiful Eye", "mphipster_overlays", "FM_Hip_M_Tat_005", "FM_Hip_F_Tat_005",1750),
                new BusinessTattoo(new List<int>(){2}, "FTW", "mpbiker_overlays", "MP_MP_Biker_Tat_038_M", "MP_MP_Biker_Tat_038_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Thief", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_012_M", "MP_Smuggler_Tattoo_012_F",1750),
                new BusinessTattoo(new List<int>(){3}, "$100", "mpbusiness_overlays", "MP_Buis_M_Neck_003", "",1750),
                new BusinessTattoo(new List<int>(){5}, "Morbid Arachnid", "mpbiker_overlays", "MP_MP_Biker_Tat_009_M", "MP_MP_Biker_Tat_009_F",1750),
                new BusinessTattoo(new List<int>(){5}, "Scorpion", "mpstunt_overlays", "MP_MP_Stunt_Tat_004_M", "MP_MP_Stunt_Tat_004_F",200)
            },
            #endregion Голова
            #region Левая рука
            new List<BusinessTattoo>()
            {
                // iTeffa => 0 - Кисть| 1 - До локтя | 2 - Выше локтя
                new BusinessTattoo(new List<int>(){1,2},"Suits", "mpvinewood_overlays", "MP_Vinewood_Tat_002_M", "MP_Vinewood_Tat_002_F",2500),
                new BusinessTattoo(new List<int>(){1,2}, "Get Lucky", "mpvinewood_overlays", "MP_Vinewood_Tat_005_M", "MP_Vinewood_Tat_005_F",3000),
                new BusinessTattoo(new List<int>(){1}, "Vice", "mpvinewood_overlays", "MP_Vinewood_Tat_014_M", "MP_Vinewood_Tat_014_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Can't Win Them All", "mpvinewood_overlays", "MP_Vinewood_Tat_019_M", "MP_Vinewood_Tat_019_F",4000),
                new BusinessTattoo(new List<int>(){1,2}, "Banknote Rose", "mpvinewood_overlays", "MP_Vinewood_Tat_026_M", "MP_Vinewood_Tat_026_F",3500),
                new BusinessTattoo(new List<int>(){1}, "$100 Bill", "mpbusiness_overlays", "MP_Buis_M_LeftArm_000", "",1850),
                new BusinessTattoo(new List<int>(){1,2}, "All-Seeing Eye", "mpbusiness_overlays", "MP_Buis_M_LeftArm_001", "",780),
                new BusinessTattoo(new List<int>(){1}, "Greed is Good", "mpbusiness_overlays", "", "MP_Buis_F_LArm_000",1800),
                new BusinessTattoo(new List<int>(){1}, "Skull Rider", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_000", "MP_Xmas2_F_Tat_000",1850),
                new BusinessTattoo(new List<int>(){1}, "Electric Snake", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_010", "MP_Xmas2_F_Tat_010",1800),
                new BusinessTattoo(new List<int>(){2}, "8 Ball Skull", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_012", "MP_Xmas2_F_Tat_012",1900),
                new BusinessTattoo(new List<int>(){0}, "Time's Up Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_020", "MP_Xmas2_F_Tat_020",1300),
                new BusinessTattoo(new List<int>(){0}, "Time's Up Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_021", "MP_Xmas2_F_Tat_021",1300),
                new BusinessTattoo(new List<int>(){0}, "Sidearm", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_004_M", "MP_Gunrunning_Tattoo_004_F",1350),
                new BusinessTattoo(new List<int>(){2}, "Bandolier", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_008_M", "MP_Gunrunning_Tattoo_008_F",1780),
                new BusinessTattoo(new List<int>(){1,2}, "Spiked Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_015_M", "MP_Gunrunning_Tattoo_015_F",3800),
                new BusinessTattoo(new List<int>(){2}, "Blood Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_016_M", "MP_Gunrunning_Tattoo_016_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Praying Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_025_M", "MP_Gunrunning_Tattoo_025_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Serpent Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_027_M", "MP_Gunrunning_Tattoo_027_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Diamond Sparkle", "mphipster_overlays", "FM_Hip_M_Tat_003", "FM_Hip_F_Tat_003",1800),
                new BusinessTattoo(new List<int>(){0}, "Bricks", "mphipster_overlays", "FM_Hip_M_Tat_007", "FM_Hip_F_Tat_007",1300),
                new BusinessTattoo(new List<int>(){2}, "Mustache", "mphipster_overlays", "FM_Hip_M_Tat_015", "FM_Hip_F_Tat_015",1800),
                new BusinessTattoo(new List<int>(){1}, "Lightning Bolt", "mphipster_overlays", "FM_Hip_M_Tat_016", "FM_Hip_F_Tat_016",1800),
                new BusinessTattoo(new List<int>(){2}, "Pizza", "mphipster_overlays", "FM_Hip_M_Tat_026", "FM_Hip_F_Tat_026",1800),
                new BusinessTattoo(new List<int>(){1}, "Padlock", "mphipster_overlays", "FM_Hip_M_Tat_027", "FM_Hip_F_Tat_027",2000),
                new BusinessTattoo(new List<int>(){1}, "Thorny Rose", "mphipster_overlays", "FM_Hip_M_Tat_028", "FM_Hip_F_Tat_028",2000),
                new BusinessTattoo(new List<int>(){0}, "Stop", "mphipster_overlays", "FM_Hip_M_Tat_034", "FM_Hip_F_Tat_034",1250),
                new BusinessTattoo(new List<int>(){2}, "Sunrise", "mphipster_overlays", "FM_Hip_M_Tat_037", "FM_Hip_F_Tat_037",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Sleeve", "mphipster_overlays", "FM_Hip_M_Tat_039", "FM_Hip_F_Tat_039",4500),
                new BusinessTattoo(new List<int>(){2}, "Triangle White", "mphipster_overlays", "FM_Hip_M_Tat_043", "FM_Hip_F_Tat_043",1850),
                new BusinessTattoo(new List<int>(){0}, "Peace", "mphipster_overlays", "FM_Hip_M_Tat_048", "FM_Hip_F_Tat_048",1300),
                new BusinessTattoo(new List<int>(){1,2}, "Piston Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_004_M", "MP_MP_ImportExport_Tat_004_F",3800),
                new BusinessTattoo(new List<int>(){1,2}, "Scarlett", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_008_M", "MP_MP_ImportExport_Tat_008_F",3750),
                new BusinessTattoo(new List<int>(){1}, "No Evil", "mplowrider_overlays", "MP_LR_Tat_005_M", "MP_LR_Tat_005_F",1780),
                new BusinessTattoo(new List<int>(){2}, "Los Santos Life", "mplowrider_overlays", "MP_LR_Tat_027_M", "MP_LR_Tat_027_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "City Sorrow", "mplowrider_overlays", "MP_LR_Tat_033_M", "MP_LR_Tat_033_F",3800),
                new BusinessTattoo(new List<int>(){1,2}, "Toxic Trails", "mpairraces_overlays", "MP_Airraces_Tattoo_003_M", "MP_Airraces_Tattoo_003_F",15700),
                new BusinessTattoo(new List<int>(){1}, "Urban Stunter", "mpbiker_overlays", "MP_MP_Biker_Tat_012_M", "MP_MP_Biker_Tat_012_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Macabre Tree", "mpbiker_overlays", "MP_MP_Biker_Tat_016_M", "MP_MP_Biker_Tat_016_F",2000),
                new BusinessTattoo(new List<int>(){2}, "Cranial Rose", "mpbiker_overlays", "MP_MP_Biker_Tat_020_M", "MP_MP_Biker_Tat_020_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Live to Ride", "mpbiker_overlays", "MP_MP_Biker_Tat_024_M", "MP_MP_Biker_Tat_024_F",3800),
                new BusinessTattoo(new List<int>(){2}, "Good Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_025_M", "MP_MP_Biker_Tat_025_F",1100),
                new BusinessTattoo(new List<int>(){2}, "Chain Fist", "mpbiker_overlays", "MP_MP_Biker_Tat_035_M", "MP_MP_Biker_Tat_035_F",1600),
                new BusinessTattoo(new List<int>(){2}, "Ride Hard Die Fast", "mpbiker_overlays", "MP_MP_Biker_Tat_045_M", "MP_MP_Biker_Tat_045_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Muffler Helmet", "mpbiker_overlays", "MP_MP_Biker_Tat_053_M", "MP_MP_Biker_Tat_053_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Poison Scorpion", "mpbiker_overlays", "MP_MP_Biker_Tat_055_M", "MP_MP_Biker_Tat_055_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Love Hustle", "mplowrider2_overlays", "MP_LR_Tat_006_M", "MP_LR_Tat_006_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Skeleton Party", "mplowrider2_overlays", "MP_LR_Tat_018_M", "MP_LR_Tat_018_F",3700),
                new BusinessTattoo(new List<int>(){1}, "My Crazy Life", "mplowrider2_overlays", "MP_LR_Tat_022_M", "MP_LR_Tat_022_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Archangel & Mary", "mpluxe_overlays", "MP_LUXE_TAT_020_M", "MP_LUXE_TAT_020_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Gabriel", "mpluxe_overlays", "MP_LUXE_TAT_021_M", "MP_LUXE_TAT_021_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Fatal Dagger", "mpluxe2_overlays", "MP_LUXE_TAT_005_M", "MP_LUXE_TAT_005_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Egyptian Mural", "mpluxe2_overlays", "MP_LUXE_TAT_016_M", "MP_LUXE_TAT_016_F",1780),
                new BusinessTattoo(new List<int>(){2}, "Divine Goddess", "mpluxe2_overlays", "MP_LUXE_TAT_018_M", "MP_LUXE_TAT_018_F",1780),
                new BusinessTattoo(new List<int>(){1}, "Python Skull", "mpluxe2_overlays", "MP_LUXE_TAT_028_M", "MP_LUXE_TAT_028_F",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Geometric Design LA", "mpluxe2_overlays", "MP_LUXE_TAT_031_M", "MP_LUXE_TAT_031_F",3800),
                new BusinessTattoo(new List<int>(){1}, "Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_004_M", "MP_Smuggler_Tattoo_004_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Horrors Of The Deep", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_008_M", "MP_Smuggler_Tattoo_008_F",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Mermaid's Curse", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_014_M", "MP_Smuggler_Tattoo_014_F",3800),
                new BusinessTattoo(new List<int>(){2}, "8 Eyed Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_001_M", "MP_MP_Stunt_Tat_001_F",1750),
                new BusinessTattoo(new List<int>(){0}, "Big Cat", "mpstunt_overlays", "MP_MP_Stunt_Tat_002_M", "MP_MP_Stunt_Tat_002_F",1250),
                new BusinessTattoo(new List<int>(){2}, "Moonlight Ride", "mpstunt_overlays", "MP_MP_Stunt_Tat_008_M", "MP_MP_Stunt_Tat_008_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Piston Head", "mpstunt_overlays", "MP_MP_Stunt_Tat_022_M", "MP_MP_Stunt_Tat_022_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Tanked", "mpstunt_overlays", "MP_MP_Stunt_Tat_023_M", "MP_MP_Stunt_Tat_023_F",3750),
                new BusinessTattoo(new List<int>(){1}, "Stuntman's End", "mpstunt_overlays", "MP_MP_Stunt_Tat_035_M", "MP_MP_Stunt_Tat_035_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Kaboom", "mpstunt_overlays", "MP_MP_Stunt_Tat_039_M", "MP_MP_Stunt_Tat_039_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Engine Arm", "mpstunt_overlays", "MP_MP_Stunt_Tat_043_M", "MP_MP_Stunt_Tat_043_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Burning Heart", "multiplayer_overlays", "FM_Tat_Award_M_001", "FM_Tat_Award_F_001",1850),
                new BusinessTattoo(new List<int>(){2}, "Racing Blonde", "multiplayer_overlays", "FM_Tat_Award_M_007", "FM_Tat_Award_F_007",1850),
                new BusinessTattoo(new List<int>(){2}, "Racing Brunette", "multiplayer_overlays", "FM_Tat_Award_M_015", "FM_Tat_Award_F_015",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Serpents", "multiplayer_overlays", "FM_Tat_M_005", "FM_Tat_F_005",1780),
                new BusinessTattoo(new List<int>(){1,2}, "Oriental Mural", "multiplayer_overlays", "FM_Tat_M_006", "FM_Tat_F_006",3800),
                new BusinessTattoo(new List<int>(){2}, "Zodiac Skull", "multiplayer_overlays", "FM_Tat_M_015", "FM_Tat_F_015",1800),
                new BusinessTattoo(new List<int>(){2}, "Lady M", "multiplayer_overlays", "FM_Tat_M_031", "FM_Tat_F_031",1850),
                new BusinessTattoo(new List<int>(){2}, "Dope Skull", "multiplayer_overlays", "FM_Tat_M_041", "FM_Tat_F_041",1800)
            },
            #endregion Левая рука
            #region Правая рука
            new List<BusinessTattoo>()
            {
                // iTeffa => 0 - Кисть | 1 - До локтя | 2 - Выше локтя
                new BusinessTattoo(new List<int>(){1,2},"Lady Luck", "mpvinewood_overlays", "MP_Vinewood_Tat_004_M", "MP_Vinewood_Tat_004_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "The Gambler's Life", "mpvinewood_overlays", "MP_Vinewood_Tat_018_M", "MP_Vinewood_Tat_018_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Queen of Roses", "mpvinewood_overlays", "MP_Vinewood_Tat_025_M", "MP_Vinewood_Tat_025_F",1000),
                new BusinessTattoo(new List<int>(){2}, "Skull & Aces", "mpvinewood_overlays", "MP_Vinewood_Tat_028_M", "MP_Vinewood_Tat_028_F",2000),
                new BusinessTattoo(new List<int>(){2}, "Dollar Skull", "mpbusiness_overlays", "MP_Buis_M_RightArm_000", "",1780),
                new BusinessTattoo(new List<int>(){1}, "Green", "mpbusiness_overlays", "MP_Buis_M_RightArm_001", "",1780),
                new BusinessTattoo(new List<int>(){1}, "Dollar Sign", "mpbusiness_overlays", "", "MP_Buis_F_RArm_000",1800),
                new BusinessTattoo(new List<int>(){2}, "Snake Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_003", "MP_Xmas2_F_Tat_003",1780),
                new BusinessTattoo(new List<int>(){2}, "Snake Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_004", "MP_Xmas2_F_Tat_004",1850),
                new BusinessTattoo(new List<int>(){1}, "Death Before Dishonor", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_008", "MP_Xmas2_F_Tat_008",1800),
                new BusinessTattoo(new List<int>(){1}, "You're Next Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_022", "MP_Xmas2_F_Tat_022",850),
                new BusinessTattoo(new List<int>(){1}, "You're Next Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_023", "MP_Xmas2_F_Tat_023",1800),
                new BusinessTattoo(new List<int>(){0}, "Fuck Luck Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_026", "MP_Xmas2_F_Tat_026",1250),
                new BusinessTattoo(new List<int>(){0}, "Fuck Luck Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_027", "MP_Xmas2_F_Tat_027",1250),
                new BusinessTattoo(new List<int>(){0}, "Grenade", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_002_M", "MP_Gunrunning_Tattoo_002_F",1250),
                new BusinessTattoo(new List<int>(){2}, "Have a Nice Day", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_021_M", "MP_Gunrunning_Tattoo_021_F",1780),
                new BusinessTattoo(new List<int>(){1}, "Combat Reaper", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_024_M", "MP_Gunrunning_Tattoo_024_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Single Arrow", "mphipster_overlays", "FM_Hip_M_Tat_001", "FM_Hip_F_Tat_001",1800),
                new BusinessTattoo(new List<int>(){1}, "Bone", "mphipster_overlays", "FM_Hip_M_Tat_004", "FM_Hip_F_Tat_004",1800),
                new BusinessTattoo(new List<int>(){2}, "Cube", "mphipster_overlays", "FM_Hip_M_Tat_008", "FM_Hip_F_Tat_008",1800),
                new BusinessTattoo(new List<int>(){0}, "Horseshoe", "mphipster_overlays", "FM_Hip_M_Tat_010", "FM_Hip_F_Tat_010",1250),
                new BusinessTattoo(new List<int>(){1}, "Spray Can", "mphipster_overlays", "FM_Hip_M_Tat_014", "FM_Hip_F_Tat_014",1800),
                new BusinessTattoo(new List<int>(){1}, "Eye Triangle", "mphipster_overlays", "FM_Hip_M_Tat_017", "FM_Hip_F_Tat_017",1850),
                new BusinessTattoo(new List<int>(){1}, "Origami", "mphipster_overlays", "FM_Hip_M_Tat_018", "FM_Hip_F_Tat_018",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Geo Pattern", "mphipster_overlays", "FM_Hip_M_Tat_020", "FM_Hip_F_Tat_020",3800),
                new BusinessTattoo(new List<int>(){1}, "Pencil", "mphipster_overlays", "FM_Hip_M_Tat_022", "FM_Hip_F_Tat_022",1800),
                new BusinessTattoo(new List<int>(){0}, "Smiley", "mphipster_overlays", "FM_Hip_M_Tat_023", "FM_Hip_F_Tat_023",1300),
                new BusinessTattoo(new List<int>(){2}, "Shapes", "mphipster_overlays", "FM_Hip_M_Tat_036", "FM_Hip_F_Tat_036",1800),
                new BusinessTattoo(new List<int>(){2}, "Triangle Black", "mphipster_overlays", "FM_Hip_M_Tat_044", "FM_Hip_F_Tat_044",1800),
                new BusinessTattoo(new List<int>(){1}, "Mesh Band", "mphipster_overlays", "FM_Hip_M_Tat_045", "FM_Hip_F_Tat_045",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Mechanical Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_003_M", "MP_MP_ImportExport_Tat_003_F",3800),
                new BusinessTattoo(new List<int>(){1,2}, "Dialed In", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_005_M", "MP_MP_ImportExport_Tat_005_F",3850),
                new BusinessTattoo(new List<int>(){1,2}, "Engulfed Block", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_006_M", "MP_MP_ImportExport_Tat_006_F",3800),
                new BusinessTattoo(new List<int>(){1,2}, "Drive Forever", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_007_M", "MP_MP_ImportExport_Tat_007_F",3800),
                new BusinessTattoo(new List<int>(){1}, "Seductress", "mplowrider_overlays", "MP_LR_Tat_015_M", "MP_LR_Tat_015_F",1980),
                new BusinessTattoo(new List<int>(){2}, "Swooping Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_007_M", "MP_MP_Biker_Tat_007_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Lady Mortality", "mpbiker_overlays", "MP_MP_Biker_Tat_014_M", "MP_MP_Biker_Tat_014_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Eagle Emblem", "mpbiker_overlays", "MP_MP_Biker_Tat_033_M", "MP_MP_Biker_Tat_033_F",1980),
                new BusinessTattoo(new List<int>(){1}, "Grim Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_042_M", "MP_MP_Biker_Tat_042_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Skull Chain", "mpbiker_overlays", "MP_MP_Biker_Tat_046_M", "MP_MP_Biker_Tat_046_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Snake Bike", "mpbiker_overlays", "MP_MP_Biker_Tat_047_M", "MP_MP_Biker_Tat_047_F",3800),
                new BusinessTattoo(new List<int>(){2}, "These Colors Don't Run", "mpbiker_overlays", "MP_MP_Biker_Tat_049_M", "MP_MP_Biker_Tat_049_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Mum", "mpbiker_overlays", "MP_MP_Biker_Tat_054_M", "MP_MP_Biker_Tat_054_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Lady Vamp", "mplowrider2_overlays", "MP_LR_Tat_003_M", "MP_LR_Tat_003_F",1780),
                new BusinessTattoo(new List<int>(){2}, "Loving Los Muertos", "mplowrider2_overlays", "MP_LR_Tat_028_M", "MP_LR_Tat_028_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Black Tears", "mplowrider2_overlays", "MP_LR_Tat_035_M", "MP_LR_Tat_035_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Floral Raven", "mpluxe_overlays", "MP_LUXE_TAT_004_M", "MP_LUXE_TAT_004_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Mermaid Harpist", "mpluxe_overlays", "MP_LUXE_TAT_013_M", "MP_LUXE_TAT_013_F",3800),
                new BusinessTattoo(new List<int>(){2}, "Geisha Bloom", "mpluxe_overlays", "MP_LUXE_TAT_019_M", "MP_LUXE_TAT_019_F",1780),
                new BusinessTattoo(new List<int>(){1}, "Intrometric", "mpluxe2_overlays", "MP_LUXE_TAT_010_M", "MP_LUXE_TAT_010_F",1780),
                new BusinessTattoo(new List<int>(){2}, "Heavenly Deity", "mpluxe2_overlays", "MP_LUXE_TAT_017_M", "MP_LUXE_TAT_017_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Floral Print", "mpluxe2_overlays", "MP_LUXE_TAT_026_M", "MP_LUXE_TAT_026_F",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Geometric Design RA", "mpluxe2_overlays", "MP_LUXE_TAT_030_M", "MP_LUXE_TAT_030_F",3800),
                new BusinessTattoo(new List<int>(){1}, "Crackshot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_001_M", "MP_Smuggler_Tattoo_001_F",1800),
                new BusinessTattoo(new List<int>(){2}, "Mutiny", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_005_M", "MP_Smuggler_Tattoo_005_F",1980),
                new BusinessTattoo(new List<int>(){1,2}, "Stylized Kraken", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_023_M", "MP_Smuggler_Tattoo_023_F",3800),
                new BusinessTattoo(new List<int>(){1}, "Poison Wrench", "mpstunt_overlays", "MP_MP_Stunt_Tat_003_M", "MP_MP_Stunt_Tat_003_F",1750),
                new BusinessTattoo(new List<int>(){2}, "Arachnid of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_009_M", "MP_MP_Stunt_Tat_009_F",1850),
                new BusinessTattoo(new List<int>(){2}, "Grave Vulture", "mpstunt_overlays", "MP_MP_Stunt_Tat_010_M", "MP_MP_Stunt_Tat_010_F",1780),
                new BusinessTattoo(new List<int>(){1,2}, "Coffin Racer", "mpstunt_overlays", "MP_MP_Stunt_Tat_016_M", "MP_MP_Stunt_Tat_016_F",3800),
                new BusinessTattoo(new List<int>(){0}, "Biker Stallion", "mpstunt_overlays", "MP_MP_Stunt_Tat_036_M", "MP_MP_Stunt_Tat_036_F",1250),
                new BusinessTattoo(new List<int>(){1}, "One Down Five Up", "mpstunt_overlays", "MP_MP_Stunt_Tat_038_M", "MP_MP_Stunt_Tat_038_F",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Seductive Mechanic", "mpstunt_overlays", "MP_MP_Stunt_Tat_049_M", "MP_MP_Stunt_Tat_049_F",3800),
                new BusinessTattoo(new List<int>(){2}, "Grim Reaper Smoking Gun", "multiplayer_overlays", "FM_Tat_Award_M_002", "FM_Tat_Award_F_002",1850),
                new BusinessTattoo(new List<int>(){1}, "Ride or Die RA", "multiplayer_overlays", "FM_Tat_Award_M_010", "FM_Tat_Award_F_010",1800),
                new BusinessTattoo(new List<int>(){1,2}, "Brotherhood", "multiplayer_overlays", "FM_Tat_M_000", "FM_Tat_F_000",3800),
                new BusinessTattoo(new List<int>(){1,2}, "Dragons", "multiplayer_overlays", "FM_Tat_M_001", "FM_Tat_F_001",3800),
                new BusinessTattoo(new List<int>(){2}, "Dragons and Skull", "multiplayer_overlays", "FM_Tat_M_003", "FM_Tat_F_003",1850),
                new BusinessTattoo(new List<int>(){1,2}, "Flower Mural", "multiplayer_overlays", "FM_Tat_M_014", "FM_Tat_F_014",3800),
                new BusinessTattoo(new List<int>(){1,2,0}, "Serpent Skull RA", "multiplayer_overlays", "FM_Tat_M_018", "FM_Tat_F_018",4500),
                new BusinessTattoo(new List<int>(){2}, "Virgin Mary", "multiplayer_overlays", "FM_Tat_M_027", "FM_Tat_F_027",1850),
                new BusinessTattoo(new List<int>(){1}, "Mermaid", "multiplayer_overlays", "FM_Tat_M_028", "FM_Tat_F_028",1850),
                new BusinessTattoo(new List<int>(){1}, "Dagger", "multiplayer_overlays", "FM_Tat_M_038", "FM_Tat_F_038",1800),
                new BusinessTattoo(new List<int>(){2}, "Lion", "multiplayer_overlays", "FM_Tat_M_047", "FM_Tat_F_047",1800)
            },
            #endregion Правая рука
            #region Левая нога
            new List<BusinessTattoo>()
            {
                // iTeffa => 0 - До колена | 1 - Выше колена
                new BusinessTattoo(new List<int>(){0},"One-armed Bandit", "mpvinewood_overlays", "MP_Vinewood_Tat_013_M", "MP_Vinewood_Tat_013_F",1850),
                new BusinessTattoo(new List<int>(){0}, "8-Ball Rose", "mpvinewood_overlays", "MP_Vinewood_Tat_027_M", "MP_Vinewood_Tat_027_F",2500),
                new BusinessTattoo(new List<int>(){0}, "Single", "mpbusiness_overlays", "", "MP_Buis_F_LLeg_000",1850),
                new BusinessTattoo(new List<int>(){0}, "Spider Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_001", "MP_Xmas2_F_Tat_001",1850),
                new BusinessTattoo(new List<int>(){0}, "Spider Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_002", "MP_Xmas2_F_Tat_002",1850),
                new BusinessTattoo(new List<int>(){0}, "Patriot Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_005_M", "MP_Gunrunning_Tattoo_005_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Stylized Tiger", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_007_M", "MP_Gunrunning_Tattoo_007_F",1800),
                new BusinessTattoo(new List<int>(){0,1}, "Death Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_011_M", "MP_Gunrunning_Tattoo_011_F",3500),
                new BusinessTattoo(new List<int>(){1}, "Rose Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_023_M", "MP_Gunrunning_Tattoo_023_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Squares", "mphipster_overlays", "FM_Hip_M_Tat_009", "FM_Hip_F_Tat_009",1800),
                new BusinessTattoo(new List<int>(){0}, "Charm", "mphipster_overlays", "FM_Hip_M_Tat_019", "FM_Hip_F_Tat_019",1850),
                new BusinessTattoo(new List<int>(){0}, "Black Anchor", "mphipster_overlays", "FM_Hip_M_Tat_040", "FM_Hip_F_Tat_040",1800),
                new BusinessTattoo(new List<int>(){0}, "LS Serpent", "mplowrider_overlays", "MP_LR_Tat_007_M", "MP_LR_Tat_007_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Presidents", "mplowrider_overlays", "MP_LR_Tat_020_M", "MP_LR_Tat_020_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Rose Tribute", "mpbiker_overlays", "MP_MP_Biker_Tat_002_M", "MP_MP_Biker_Tat_002_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Ride or Die LL", "mpbiker_overlays", "MP_MP_Biker_Tat_015_M", "MP_MP_Biker_Tat_015_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Bad Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_027_M", "MP_MP_Biker_Tat_027_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Engulfed Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_036_M", "MP_MP_Biker_Tat_036_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Scorched Soul", "mpbiker_overlays", "MP_MP_Biker_Tat_037_M", "MP_MP_Biker_Tat_037_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Ride Free", "mpbiker_overlays", "MP_MP_Biker_Tat_044_M", "MP_MP_Biker_Tat_044_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Bone Cruiser", "mpbiker_overlays", "MP_MP_Biker_Tat_056_M", "MP_MP_Biker_Tat_056_F",1850),
                new BusinessTattoo(new List<int>(){0,1}, "Laughing Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_057_M", "MP_MP_Biker_Tat_057_F",3500),
                new BusinessTattoo(new List<int>(){0}, "Death Us Do Part", "mplowrider2_overlays", "MP_LR_Tat_029_M", "MP_LR_Tat_029_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Serpent of Death", "mpluxe_overlays", "MP_LUXE_TAT_000_M", "MP_LUXE_TAT_000_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Cross of Roses", "mpluxe2_overlays", "MP_LUXE_TAT_011_M", "MP_LUXE_TAT_011_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Dagger Devil", "mpstunt_overlays", "MP_MP_Stunt_Tat_007_M", "MP_MP_Stunt_Tat_007_F",1780),
                new BusinessTattoo(new List<int>(){1}, "Dirt Track Hero", "mpstunt_overlays", "MP_MP_Stunt_Tat_013_M", "MP_MP_Stunt_Tat_013_F",1800),
                new BusinessTattoo(new List<int>(){0,1}, "Golden Cobra", "mpstunt_overlays", "MP_MP_Stunt_Tat_021_M", "MP_MP_Stunt_Tat_021_F",3500),
                new BusinessTattoo(new List<int>(){0}, "Quad Goblin", "mpstunt_overlays", "MP_MP_Stunt_Tat_028_M", "MP_MP_Stunt_Tat_028_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Stunt Jesus", "mpstunt_overlays", "MP_MP_Stunt_Tat_031_M", "MP_MP_Stunt_Tat_031_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Dragon and Dagger", "multiplayer_overlays", "FM_Tat_Award_M_009", "FM_Tat_Award_F_009",1850),
                new BusinessTattoo(new List<int>(){0}, "Melting Skull", "multiplayer_overlays", "FM_Tat_M_002", "FM_Tat_F_002",1850),
                new BusinessTattoo(new List<int>(){0}, "Dragon Mural", "multiplayer_overlays", "FM_Tat_M_008", "FM_Tat_F_008",1850),
                new BusinessTattoo(new List<int>(){0}, "Serpent Skull LL", "multiplayer_overlays", "FM_Tat_M_021", "FM_Tat_F_021",1850),
                new BusinessTattoo(new List<int>(){0}, "Hottie", "multiplayer_overlays", "FM_Tat_M_023", "FM_Tat_F_023",1850),
                new BusinessTattoo(new List<int>(){0}, "Smoking Dagger", "multiplayer_overlays", "FM_Tat_M_026", "FM_Tat_F_026",1850),
                new BusinessTattoo(new List<int>(){0}, "Faith LL", "multiplayer_overlays", "FM_Tat_M_032", "FM_Tat_F_032",1850),
                new BusinessTattoo(new List<int>(){0,1}, "Chinese Dragon", "multiplayer_overlays", "FM_Tat_M_033", "FM_Tat_F_033",3500),
                new BusinessTattoo(new List<int>(){0}, "Dragon LL", "multiplayer_overlays", "FM_Tat_M_035", "FM_Tat_F_035",1800),
                new BusinessTattoo(new List<int>(){0}, "Grim Reaper", "multiplayer_overlays", "FM_Tat_M_037", "FM_Tat_F_037",1850)
            },
            #endregion Левая нога
            #region Правая нога
            new List<BusinessTattoo>()
            {
	            // iTeffa => 0 - До колена | 1 - Выше колена.
                new BusinessTattoo(new List<int>(){0},"Cash is King", "mpvinewood_overlays", "MP_Vinewood_Tat_020_M", "MP_Vinewood_Tat_020_F",2500),
                new BusinessTattoo(new List<int>(){0}, "Diamond Crown", "mpbusiness_overlays", "", "MP_Buis_F_RLeg_000",1800),
                new BusinessTattoo(new List<int>(){0}, "Floral Dagger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_014", "MP_Xmas2_F_Tat_014",1750),
                new BusinessTattoo(new List<int>(){0}, "Combat Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_006_M", "MP_Gunrunning_Tattoo_006_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Restless Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_026_M", "MP_Gunrunning_Tattoo_026_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Pistol Ace", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_030_M", "MP_Gunrunning_Tattoo_030_F",16850),
                new BusinessTattoo(new List<int>(){0}, "Grub", "mphipster_overlays", "FM_Hip_M_Tat_038", "FM_Hip_F_Tat_038",1800),
                new BusinessTattoo(new List<int>(){0}, "Sparkplug", "mphipster_overlays", "FM_Hip_M_Tat_042", "FM_Hip_F_Tat_042",1800),
                new BusinessTattoo(new List<int>(){0}, "Ink Me", "mplowrider_overlays", "MP_LR_Tat_017_M", "MP_LR_Tat_017_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Dance of Hearts", "mplowrider_overlays", "MP_LR_Tat_023_M", "MP_LR_Tat_023_F",1850),
                new BusinessTattoo(new List<int>(){0,1}, "Dragon's Fury", "mpbiker_overlays", "MP_MP_Biker_Tat_004_M", "MP_MP_Biker_Tat_004_F",3500),
                new BusinessTattoo(new List<int>(){0}, "Western Insignia", "mpbiker_overlays", "MP_MP_Biker_Tat_022_M", "MP_MP_Biker_Tat_022_F",1800),
                new BusinessTattoo(new List<int>(){1}, "Dusk Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_028_M", "MP_MP_Biker_Tat_028_F",1800),
                new BusinessTattoo(new List<int>(){1}, "American Made", "mpbiker_overlays", "MP_MP_Biker_Tat_040_M", "MP_MP_Biker_Tat_040_F",1850),
                new BusinessTattoo(new List<int>(){0}, "STFU", "mpbiker_overlays", "MP_MP_Biker_Tat_048_M", "MP_MP_Biker_Tat_048_F",1800),
                new BusinessTattoo(new List<int>(){0}, "San Andreas Prayer", "mplowrider2_overlays", "MP_LR_Tat_030_M", "MP_LR_Tat_030_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Elaborate Los Muertos", "mpluxe_overlays", "MP_LUXE_TAT_001_M", "MP_LUXE_TAT_001_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Starmetric", "mpluxe2_overlays", "MP_LUXE_TAT_023_M", "MP_LUXE_TAT_023_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Homeward Bound", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_020_M", "MP_Smuggler_Tattoo_020_F",3500),
                new BusinessTattoo(new List<int>(){0}, "Demon Spark Plug", "mpstunt_overlays", "MP_MP_Stunt_Tat_005_M", "MP_MP_Stunt_Tat_005_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Praying Gloves", "mpstunt_overlays", "MP_MP_Stunt_Tat_015_M", "MP_MP_Stunt_Tat_015_F",1850),
                new BusinessTattoo(new List<int>(){0}, "Piston Angel", "mpstunt_overlays", "MP_MP_Stunt_Tat_020_M", "MP_MP_Stunt_Tat_020_F",1850),
                new BusinessTattoo(new List<int>(){1}, "Speed Freak", "mpstunt_overlays", "MP_MP_Stunt_Tat_025_M", "MP_MP_Stunt_Tat_025_F",1800),
                new BusinessTattoo(new List<int>(){0}, "Wheelie Mouse", "mpstunt_overlays", "MP_MP_Stunt_Tat_032_M", "MP_MP_Stunt_Tat_032_F",1750),
                new BusinessTattoo(new List<int>(){0,1}, "Severed Hand", "mpstunt_overlays", "MP_MP_Stunt_Tat_045_M", "MP_MP_Stunt_Tat_045_F",3500),
                new BusinessTattoo(new List<int>(){0}, "Brake Knife", "mpstunt_overlays", "MP_MP_Stunt_Tat_047_M", "MP_MP_Stunt_Tat_047_F",1750),
                new BusinessTattoo(new List<int>(){0}, "Skull and Sword", "multiplayer_overlays", "FM_Tat_Award_M_006", "FM_Tat_Award_F_006",1850),
                new BusinessTattoo(new List<int>(){0}, "The Warrior", "multiplayer_overlays", "FM_Tat_M_007", "FM_Tat_F_007",1850),
                new BusinessTattoo(new List<int>(){0}, "Tribal", "multiplayer_overlays", "FM_Tat_M_017", "FM_Tat_F_017",1800),
                new BusinessTattoo(new List<int>(){0}, "Fiery Dragon", "multiplayer_overlays", "FM_Tat_M_022", "FM_Tat_F_022",1850),
                new BusinessTattoo(new List<int>(){0}, "Broken Skull", "multiplayer_overlays", "FM_Tat_M_039", "FM_Tat_F_039",1850),
                new BusinessTattoo(new List<int>(){0,1}, "Flaming Skull", "multiplayer_overlays", "FM_Tat_M_040", "FM_Tat_F_040",3400),
                new BusinessTattoo(new List<int>(){0}, "Flaming Scorpion", "multiplayer_overlays", "FM_Tat_M_042", "FM_Tat_F_042",1850),
                new BusinessTattoo(new List<int>(){0}, "Indian Ram", "multiplayer_overlays", "FM_Tat_M_043", "FM_Tat_F_043",1850)
            }
            #endregion Правая нога
        };
        #endregion Татуировки
        #region Тюниг транспорта
        public static Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>> Tuning = new Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>>()
        {
            #region Sultan
            { "Sultan", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 2000),
                    new Tuple<int, string, int>(0, "Титановый глушитель Tuner", 8000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 3000),
                    new Tuple<int, string, int>(0, "Заказные пороги", 9000),
                }},
                { 2, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный капот", 2000),
                    new Tuple<int, string, int>(0, "С двойным забором воздуха", 11000),
                    new Tuple<int, string, int>(1, "Карбоновый капот 1", 14000),
                    new Tuple<int, string, int>(2, "Карбоновый капот 2", 15000),
                    new Tuple<int, string, int>(3, "Капот с воздухозаборником", 16000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Нет", 1000),
                    new Tuple<int, string, int>(0, "Низкий спойлер", 6000),
                    new Tuple<int, string, int>(1, "Поднятый спойлер", 8000),
                    new Tuple<int, string, int>(2, "GT Wing", 12000),
                }},
                { 6, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартная крыша", 5000),
                    new Tuple<int, string, int>(0, "Полоса на лобовое стекло", 2000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Передний сплиттер", 14000),
                    new Tuple<int, string, int>(1, "Сплиттер и интеркулер", 18000),
                }},
                { 9, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 5000),
                    new Tuple<int, string, int>(0, "Карбоновый зад. диффузор", 18000),
                }}
            }},
            #endregion Sultan
            #region Kuruma
            { "Kuruma", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 5000),
                    new Tuple<int, string, int>(0, "Двойной глушитель", 10000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 5000),
                    new Tuple<int, string, int>(0, "Заказные пороги осн.цвета", 11000),
                    new Tuple<int, string, int>(1, "Заказные пороги доп.цвета", 15000),
                    new Tuple<int, string, int>(2, "Заказные карбоновые пороги", 20000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный спойлер", 5000),
                    new Tuple<int, string, int>(0, "Спойлер доп.цвета", 7000),
                    new Tuple<int, string, int>(1, "Низкий карбоновый спойлер", 11000),
                    new Tuple<int, string, int>(2, "Низкий спойлер осн.цвета", 13000),
                    new Tuple<int, string, int>(3, "Средний карбоновый спойлер", 15000),
                    new Tuple<int, string, int>(4, "GT Wing", 25000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Заказной бампер осн.цвета", 11000),
                    new Tuple<int, string, int>(1, "Заказной бампер доп.цвета", 15000),
                    new Tuple<int, string, int>(2, "Заказной карбоновый бампер", 15000),
                }}
            }},
            #endregion Kuruma
            #region Jackal
            { "Jackal", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 5000),
                    new Tuple<int, string, int>(0, "Сдвоенный глушитель", 9000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 5000),
                    new Tuple<int, string, int>(0, "Заказные пороги", 11000),
                }},
                { 2, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный капот", 5000),
                    new Tuple<int, string, int>(0, "Капот с воздухозаборником", 9000),
                    new Tuple<int, string, int>(1, "Капот с забором воздуха", 11000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Нет", 5000),
                    new Tuple<int, string, int>(0, "Заказной спойлер 1", 9000),
                    new Tuple<int, string, int>(1, "Заказной спойлер 2", 13000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Передний сплиттер", 11000),
                }},
                { 9, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 5000),
                    new Tuple<int, string, int>(0, "Заказной задний бампер", 13000),
                }}
            }},
            #endregion Jackal
            #region Surano
            { "Surano", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные глушитель", 5000),
                    new Tuple<int, string, int>(0, "Хромированный глушитель", 13000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 5000),
                    new Tuple<int, string, int>(0, "Заказные пороги", 11000),
                }},
                { 2, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный капот", 5000),
                    new Tuple<int, string, int>(0, "Капот с забором воздуха", 13000),
                    new Tuple<int, string, int>(1, "Карбоновый капот", 15000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный спойлер", 5000),
                    new Tuple<int, string, int>(0, "Крашеный спойлер", 9000),
                    new Tuple<int, string, int>(1, "Поднятый спойлер", 12000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Передний сплиттер", 15000),
                }},
                { 9, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 5000),
                    new Tuple<int, string, int>(0, "Карбоновый зад. диффузор", 17000),
                }}
            }},
            #endregion Surano
            #region Dubsta
            { "Dubsta", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 5000),
                    new Tuple<int, string, int>(0, "Титановый глушитель ", 9000),
                    new Tuple<int, string, int>(1, "Сдвоенный титановый ", 11000),
                }},
                { 2, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный капот", 5000),
                    new Tuple<int, string, int>(0, "Капот внедорожника", 11000),
                    new Tuple<int, string, int>(1, "Капот с запаской", 13000),

                }},
                { 4, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартная решетка", 5000),
                    new Tuple<int, string, int>(0, "Сетчатая решетка", 9000),
                    new Tuple<int, string, int>(1, "Черная решетка", 10000),
                    new Tuple<int, string, int>(2, "Хромированная решетка", 11000),
                }},
                { 5, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартное левое крыло", 5000),
                    new Tuple<int, string, int>(0, "Шноркель", 11000),
                }},
                { 6, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартная крыша", 5000),
                    new Tuple<int, string, int>(0, "Багажник на крыше", 7000),
                    new Tuple<int, string, int>(1, "Багажник с прожекторами", 9000),
                    new Tuple<int, string, int>(2, "Черный багажник на крыше", 11000),
                    new Tuple<int, string, int>(3, "Багажник с прожекторами", 13000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Хромированный кенгурятник", 9000),
                    new Tuple<int, string, int>(1, "Кенгурятник с дугой", 11000),
                    new Tuple<int, string, int>(2, "Кенгурятник с фарами", 13000),
                    new Tuple<int, string, int>(3, "Кенгурятник с дугой и фары", 15000),
                    new Tuple<int, string, int>(4, "Черный кенгурятник", 13000),
                    new Tuple<int, string, int>(5, "Кенгурятник с дугой и фыры", 15000),
                }},
                { 9, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 5000),
                    new Tuple<int, string, int>(0, "Хромированный бампер", 11000),
                    new Tuple<int, string, int>(1, "Черный бампер", 13000),
                }}
            }},
            #endregion Dubsta
            #region Rocoto
            {"Rocoto", new Dictionary<int, List<Tuple<int, string, int>>>() {}}
            #endregion
        };
        #endregion
        #region Тюниг цены
        public static Dictionary<int, Dictionary<string, int>> TuningPrices = new Dictionary<int, Dictionary<string, int>>()
        {
            #region Engine Menu
            { 10, new Dictionary<string, int>() {
                { "-1", 7000 },
                { "0", 9000 },
                { "1", 10500 },
                { "2", 12000 },
                { "3", 14950 },
            }},
            #endregion
            #region Turbo menu
            { 11, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 18000 },
            }},
            #endregion
            #region Horn menu
            { 12, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 3000 },
                { "1", 3900 },
                { "2", 4500 },
                { "3", 4500 },
                { "4", 4500 },
                { "5", 4500 },
                { "6", 4500 },
                { "7", 4500 },
                { "8", 4500 },
                { "9", 4500 },
                { "10", 4500 },
                { "11", 4500 },
                { "12", 4500 },
                { "13", 4500 },
                { "14", 4500 },
                { "15", 4500 },
                { "16", 4500 },
                { "17", 4500 },
                { "18", 4500 },
                { "19", 4500 },
                { "20", 4500 },
                { "21", 4500 },
                { "22", 4500 },
                { "23", 4500 },
                { "24", 4500 },
                { "25", 4500 },
                { "26", 4500 },
                { "27", 4500 },
                { "28", 4500 },
                { "29", 4500 },
                { "30", 4500 },
                { "31", 4500 },
                { "32", 4500 },
                { "33", 4500 },
                { "34", 4500 },
            }},
            #endregion
            #region Transmission menu
            { 13, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 6000 },
                { "1", 10500 },
                { "2", 12000 },
            }},
            #endregion
            #region Glasses menu
            { 14, new Dictionary<string, int>() {
                { "0", 5000 },
                { "3", 4500 },
                { "2", 6000 },
                { "1", 9000 },
            }},
            #endregion
            #region Suspention Menu
            { 15, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 3000 },
                { "1", 6000 },
                { "2", 9000 },
                { "3", 12000 },
            }},
            #endregion
            #region Brakes menu
            { 16, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 4500 },
                { "1", 7000 },
                { "2", 10500 },
            }},
            #endregion
            #region Lights menu
            { 17, new Dictionary<string, int>() {
                { "-1", 5000 },
                { "0", 7000 },
                { "1", 50000 },
                { "2", 50000 },
                { "3", 50000 },
                { "4", 50000 },
                { "5", 50000 },
                { "6", 50000 },
                { "7", 50000 },
                { "8", 50000 },
                { "9", 50000 },
                { "10", 50000 },
                { "11", 50000 },
                { "12", 50000 },
            }},
            #endregion
            #region Numbers menu
            { 18, new Dictionary<string, int>() {
                { "0", 5000 },
                { "1", 3000 },
                { "2", 3000 },
                { "3", 3000 },
                { "4", 3000 },
            }},
            #endregion
        };
        #endregion
        #region Тюниг Колеса
        public static Dictionary<int, Dictionary<int, int>> TuningWheels = new Dictionary<int, Dictionary<int, int>>()
        {
            #region Спортивные
            { 0, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 27600 },
                { 1, 39000 },
                { 2, 42000 },
                { 3, 39600 },
                { 4, 110000 },
                { 5, 42000 },
                { 6, 41400 },
                { 7, 36000 },
                { 8, 36300 },
                { 9, 39000 },
                { 10, 45900 },
                { 11, 36900 },
                { 12, 32700 },
                { 13, 39000 },
                { 14, 33600 },
                { 15, 39600 },
                { 16, 28200 },
                { 17, 4500 },
                { 18, 29700 },
                { 19, 4500 },
                { 20, 39600 },
                { 21, 42000 },
                { 22, 49800 },
                { 23, 36000 },
                { 24, 39000 },
            }},
            #endregion
            #region Маслкары
            { 1, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 3000 },
                { 1, 15000 },
                { 2, 4950 },
                { 3, 18000 },
                { 4, 19500 },
                { 5, 16800 },
                { 6, 17700 },
                { 7, 21000 },
                { 8, 18000 },
                { 9, 21000 },
                { 10, 18000 },
                { 11, 4950 },
                { 12, 15000 },
                { 13, 18000 },
                { 14, 15000 },
                { 15, 18000 },
                { 16, 24000 },
                { 17, 21000 },
            }},
            #endregion
            #region Лоурайдер
            { 2, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 18300 },
                { 1, 19500 },
                { 2, 18300 },
                { 3, 20700 },
                { 4, 21000 },
                { 5, 2160 },
                { 6, 22500 },
                { 7, 24000 },
                { 8, 25500 },
                { 9, 25500 },
                { 10, 4500 },
                { 11, 18000 },
                { 12, 18300 },
                { 13, 21000 },
                { 14, 24000 },
            }},
            #endregion
            #region Вездеход
            { 3, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 18000 },
                { 1, 24000 },
                { 2, 27000 },
                { 3, 30300 },
                { 4, 17100 },
                { 5, 20100 },
                { 6, 26100 },
                { 7, 2160 },
                { 8, 26400 },
                { 9, 30000 },
            }},
            #endregion
            #region Внедорожник
            { 4, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 18000 },
                { 1, 22500 },
                { 2, 18900 },
                { 3, 23700 },
                { 4, 24000 },
                { 5, 27600 },
                { 6, 18900 },
                { 7, 15600 },
                { 8, 26700 },
                { 9, 22200 },
                { 10, 18600 },
                { 11, 19800 },
                { 12, 24000 },
                { 13, 21000 },
                { 14, 24900 },
                { 15, 18600 },
                { 16, 110000 },
            }},
            #endregion
            #region Тюннер
            { 5, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 2160 },
                { 1, 24000 },
                { 2, 24600 },
                { 3, 30600 },
                { 4, 27300 },
                { 5, 26100 },
                { 6, 27600 },
                { 7, 24300 },
                { 8, 27600 },
                { 9, 22500 },
                { 10, 30900 },
                { 11, 24300 },
                { 12, 27600 },
                { 13, 30000 },
                { 14, 29700 },
                { 15, 24600 },
                { 16, 27300 },
                { 17, 28500 },
                { 18, 24600 },
                { 19, 27900 },
                { 20, 28800 },
                { 21, 29100 },
                { 22, 24600 },
                { 23, 21900 },
            }},
            #endregion
            #region Эксклюзивные
            { 7, new Dictionary<int, int>() {
                { -1, 3000 },
                { 0, 36000 },
                { 1, 21000 },
                { 2, 246000 },
                { 3, 2160 },
                { 4, 24000 },
                { 5, 26400 },
                { 6, 36000 },
                { 7, 27000 },
                { 8, 30600 },
                { 9, 30000 },
                { 10, 110000 },
                { 11, 30300 },
                { 12, 36300 },
                { 13, 30300 },
                { 14, 39300 },
                { 15, 36030 },
                { 16, 36300 },
                { 17, 30300 },
                { 18, 110000 },
                { 19, 30300 },
            }},
            #endregion
        };
        #endregion
        #region Колличество Продукции
        public static Dictionary<string, int> ProductsCapacity = new Dictionary<string, int>()
        {
            #region 24/7 Маркет
            { "Бургер", 250},
            { "Хот-Дог", 100},
            { "Сэндвич", 100},
            { "eCola", 100},
            { "Sprunk", 100},
            { "Монтировка", 50},
            { "Фонарик", 50},
            { "Молоток", 50},
            { "Гаечный ключ", 50},
            { "Канистра бензина", 50},
            { "Чипсы", 50},
            { "Пицца", 50},
            { "Сим-карта", 50},
            { "Связка ключей", 50},
            #endregion
            { "Расходники", 800 }, // tattoo shop
            { "Татуировки", 0 },
            { "Парики", 0 }, // barber-shop
            { "Бензин", 20000}, // petrol
            { "Одежда", 7000}, // clothes
            { "Маски", 100}, // masks
            { "Запчасти", 10000}, // ls customs
            { "Средство для мытья", 200 }, // carwash
            { "Pistol", 20}, // gun shop
            { "CombatPistol", 20},
            { "Revolver", 20},
            { "HeavyPistol", 20},
            { "BullpupShotgun", 20},
            { "CombatPDW", 20},
            { "MachinePistol", 20},
            { "Патроны", 5000},
            #region Автосалоны
            {"Sultan", 100},
            {"Kuruma", 100},
            {"Jackal", 100},
            {"Surano", 100},
            {"Dubsta", 100},
            {"Rocoto", 100},
            #endregion Автосалоны
            #region FishShop
            { "Удочка", 10 },
            { "Улучшенная удочка", 10 },
            { "Удочка MK2", 10 },
            { "Наживка", 10 },
            #endregion FishShop
            #region SellShop
            { "Корюшка", 1 },
            { "Кунджа", 1 },
            { "Лосось", 1 },
            { "Окунь", 1 },
            { "Осётр", 1 },
            { "Скат", 1 },
            { "Тунец", 1 },
            { "Угорь", 1 },
            { "Чёрный амур", 1 },
            { "Щука", 1 }
            #endregion SellShop
        };
        #endregion Колличество Продукции
        #region Цена - Продукции
        public static Dictionary<string, int> ProductsOrderPrice = new Dictionary<string, int>()
        {
            #region 24/7 Маркет
            {"Бургер",100},
            {"Хот-Дог",60},
            {"Сэндвич",30},
            {"eCola",20},
            {"Sprunk",30},
            {"Монтировка",200},
            {"Фонарик",240},
            {"Молоток",200},
            {"Гаечный ключ",200},
            {"Канистра бензина",120},
            {"Чипсы",60},
            {"Пицца",100},
            {"Сим-карта",200},
            {"Связка ключей",200},
            #endregion
            {"Расходники",50},
            {"Татуировки",20},
            {"Парики",20},
            {"Бензин",1},
            {"Одежда",50},
            {"Маски",2000},
            {"Запчасти",400},
            {"Средство для мытья",200},
            {"Pistol",720},
            {"CombatPistol",900},
            {"Revolver",12800},
            {"HeavyPistol",1440},
            {"BullpupShotgun",2880},
            {"CombatPDW",3600},
            {"MachinePistol",2160},
            {"Патроны",4},
            #region Автосалоны
            {"Sultan", 100000},
            {"Kuruma", 100000},
            {"Jackal", 100000},
            {"Surano", 100000},
            {"Dubsta", 100000},
            {"Rocoto", 100000},
            #endregion Автосалоны
            #region FishShop
            { "Удочка", 2000 },
            { "Улучшенная удочка", 5000 },
            { "Удочка MK2", 9000 },
            { "Наживка", 45 },
            #endregion FishShop
            #region SellShop
            {"Корюшка",13},
            {"Кунджа",16},
            {"Лосось",10},
            {"Окунь",4},
            {"Осётр",5},
            {"Скат",12},
            {"Тунец",18},
            {"Угорь",5},
            {"Чёрный амур",15},
            {"Щука",6},
            #endregion SellShop
        };
        #endregion
        #region Подключаем список бизнесов
        public static List<Product> fillProductList(int type)
        {
            List<Product> products_list = new List<Product>();
            switch (type)
            {
                case 0:
                    foreach (var name in MarketProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 1:
                    products_list.Add(new Product(ProductsOrderPrice["Бензин"], 0, 0, "Бензин", false));
                    break;
                case 2:
                    foreach (var name in CarsNames[0])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 3:
                    foreach (var name in CarsNames[1])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 4:
                    foreach (var name in CarsNames[2])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 5:
                    foreach (var name in CarsNames[3])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 6:
                    foreach (var name in CarsNames[4])
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 0, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 7:
                    foreach (var name in GunNames)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 5, name, false);
                        products_list.Add(product);
                    }
                    products_list.Add(new Product(ProductsOrderPrice["Патроны"], 0, 5, "Патроны", false));
                    break;
                case 8:
                    products_list.Add(new Product(100, 200, 10, "Одежда", false));
                    break;
                case 9:
                    products_list.Add(new Product(100, 100, 0, "Расходники", false));
                    products_list.Add(new Product(100, 0, 0, "Татуировки", false));
                    break;
                case 10:
                    products_list.Add(new Product(100, 100, 0, "Расходники", false));
                    products_list.Add(new Product(100, 0, 0, "Парики", false));
                    break;
                case 11:
                    products_list.Add(new Product(100, 50, 0, "Маски", false));
                    break;
                case 12:
                    products_list.Add(new Product(100, 1000, 0, "Запчасти", false));
                    break;
                case 13:
                    products_list.Add(new Product(200, 200, 0, "Средство для мытья", false));
                    break;
                case 14:
                    foreach (var name in FishProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
                case 15:
                    foreach (var name in SellProducts)
                    {
                        Product product = new Product(ProductsOrderPrice[name], 0, 1, name, false);
                        products_list.Add(product);
                    }
                    break;
            }
            return products_list;
        }
        #endregion
        #region Список товаров
        public static int GetBuyingItemType(string name)
        {
            var type = -1;
            switch (name)
            {
                case "Монтировка":
                    type = (int)ItemType.Crowbar;
                    break;
                case "Фонарик":
                    type = (int)ItemType.Flashlight;
                    break;
                case "Молоток":
                    type = (int)ItemType.Hammer;
                    break;
                case "Гаечный ключ":
                    type = (int)ItemType.Wrench;
                    break;
                case "Канистра бензина":
                    type = (int)ItemType.GasCan;
                    break;
                case "Чипсы":
                    type = (int)ItemType.Сrisps;
                    break;
                case "Пицца":
                    type = (int)ItemType.Pizza;
                    break;
                case "Бургер":
                    type = (int)ItemType.Burger;
                    break;
                case "Хот-Дог":
                    type = (int)ItemType.HotDog;
                    break;
                case "Сэндвич":
                    type = (int)ItemType.Sandwich;
                    break;
                case "eCola":
                    type = (int)ItemType.eCola;
                    break;
                case "Sprunk":
                    type = (int)ItemType.Sprunk;
                    break;
                case "Связка ключей":
                    type = (int)ItemType.KeyRing;
                    break;
                case "Удочка":
                    type = (int)ItemType.Rod;
                    break;
                case "Улучшенная удочка":
                    type = (int)ItemType.RodUpgrade;
                    break;
                case "Удочка MK2":
                    type = (int)ItemType.RodMK2;
                    break;
                case "Наживка":
                    type = (int)ItemType.Naz;
                    break;
            }
            return type;
        }
        #endregion

        public static void interactionPressed(Player player)
        {
            if (player.GetData<int>("BIZ_ID") == -1) return;
            if (player.HasData("FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вас кто-то тащит за собой", 3000);
                return;
            }
            Business biz = BizList[player.GetData<int>("BIZ_ID")];

            if (biz.Owner != "Государство" && !Main.PlayerNames.ContainsValue(biz.Owner))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Данный {BusinessTypeNames[biz.Type]} в данный момент не работает", 3000);
                return;
            }

            switch (biz.Type)
            {
                case 0:
                    OpenBizShopMenu(player);
                    return;
                case 1:
                    if (!player.IsInVehicle) return;
                    Vehicle vehicle = player.Vehicle;
                    if (vehicle == null) return; //check
                    if (player.VehicleSeat != 0) return;
                    OpenPetrolMenu(player);
                    return;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    if (player.HasData("FOLLOWER"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Отпустите человека", 3000);
                        return;
                    }
                    player.SetData("CARROOMID", biz.ID);
                    CarRoom.enterCarroom(player, biz.Products[0].Name);
                    return;
                case 7:
                    player.SetData("GUNSHOP", biz.ID);
                    OpenGunShopMenu(player);
                    return;
                case 8:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    player.SetData("CLOTHES_SHOP", biz.ID);
                    Trigger.ClientEvent(player, "openClothes", biz.Products[0].Price);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    NAPI.Entity.SetEntityDimension(player, Dimensions.RequestPrivateDimension(player));
                    return;
                case 9:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    player.SetData("BODY_SHOP", biz.ID);
                    Main.Players[player].ExteriorPos = player.Position;
                    var dim = Dimensions.RequestPrivateDimension(player);
                    NAPI.Entity.SetEntityDimension(player, dim);
                    NAPI.Entity.SetEntityPosition(player, new Vector3(324.9798, 180.6418, 103.6665));
                    player.Rotation = new Vector3(0, 0, 101.0228);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ClearClothes(player, Main.Players[player].Gender);

                    Trigger.ClientEvent(player, "openBody", false, biz.Products[1].Price);
                    return;
                case 10:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    player.SetData("BODY_SHOP", biz.ID);
                    dim = Dimensions.RequestPrivateDimension(player);
                    NAPI.Entity.SetEntityDimension(player, dim);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ClearClothes(player, Main.Players[player].Gender);
                    Trigger.ClientEvent(player, "openBody", true, biz.Products[1].Price);
                    return;
                case 11:
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны закончить рабочий день", 3000);
                        return;
                    }
                    player.SetData("MASKS_SHOP", biz.ID);
                    Trigger.ClientEvent(player, "openMasks", biz.Products[0].Price);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    Customization.ApplyMaskFace(player);
                    return;
                case 12:
                    if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в личной машине", 3000);
                        return;
                    }
                    if (player.Vehicle.Class == 13)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Велосипед не может быть затюнингован", 3000);
                        return;
                    }
                    if (player.Vehicle.Class == 8)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                        return;
                    }
                    var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                    if (!Tuning.ContainsKey(vdata.Model))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                        return;
                    }

                    var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                    foreach (var p in occupants)
                    {
                        if (p != player)
                            VehicleManager.WarpPlayerOutOfVehicle(p);
                    }

                    Trigger.ClientEvent(player, "tuningSeatsCheck");
                    return;
                case 13:
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "openDialog", "CARWASH_PAY", $"Вы хотите помыть машину за ${biz.Products[0].Price}$?");
                    return;
                case 14:
                    OpenBizShopMenu(player);
                    return;
                case 15:
                    RodManager.OpenBizSellShopMenu(player);
                    return;
            }
        }

        [RemoteEvent("fishshop")]
        public static void Event_FishShopCallback(Player client, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (client.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[client.GetData<int>("BIZ_ID")];

                var prod = biz.Products[index];

                var type = GetBuyingItemType(prod.Name);

                nItem item = new nItem((ItemType)type);

                var aItem = nInventory.Find(Main.Players[client].UUID, RodManager.GetSellingItemType(prod.Name));
                if (aItem == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет {prod.Name}", 3000);
                    return;
                }

                var prices = prod.Price * Main.pluscost;

                nInventory.Remove(client, RodManager.GetSellingItemType(prod.Name), 1);
                Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы продали {prod.Name}", 3000);
                Finance.Wallet.Change(client, +prices);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prices, $"sellShop");
            }
            catch (Exception e) { Log.Write($"SellShop: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }
        public static void Carwash_Pay(Player player)
        {
            try
            {
                if (player.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[player.GetData<int>("BIZ_ID")];

                if (player.IsInVehicle)
                {
                    if (player.VehicleSeat == 0)
                    {
                        if (VehicleStreaming.GetVehicleDirt(player.Vehicle) >= 0f)
                        {
                            if (Main.Players[player].Money < biz.Products[0].Price)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                                return;
                            }

                            if (!takeProd(biz.ID, 1, "Средство для мытья", biz.Products[0].Price))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно товара на складе", 3000);
                                return;
                            }
                            GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price, "carwash");
                            Finance.Wallet.Change(player, -biz.Products[0].Price);

                            VehicleStreaming.SetVehicleDirt(player.Vehicle, 0.0f);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Ваш транспорт был помыт.", 3000);
                        }
                        else
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.TopCenter, "Ваш транспорт не грязный.", 3000);
                    }
                    else
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Мыть транспорт может только водитель.", 3000);
                }
                return;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return;
            }
        }
        [RemoteEvent("tuningSeatsCheck")]
        public static void RemoteEvent_tuningSeatsCheck(Player player)
        {
            try
            {
                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }
                if (player.Vehicle.Class == 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                    return;
                }
                var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                if (!Tuning.ContainsKey(vdata.Model))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                    return;
                }

                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (player.HasData("FOLLOWING"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }
                Business biz = BizList[player.GetData<int>("BIZ_ID")];

                Main.Players[player].TuningShop = biz.ID;

                var veh = player.Vehicle;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(veh, dim);
                NAPI.Entity.SetEntityDimension(player, dim);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, new Vector3(-337.7784, -136.5316, 39.4032));
                NAPI.Entity.SetEntityRotation(veh, new Vector3(0.04308624, 0.07037075, 148.9986));

                var modelPrice = ProductsOrderPrice[VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model];
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                Trigger.ClientEvent(player, "openTun", biz.Products[0].Price, VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model, modelPriceMod, JsonConvert.SerializeObject(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Components));
            }
            catch (Exception e) { Log.Write("tuningSeatsCheck: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("exitTuning")]
        public static void RemoteEvent_exitTuning(Player player)
        {
            try
            {
                int bizID = Main.Players[player].TuningShop;

                var veh = player.Vehicle;
                NAPI.Entity.SetEntityDimension(veh, 0);
                NAPI.Entity.SetEntityDimension(player, 0);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, BizList[bizID].EnterPoint + new Vector3(0, 0, 1.0));
                VehicleManager.ApplyCustomization(veh);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].TuningShop = -1;
            }
            catch (Exception e) { Log.Write("ExitTuning: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("buyTuning")]
        public static void RemoteEvent_buyTuning(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                int bizID = Main.Players[player].TuningShop;
                Business biz = BizList[bizID];

                var cat = Convert.ToInt32(arguments[0].ToString());
                var id = Convert.ToInt32(arguments[1].ToString());

                var wheelsType = -1;
                var r = 0;
                var g = 0;
                var b = 0;

                if (cat == 19)
                    wheelsType = Convert.ToInt32(arguments[2].ToString());
                else if (cat == 20)
                {
                    r = Convert.ToInt32(arguments[2].ToString());
                    g = Convert.ToInt32(arguments[3].ToString());
                    b = Convert.ToInt32(arguments[4].ToString());
                }

                var vehModel = VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model;

                var modelPrice = ProductsOrderPrice[vehModel];
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                var price = 0;
                if (cat <= 9)
                    price = Convert.ToInt32(Tuning[vehModel][cat].FirstOrDefault(el => el.Item1 == id).Item3 * biz.Products[0].Price / 100.0);
                else if (cat <= 18)
                    price = Convert.ToInt32(TuningPrices[cat][id.ToString()] * modelPriceMod * biz.Products[0].Price / 100.0);
                else if (cat == 19)
                    price = Convert.ToInt32(TuningWheels[wheelsType][id] * biz.Products[0].Price / 100.0);
                else
                    price = Convert.ToInt32(5000 * biz.Products[0].Price / 100.0);

                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вам не хватает ещё {price - Main.Players[player].Money}$ для покупки этой модификации", 3000);
                    Trigger.ClientEvent(player, "tunBuySuccess", -2);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 2000);
                if (amount <= 0) amount = 1;
                if (!takeProd(biz.ID, amount, "Запчасти", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "В данной автомастерской закончились все запчасти", 3000);
                    Trigger.ClientEvent(player, "tunBuySuccess", -2);
                    return;
                }

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, $"buyTuning({player.Vehicle.NumberPlate},{cat},{id})");
                Finance.Wallet.Change(player, -price);
                Trigger.ClientEvent(player, "tunBuySuccess", id);

                var number = player.Vehicle.NumberPlate;

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
                        player.Vehicle.SetSharedData("hlcolor", id);
                        Trigger.ClientEvent(player, "VehStream_SetVehicleHeadLightColor", player.Vehicle.Handle, id);
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        VehicleManager.Vehicles[number].Components.WheelsType = wheelsType;
                        break;
                    case 20:
                        if (id == 0)
                            VehicleManager.Vehicles[number].Components.PrimColor = new Color(r, g, b);
                        else if (id == 1)
                            VehicleManager.Vehicles[number].Components.SecColor = new Color(r, g, b);
                        else
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(r, g, b);
                        break;
                }
                VehicleManager.Save(number);
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы купили и установили данную модификацию", 3000);
                Trigger.ClientEvent(player, "tuningUpd", JsonConvert.SerializeObject(VehicleManager.Vehicles[number].Components));
            }
            catch (Exception e) { Log.Write("buyTuning: " + e.Message, nLog.Type.Error); }
        }
        public static bool takeProd(int bizid, int amount, string prodname, int addMoney)
        {
            try
            {
                Business biz = BizList[bizid];
                foreach (var p in biz.Products)
                {
                    if (p.Name != prodname) continue;
                    if (p.Lefts - amount < 0)
                        return false;

                    p.Lefts -= amount;

                    if (biz.Owner == "Государство") break;
                    Bank.Data bData = Bank.Get(Main.PlayerBankAccs[biz.Owner]);
                    if (bData.ID == 0)
                    {
                        Log.Write($"TakeProd BankID error: {bizid.ToString()}({biz.Owner}) {amount.ToString()} {prodname} {addMoney.ToString()}", nLog.Type.Error);
                        return false;
                    }
                    if (!Bank.Change(bData.ID, addMoney, false))
                    {
                        Log.Write($"TakeProd error: {bizid.ToString()}({biz.Owner}) {amount.ToString()} {prodname} {addMoney.ToString()}", nLog.Type.Error);
                        return false;
                    }
                    GameLog.Money($"biz({bizid})", $"bank({bData.ID})", addMoney, "bizProfit");
                    Log.Write($"{biz.Owner}'s business get {addMoney}$ for '{prodname}'", nLog.Type.Success);
                    break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static int getPriceOfProd(int bizid, string prodname)
        {
            Business biz = BizList[bizid];
            var price = 0;
            foreach (var p in biz.Products)
            {
                if (p.Name == prodname)
                {
                    price = p.Price;
                    break;
                }
            }
            return price;
        }
        public static Vector3 getNearestBiz(Player player, int type)
        {
            Vector3 nearestBiz = new Vector3();
            foreach (var b in BizList)
            {
                Business biz = BizList[b.Key];
                if (biz.Type != type) continue;
                if (nearestBiz == null) nearestBiz = biz.EnterPoint;
                if (player.Position.DistanceTo(biz.EnterPoint) < player.Position.DistanceTo(nearestBiz))
                    nearestBiz = biz.EnterPoint;
            }
            return nearestBiz;
        }
        private static List<int> clothesOutgo = new List<int>()
        {
            1, // Головные уборы
            4, // Верхняя одежда
            3, // Нижняя одежда
            2, // Треники abibas
            1, // Кеды нike
        };
        [RemoteEvent("cancelMasks")]
        public static void RemoteEvent_cancelMasks(Player player)
        {
            try
            {
                player.StopAnimation();
                Customization.ApplyCharacter(player);
                Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
            }
            catch (Exception e) { Log.Write("cancelMasks: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("buyMasks")]
        public static void RemoteEvent_buyMasks(Player player, int variation, int texture)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("MASKS_SHOP")];
                var prod = biz.Products[0];

                var tempPrice = Customization.Masks.FirstOrDefault(f => f.Variation == variation).Price;

                var price = Convert.ToInt32((tempPrice / 100.0) * prod.Price);

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (!takeProd(biz.ID, 1, "Маски", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно товара на складе", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, "buyMask");
                Finance.Wallet.Change(player, -price);

                Customization.AddClothes(player, ItemType.Mask, variation, texture);

                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы купили новую маску. Она была добавлена в Ваш инвентарь.", 3000);
                return;
            }
            catch (Exception e) { Log.Write("buyMasks: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("cancelClothes")]
        public static void RemoteEvent_cancelClothes(Player player)
        {
            try
            {
                player.StopAnimation();
                Customization.ApplyCharacter(player);
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
            }
            catch (Exception e) { Log.Write("cancelClothes: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("buyClothes")]
        public static void RemoteEvent_buyClothes(Player player, int type, int variation, int texture)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("CLOTHES_SHOP")];
                var prod = biz.Products[0];

                var tempPrice = 0;
                switch (type)
                {
                    case 0:
                        tempPrice = Customization.Hats[Main.Players[player].Gender].FirstOrDefault(h => h.Variation == variation).Price;
                        break;
                    case 1:
                        tempPrice = Customization.Tops[Main.Players[player].Gender].FirstOrDefault(t => t.Variation == variation).Price;
                        break;
                    case 2:
                        tempPrice = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(h => h.Value.Top == variation).Value.Price;
                        break;
                    case 3:
                        tempPrice = Customization.Legs[Main.Players[player].Gender].FirstOrDefault(l => l.Variation == variation).Price;
                        break;
                    case 4:
                        tempPrice = Customization.Feets[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 5:
                        tempPrice = Customization.Gloves[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 6:
                        tempPrice = Customization.Accessories[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 7:
                        tempPrice = Customization.Glasses[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                    case 8:
                        tempPrice = Customization.Jewerly[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                        break;
                }
                var price = Convert.ToInt32((tempPrice / 100.0) * prod.Price);

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 50);
                if (amount <= 0) amount = 1;
                if (!takeProd(biz.ID, amount, "Одежда", price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно товара на складе", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", price, "buyClothes");
                Finance.Wallet.Change(player, -price);

                switch (type)
                {
                    case 0:
                        Customization.AddClothes(player, ItemType.Hat, variation, texture);
                        break;
                    case 1:
                        Customization.AddClothes(player, ItemType.Top, variation, texture);
                        break;
                    case 2:
                        var id = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(u => u.Value.Top == variation);
                        Customization.AddClothes(player, ItemType.Undershit, id.Key, texture);
                        break;
                    case 3:
                        Customization.AddClothes(player, ItemType.Leg, variation, texture);
                        break;
                    case 4:
                        Customization.AddClothes(player, ItemType.Feet, variation, texture);
                        break;
                    case 5:
                        Customization.AddClothes(player, ItemType.Gloves, variation, texture);
                        break;
                    case 6:
                        Customization.AddClothes(player, ItemType.Accessories, variation, texture);
                        break;
                    case 7:
                        Customization.AddClothes(player, ItemType.Glasses, variation, texture);
                        break;
                    case 8:
                        Customization.AddClothes(player, ItemType.Jewelry, variation, texture);
                        break;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы купили новую одежду. Она была добавлена в Ваш инвентарь.", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyClothes: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("cancelBody")]
        public static void RemoteEvent_cancelTattoo(Player player)
        {
            try
            {
                Business biz = BizList[player.GetData<int>("BODY_SHOP")];
                NAPI.Entity.SetEntityDimension(player, 0);
                NAPI.Entity.SetEntityPosition(player, biz.EnterPoint + new Vector3(0, 0, 1.12));
                Main.Players[player].ExteriorPos = new Vector3();
                Customization.ApplyCharacter(player);
            }
            catch (Exception e) { Log.Write("CancelBody: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("buyTattoo")]
        public static void RemoteEvent_buyTattoo(Player player, params object[] arguments)
        {
            try
            {
                var zone = Convert.ToInt32(arguments[0].ToString());
                var tattooID = Convert.ToInt32(arguments[1].ToString());
                var tattoo = BusinessTattoos[zone][tattooID];
                Log.Debug($"buyTattoo zone: {zone} | id: {tattooID}");

                Business biz = BizList[player.GetData<int>("BODY_SHOP")];

                var prod = biz.Products.FirstOrDefault(p => p.Name == "Татуировки");
                player.SendChatMessage(" prod" + prod);
                double price = tattoo.Price / 100.0 * prod.Price;
                if (Main.Players[player].Money < Convert.ToInt32(price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 100);
                if (amount <= 0) amount = 1;
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", Convert.ToInt32(price), "buyTattoo");
                Finance.Wallet.Change(player, -Convert.ToInt32(price));

                var tattooHash = (Main.Players[player].Gender) ? tattoo.MaleHash : tattoo.FemaleHash;
                List<Tattoo> validTattoos = new List<Tattoo>();
                foreach (var t in Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone])
                {
                    var isValid = true;
                    foreach (var slot in tattoo.Slots)
                    {
                        if (t.Slots.Contains(slot))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid) validTattoos.Add(t);
                }

                validTattoos.Add(new Tattoo(tattoo.Dictionary, tattooHash, tattoo.Slots));
                Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone] = validTattoos;

                player.SetSharedData("TATTOOS", JsonConvert.SerializeObject(Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos));

                Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вам набили татуировку {tattoo.Name} за {Convert.ToInt32(price)}$", 3000);
            }
            catch (Exception e) { Log.Write("BuyTattoo: " + e.Message, nLog.Type.Error); }
        }
        public static Dictionary<string, List<int>> BarberPrices = new Dictionary<string, List<int>>()
        {
            { "hair", new List<int>() {
                400,
                350,
                350,
                450,
                450,
                600,
                450,
                1100,
                450,
                600,
                600,
                400,
                350,
                2000,
                750,
                1500,
                450,
                600,
                600,
                400,
                350,
                2000,
                750,
                1500,
            }},
            { "beard", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
            }},
            { "eyebrows", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "chesthair", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "lenses", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                1000,
            }},
            { "lipstick", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                300,
            }},
            { "blush", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
            }},
            { "makeup", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
            }},
        };
        [RemoteEvent("buyBarber")]
        public static void RemoteEvent_buyBarber(Player player, string id, int style, int color)
        {
            try
            {
                Log.Debug($"buyBarber: id - {id} | style - {style} | color - {color}");

                Business biz = BizList[player.GetData<int>("BODY_SHOP")];

                if ((id == "lipstick" || id == "blush" || id == "makeup") && Main.Players[player].Gender && style != 255)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Доступно только для персонажей женского пола", 3000);
                    return;
                }

                var prod = biz.Products.FirstOrDefault(p => p.Name == "Парики");
                double price;
                if (id == "hair")
                {
                    if (style >= 23) price = BarberPrices[id][23] / 100.0 * prod.Price;
                    else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod.Price : BarberPrices[id][style] / 100.0 * prod.Price;
                }
                else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod.Price : BarberPrices[id][style] / 100.0 * prod.Price;
                if (Main.Players[player].Money < Convert.ToInt32(price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                    return;
                }
                if (!takeProd(biz.ID, 1, "Расходники", Convert.ToInt32(price)))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Этот барбер-шоп не может оказать эту услугу в данный момент", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", Convert.ToInt32(price), "buyBarber");
                Finance.Wallet.Change(player, -Convert.ToInt32(price));

                switch (id)
                {
                    case "hair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Hair = new HairData(style, color, color);
                        break;
                    case "beard":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[1].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BeardColor = color;
                        break;
                    case "eyebrows":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[2].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyebrowColor = color;
                        break;
                    case "chesthair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[10].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].ChestHairColor = color;
                        break;
                    case "lenses":
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyeColor = style;
                        break;
                    case "lipstick":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[8].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].LipstickColor = color;
                        break;
                    case "blush":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[5].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BlushColor = color;
                        break;
                    case "makeup":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[4].Value = style;
                        break;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы оплатили услугу Барбер-Шопа ({Convert.ToInt32(price)}$)", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyBarber: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("petrol")]
        public static void fillCar(Player player, int lvl)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle == null) return; //check
                if (player.VehicleSeat != 0) return;
                if (lvl <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (!vehicle.HasSharedData("PETROL"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно заправить эту машину", 3000);
                    return;
                }
                if (Globals.VehicleStreaming.GetEngineState(vehicle))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Чтобы начать заправляться - заглушите транспорт.", 3000);
                    return;
                }
                int fuel = vehicle.GetSharedData<int>("PETROL");
                if (fuel >= VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У транспорта полный бак", 3000);
                    return;
                }

                var isGov = false;
                if (lvl == 9999)
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                else if (lvl == 99999)
                {
                    isGov = true;
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                }

                if (lvl < 0) return;

                int tfuel = fuel + lvl;
                if (tfuel > VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Введите корректные данные", 3000);
                    return;
                }
                Business biz = BizList[player.GetData<int>("BIZ_ID")];
                if (isGov)
                {
                    int frac = Main.Players[player].FractionID;
                    if (Fractions.Manager.FractionTypes[frac] != 2)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Чтобы заправить транспорт за гос. счет, Вы должны состоять в гос. организации", 3000);
                        return;
                    }
                    if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") != "FRACTION" || vehicle.GetData<int>("FRACTION") != frac)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете заправить за государственный счет не государственный транспорт", 3000);
                        return;
                    }
                    if (Fractions.Stocks.fracStocks[frac].FuelLeft < lvl * biz.Products[0].Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Лимит на заправку гос. транспорта за день исчерпан", 3000);
                        return;
                    }
                }
                else
                {
                    if (Main.Players[player].Money < lvl * biz.Products[0].Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно средств (не хватает {lvl * biz.Products[0].Price - Main.Players[player].Money}$)", 3000);
                        return;
                    }
                }
                if (!takeProd(biz.ID, lvl, "Бензин", lvl * biz.Products[0].Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"На заправке осталось {biz.Products[0].Lefts}л", 3000);
                    return;
                }
                if (isGov)
                {
                    GameLog.Money($"frac(6)", $"biz({biz.ID})", lvl * biz.Products[0].Price, "buyPetrol");
                    Fractions.Stocks.fracStocks[6].Money -= lvl * biz.Products[0].Price;
                    Fractions.Stocks.fracStocks[Main.Players[player].FractionID].FuelLeft -= lvl * biz.Products[0].Price;
                }
                else
                {
                    GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", lvl * biz.Products[0].Price, "buyPetrol");
                    Finance.Wallet.Change(player, -lvl * biz.Products[0].Price);
                }

                vehicle.SetSharedData("PETROL", tfuel);

                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "PERSONAL")
                {
                    var number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);
                    VehicleManager.Vehicles[number].Fuel += lvl;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Транспорт заправлен", 3000);
                Commands.RPChat("me", player, $"заправил(а) транспортное средство");
            }
            catch (Exception e) { Log.Write("Petrol: " + e.Message, nLog.Type.Error); }
        }
        public static void bizNewPrice(Player player, int price, int BizID)
        {
            if (!Main.Players[player].BizIDs.Contains(BizID)) return;
            Business biz = BizList[BizID];
            var prodName = player.GetData<string>("SELECTPROD");

            double minPrice = (biz.Type == 8 || biz.Type == 11 || biz.Type == 12 || prodName == "Татуировки" || prodName == "Парики"
                || prodName == "Патроны") ? 80 : (biz.Type == 1) ? 2 : ProductsOrderPrice[player.GetData<string>("SELECTPROD")] * 0.8;
            double maxPrice = (biz.Type == 8 || biz.Type == 11 || biz.Type == 12 || prodName == "Татуировки" || prodName == "Парики"
                || prodName == "Патроны") ? 150 : (biz.Type == 1) ? 7 : ProductsOrderPrice[player.GetData<string>("SELECTPROD")] * 1.2;

            if (price < minPrice || price > maxPrice)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно установить такую цену", 3000);
                OpenBizProductsMenu(player);
                return;
            }
            foreach (var p in biz.Products)
            {
                if (p.Name == prodName)
                {
                    p.Price = price;
                    string ch = (biz.Type == 8 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики") ? "%" : "$";

                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Теперь {p.Name} стоит {p.Price}{ch}", 3000);
                    if (p.Name == "Бензин") biz.UpdateLabel();
                    OpenBizProductsMenu(player);
                    return;
                }
            }
        }
        public static void bizOrder(Player player, int amount, int BizID)
        {
            if (!Main.Players[player].BizIDs.Contains(BizID)) return;
            Business biz = BizList[BizID];

            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Неверное значение", 3000);
                OpenBizProductsMenu(player);
                return;
            }

            foreach (var p in biz.Products)
            {
                if (p.Name == player.GetData<string>("SELECTPROD"))
                {
                    if (p.Ordered)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы уже заказали этот товар", 3000);
                        OpenBizProductsMenu(player);
                        return;
                    }

                    if (biz.Type >= 3 && biz.Type <= 6)
                    {
                        if (amount > 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Укажите значение от 1 до 3", 3000);
                            OpenBizProductsMenu(player);
                            return;
                        }
                    }

                    var price = (p.Name == "Патроны") ? 4 : ProductsOrderPrice[p.Name];
                    if (!Bank.Change(Main.Players[player].Bank, -amount * price))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств на счету", 3000);
                        return;
                    }
                    GameLog.Money($"bank({Main.Players[player].Bank})", $"server", amount * price, "bizOrder");
                    var order = new Order(p.Name, amount);
                    p.Ordered = true;

                    var random = new Random();
                    do
                    {
                        order.UID = random.Next(000000, 999999);
                    } while (BusinessManager.Orders.ContainsKey(order.UID));
                    BusinessManager.Orders.Add(order.UID, biz.ID);

                    biz.Orders.Add(order);

                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы заказали {p.Name} в количестве {amount}. №{order.UID}", 3000);
                    player.SendChatMessage($"Номер Вашего заказа: {order.UID}");
                    return;
                }
            }
        }
        public static void buyBusinessCommand(Player player)
        {
            if (!player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться около бизнеса", 3000);
                return;
            }
            int id = player.GetData<int>("BIZ_ID");
            Business biz = BusinessManager.BizList[id];
            if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете приобрести больше {Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl]} бизнесов", 3000);
                return;
            }
            if (biz.Owner == "Государство")
            {
                if (!Finance.Wallet.Change(player, -biz.SellPrice))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас не хватает средств", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"server", biz.SellPrice, $"buyBiz({biz.ID})");
                Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Поздравляем! Вы купили {BusinessManager.BusinessTypeNames[biz.Type]}, не забудьте внести налог за него в банкомате", 3000);
                biz.Owner = player.Name.ToString();
            }
            else if (biz.Owner == player.Name.ToString())
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Этот бизнес принадлежит Вам", 3000);
                return;
            }
            else if (biz.Owner != player.Name.ToString())
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Этот бизнес принадлежит другому игроку", 3000);
                return;
            }

            biz.UpdateLabel();
            foreach (var p in biz.Products)
                p.Lefts = 0;
            var newOrders = new List<Order>();
            foreach (var o in biz.Orders)
            {
                if (o.Taked) newOrders.Add(o);
                else Orders.Remove(o.UID);
            }
            biz.Orders = newOrders;

            Main.Players[player].BizIDs.Add(id);
            var tax = Convert.ToInt32(biz.SellPrice / 10000);
            Finance.Bank.Accounts[biz.BankID].Balance = tax * 2;

            var split = biz.Owner.Split('_');
            Connect.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
            Connect.Query($"UPDATE businesses SET owner='{biz.Owner}' WHERE id='{biz.ID}'");
        }
        public static void createBusinessUnloadpoint(Player player, int bizid)
        {
            if (!Group.CanUseCmd(player, "createunloadpoint")) return;
            var pos = player.Position;
            BizList[bizid].UnloadPoint = pos;
            Connect.Query($"UPDATE businesses SET unloadpoint='{JsonConvert.SerializeObject(pos)}' WHERE id={bizid}");
            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Успешно создана точка разгрузки для бизнеса ID: {bizid}", 3000);
        }
        public static void createBusinessCommand(Player player, int govPrice, int type)
        {
            if (!Group.CanUseCmd(player, "createbusiness")) return;
            var pos = player.Position;
            pos.Z -= 1.12F;
            string productlist = "";
            List<Product> products_list = BusinessManager.fillProductList(type);
            productlist = JsonConvert.SerializeObject(products_list);
            lastBizID++;

            var bankID = Finance.Bank.Create("", 3, 1000);
            Connect.Query($"INSERT INTO businesses (id, owner, sellprice, type, products, enterpoint, unloadpoint, money, mafia, orders) " +
                $"VALUES ({lastBizID}, 'Государство', {govPrice}, {type}, '{productlist}', '{JsonConvert.SerializeObject(pos)}', '{JsonConvert.SerializeObject(new Vector3())}', {bankID}, -1, '{JsonConvert.SerializeObject(new List<Order>())}')");

            Business biz = new Business(lastBizID, "Государство", govPrice, type, products_list, pos, new Vector3(), bankID, -1, new List<Order>());
            biz.UpdateLabel();
            BizList.Add(lastBizID, biz);

            if (type == 7)
            {
                Connect.Query($"INSERT INTO `weapons`(`id`,`lastserial`) VALUES({biz.ID},0)");
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы создали бизнес {BusinessManager.BusinessTypeNames[type]}", 3000);
        }
        public static void deleteBusinessCommand(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "deletebusiness")) return;


            Connect.Query($"DELETE FROM businesses WHERE id={id}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы удалили бизнес", 3000);
            Business biz = BizList.FirstOrDefault(b => b.Value.ID == id).Value;
            if (biz.Type == 7)
            {
                Connect.Query($"DELETE FROM `weapons` WHERE id={id}");
            }
            var owner = NAPI.Player.GetPlayerFromName(biz.Owner);
            if (owner == null)
            {
                var split = biz.Owner.Split('_');
                List<int> ownerBizs = new List<int>();
                var data = Connect.QueryRead($"SELECT biz FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                foreach (DataRow Row in data.Rows) ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                ownerBizs.Remove(biz.ID);
                Connect.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
            }
            else
            {
                Main.Players[owner].BizIDs.Remove(id);
                Wallet.Change(owner, biz.SellPrice);
            }
            biz.Destroy();
            BizList.Remove(biz.ID);
        }
        public static void sellBusinessCommand(Player player, Player target, int price)
        {
            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            if (Main.Players[player].BizIDs.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет бизнеса", 3000);
                return;
            }

            if (Main.Players[target].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[target].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок купил максимум бизнесов", 3000);
                return;
            }

            var biz = BizList[Main.Players[player].BizIDs[0]];
            if (price < biz.SellPrice / 2 || price > biz.SellPrice * 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно продать бизнес за такую цену. Укажите цену от {biz.SellPrice / 2}$ до {biz.SellPrice * 3}$", 3000);
                return;
            }

            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У игрока недостаточно денег", 3000);
                return;
            }

            Trigger.ClientEvent(target, "openDialog", "BUSINESS_BUY", $"{player.Name} предложил Вам купить {BusinessTypeNames[biz.Type]} за ${price}");
            target.SetData("SELLER", player);
            target.SetData("SELLPRICE", price);
            target.SetData("SELLBIZID", biz.ID);

            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы предложили игроку ({target.Value}) купить Ваш бизнес за {price}$", 3000);
        }
        public static void acceptBuyBusiness(Player player)
        {
            Player seller = player.GetData<Player>("SELLER");
            if (!Main.Players.ContainsKey(seller) || !Main.Players.ContainsKey(player)) return;

            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Игрок слишком далеко", 3000);
                return;
            }

            var price = player.GetData<int>("SELLPRICE");
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас недостаточно денег", 3000);
                return;
            }

            Business biz = BizList[player.GetData<int>("SELLBIZID")];
            if (!Main.Players[seller].BizIDs.Contains(biz.ID))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Бизнес больше не принадлежит игроку", 3000);
                return;
            }

            if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас максимальное кол-во бизнесов", 3000);
                return;
            }

            Main.Players[player].BizIDs.Add(biz.ID);
            Main.Players[seller].BizIDs.Remove(biz.ID);

            biz.Owner = player.Name.ToString();
            var split1 = seller.Name.Split('_');
            var split2 = player.Name.Split('_');
            Connect.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[seller].BizIDs)}' WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
            Connect.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split2[0]}' AND lastname='{split2[1]}'");
            Connect.Query($"UPDATE businesses SET owner='{biz.Owner}' WHERE id='{biz.ID}'");
            biz.UpdateLabel();

            Finance.Wallet.Change(player, -price);
            Finance.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"buyBiz({biz.ID})");

            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы купили у {seller.Name.Replace('_', ' ')} {BusinessTypeNames[biz.Type]} за {price}$", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.TopCenter, $"{player.Name.Replace('_', ' ')} купил у Вас {BusinessTypeNames[biz.Type]} за {price}$", 3000);
        }
        #region Menus
        #region manage biz
        public static void OpenBizListMenu(Player player)
        {
            if (Main.Players[player].BizIDs.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет ни одного бизнеса", 3000);
                return;
            }

            Menu menu = new Menu("bizlist", false, false);
            menu.Callback = callback_bizlist;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Ваши бизнесы";
            menu.Add(menuItem);

            foreach (var id in Main.Players[player].BizIDs)
            {
                menuItem = new Menu.Item(id.ToString(), Menu.MenuItem.Button);
                menuItem.Text = BusinessManager.BusinessTypeNames[BusinessManager.BizList[id].Type];
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizlist(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "close":
                    MenuManager.Close(player);
                    return;
                default:
                    OpenBizManageMenu(player, Convert.ToInt32(item.ID));
                    player.SetData("SELECTEDBIZ", Convert.ToInt32(item.ID));
                    return;
            }
        }

        public static void OpenBizManageMenu(Player player, int id)
        {
            if (!Main.Players[player].BizIDs.Contains(id))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас больше нет этого бизнеса", 3000);
                return;
            }

            Menu menu = new Menu("bizmanage", false, false);
            menu.Callback = callback_bizmanage;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Управление бизнесом";
            menu.Add(menuItem);

            menuItem = new Menu.Item("products", Menu.MenuItem.Button);
            menuItem.Text = "Товары";
            menu.Add(menuItem);

            Business biz = BizList[id];
            menuItem = new Menu.Item("tax", Menu.MenuItem.Card);
            menuItem.Text = $"Налог: {Convert.ToInt32(biz.SellPrice / 100 * 0.013)}$/ч";
            menu.Add(menuItem);

            menuItem = new Menu.Item("money", Menu.MenuItem.Card);
            menuItem.Text = $"Счёт бизнеса: {Finance.Bank.Accounts[biz.BankID].Balance}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = "Продать бизнес";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizmanage(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "products":
                    MenuManager.Close(client);
                    OpenBizProductsMenu(client);
                    return;
                case "sell":
                    MenuManager.Close(client);
                    OpenBizSellMenu(client);
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }

        public static void OpenBizSellMenu(Player player)
        {
            Menu menu = new Menu("bizsell", false, false);
            menu.Callback = callback_bizsell;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа";
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];
            var price = biz.SellPrice / 100 * 70;
            menuItem = new Menu.Item("govsell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать государству (${price})";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizsell(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (!client.HasData("SELECTEDBIZ") || !Main.Players[client].BizIDs.Contains(client.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(client);
                return;
            }

            var bizID = client.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];
            switch (item.ID)
            {
                case "govsell":
                    var price = biz.SellPrice / 100 * 70;
                    Finance.Wallet.Change(client, price);
                    GameLog.Money($"server", $"player({Main.Players[client].UUID})", price, $"sellBiz({biz.ID})");

                    Main.Players[client].BizIDs.Remove(bizID);
                    biz.Owner = "Государство";
                    biz.UpdateLabel();

                    Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы продали бизнес государству за {price}$", 3000);
                    MenuManager.Close(client);
                    return;
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, bizID);
                    return;
            }
        }

        public static void OpenBizProductsMenu(Player player)
        {
            if (!player.HasData("SELECTEDBIZ") || !Main.Players[player].BizIDs.Contains(player.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("bizproducts", false, false);
            menu.Callback = callback_bizprod;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Товары";
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");

            Business biz = BizList[bizID];
            foreach (var p in biz.Products)
            {
                menuItem = new Menu.Item(p.Name, Menu.MenuItem.Button);
                menuItem.Text = p.Name;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_bizprod(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, client.GetData<int>("SELECTEDBIZ"));
                    return;
                default:
                    MenuManager.Close(client);
                    OpenBizSettingMenu(client, item.ID);
                    return;
            }
        }

        public static void OpenBizSettingMenu(Player player, string product)
        {
            if (!player.HasData("SELECTEDBIZ") || !Main.Players[player].BizIDs.Contains(player.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(player);
                return;
            }

            Menu menu = new Menu("bizsetting", false, false);
            menu.Callback = callback_bizsetting;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = product;
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");
            Business biz = BizList[bizID];

            foreach (var p in biz.Products)
                if (p.Name == product)
                {
                    string ch = (biz.Type == 8 || biz.Type == 11 || biz.Type == 12 || product == "Татуировки" || product == "Парики" || product == "Патроны") ? "%" : "$";
                    menuItem = new Menu.Item("price", Menu.MenuItem.Card);
                    menuItem.Text = $"Текущая цена: {p.Price}{ch}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("lefts", Menu.MenuItem.Card);
                    menuItem.Text = $"Кол-во на складе: {p.Lefts}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("capacity", Menu.MenuItem.Card);
                    menuItem.Text = $"Вместимость склада: {ProductsCapacity[p.Name]}";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("setprice", Menu.MenuItem.Button);
                    menuItem.Text = "Установить цену";
                    menu.Add(menuItem);

                    var price = (product == "Патроны") ? 4 : ProductsOrderPrice[product];
                    menuItem = new Menu.Item("order", Menu.MenuItem.Button);
                    menuItem.Text = $"Заказать: {price}$/шт";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("cancel", Menu.MenuItem.Button);
                    menuItem.Text = "Отменить заказ";
                    menu.Add(menuItem);

                    menuItem = new Menu.Item("back", Menu.MenuItem.Button);
                    menuItem.Text = "Назад";
                    menu.Add(menuItem);

                    player.SetData("SELECTPROD", product);
                    menu.Open(player);
                    return;
                }
        }
        private static void callback_bizsetting(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {

            var bizID = client.GetData<int>("SELECTEDBIZ");
            switch (item.ID)
            {
                case "setprice":
                    MenuManager.Close(client);
                    if (client.GetData<string>("SELECTPROD") == "Расходники")
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно установить цену на этот товар", 3000);
                        return;
                    }
                    Main.OpenInputMenu(client, "Введите новую цену:", "biznewprice");
                    return;
                case "order":
                    MenuManager.Close(client);
                    if (client.GetData<string>("SELECTPROD") == "Татуировки" || client.GetData<string>("SELECTPROD") == "Парики")
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Если хотите возобновить продажу услуг, то закажите расходные материалы", 3000);
                        return;
                    }
                    Main.OpenInputMenu(client, "Введите кол-во:", "bizorder");
                    return;
                case "cancel":
                    Business biz = BizList[bizID];
                    var prodName = client.GetData<string>("SELECTPROD");

                    foreach (var p in biz.Products)
                    {
                        if (p.Name != prodName) continue;
                        if (p.Ordered)
                        {
                            var order = biz.Orders.FirstOrDefault(o => o.Name == prodName);
                            if (order == null)
                            {
                                p.Ordered = false;
                                return;
                            }
                            if (order.Taked)
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не можете отменить заказ, пока его доставляют", 3000);
                                return;
                            }
                            biz.Orders.Remove(order);
                            p.Ordered = false;

                            Finance.Wallet.Change(client, order.Amount * ProductsOrderPrice[prodName]);
                            GameLog.Money($"server", $"player({Main.Players[client].UUID})", order.Amount * ProductsOrderPrice[prodName], $"orderCancel");
                            Notify.Send(client, NotifyType.Info, NotifyPosition.TopCenter, $"Вы отменили заказ на {prodName}", 3000);
                        }
                        else Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не заказывали этот товар", 3000);
                        return;
                    }
                    return;
                case "back":
                    MenuManager.Close(client);
                    OpenBizManageMenu(client, bizID);
                    return;
            }
        }
        #endregion

        public static void OpenBizShopMenu(Player player)
        {
            Business biz = BizList[player.GetData<int>("BIZ_ID")];
            List<List<string>> items = new List<List<string>>();

            foreach (var p in biz.Products)
            {
                List<string> item = new List<string>();
                item.Add(p.Name);
                item.Add($"{p.Price}$");
                items.Add(item);
            }
            string json = JsonConvert.SerializeObject(items);
            Trigger.ClientEvent(player, "shop", json);
        }
        [RemoteEvent("shop")]
        public static void Event_ShopCallback(Player client, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (client.GetData<int>("BIZ_ID") == -1) return;
                Business biz = BizList[client.GetData<int>("BIZ_ID")];

                var prod = biz.Products[index];
                if (Main.Players[client].Money < prod.Price)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (prod.Name == "Сим-карта")
                {
                    if (!takeProd(biz.ID, 1, prod.Name, prod.Price))
                    {
                        Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно товара на складе", 3000);
                        return;
                    }

                    if (Main.Players[client].Sim != -1) Main.SimCards.Remove(Main.Players[client].Sim);
                    Main.Players[client].Sim = Main.GenerateSimcard(Main.Players[client].UUID);
                    Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили сим-карту с номером {Main.Players[client].Sim}", 3000);
                    Interface.Dashboard.sendStats(client);
                }
                else
                {
                    var type = GetBuyingItemType(prod.Name);
                    if (type != -1)
                    {
                        var tryAdd = nInventory.TryAdd(client, new nItem((ItemType)type));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Ваш инвентарь больше не может вместить {prod.Name}", 3000);
                            return;
                        }
                        else
                        {
                            if (!takeProd(biz.ID, 1, prod.Name, prod.Price))
                            {
                                Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно товара на складе", 3000);
                                return;
                            }
                            nItem item = ((ItemType)type == ItemType.KeyRing) ? new nItem(ItemType.KeyRing, 1, "") : new nItem((ItemType)type);
                            nInventory.Add(client, item);
                        }
                        Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили {prod.Name}", 3000);
                    }
                }
                Finance.Wallet.Change(client, -prod.Price);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prod.Price, $"buyShop");
            }
            catch (Exception e) { Log.Write($"BuyShop: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        public static void OpenPetrolMenu(Player player)
        {
            Business biz = BizList[player.GetData<int>("BIZ_ID")];
            Product prod = biz.Products[0];

            Trigger.ClientEvent(player, "openPetrol");
            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Цена за литр: {prod.Price}$", 7000);
        }
        private static void callback_petrol(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "fill":
                    MenuManager.Close(client);
                    Main.OpenInputMenu(client, "Введите кол-во литров:", "fillcar");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }

        public static void OpenGunShopMenu(Player player)
        {
            List<List<int>> prices = new List<List<int>>();

            Business biz = BizList[player.GetData<int>("GUNSHOP")];
            for (int i = 0; i < 3; i++)
            {
                List<int> p = new List<int>();
                foreach (var g in biz.Products)
                {
                    if (gunsCat[i].Contains(g.Name))
                        p.Add(g.Price);
                }
                prices.Add(p);
            }

            var ammoPrice = biz.Products.FirstOrDefault(p => p.Name == "Патроны").Price;
            prices.Add(new List<int>());
            foreach (var ammo in AmmoPrices)
            {
                prices[3].Add(Convert.ToInt32(ammo));
            }

            string json = JsonConvert.SerializeObject(prices);
            Log.Debug(json);
            Trigger.ClientEvent(player, "openWShop", biz.ID, json);
        }

        [RemoteEvent("wshopammo")]
        public static void Event_WShopAmmo(Player client, string text1, string text2)
        {
            try
            {
                var category = Convert.ToInt32(text1.Replace("wbuyslider", null));
                var needMoney = Convert.ToInt32(text2.Trim('$'));
                var ammo = needMoney / AmmoPrices[category];
                var bizid = client.GetData<int>("GUNSHOP");
                if (!Main.Players[client].Licenses[6])
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет лицензии на оружие", 3000);
                    return;
                }

                if (ammo == 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Вы не указали количество патрон", 3000);
                    return;
                }

                var tryAdd = nInventory.TryAdd(client, new nItem(AmmoTypes[category], ammo));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                Business biz = BizList[bizid];
                var prod = biz.Products.FirstOrDefault(p => p.Name == "Патроны");
                var totalPrice = ammo * AmmoPrices[category];

                if (Main.Players[client].Money < totalPrice)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно средств", 3000);
                    return;
                }

                if (!takeProd(bizid, Convert.ToInt32(AmmoPrices[category] / 10.0 * ammo), prod.Name, totalPrice))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно товара на складе", 3000);
                    return;
                }

                Finance.Wallet.Change(client, -totalPrice);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", totalPrice, $"buyWShop(ammo)");
                nInventory.Add(client, new nItem(AmmoTypes[category], ammo));
                Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили {nInventory.ItemsNames[(int)AmmoTypes[category]]} x{ammo} за {totalPrice}$", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
        }
        private static List<int> AmmoPrices = new List<int>() { 4, 8, 15, 110, 8 };
        private static List<ItemType> AmmoTypes = new List<ItemType>() { ItemType.PistolAmmo, ItemType.SMGAmmo, ItemType.RiflesAmmo, ItemType.SniperAmmo, ItemType.ShotgunsAmmo };

        [RemoteEvent("wshop")]
        public static void Event_WShop(Player client, int cat, int index)
        {
            try
            {
                var prodName = gunsCat[cat][index];
                var bizid = client.GetData<int>("GUNSHOP");
                if (!Main.Players[client].Licenses[6])
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас нет лицензии на оружие", 3000);
                    return;
                }
                Business biz = BizList[bizid];
                var prod = biz.Products.FirstOrDefault(p => p.Name == prodName);

                if (Main.Players[client].Money < prod.Price)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно средств", 3000);
                    return;
                }

                ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), prod.Name);

                var tryAdd = nInventory.TryAdd(client, new nItem(wType));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                if (!takeProd(bizid, 1, prod.Name, prod.Price))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.TopCenter, $"Недостаточно товара на складе", 3000);
                    return;
                }

                Finance.Wallet.Change(client, -prod.Price);
                GameLog.Money($"player({Main.Players[client].UUID})", $"biz({biz.ID})", prod.Price, $"buyWShop({prod.Name})");
                Weapons.GiveWeapon(client, wType, Weapons.GetSerial(false, biz.ID));

                Notify.Send(client, NotifyType.Success, NotifyPosition.TopCenter, $"Вы купили {prod.Name} за {prod.Price}$", 3000);
                return;
            }
            catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
        }
        private static List<List<string>> gunsCat = new List<List<string>>()
        {
            new List<string>()
            {
                "Pistol",
                "CombatPistol",
                "Revolver",
                "HeavyPistol",
            },
            new List<string>()
            {
                "BullpupShotgun",
            },
            new List<string>()
            {
                "CombatPDW",
                "MachinePistol",
            },
        };
        #endregion
        public static void changeOwner(string oldName, string newName)
        {
            List<int> toChange = new List<int>();
            lock (BizList)
            {
                foreach (KeyValuePair<int, Business> biz in BizList)
                {
                    if (biz.Value.Owner != oldName) continue;
                    Log.Write($"The biz was found! [{biz.Key}]");
                    toChange.Add(biz.Key);
                }
                foreach (int id in toChange)
                {
                    BizList[id].Owner = newName;
                    BizList[id].UpdateLabel();
                    BizList[id].Save();
                }
            }
        }
    }
}