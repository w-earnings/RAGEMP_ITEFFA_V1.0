using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading;
using GTANetworkAPI;
using Newtonsoft.Json;
using iTeffa.Settings;
using iTeffa.Globals.Character;

namespace iTeffa.Globals
{
    class nInventory : Script
    {
        public static Dictionary<int, string> ItemsNames = new Dictionary<int, string>
        {
            {-1, "Маска" },
            {-3, "Перчатки" },
            {-4, "Штаны"},
            {-5, "Рюкзак"},
            {-6, "Обувь"},
            {-7, "Аксессуар"},
            {-8, "Нижняя одежда"},
            {-9, "Бронежилет"},
            {-10, "Украшения"},
            {-11, "Верхняя одежда" },
            {-12, "Головной убор" },
            {-13, "Очки" },
            {-14, "Аксессуар" },
            {1, "Аптечка"},
            {2, "Канистра"},
            {3, "Чипсы"},
            {4, "Пиво"},
            {5, "Пицца"},
            {6, "Бургер"},
            {7, "Хот-Дог"},
            {8, "Сэндвич"},
            {9, "eCola"},
            {10, "Sprunk"},
            {11, "Отмычка для замков"},
            {12, "Сумка с деньгами"},
            {13, "Материалы"},
            {14, "Наркотики"},
            {15, "Сумка с дрелью"},
            {16, "Военная отмычка"},
            {17, "Мешок"},
            {18, "Стяжки"},
            {19, "Ключи от машины"},
            {40, "Подарок"},
            {41, "Связка ключей"},

            {20, "«На корке лимона»"},
            {21, "«На бруснике»"},
            {22, "«Русский стандарт»"},
            {23, "«Asahi»"},
            {24, "«Midori»"},
            {25, "«Yamazaki»"},
            {26, "«Martini Asti»"},
            {27, "«Sambuca»"},
            {28, "«Campari»"},
            {29, "«Дживан»"},
            {30, "«Арарат»"},
            {31, "«Noyan Tapan»"},

            {100, "Pistol" },
            {101, "Combat Pistol" },
            {102, "Pistol 50" },
            {103, "SNS Pistol" },
            {104, "Heavy Pistol" },
            {105, "Vintage Pistol" },
            {106, "Marksman Pistol" },
            {107, "Revolver" },
            {108, "AP Pistol" },
            {109, "Stun Gun" },
            {110, "Flare Gun" },
            {111, "Double Action" },
            {112, "Pistol Mk2" },
            {113, "SNSPistol Mk2" },
            {114, "Revolver Mk2" },

            {115, "Micro SMG" },
            {116, "Machine Pistol" },
            {117, "SMG" },
            {118, "Assault SMG" },
            {119, "Combat PDW" },
            {120, "MG" },
            {121, "Combat MG" },
            {122, "Gusenberg" },
            {123, "Mini SMG" },
            {124, "SMG Mk2" },
            {125, "Combat MG Mk2" },

            {126, "Assault Rifle" },
            {127, "Carbine Rifle" },
            {128, "Advanced Rifle" },
            {129, "Special Carbine" },
            {130, "Bullpup Rifle" },
            {131, "Compact Rifle" },
            {132, "Assault Rifle Mk2" },
            {133, "Carbine Rifle Mk2" },
            {134, "Special Carbine Mk2" },
            {135, "Bullpup Rifle Mk2" },

            {136, "Sniper Rifle" },
            {137, "Heavy Sniper" },
            {138, "Marksman Rifle" },
            {139, "Heavy Sniper Mk2" },
            {140, "Marksman Rifle Mk2" },

            {141, "Pump Shotgun" },
            {142, "SawnOff Shotgun" },
            {143, "Bullpup Shotgun" },
            {144, "Assault Shotgun" },
            {145, "Musket" },
            {146, "Heavy Shotgun" },
            {147, "Double Barrel Shotgun" },
            {148, "Sweeper Shotgun" },
            {149, "Pump Shotgun Mk2" },

            {180, "Нож" },
            {181, "Дубинка" },
            {182, "Молоток" },
            {183, "Бита" },
            {184, "Лом" },
            {185, "Гольф клюшка" },
            {186, "Бутылка" },
            {187, "Кинжал" },
            {188, "Топор" },
            {189, "Кастет" },
            {190, "Мачете" },
            {191, "Фонарик" },
            {192, "Швейцарский нож" },
            {193, "Кий" },
            {194, "Ключ" },
            {195, "Боевой топор" },

            {200, "Пистолетный калибр" },
            {201, "Малый калибр" },
            {202, "Автоматный калибр" },
            {203, "Снайперский калибр" },
            {204, "Дробь" },

            {205, "Удочка" },
            {206, "Улучшенная удочка" },
            {207, "Удочка MK2" },
            {208, "Наживка" },
            {209, "Корюшка" },
            {210, "Кунджа" },
            {211, "Лосось" },
            {212, "Окунь" },
            {213, "Осётр" },
            {214, "Скат" },
            {215, "Тунец" },
            {216, "Угорь" },
            {217, "Чёрный амур" },
            {218, "Щука" },

            {777, "Рем.Компплект" },
        };
        public static Dictionary<int, string> ItemsDescriptions = new Dictionary<int, string>();
        public static Dictionary<ItemType, uint> ItemModels = new Dictionary<ItemType, uint>()
        {
            { ItemType.Hat, 1619813869 },
            { ItemType.Mask, 3887136870 },
            { ItemType.Gloves, 3125389411 },
            { ItemType.Leg, 2086911125 },
            { ItemType.Bag, 0000000 },
            { ItemType.Feet, 1682675077 },
            { ItemType.Jewelry, 2329969874 },
            { ItemType.Undershit, 578126062 },
            { ItemType.BodyArmor, 701173564 },
            { ItemType.Unknown, 0000000 },
            { ItemType.Top, 3038378640 },
            { ItemType.Glasses, 2329969874 },
            { ItemType.Accessories, 2329969874 },

            { ItemType.Drugs, 4293279169 },
            { ItemType.Material, 3045218749 },
            { ItemType.Debug, 0000000 },
            { ItemType.HealthKit, 678958360 },
            { ItemType.GasCan, 786272259 },
            { ItemType.Сrisps, 2564432314 },
            { ItemType.Beer, 1940235411 },
            { ItemType.Pizza, 604847691 },
            { ItemType.Burger, 2240524752 },
            { ItemType.HotDog, 2565741261 },
            { ItemType.Sandwich, 987331897 },
            { ItemType.eCola, 144995201 },
            { ItemType.Sprunk, 2973713592 },
            { ItemType.Lockpick, 977923025 },
            { ItemType.ArmyLockpick, 977923025 },
            { ItemType.Pocket, 3887136870 },
            { ItemType.Cuffs, 3887136870 },
            { ItemType.CarKey, 977923025 },
            { ItemType.Present, NAPI.Util.GetHashKey("prop_box_ammo07a") },
            { ItemType.KeyRing, 977923025 },

            { ItemType.RusDrink1, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.RusDrink2, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.RusDrink3, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.YakDrink1, NAPI.Util.GetHashKey("prop_cs_beer_bot_02") },
            { ItemType.YakDrink2, NAPI.Util.GetHashKey("prop_wine_red") },
            { ItemType.YakDrink3, NAPI.Util.GetHashKey("p_whiskey_bottle_s") },
            { ItemType.LcnDrink1, NAPI.Util.GetHashKey("prop_wine_white") },
            { ItemType.LcnDrink2, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.LcnDrink3, NAPI.Util.GetHashKey("prop_wine_red") },
            { ItemType.ArmDrink1, NAPI.Util.GetHashKey("prop_bottle_cognac") },
            { ItemType.ArmDrink2, NAPI.Util.GetHashKey("prop_bottle_cognac") },
            { ItemType.ArmDrink3, NAPI.Util.GetHashKey("prop_bottle_cognac") },

            { ItemType.Pistol, NAPI.Util.GetHashKey("w_pi_pistol") },
            { ItemType.CombatPistol, NAPI.Util.GetHashKey("w_pi_combatpistol") },
            { ItemType.Pistol50, NAPI.Util.GetHashKey("w_pi_pistol50") },
            { ItemType.SNSPistol, NAPI.Util.GetHashKey("w_pi_sns_pistol") },
            { ItemType.HeavyPistol, NAPI.Util.GetHashKey("w_pi_heavypistol") },
            { ItemType.VintagePistol, NAPI.Util.GetHashKey("w_pi_vintage_pistol") },
            { ItemType.MarksmanPistol, NAPI.Util.GetHashKey("w_pi_singleshot") },
            { ItemType.Revolver, NAPI.Util.GetHashKey("w_pi_revolver") },
            { ItemType.APPistol, NAPI.Util.GetHashKey("w_pi_appistol") },
            { ItemType.StunGun, NAPI.Util.GetHashKey("w_pi_stungun") },
            { ItemType.FlareGun, NAPI.Util.GetHashKey("w_pi_flaregun") },
            { ItemType.DoubleAction, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PistolMk2, NAPI.Util.GetHashKey("w_pi_pistolmk2") },
            { ItemType.SNSPistolMk2, NAPI.Util.GetHashKey("w_pi_sns_pistolmk2") },
            { ItemType.RevolverMk2, NAPI.Util.GetHashKey("w_pi_revolvermk2") },

            { ItemType.MicroSMG, NAPI.Util.GetHashKey("w_sb_microsmg") },
            { ItemType.MachinePistol, NAPI.Util.GetHashKey("w_sb_compactsmg") },
            { ItemType.SMG, NAPI.Util.GetHashKey("w_sb_smg") },
            { ItemType.AssaultSMG, NAPI.Util.GetHashKey("w_sb_assaultsmg") },
            { ItemType.CombatPDW, NAPI.Util.GetHashKey("w_sb_pdw") },
            { ItemType.MG, NAPI.Util.GetHashKey("w_mg_mg") },
            { ItemType.CombatMG, NAPI.Util.GetHashKey("w_mg_combatmg") },
            { ItemType.Gusenberg, NAPI.Util.GetHashKey("w_sb_gusenberg") },
            { ItemType.MiniSMG, NAPI.Util.GetHashKey("w_sb_minismg") },
            { ItemType.SMGMk2, NAPI.Util.GetHashKey("w_sb_smgmk2") },
            { ItemType.CombatMGMk2, NAPI.Util.GetHashKey("w_mg_combatmgmk2") },

            { ItemType.AssaultRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle") },
            { ItemType.CarbineRifle, NAPI.Util.GetHashKey("w_ar_carbinerifle") },
            { ItemType.AdvancedRifle, NAPI.Util.GetHashKey("w_ar_advancedrifle") },
            { ItemType.SpecialCarbine, NAPI.Util.GetHashKey("w_ar_specialcarbine") },
            { ItemType.BullpupRifle, NAPI.Util.GetHashKey("w_ar_bullpuprifle") },
            { ItemType.CompactRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle_smg") },
            { ItemType.AssaultRifleMk2, NAPI.Util.GetHashKey("w_ar_assaultriflemk2") },
            { ItemType.CarbineRifleMk2, NAPI.Util.GetHashKey("w_ar_carbineriflemk2") },
            { ItemType.SpecialCarbineMk2, NAPI.Util.GetHashKey("w_ar_specialcarbinemk2") },
            { ItemType.BullpupRifleMk2, NAPI.Util.GetHashKey("w_ar_bullpupriflemk2") },

            { ItemType.SniperRifle, NAPI.Util.GetHashKey("w_sr_sniperrifle") },
            { ItemType.HeavySniper, NAPI.Util.GetHashKey("w_sr_heavysniper") },
            { ItemType.MarksmanRifle, NAPI.Util.GetHashKey("w_sr_marksmanrifle") },
            { ItemType.HeavySniperMk2, NAPI.Util.GetHashKey("w_sr_heavysnipermk2") },
            { ItemType.MarksmanRifleMk2, NAPI.Util.GetHashKey("w_sr_marksmanriflemk2") },

            { ItemType.PumpShotgun, NAPI.Util.GetHashKey("w_sg_pumpshotgun") },
            { ItemType.SawnOffShotgun, NAPI.Util.GetHashKey("w_sg_sawnoff") },
            { ItemType.BullpupShotgun, NAPI.Util.GetHashKey("w_sg_bullpupshotgun") },
            { ItemType.AssaultShotgun, NAPI.Util.GetHashKey("w_sg_assaultshotgun") },
            { ItemType.Musket, NAPI.Util.GetHashKey("w_ar_musket") },
            { ItemType.HeavyShotgun, NAPI.Util.GetHashKey("w_sg_heavyshotgun") },
            { ItemType.DoubleBarrelShotgun, NAPI.Util.GetHashKey("w_sg_doublebarrel") },
            { ItemType.SweeperShotgun, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PumpShotgunMk2, NAPI.Util.GetHashKey("w_sg_pumpshotgunmk2") },

            { ItemType.Knife, NAPI.Util.GetHashKey("w_me_knife_01") },
            { ItemType.Nightstick, NAPI.Util.GetHashKey("w_me_nightstick") },
            { ItemType.Hammer, NAPI.Util.GetHashKey("w_me_hammer") },
            { ItemType.Bat, NAPI.Util.GetHashKey("w_me_bat") },
            { ItemType.Crowbar, NAPI.Util.GetHashKey("w_me_crowbar") },
            { ItemType.GolfClub, NAPI.Util.GetHashKey("w_me_gclub") },
            { ItemType.Bottle, NAPI.Util.GetHashKey("w_me_bottle") },
            { ItemType.Dagger, NAPI.Util.GetHashKey("w_me_dagger") },
            { ItemType.Hatchet, NAPI.Util.GetHashKey("w_me_hatchet") },
            { ItemType.KnuckleDuster, NAPI.Util.GetHashKey("w_me_knuckle") },
            { ItemType.Machete, NAPI.Util.GetHashKey("prop_ld_w_me_machette") },
            { ItemType.Flashlight, NAPI.Util.GetHashKey("w_me_flashlight") },
            { ItemType.SwitchBlade, NAPI.Util.GetHashKey("w_me_switchblade") },
            { ItemType.PoolCue, NAPI.Util.GetHashKey("prop_pool_cue") },
            { ItemType.Wrench, NAPI.Util.GetHashKey("prop_cs_wrench") },
            { ItemType.BattleAxe, NAPI.Util.GetHashKey("w_me_battleaxe") },

            { ItemType.PistolAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.RiflesAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.ShotgunsAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.SMGAmmo, NAPI.Util.GetHashKey("w_am_case") },
            { ItemType.SniperAmmo, NAPI.Util.GetHashKey("w_am_case") },

            /* Fishing */
            { ItemType.Rod, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.RodUpgrade, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.RodMK2, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.Naz, NAPI.Util.GetHashKey("ng_proc_paintcan02a") },
            { ItemType.Koroska, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Kyndja, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Lococ, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Okyn, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Ocetr, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Skat, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Tunec, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Ygol, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Amyr, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Chyka, NAPI.Util.GetHashKey("prop_starfish_01") },
            { ItemType.Remka, NAPI.Util.GetHashKey("prop_tool_box_01") },
        };

        public static Dictionary<ItemType, Vector3> ItemsPosOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3(0, 0, -0.93) },
            { ItemType.Mask, new Vector3(0, 0, -1) },
            { ItemType.Gloves, new Vector3(0, 0, -1) },
            { ItemType.Leg, new Vector3(0, 0, -0.85) },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3(0, 0, -0.95) },
            { ItemType.Jewelry, new Vector3(0, 0, -0.98) },
            { ItemType.Undershit, new Vector3(0, 0, -0.98) },
            { ItemType.BodyArmor, new Vector3(0, 0, -0.88) },
            { ItemType.Unknown, new Vector3() },
            { ItemType.Top, new Vector3(0, 0, -0.96) },
            { ItemType.Glasses, new Vector3(0, 0, -0.98) },
            { ItemType.Accessories, new Vector3(0, 0, -0.98) },

