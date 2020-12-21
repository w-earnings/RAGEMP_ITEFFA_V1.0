using GTANetworkAPI;
using System;
using System.Collections.Generic;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class ArmedBody : Script
    {
        static readonly string[] WeaponKeys = { "WEAPON_OBJ_PISTOL", "WEAPON_OBJ_SMG", "WEAPON_OBJ_BACK_RIGHT", "WEAPON_OBJ_BACK_LEFT" };

        private static readonly Nlogs Log = new Nlogs("Armedbody");

        public enum WeaponAttachmentType
        {
            RightLeg = 0,
            LeftLeg,
            RightBack,
            LeftBack
        }
        internal class WeaponAttachmentInfo
        {
            public string Model;
            public WeaponAttachmentType Type;

            public WeaponAttachmentInfo(string model, WeaponAttachmentType type)
            {
                Model = model;
                Type = type;
            }
        }
        static readonly Dictionary<WeaponHash, WeaponAttachmentInfo> WeaponData = new Dictionary<WeaponHash, WeaponAttachmentInfo>
        {
            // pistols
            { WeaponHash.Pistol, new WeaponAttachmentInfo("w_pi_pistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Combatpistol, new WeaponAttachmentInfo("w_pi_combatpistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Pistol50, new WeaponAttachmentInfo("w_pi_pistol50", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Snspistol, new WeaponAttachmentInfo("w_pi_sns_pistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Heavypistol, new WeaponAttachmentInfo("w_pi_heavypistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Vintagepistol, new WeaponAttachmentInfo("w_pi_vintage_pistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Marksmanpistol, new WeaponAttachmentInfo("w_pi_singleshot", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Revolver, new WeaponAttachmentInfo("w_pi_revolver", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Appistol, new WeaponAttachmentInfo("w_pi_appistol", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Stungun, new WeaponAttachmentInfo("w_pi_stungun", WeaponAttachmentType.RightLeg) },
            { WeaponHash.Flaregun, new WeaponAttachmentInfo("w_pi_flaregun", WeaponAttachmentType.RightLeg) },

            // smgs
            { WeaponHash.Microsmg, new WeaponAttachmentInfo("w_sb_microsmg", WeaponAttachmentType.LeftLeg) },
            { WeaponHash.Machinepistol, new WeaponAttachmentInfo("w_sb_compactsmg", WeaponAttachmentType.LeftLeg) },
            { WeaponHash.Minismg, new WeaponAttachmentInfo("w_sb_minismg", WeaponAttachmentType.LeftLeg) },

            // big smgs
            { WeaponHash.Smg, new WeaponAttachmentInfo("w_sb_smg", WeaponAttachmentType.RightBack) },
            { WeaponHash.Assaultsmg, new WeaponAttachmentInfo("w_sb_assaultsmg", WeaponAttachmentType.RightBack) },
            { WeaponHash.Combatpdw, new WeaponAttachmentInfo("w_sb_pdw", WeaponAttachmentType.RightBack) },
            { WeaponHash.Gusenberg, new WeaponAttachmentInfo("w_sb_gusenberg", WeaponAttachmentType.RightBack) },

            // shotguns
            { WeaponHash.Pumpshotgun, new WeaponAttachmentInfo("w_sg_pumpshotgun", WeaponAttachmentType.LeftBack) },
            //{ WeaponHash.SawnoffShotgun, new WeaponAttachmentInfo("w_sg_sawnoff", WeaponAttachmentType.LeftBack) },
            { WeaponHash.Bullpupshotgun, new WeaponAttachmentInfo("w_sg_bullpupshotgun", WeaponAttachmentType.LeftBack) },
            { WeaponHash.Assaultshotgun, new WeaponAttachmentInfo("w_sg_assaultshotgun", WeaponAttachmentType.LeftBack) },
            { WeaponHash.Heavyshotgun, new WeaponAttachmentInfo("w_sg_heavyshotgun", WeaponAttachmentType.LeftBack) },
            { WeaponHash.Doubleaction, new WeaponAttachmentInfo("w_sg_doublebarrel", WeaponAttachmentType.LeftBack) },

            // assault rifles
            { WeaponHash.Assaultrifle, new WeaponAttachmentInfo("w_ar_assaultrifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Carbinerifle, new WeaponAttachmentInfo("w_ar_carbinerifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Advancedrifle, new WeaponAttachmentInfo("w_ar_advancedrifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Specialcarbine, new WeaponAttachmentInfo("w_ar_specialcarbine", WeaponAttachmentType.RightBack) },
            { WeaponHash.Bullpuprifle, new WeaponAttachmentInfo("w_ar_bullpuprifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Compactrifle, new WeaponAttachmentInfo("w_ar_assaultrifle_smg", WeaponAttachmentType.RightBack) },

            // sniper rifles
            { WeaponHash.Marksmanrifle, new WeaponAttachmentInfo("w_sr_marksmanrifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Sniperrifle, new WeaponAttachmentInfo("w_sr_sniperrifle", WeaponAttachmentType.RightBack) },
            { WeaponHash.Heavysniper, new WeaponAttachmentInfo("w_sr_heavysniper", WeaponAttachmentType.RightBack) },

            // lmgs
            { WeaponHash.Mg, new WeaponAttachmentInfo("w_mg_mg", WeaponAttachmentType.LeftBack) },
            { WeaponHash.Combatmg, new WeaponAttachmentInfo("w_mg_combatmg", WeaponAttachmentType.LeftBack) }
        };

        #region Methods
        public static void CreateWeaponProp(Player player, WeaponHash weapon)
        {
            if (!WeaponData.ContainsKey(weapon)) return;
            RemoveWeaponProp(player, WeaponData[weapon].Type);

            // make sure player has the weapon
            if (Array.IndexOf(player.Weapons, weapon) == -1) return;

            Vector3 offset = new Vector3(0.0, 0.0, 0.0);
            Vector3 rotation = new Vector3(0.0, 0.0, 0.0);

            switch (WeaponData[weapon].Type)
            {
                case WeaponAttachmentType.RightLeg:
                    offset = new Vector3(0.02, 0.06, 0.1);
                    rotation = new Vector3(-100.0, 0.0, 0.0);
                    break;

                case WeaponAttachmentType.LeftLeg:
                    offset = new Vector3(0.08, 0.03, -0.1);
                    rotation = new Vector3(-80.77, 0.0, 0.0);
                    break;

                case WeaponAttachmentType.RightBack:
                    offset = new Vector3(-0.1, -0.15, -0.13);
                    rotation = new Vector3(0.0, 0.0, 3.5);
                    break;

                case WeaponAttachmentType.LeftBack:
                    offset = new Vector3(-0.1, -0.15, 0.11);
                    rotation = new Vector3(-180.0, 0.0, 0.0);
                    break;
            }

            GTANetworkAPI.Object temp_handle = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(WeaponData[weapon].Model), player.Position, player.Rotation, 255, 0);
            //temp_handle.AttachTo(player, bone, offset, rotation);

            NAPI.Data.SetEntityData(player, WeaponKeys[(int)WeaponData[weapon].Type], temp_handle);
        }

        public static void RemoveWeaponProp(Player player, WeaponAttachmentType type)
        {
            int type_int = (int)type;
            if (!NAPI.Data.HasEntityData(player, WeaponKeys[type_int])) return;

            GTANetworkAPI.Object obj = NAPI.Data.GetEntityData(player, WeaponKeys[type_int]);
            obj.Delete();

            NAPI.Data.ResetEntityData(player, WeaponKeys[type_int]);
        }

        public static void RemoveWeaponProps(Player player)
        {
            foreach (string key in WeaponKeys)
            {
                if (!NAPI.Data.HasEntityData(player, key)) continue;

                GTANetworkAPI.Object obj = NAPI.Data.GetEntityData(player, key);
                obj.Delete();

                NAPI.Data.ResetEntityData(player, key);
            }
        }
        #endregion

        #region Exported Methods
        public void RemovePlayerWeapon(Player player, WeaponHash weapon)
        {
            if (WeaponData.ContainsKey(weapon))
            {
                string key = WeaponKeys[(int)WeaponData[weapon].Type];

                if (NAPI.Data.HasEntityData(player, key))
                {
                    GTANetworkAPI.Object obj = NAPI.Data.GetEntityData(player, key);

                    if (obj.Model == NAPI.Util.GetHashKey(WeaponData[weapon].Model))
                    {
                        obj.Delete();
                        NAPI.Data.ResetEntityData(player, key);
                    }
                }
            }
            NAPI.Player.RemovePlayerWeapon(player, weapon);
        }

        public void RemoveAllPlayerWeapons(Player player)
        {
            RemoveWeaponProps(player);
            NAPI.Player.RemoveAllPlayerWeapons(player);
        }
        #endregion

        #region Events
        [ServerEvent(Event.ResourceStop)]
        public void ArmedBody_Exit()
        {
            try
            {
                foreach (Player player in NAPI.Pools.GetAllPlayers()) RemoveWeaponProps(player);
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, Nlogs.Type.Error); }
        }
        #endregion
    }
}