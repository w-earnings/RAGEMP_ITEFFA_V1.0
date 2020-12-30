﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class Weapons : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Weapons");
        
        internal enum Hash : int
        {
            /* Handguns */
            Knife = -1716189206,
            Nightstick = 1737195953,
            Hammer = 1317494643,
            Bat = -1786099057,
            Crowbar = -2067956739,
            GolfClub = 1141786504,
            Bottle = -102323637,
            Dagger = -1834847097,
            Hatchet = -102973651,
            KnuckleDuster = -656458692,
            Machete = -581044007,
            Flashlight = -1951375401,
            SwitchBlade = -538741184,
            PoolCue = -1810795771,
            Wrench = 419712736,
            BattleAxe = -853065399,
            /* Pistols */
            Pistol = 453432689,
            CombatPistol = 1593441988,
            Pistol50 = -1716589765,
            SNSPistol = -1076751822,
            HeavyPistol = -771403250,
            VintagePistol = 137902532,
            MarksmanPistol = -598887786,
            Revolver = -1045183535,
            APPistol = 584646201,
            StunGun = 911657153,
            FlareGun = 1198879012,
            DoubleAction,
            PistolMk2 = -1075685676,
            SNSPistolMk2 = -2009644972,
            RevolverMk2 = -879347409,
            ceramicpistol = 727643628,
            /* SMG */
            MicroSMG = 324215364,
            MachinePistol = -619010992,
            SMG = 736523883,
            AssaultSMG = -270015777,
            CombatPDW = 171789620,
            MG = -1660422300,
            CombatMG = 2144741730,
            Gusenberg = 1627465347,
            MiniSMG = -1121678507,
            SMGMk2 = 2024373456,
            CombatMGMk2 = -608341376,
            /* Rifles */
            AssaultRifle = -1074790547,
            CarbineRifle = -2084633992,
            AdvancedRifle = -1357824103,
            SpecialCarbine = -1063057011,
            BullpupRifle = 2132975508,
            CompactRifle = 1649403952,
            AssaultRifleMk2 = 961495388,
            CarbineRifleMk2 = -86904375,
            SpecialCarbineMk2 = -1768145561,
            BullpupRifleMk2 = -2066285827,
            /* Sniper */
            SniperRifle = 100416529,
            HeavySniper = 205991906,
            MarksmanRifle = -952879014,
            HeavySniperMk2 = 177293209,
            MarksmanRifleMk2 = 1785463520,
            /* Shotguns */
            PumpShotgun = 487013001,
            SawnOffShotgun = 2017895192,
            BullpupShotgun = -1654528753,
            AssaultShotgun = -494615257,
            Musket = -1466123874,
            HeavyShotgun = 984333226,
            DoubleBarrelShotgun = -275439685,
            SweeperShotgun = 317205821,
            PumpShotgunMk2 = 1432025498,
            /* Heavy */
            GrenadeLauncher = -1568386805,
            RPG = -1312131151,
            Minigun = 1119849093,
            Firework = 2138347493,
            Railgun = 1834241177,
            HomingLauncher = 1672152130,
            GrenadeLauncherSmoke = 1305664598,
            CompactGrenadeLauncher = 125959754,
            /* Throwables & Misc */
            Grenade = -1813897027,
            StickyBomb = 741814745,
            ProximityMine = -1420407917,
            BZGas = -1600701090,
            Molotov = 615608432,
            FireExtinguisher = 101631238,
            PetrolCan = 883325847,
            Flare = 1233104067,
            Ball = 600439132,
            Snowball = 126349499,
            SmokeGrenade = -37975472,
            PipeBomb = -1169823560,
            Parachute = 615608432
        }
        
        public static Hash GetHash(string name)
        {
            Log.Debug($"{name} {Convert.ToString((Hash)Enum.Parse(typeof(Hash), name))}");
            return (Hash)Enum.Parse(typeof(Hash), name);
        }

        public static Dictionary<ItemType, ItemType> WeaponsAmmoTypes = new Dictionary<ItemType, ItemType>()
        {
            { ItemType.Pistol, ItemType.PistolAmmo },
            { ItemType.CombatPistol, ItemType.PistolAmmo },
            { ItemType.Pistol50, ItemType.PistolAmmo },
            { ItemType.SNSPistol, ItemType.PistolAmmo },
            { ItemType.HeavyPistol, ItemType.PistolAmmo },
            { ItemType.VintagePistol, ItemType.PistolAmmo },
            { ItemType.MarksmanPistol, ItemType.PistolAmmo },
            { ItemType.Revolver, ItemType.PistolAmmo },
            { ItemType.APPistol, ItemType.PistolAmmo },
            { ItemType.FlareGun, ItemType.PistolAmmo },
            { ItemType.DoubleAction, ItemType.PistolAmmo },
            { ItemType.PistolMk2, ItemType.PistolAmmo },
            { ItemType.SNSPistolMk2, ItemType.PistolAmmo },
            { ItemType.RevolverMk2, ItemType.PistolAmmo },

            { ItemType.MicroSMG, ItemType.SMGAmmo },
            { ItemType.MachinePistol, ItemType.SMGAmmo },
            { ItemType.SMG, ItemType.SMGAmmo },
            { ItemType.AssaultSMG, ItemType.SMGAmmo },
            { ItemType.CombatPDW, ItemType.SMGAmmo },
            { ItemType.MG, ItemType.SMGAmmo },
            { ItemType.CombatMG, ItemType.SMGAmmo },
            { ItemType.Gusenberg, ItemType.SMGAmmo },
            { ItemType.MiniSMG, ItemType.SMGAmmo },
            { ItemType.SMGMk2, ItemType.SMGAmmo },
            { ItemType.CombatMGMk2, ItemType.SMGAmmo },

            { ItemType.AssaultRifle, ItemType.RiflesAmmo },
            { ItemType.CarbineRifle, ItemType.RiflesAmmo },
            { ItemType.AdvancedRifle, ItemType.RiflesAmmo },
            { ItemType.SpecialCarbine, ItemType.RiflesAmmo },
            { ItemType.BullpupRifle, ItemType.RiflesAmmo },
            { ItemType.CompactRifle, ItemType.RiflesAmmo },
            { ItemType.AssaultRifleMk2, ItemType.RiflesAmmo },
            { ItemType.CarbineRifleMk2, ItemType.RiflesAmmo },
            { ItemType.SpecialCarbineMk2, ItemType.RiflesAmmo },
            { ItemType.BullpupRifleMk2, ItemType.RiflesAmmo },

            { ItemType.SniperRifle, ItemType.SniperAmmo },
            { ItemType.HeavySniper, ItemType.SniperAmmo },
            { ItemType.MarksmanRifle, ItemType.SniperAmmo },
            { ItemType.HeavySniperMk2, ItemType.SniperAmmo },
            { ItemType.MarksmanRifleMk2, ItemType.SniperAmmo },

            { ItemType.PumpShotgun, ItemType.ShotgunsAmmo },
            { ItemType.SawnOffShotgun, ItemType.ShotgunsAmmo },
            { ItemType.BullpupShotgun, ItemType.ShotgunsAmmo },
            { ItemType.AssaultShotgun, ItemType.ShotgunsAmmo },
            { ItemType.Musket, ItemType.ShotgunsAmmo },
            { ItemType.HeavyShotgun, ItemType.ShotgunsAmmo },
            { ItemType.DoubleBarrelShotgun, ItemType.ShotgunsAmmo },
            { ItemType.SweeperShotgun, ItemType.ShotgunsAmmo },
            { ItemType.PumpShotgunMk2, ItemType.ShotgunsAmmo },
        };
        public static Dictionary<ItemType, int> WeaponsClipsMax = new Dictionary<ItemType, int>()
        {
            { ItemType.Pistol, 12 },
            { ItemType.CombatPistol, 12 },
            { ItemType.Pistol50, 9 },
            { ItemType.SNSPistol, 6 },
            { ItemType.HeavyPistol, 18 },
            { ItemType.VintagePistol, 7 },
            { ItemType.MarksmanPistol, 1 },
            { ItemType.Revolver, 6 },
            { ItemType.APPistol, 18 },
            { ItemType.StunGun, 0 },
            { ItemType.FlareGun, 1 },
            { ItemType.DoubleAction, 6 }, // closed
            { ItemType.PistolMk2, 12 }, // closed
            { ItemType.SNSPistolMk2, 6 }, // closed
            { ItemType.RevolverMk2, 6 }, // closed

            { ItemType.MicroSMG, 16 },
            { ItemType.MachinePistol, 12 },
            { ItemType.SMG, 30 },
            { ItemType.AssaultSMG, 30 },
            { ItemType.CombatPDW, 30 },
            { ItemType.MG, 54 },
            { ItemType.CombatMG, 100 },
            { ItemType.Gusenberg, 30 },
            { ItemType.MiniSMG, 20 },
            { ItemType.SMGMk2, 30 }, // closed
            { ItemType.CombatMGMk2, 100 }, // closed

            { ItemType.AssaultRifle, 30 },
            { ItemType.CarbineRifle, 30 },
            { ItemType.AdvancedRifle, 30 },
            { ItemType.SpecialCarbine, 30 },
            { ItemType.BullpupRifle, 30 },
            { ItemType.CompactRifle, 30 },
            { ItemType.AssaultRifleMk2, 30 }, // closed
            { ItemType.CarbineRifleMk2, 30 }, // closed
            { ItemType.SpecialCarbineMk2, 30 }, // closed
            { ItemType.BullpupRifleMk2, 30 }, // closed

            { ItemType.SniperRifle, 10 },
            { ItemType.HeavySniper, 6 },
            { ItemType.MarksmanRifle, 8 },
            { ItemType.HeavySniperMk2, 6 }, // closed
            { ItemType.MarksmanRifleMk2, 8 }, // closed

            { ItemType.PumpShotgun, 8 },
            { ItemType.SawnOffShotgun, 8 },
            { ItemType.BullpupShotgun, 14 },
            { ItemType.AssaultShotgun, 8 },
            { ItemType.Musket, 1 },
            { ItemType.HeavyShotgun, 6 },
            { ItemType.DoubleBarrelShotgun, 2 },
            { ItemType.SweeperShotgun, 10 },
            { ItemType.PumpShotgunMk2, 8 }, // closed
        };

        public static Dictionary<int, int> FractionsLastSerial = new Dictionary<int, int>()
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 },
            { 6, 0 },
            { 7, 0 },
            { 8, 0 },
            { 9, 0 },
            { 10, 0 },
            { 11, 0 },
            { 12, 0 },
            { 13, 0 },
            { 14, 0 },
        };
        public static Dictionary<int, int> BusinessesLastSerial = new Dictionary<int, int>();

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var result = Database.QueryRead("SELECT * FROM `weapons`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("Table 'weapons' returns null result", Plugins.Logs.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    var id = Convert.ToInt32(Row["id"]);
                    var lastserial = Convert.ToInt32(Row["lastserial"]);

                    BusinessesLastSerial.Add(id, lastserial);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, Plugins.Logs.Type.Error); }
        }
        
        public static void Event_PlayerDeath(Player player, Player killer, uint reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                var UUID = Main.Players[player].UUID;
                if (!player.HasData("LastActiveWeap")) return;
                var wType = (ItemType)player.GetData<object>("LastActiveWeap");
                var activeWeapon = nInventory.Items[UUID].FirstOrDefault(i => i.Type == wType);
                if (activeWeapon != null)
                {
                    if (WeaponsAmmoTypes.ContainsKey(activeWeapon.Type))
                    {
                        var aType = WeaponsAmmoTypes[activeWeapon.Type];
                        var aItem = nInventory.Find(UUID, aType);
                        if (aItem != null)
                        {
                            nInventory.Remove(player, aType, aItem.Count);
                            Items.onDrop(player, new nItem(aType, aItem.Count), 1);
                        }
                    }

                    Items.onDrop(player, new nItem(activeWeapon.Type, 1, activeWeapon.Data), 1);
                    nInventory.Remove(player, activeWeapon);
                    Plugins.Trigger.ClientEvent(player, "removeAllWeapons");
                    NAPI.Task.Run(() => { try { player.RemoveAllWeapons(); } catch { } }, 100);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, Plugins.Logs.Type.Error); }
        }

        public static void Event_OnPlayerDisconnected(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            var weaponItem = nInventory.Items[Main.Players[player].UUID].FirstOrDefault(i => (nInventory.WeaponsItems.Contains(i.Type) || nInventory.MeleeWeaponsItems.Contains(i.Type)) && i.IsActive);
            if (weaponItem == null) return;
            int id = nInventory.FindIndex(Main.Players[player].UUID, weaponItem.Type);
            nInventory.Items[Main.Players[player].UUID][id].IsActive = false;
        }

        public static string GetSerial(bool isFraction, int id)
        {
            if (isFraction)
            {
                var fractionType = Fractions.Manager.FractionTypes[id];
                if (fractionType == 0 || fractionType == 1)
                {
                    return $"{1000 + id}xxxxx";
                }
                else
                {
                    var serial = 100000000 + id * 100000 + FractionsLastSerial[id];
                    FractionsLastSerial[id]++;

                    if (FractionsLastSerial[id] >= 99999)
                        FractionsLastSerial[id] = 0;

                    return serial.ToString();
                }
            }
            else
            {
                var serial = 200000000 + id * 100000 + BusinessesLastSerial[id];
                BusinessesLastSerial[id]++;

                if (BusinessesLastSerial[id] >= 99999)
                    BusinessesLastSerial[id] = 0;

                return serial.ToString();
            }
        }

        public static bool GiveWeapon(Player player, ItemType type, string serial)
        {
            var tryAdd = nInventory.TryAdd(player, new nItem(type));
            if (tryAdd == -1) return false;
            nInventory.Add(player, new nItem(type, 1, serial));
            return true;
        }

        public static void RemoveAll(Player player, bool ammo)
        {
            if (!Main.Players.ContainsKey(player)) return;
            int UUID = Main.Players[player].UUID;
            for (int i = nInventory.Items[UUID].Count - 1; i >= 0; i--)
            {
                if (i >= nInventory.Items[UUID].Count) continue;
                if (nInventory.WeaponsItems.Contains(nInventory.Items[UUID][i].Type) || nInventory.Items[UUID][i].Type == ItemType.StunGun || (nInventory.Items[UUID][i].Type == ItemType.Mask && !nInventory.Items[UUID][i].IsActive) ||
                    nInventory.MeleeWeaponsItems.Contains(nInventory.Items[UUID][i].Type) || (ammo && nInventory.AmmoItems.Contains(nInventory.Items[UUID][i].Type)))
                    nInventory.Items[UUID].RemoveAt(i);
            }
            Plugins.Trigger.ClientEvent(player, "removeAllWeapons");
            NAPI.Task.Run(() => { try { player.RemoveAllWeapons(); } catch { } }, 100);
            Interface.Dashboard.sendItems(player);
        }

        public static void SaveWeaponsDB()
        {
            foreach (var dict in BusinessesLastSerial)
                Database.Query($"UPDATE `weapons` SET `lastserial`={dict.Value} WHERE `id`={dict.Key}");
        }

        [RemoteEvent("playerReload")]
        public static void RemoteEvent_playerReload(Player player, int hash, int ammoInClip)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !nInventory.Items.ContainsKey(Main.Players[player].UUID)) return;
                Hash wHash = (Hash)hash;
                var wName = wHash.ToString();
                var wItemType = (ItemType)Enum.Parse(typeof(ItemType), wName);
                if (!WeaponsAmmoTypes.ContainsKey(wItemType)) return;
                if (ammoInClip == WeaponsClipsMax[wItemType]) return;

                var wAmmoType = WeaponsAmmoTypes[wItemType];
                var ammoItem = nInventory.Find(Main.Players[player].UUID, wAmmoType);
                var ammoLefts = (ammoItem == null) ? 0 : ammoItem.Count;
                if (ammoLefts == 0) return;

                if (ammoInClip > WeaponsClipsMax[wItemType]) ammoInClip = WeaponsClipsMax[wItemType];
                var ammo = (ammoLefts < WeaponsClipsMax[wItemType] - ammoInClip) ? ammoLefts : WeaponsClipsMax[wItemType] - ammoInClip;
                nInventory.Remove(player, wAmmoType, ammo);
                Plugins.Trigger.ClientEvent(player, "wgive", hash, ammo, true, true);
            }
            catch (Exception e) { Log.Write("PlayeReloadWeapon: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [RemoteEvent("playerTakeoffWeapon")]
        public static void RemoteEvent_playerTakeoffWeapon(Player player, int hash, int ammoInClip) // вызывается, если игрок убрал оружие самостоятельно
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !nInventory.Items.ContainsKey(Main.Players[player].UUID)) return;
                Hash wHash = (Hash)hash;
                var wName = wHash.ToString();
                var wItemType = (ItemType)Enum.Parse(typeof(ItemType), wName);

                var wItem = nInventory.Items[Main.Players[player].UUID].FirstOrDefault(i => i.Type == wItemType && i.IsActive);
                if (wItem == null) return;
                wItem.IsActive = false;
                Interface.Dashboard.sendItems(player);

                if (!WeaponsAmmoTypes.ContainsKey(wItemType)) return;

                var aType = WeaponsAmmoTypes[wItemType];

                var tryAdd = nInventory.TryAdd(player, new nItem(aType, ammoInClip));
                if (tryAdd == -1) tryAdd = ammoInClip;

                if (ammoInClip > WeaponsClipsMax[wItemType]) ammoInClip = WeaponsClipsMax[wItemType];
                if (ammoInClip - tryAdd > 0)
                    nInventory.Add(player, new nItem(aType, ammoInClip - tryAdd));

                if (tryAdd > 0)
                {
                    if (tryAdd > WeaponsClipsMax[wItemType]) tryAdd = 1;
                    Items.onDrop(player, new nItem(aType, tryAdd), 1);
                }
            }
            catch (Exception e) { Log.Write("PlayeTakeoffWeapon: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [RemoteEvent("takeoffWeapon")]
        public static void RemoteEvent_takeoffWeapon(Player player, int hash, int ammoInClip) // вызывается, если оружие убрали серверно
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !nInventory.Items.ContainsKey(Main.Players[player].UUID)) return;
                Hash wHash = (Hash)hash;
                var wName = wHash.ToString();
                var wItemType = (ItemType)Enum.Parse(typeof(ItemType), wName);

                if (!WeaponsAmmoTypes.ContainsKey(wItemType)) return;

                var aType = WeaponsAmmoTypes[wItemType];

                var tryAdd = nInventory.TryAdd(player, new nItem(aType, ammoInClip));
                if (tryAdd == -1) tryAdd = ammoInClip;

                if (ammoInClip > WeaponsClipsMax[wItemType]) ammoInClip = WeaponsClipsMax[wItemType];
                if (ammoInClip - tryAdd > 0)
                    nInventory.Add(player, new nItem(aType, ammoInClip - tryAdd));

                if (tryAdd > 0)
                {
                    if (tryAdd > WeaponsClipsMax[wItemType]) tryAdd = 1;
                    Items.onDrop(player, new nItem(aType, tryAdd), 1);
                }
            }
            catch (Exception e) { Log.Write("takeoffWeapon: " + e.Message, Plugins.Logs.Type.Error); }
        }

        [RemoteEvent("changeweap")]
        public static void RemoteEvent_changeWeapon(Player player, int key)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !nInventory.Items.ContainsKey(Main.Players[player].UUID)) return;
                var UUID = Main.Players[player].UUID;
                switch (key)
                {
                    case 1:
                        {
                            var wItem = nInventory.Items[UUID].FirstOrDefault(i => nInventory.WeaponsItems.Contains(i.Type) && WeaponsAmmoTypes[i.Type] == ItemType.PistolAmmo);
                            if (wItem != null) Items.onUse(player, wItem, -1);
                        }
                        return;
                    case 2:
                        {
                            var wItem = nInventory.Items[UUID].FirstOrDefault(i => nInventory.WeaponsItems.Contains(i.Type) && WeaponsAmmoTypes[i.Type] == ItemType.SMGAmmo);
                            if (wItem != null) Items.onUse(player, wItem, -1);
                        }
                        return;
                    case 3:
                        {
                            var wItem = nInventory.Items[UUID].FirstOrDefault(i => i.Type == ItemType.StunGun);
                            if (wItem != null) Items.onUse(player, wItem, -1);
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("changeweap: " + e.Message, Plugins.Logs.Type.Error); }
        }
    }
}