            { ItemType.Drugs, new Vector3(0, 0, -0.95) },
            { ItemType.Material, new Vector3(0, 0, -0.6) },
            { ItemType.Debug, new Vector3() },
            { ItemType.HealthKit, new Vector3(0, 0, -0.9) },
            { ItemType.GasCan, new Vector3(0, 0, -1) },
            { ItemType.Сrisps, new Vector3(0, 0, -1) },
            { ItemType.Beer, new Vector3(0, 0, -1) },
            { ItemType.Pizza, new Vector3(0, 0, -1) },
            { ItemType.Burger, new Vector3(0, 0, -0.97) },
            { ItemType.HotDog, new Vector3(0, 0, -0.97) },
            { ItemType.Sandwich, new Vector3(0, 0, -0.99) },
            { ItemType.eCola, new Vector3(0, 0, -1) },
            { ItemType.Sprunk, new Vector3(0, 0, -1) },
            { ItemType.Lockpick, new Vector3(0, 0, -0.98) },
            { ItemType.ArmyLockpick, new Vector3(0, 0, -0.98) },
            { ItemType.Pocket, new Vector3(0, 0, -0.98) },
            { ItemType.Cuffs, new Vector3(0, 0, -0.98) },
            { ItemType.CarKey, new Vector3(0, 0, -0.98) },
            { ItemType.Present, new Vector3(0, 0, -0.98) },
            { ItemType.KeyRing, new Vector3(0, 0, -0.98) },

            { ItemType.RusDrink1, new Vector3(0, 0, -1) },
            { ItemType.RusDrink2, new Vector3(0, 0, -1) },
            { ItemType.RusDrink3, new Vector3(0, 0, -1) },
            { ItemType.YakDrink1, new Vector3(0, 0, -0.87) },
            { ItemType.YakDrink2, new Vector3(0, 0, -1) },
            { ItemType.YakDrink3, new Vector3(0, 0, -0.87) },
            { ItemType.LcnDrink1, new Vector3(0, 0, -1) },
            { ItemType.LcnDrink2, new Vector3(0, 0, -1) },
            { ItemType.LcnDrink3, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink1, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink2, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink3, new Vector3(0, 0, -1) },

            { ItemType.Pistol, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Pistol50, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistol, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyPistol, new Vector3(0, 0, -0.99) },
            { ItemType.VintagePistol, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Revolver, new Vector3(0, 0, -0.99) },
            { ItemType.APPistol, new Vector3(0, 0, -0.99) },
            { ItemType.StunGun, new Vector3(0, 0, -0.99) },
            { ItemType.FlareGun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleAction, new Vector3(0, 0, -0.99) },
            { ItemType.PistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.RevolverMk2, new Vector3(0, 0, -0.99) },

            { ItemType.MicroSMG, new Vector3(0, 0, -0.99) },
            { ItemType.MachinePistol, new Vector3(0, 0, -0.99) },
            { ItemType.SMG, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultSMG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPDW, new Vector3(0, 0, -0.99) },
            { ItemType.MG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMG, new Vector3(0, 0, -0.99) },
            { ItemType.Gusenberg, new Vector3(0, 0, -0.99) },
            { ItemType.MiniSMG, new Vector3(0, 0, -0.99) },
            { ItemType.SMGMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMGMk2, new Vector3(0, 0, -0.99) },

            { ItemType.AssaultRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AdvancedRifle, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbine, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CompactRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbineMk2, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.SniperRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniper, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniperMk2, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.PumpShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SawnOffShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.Musket, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleBarrelShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SweeperShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.PumpShotgunMk2, new Vector3(0, 0, -0.99) },

            { ItemType.Knife, new Vector3(0, 0, -0.99) },
            { ItemType.Nightstick, new Vector3(0, 0, -0.99) },
            { ItemType.Hammer, new Vector3(0, 0, -0.99) },
            { ItemType.Bat, new Vector3(0, 0, -0.99) },
            { ItemType.Crowbar, new Vector3(0, 0, -0.99) },
            { ItemType.GolfClub, new Vector3(0, 0, -0.99) },
            { ItemType.Bottle, new Vector3(0, 0, -0.99) },
            { ItemType.Dagger, new Vector3(0, 0, -0.99) },
            { ItemType.Hatchet, new Vector3(0, 0, -0.99) },
            { ItemType.KnuckleDuster, new Vector3(0, 0, -0.99) },
            { ItemType.Machete, new Vector3(0, 0, -0.99) },
            { ItemType.Flashlight, new Vector3(0, 0, -0.99) },
            { ItemType.SwitchBlade, new Vector3(0, 0, -0.99) },
            { ItemType.PoolCue, new Vector3(0, 0, -0.99) },
            { ItemType.Wrench, new Vector3(0, 0, -0.985) },
            { ItemType.BattleAxe, new Vector3(0, 0, -0.99) },

            { ItemType.PistolAmmo, new Vector3(0, 0, -1) },
            { ItemType.RiflesAmmo, new Vector3(0, 0, -1) },
            { ItemType.ShotgunsAmmo, new Vector3(0, 0, -1) },
            { ItemType.SMGAmmo, new Vector3(0, 0, -1) },
            { ItemType.SniperAmmo, new Vector3(0, 0, -1) },

            /* Fishing */
            { ItemType.Rod, new Vector3(0, 0, -0.99) },
            { ItemType.RodUpgrade, new Vector3(0, 0, -0.99) },
            { ItemType.RodMK2, new Vector3(0, 0, -0.99) },
            { ItemType.Naz, new Vector3(0, 0, -0.99) },
            { ItemType.Koroska, new Vector3(0, 0, -0.99) },
            { ItemType.Kyndja, new Vector3(0, 0, -0.99) },
            { ItemType.Lococ, new Vector3(0, 0, -0.99) },
            { ItemType.Okyn, new Vector3(0, 0, -0.99) },
            { ItemType.Ocetr, new Vector3(0, 0, -0.99) },
            { ItemType.Skat, new Vector3(0, 0, -0.99) },
            { ItemType.Tunec, new Vector3(0, 0, -0.99) },
            { ItemType.Ygol, new Vector3(0, 0, -0.99) },
            { ItemType.Amyr, new Vector3(0, 0, -0.99) },
            { ItemType.Chyka, new Vector3(0, 0, -0.99) },

            { ItemType.Remka, new Vector3(0, 0, -1) },
        };
        public static Dictionary<ItemType, Vector3> ItemsRotOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3() },
            { ItemType.Mask, new Vector3() },
            { ItemType.Gloves, new Vector3(90, 0, 0) },
            { ItemType.Leg, new Vector3() },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3() },
            { ItemType.Jewelry, new Vector3() },
            { ItemType.Undershit, new Vector3() },
            { ItemType.BodyArmor, new Vector3(90, 90, 0) },
            { ItemType.Unknown, new Vector3() },
            { ItemType.Top, new Vector3() },
            { ItemType.Glasses, new Vector3() },
            { ItemType.Accessories, new Vector3() },

            { ItemType.Drugs, new Vector3() },
            { ItemType.Material, new Vector3() },
            { ItemType.Debug, new Vector3() },
            { ItemType.HealthKit, new Vector3() },
            { ItemType.GasCan, new Vector3() },
            { ItemType.Сrisps, new Vector3(90, 90, 0) },
            { ItemType.Beer, new Vector3() },
            { ItemType.Pizza, new Vector3() },
            { ItemType.Burger, new Vector3() },
            { ItemType.HotDog, new Vector3() },
            { ItemType.Sandwich, new Vector3() },
            { ItemType.eCola, new Vector3() },
            { ItemType.Sprunk, new Vector3() },
            { ItemType.Lockpick, new Vector3() },
            { ItemType.ArmyLockpick, new Vector3() },
            { ItemType.Pocket, new Vector3() },
            { ItemType.Cuffs, new Vector3() },
            { ItemType.CarKey, new Vector3() },
            { ItemType.Present, new Vector3() },
            { ItemType.KeyRing, new Vector3() },

            { ItemType.RusDrink1, new Vector3() },
            { ItemType.RusDrink2, new Vector3() },
            { ItemType.RusDrink3, new Vector3() },
            { ItemType.YakDrink1, new Vector3() },
            { ItemType.YakDrink2, new Vector3() },
            { ItemType.YakDrink3, new Vector3() },
            { ItemType.LcnDrink1, new Vector3() },
            { ItemType.LcnDrink2, new Vector3() },
            { ItemType.LcnDrink3, new Vector3() },
            { ItemType.ArmDrink1, new Vector3() },
            { ItemType.ArmDrink2, new Vector3() },
            { ItemType.ArmDrink3, new Vector3() },

            { ItemType.Pistol, new Vector3(90, 0, 0) },
            { ItemType.CombatPistol, new Vector3(90, 0, 0) },
            { ItemType.Pistol50, new Vector3(90, 0, 0) },
            { ItemType.SNSPistol, new Vector3(90, 0, 0) },
            { ItemType.HeavyPistol, new Vector3(90, 0, 0) },
            { ItemType.VintagePistol, new Vector3(90, 0, 0) },
            { ItemType.MarksmanPistol, new Vector3(90, 0, 0) },
            { ItemType.Revolver, new Vector3(90, 0, 0) },
            { ItemType.APPistol, new Vector3(90, 0, 0) },
            { ItemType.StunGun, new Vector3(90, 0, 0) },
            { ItemType.FlareGun, new Vector3(90, 0, 0) },
            { ItemType.DoubleAction, new Vector3(90, 0, 0) },
            { ItemType.PistolMk2, new Vector3(90, 0, 0) },
            { ItemType.SNSPistolMk2, new Vector3(90, 0, 0) },
            { ItemType.RevolverMk2, new Vector3(90, 0, 0) },

            { ItemType.MicroSMG, new Vector3(90, 0, 0) },
            { ItemType.MachinePistol, new Vector3(90, 0, 0) },
            { ItemType.SMG, new Vector3(90, 0, 0) },
            { ItemType.AssaultSMG, new Vector3(90, 0, 0) },
            { ItemType.CombatPDW, new Vector3(90, 0, 0) },
            { ItemType.MG, new Vector3(90, 0, 0) },
            { ItemType.CombatMG, new Vector3(90, 0, 0) },
            { ItemType.Gusenberg, new Vector3(90, 0, 0) },
            { ItemType.MiniSMG, new Vector3(90, 0, 0) },
            { ItemType.SMGMk2, new Vector3(90, 0, 0) },
            { ItemType.CombatMGMk2, new Vector3(90, 0, 0) },

            { ItemType.AssaultRifle, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifle, new Vector3(90, 0, 0) },
            { ItemType.AdvancedRifle, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbine, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifle, new Vector3(90, 0, 0) },
            { ItemType.CompactRifle, new Vector3(90, 0, 0) },
            { ItemType.AssaultRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbineMk2, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.SniperRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniper, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniperMk2, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.PumpShotgun, new Vector3(90, 0, 0) },
            { ItemType.SawnOffShotgun, new Vector3(90, 0, 0) },
            { ItemType.BullpupShotgun, new Vector3(90, 0, 0) },
            { ItemType.AssaultShotgun, new Vector3(90, 0, 0) },
            { ItemType.Musket, new Vector3(90, 0, 0) },
            { ItemType.HeavyShotgun, new Vector3(90, 0, 0) },
            { ItemType.DoubleBarrelShotgun, new Vector3(90, 0, 0) },
            { ItemType.SweeperShotgun, new Vector3(90, 0, 0) },
            { ItemType.PumpShotgunMk2, new Vector3(90, 0, 0) },

            { ItemType.Knife, new Vector3(90, 0, 0) },
            { ItemType.Nightstick, new Vector3(90, 0, 0) },
            { ItemType.Hammer, new Vector3(90, 0, 0) },
            { ItemType.Bat, new Vector3(90, 0, 0) },
            { ItemType.Crowbar, new Vector3(90, 0, 0) },
            { ItemType.GolfClub, new Vector3(90, 0, 0) },
            { ItemType.Bottle, new Vector3(90, 0, 0) },
            { ItemType.Dagger, new Vector3(90, 0, 0) },
            { ItemType.Hatchet, new Vector3(90, 0, 0) },
            { ItemType.KnuckleDuster, new Vector3(90, 0, 0) },
            { ItemType.Machete, new Vector3(90, 0, 0) },
            { ItemType.Flashlight, new Vector3(90, 0, 0) },
            { ItemType.SwitchBlade, new Vector3(90, 0, 0) },
            { ItemType.PoolCue, new Vector3(90, 0, 0) },
            { ItemType.Wrench, new Vector3(-12, 0, 0) },
            { ItemType.BattleAxe, new Vector3(90, 0, 0) },

            { ItemType.PistolAmmo, new Vector3(90, 0, 0) },
            { ItemType.RiflesAmmo, new Vector3(90, 0, 0) },
            { ItemType.ShotgunsAmmo, new Vector3(90, 0, 0) },
            { ItemType.SMGAmmo, new Vector3(90, 0, 0) },
            { ItemType.SniperAmmo, new Vector3(90, 0, 0) },

            /* Fishing */
            { ItemType.Rod, new Vector3(90, 0, 0) },
            { ItemType.RodUpgrade, new Vector3(90, 0, 0) },
            { ItemType.RodMK2, new Vector3(90, 0, 0) },
            { ItemType.Naz, new Vector3(90, 0, 0) },
            { ItemType.Koroska, new Vector3(90, 0, 0) },
            { ItemType.Kyndja, new Vector3(90, 0, 0) },
            { ItemType.Lococ, new Vector3(90, 0, 0) },
            { ItemType.Okyn, new Vector3(90, 0, 0) },
            { ItemType.Ocetr, new Vector3(90, 0, 0) },
            { ItemType.Skat, new Vector3(90, 0, 0) },
            { ItemType.Tunec, new Vector3(90, 0, 0) },
            { ItemType.Ygol, new Vector3(90, 0, 0) },
            { ItemType.Amyr, new Vector3(90, 0, 0) },
            { ItemType.Chyka, new Vector3(90, 0, 0) },

            { ItemType.Remka, new Vector3() },
        };

        public static Dictionary<ItemType, int> ItemsStacks = new Dictionary<ItemType, int>()
        {
            { ItemType.BagWithMoney, 1 },
            { ItemType.Material, 300 },
            { ItemType.Drugs, 50 },
            { ItemType.BagWithDrill, 1 },
            { ItemType.Debug, 10000 },
            { ItemType.HealthKit, 5 },
            { ItemType.GasCan, 2 },
            { ItemType.Сrisps, 4 },
            { ItemType.Beer, 5 },
            { ItemType.Pizza, 3 },
            { ItemType.Burger, 4 },
            { ItemType.HotDog, 5 },
            { ItemType.Sandwich, 7 },
            { ItemType.eCola, 5 },
            { ItemType.Sprunk, 5 },
            { ItemType.Lockpick, 10 },
            { ItemType.ArmyLockpick, 10 },
            { ItemType.Pocket, 5 },
            { ItemType.Cuffs, 5 },
            { ItemType.CarKey, 1 },
            { ItemType.Present, 1 },
            { ItemType.KeyRing, 1 },

            { ItemType.Mask, 1 },
            { ItemType.Gloves, 1 },
            { ItemType.Leg, 1 },
            { ItemType.Bag, 1 },
            { ItemType.Feet, 1 },
            { ItemType.Jewelry, 1 },
            { ItemType.Undershit, 1 },
            { ItemType.BodyArmor, 1 },
            { ItemType.Unknown, 1 },
            { ItemType.Top, 1 },
            { ItemType.Hat, 1 },
            { ItemType.Glasses, 1 },
            { ItemType.Accessories, 1 },

            { ItemType.RusDrink1, 5 },
            { ItemType.RusDrink2, 5 },
            { ItemType.RusDrink3, 5 },

            { ItemType.YakDrink1, 5 },
            { ItemType.YakDrink2, 5 },
            { ItemType.YakDrink3, 5 },

            { ItemType.LcnDrink1, 5 },
            { ItemType.LcnDrink2, 5 },
            { ItemType.LcnDrink3, 5 },

            { ItemType.ArmDrink1, 5 },
            { ItemType.ArmDrink2, 5 },
            { ItemType.ArmDrink3, 5 },

            { ItemType.Pistol, 1 },
            { ItemType.CombatPistol, 1 },
            { ItemType.Pistol50, 1 },
            { ItemType.SNSPistol, 1 },
            { ItemType.HeavyPistol, 1 },
            { ItemType.VintagePistol, 1 },
            { ItemType.MarksmanPistol, 1 },
            { ItemType.Revolver, 1 },
            { ItemType.APPistol, 1 },
            { ItemType.StunGun, 1 },
            { ItemType.FlareGun, 1 },
            { ItemType.DoubleAction, 1 },
            { ItemType.PistolMk2, 1 },
            { ItemType.SNSPistolMk2, 1 },
            { ItemType.RevolverMk2, 1 },

            { ItemType.MicroSMG, 1 },
            { ItemType.MachinePistol, 1 },
            { ItemType.SMG, 1 },
            { ItemType.AssaultSMG, 1 },
            { ItemType.CombatPDW, 1 },
            { ItemType.MG, 1 },
            { ItemType.CombatMG, 1 },
            { ItemType.Gusenberg, 1 },
            { ItemType.MiniSMG, 1 },
            { ItemType.SMGMk2, 1 },
            { ItemType.CombatMGMk2, 1 },

            { ItemType.AssaultRifle, 1 },
            { ItemType.CarbineRifle, 1 },
            { ItemType.AdvancedRifle, 1 },
            { ItemType.SpecialCarbine, 1 },
            { ItemType.BullpupRifle, 1 },
            { ItemType.CompactRifle, 1 },
            { ItemType.AssaultRifleMk2, 1 },
            { ItemType.CarbineRifleMk2, 1 },
            { ItemType.SpecialCarbineMk2, 1 },
            { ItemType.BullpupRifleMk2, 1 },

            { ItemType.SniperRifle, 1 },
            { ItemType.HeavySniper, 1 },
            { ItemType.MarksmanRifle, 1 },
            { ItemType.HeavySniperMk2, 1 },
            { ItemType.MarksmanRifleMk2, 1 },

            { ItemType.PumpShotgun, 1 },
            { ItemType.SawnOffShotgun, 1 },
            { ItemType.BullpupShotgun, 1 },
            { ItemType.AssaultShotgun, 1 },
            { ItemType.Musket, 1 },
            { ItemType.HeavyShotgun, 1 },
            { ItemType.DoubleBarrelShotgun, 1 },
            { ItemType.SweeperShotgun, 1 },
            { ItemType.PumpShotgunMk2, 1 },

            { ItemType.Knife, 1 },
            { ItemType.Nightstick, 1 },
            { ItemType.Hammer, 1 },
            { ItemType.Bat, 1 },
            { ItemType.Crowbar, 1 },
            { ItemType.GolfClub, 1 },
            { ItemType.Bottle, 1 },
            { ItemType.Dagger, 1 },
            { ItemType.Hatchet, 1 },
            { ItemType.KnuckleDuster, 1 },
            { ItemType.Machete, 1 },
            { ItemType.Flashlight, 1 },
            { ItemType.SwitchBlade, 1 },
            { ItemType.PoolCue, 1 },
            { ItemType.Wrench, 1 },
            { ItemType.BattleAxe, 1 },

            { ItemType.PistolAmmo, 120 },
            { ItemType.RiflesAmmo, 200 },
            { ItemType.ShotgunsAmmo, 100 },
            { ItemType.SMGAmmo, 200 },
            { ItemType.SniperAmmo, 20 },

            /* Fishing */
            { ItemType.Rod, 1 },
            { ItemType.RodUpgrade, 1 },
            { ItemType.RodMK2, 1 },
            { ItemType.Naz, 100 },
            { ItemType.Koroska, 30 },
            { ItemType.Kyndja, 30 },
            { ItemType.Lococ, 30 },
            { ItemType.Okyn, 30 },
            { ItemType.Ocetr, 30 },
            { ItemType.Skat, 30 },
            { ItemType.Tunec, 30 },
            { ItemType.Ygol, 30 },
            { ItemType.Amyr, 30 },
            { ItemType.Chyka, 30 },

            { ItemType.Remka, 7 },
        };

        public static List<ItemType> ClothesItems = new List<ItemType>()
        {
            ItemType.Mask,
            ItemType.Gloves,
            ItemType.Leg,
            ItemType.Bag,
            ItemType.Feet,
            ItemType.Jewelry,
            ItemType.Undershit,
            ItemType.BodyArmor,
            ItemType.Unknown,
            ItemType.Top,
            ItemType.Hat,
            ItemType.Glasses,
            ItemType.Accessories,
        };
        public static List<ItemType> WeaponsItems = new List<ItemType>()
        {
            ItemType.Pistol,
            ItemType.CombatPistol,
            ItemType.Pistol50,
            ItemType.SNSPistol,
            ItemType.HeavyPistol,
            ItemType.VintagePistol,
            ItemType.MarksmanPistol,
            ItemType.Revolver,
            ItemType.APPistol,
            ItemType.FlareGun,
            ItemType.DoubleAction,
            ItemType.PistolMk2,
            ItemType.SNSPistolMk2,
            ItemType.RevolverMk2,

            ItemType.MicroSMG,
            ItemType.MachinePistol,
            ItemType.SMG,
            ItemType.AssaultSMG,
            ItemType.CombatPDW,
            ItemType.MG,
            ItemType.CombatMG,
            ItemType.Gusenberg,
            ItemType.MiniSMG,
            ItemType.SMGMk2,
            ItemType.CombatMGMk2,

            ItemType.AssaultRifle,
            ItemType.CarbineRifle,
            ItemType.AdvancedRifle,
            ItemType.SpecialCarbine,
            ItemType.BullpupRifle,
            ItemType.CompactRifle,
            ItemType.AssaultRifleMk2,
            ItemType.CarbineRifleMk2,
            ItemType.SpecialCarbineMk2,
            ItemType.BullpupRifleMk2,

            ItemType.SniperRifle,
            ItemType.HeavySniper,
            ItemType.MarksmanRifle,
            ItemType.HeavySniperMk2,
            ItemType.MarksmanRifleMk2,

            ItemType.PumpShotgun,
            ItemType.SawnOffShotgun,
            ItemType.BullpupShotgun,
            ItemType.AssaultShotgun,
            ItemType.Musket,
            ItemType.HeavyShotgun,
            ItemType.DoubleBarrelShotgun,
            ItemType.SweeperShotgun,
            ItemType.PumpShotgunMk2,
        };
        public static List<ItemType> MeleeWeaponsItems = new List<ItemType>()
        {
            ItemType.Knife,
            ItemType.Nightstick,
            ItemType.Hammer,
            ItemType.Bat,
            ItemType.Crowbar,
            ItemType.GolfClub,
            ItemType.Bottle,
            ItemType.Dagger,
            ItemType.Hatchet,
            ItemType.KnuckleDuster,
            ItemType.Machete,
            ItemType.Flashlight,
            ItemType.SwitchBlade,
            ItemType.PoolCue,
            ItemType.Wrench,
            ItemType.BattleAxe,
            ItemType.StunGun,
        };
        public static List<ItemType> AmmoItems = new List<ItemType>()
        {
            ItemType.PistolAmmo,
            ItemType.RiflesAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.SMGAmmo,
            ItemType.SniperAmmo
        };
        public static List<ItemType> AlcoItems = new List<ItemType>()
        {
            ItemType.LcnDrink1,
            ItemType.LcnDrink2,
            ItemType.LcnDrink3,
            ItemType.RusDrink1,
            ItemType.RusDrink2,
            ItemType.RusDrink3,
            ItemType.YakDrink1,
            ItemType.YakDrink2,
            ItemType.YakDrink3,
            ItemType.ArmDrink1,
            ItemType.ArmDrink2,
            ItemType.ArmDrink3,
        };

        #region D2U ItemType.Present
        public static readonly List<Tuple<int, int>> PresentsTypes = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0, 5),
            new Tuple<int, int>(1, 4),
            new Tuple<int, int>(2, 3),
            new Tuple<int, int>(5, 0),
            new Tuple<int, int>(4, 1),
            new Tuple<int, int>(3, 2),
        };
        public static readonly List<int> TypesCounts = new List<int>()
        {
            10, 25, 50, 1000, 5000, 10000
        };
        #endregion

        // UUID, Items by index
        public static Dictionary<int, List<nItem>> Items = new Dictionary<int, List<nItem>>();
        private static nLog Log = new nLog("nInventory");
        private static Timer SaveTimer;

        #region Constructor
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Log.Write("Loading player items...", nLog.Type.Info);
                // // //
                var result = Connect.QueryRead($"SELECT * FROM `inventory`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["uuid"]);
                    string json = Convert.ToString(Row["items"]);
                    List<nItem> items = JsonConvert.DeserializeObject<List<nItem>>(json);
                    Items.Add(UUID, items);
                }
                SaveTimer = new Timer(new TimerCallback(SaveAll), null, 0, 1800000);
                Log.Write("Items loaded.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_CONSTRUCT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Add/Remove item
        public static void Add(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int index = FindIndex(UUID, item.Type);
                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
                {
                    Items[UUID].Add(item);
                    Interface.Dashboard.Update(player, item, Items[UUID].IndexOf(item));
                }
                else
                {
                    if (index != -1)
                    {
                        int count = Items[UUID][index].Count;
                        Items[UUID][index].Count = count + item.Count;
                        Interface.Dashboard.Update(player, Items[UUID][index], index);
                        Log.Debug($"Added existing item! {UUID.ToString()}:{index.ToString()}");
                    }
                    else
                    {
                        Items[UUID].Add(item);
                        Interface.Dashboard.Update(player, item, Items[UUID].IndexOf(item));
                    }
                }
                Log.Debug($"Item added. {UUID.ToString()}:{index.ToString()}");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static int TryAdd(Player client, nItem item)
        {
            try
            {
                int UUID = Main.Players[client].UUID;
                int index = FindIndex(UUID, item.Type);
                int tail = 0;
                if (ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
                {
                    if (isFull(UUID))
                        return -1;
                }
                else if (WeaponsItems.Contains(item.Type))
                {
                    if (isFull(UUID))
                        return -1;

                    var ammoType = Weapons.WeaponsAmmoTypes[item.Type];
                    var sameTypeWeapon = Items[UUID].FirstOrDefault(i => WeaponsItems.Contains(i.Type) && Weapons.WeaponsAmmoTypes[i.Type] == ammoType);
                    if (sameTypeWeapon != null) return -1;
                }
                else if (MeleeWeaponsItems.Contains(item.Type))
                {
                    if (isFull(UUID))
                        return -1;

                    var sameWeapon = Items[UUID].FirstOrDefault(i => i.Type == item.Type);
                    if (sameWeapon != null) return -1;
                }
                else
                {
                    if (index != -1)
                    {
                        int max = (ItemsStacks.ContainsKey(item.Type)) ? ItemsStacks[item.Type] : 1;
                        int count = Items[UUID][index].Count;
                        int temp = count + item.Count;
                        if (temp > max)
                        {
                            tail = temp - max;
                            return tail;
                        }
                    }
                    else
                    {
                        if (item.Count > ItemsStacks[item.Type])
                        {
                            tail = item.Count - ItemsStacks[item.Type];
                            return tail;
                        }
                        else if (isFull(UUID))
                            return -1;
                    }
                }
                return tail;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
                return 0;
            }
        }
        public static void Remove(Player player, ItemType type, int count)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int Index = FindIndex(UUID, type);
                if (Index != -1)
                {
                    int temp = Items[UUID][Index].Count - count;
                    if (temp > 0)
                    {
                        Items[UUID][Index].Count = temp;
                        Interface.Dashboard.Update(player, Items[UUID][Index], Index);
                    }
                    else
                    {
                        Items[UUID].RemoveAt(Index);
                        Interface.Dashboard.sendItems(player);
                    }
                }
                Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }
        public static void Remove(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;

                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.BagWithDrill 
                    || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
                {
                    Items[UUID].Remove(item);
                    Interface.Dashboard.sendItems(player);
                    Log.Debug($"Item removed. {UUID.ToString()}:TYPE {(int)item.Type}");
                }
                else
                {
                    int Index = FindIndex(UUID, item.Type);
                    if (Index != -1)
                    {
                        int temp = Items[UUID][Index].Count - item.Count;
                        if (temp > 0)
                        {
                            Items[UUID][Index].Count = temp;
                            Interface.Dashboard.Update(player, Items[UUID][Index], Index);
                        }
                        else
                        {
                            Items[UUID].RemoveAt(Index);
                            Interface.Dashboard.sendItems(player);
                        }
                    }
                    Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                }
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }
        #endregion

        #region Save items to db
        public static void SaveAll(object state = null)
        {
            try
            {
                Log.Write("Saving items...", nLog.Type.Info);
                if (Items.Count == 0) return;
                Dictionary<int, List<nItem>> cItems = new Dictionary<int, List<nItem>>(Items);

                foreach (KeyValuePair<int, List<nItem>> kvp in cItems)
                {
                    int UUID = kvp.Key;
                    string json = JsonConvert.SerializeObject(kvp.Value);
                    Connect.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                }
                Log.Write("Items has been saved to DB.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVEALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void Save(int UUID)
        {
            try
            {
                if (!Items.ContainsKey(UUID)) return;
                Log.Write($"Saving items for {UUID}", nLog.Type.Info);
                string json = JsonConvert.SerializeObject(Items[UUID]);
                Connect.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                Log.Write("Items has been saved to DB.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region SPECIAL
        public static nItem Find(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            nItem result = items.Find(i => i.Type == type);
            return result;
        }
        public static int FindIndex(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            int result = items.FindIndex(i => i.Type == type);
            return result;
        }

        public static bool isFull(int UUID)
        {
            if (Items[UUID].Count >= 20) return true;
            else return false;
        }

        public static void Check(int uuid)
        { //if items dict does not contains account uuid, then add him
            if (!Items.ContainsKey(uuid))
            {
                Items.Add(uuid, new List<nItem>());
                Connect.Query($"INSERT INTO `inventory`(`uuid`,`items`) VALUES ({uuid},'{JsonConvert.SerializeObject(new List<nItem>())}')");
                Log.Debug("Player added");
            }
        }

        public static void UnActiveItem(Player player, ItemType type)
        {
            var items = Items[Main.Players[player].UUID];
            foreach (var i in items)
                if (i.Type == type && i.IsActive)
                {
                    i.IsActive = false;
                    Interface.Dashboard.Update(player, i, items.IndexOf(i));
                }
            Items[Main.Players[player].UUID] = items;
        }
        public static void ClearWithoutClothes(Player player)
        {
            try
            {
                int uuid = Main.Players[player].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing) upd.Add(item);

                Items[uuid] = upd;
                Interface.Dashboard.sendItems(player);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        public static void ClearAllClothes(Player client)
        {
            try
            {
                int uuid = Main.Players[client].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (!ClothesItems.Contains(item.Type)) upd.Add(item);

                Items[uuid] = upd;
                Interface.Dashboard.sendItems(client);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
    }
}
