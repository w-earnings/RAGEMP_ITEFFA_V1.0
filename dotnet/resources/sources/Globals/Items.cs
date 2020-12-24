using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using iTeffa.Settings;

namespace iTeffa.Globals
{
    class Items : Script
    {
        private static readonly Nlogs Log = new Nlogs("Items");

        public static List<int> ItemsDropped = new List<int>();
        public static List<int> InProcessering = new List<int>();
        [ServerEvent(Event.EntityDeleted)]
        public void Event_OnEntityDeleted(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Object && NAPI.Data.HasEntityData(entity, "DELETETIMER"))
                {
                    Timers.Stop(NAPI.Data.GetEntityData(entity, "DELETETIMER"));
                    ItemsDropped.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                    InProcessering.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                }
            }
            catch (Exception e)
            {
                Log.Write("Event_OnEntityDeleted: " + e.Message, Nlogs.Type.Error);
            }
        }

        public static void deleteObject(GTANetworkAPI.Object obj)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    //Main.StopT(obj.GetData<object>("DELETETIMER"), "timer_33");
                    obj.ResetData("DELETETIMER");
                    ItemsDropped.Remove(obj.GetData<int>("ID"));
                    InProcessering.Remove(obj.GetData<int>("ID"));
                    obj.Delete();
                }
                catch (Exception e)
                {
                    Log.Write("UpdateObject: " + e.Message, Nlogs.Type.Error);
                }
            }, 0);
        }

        public static void onUse(Player player, nItem item, int index)
        {
            try
            {
                var UUID = Main.Players[player].UUID;
                if (nInventory.ClothesItems.Contains(item.Type) && item.Type != ItemType.BodyArmor && item.Type != ItemType.Mask)
                {
                    var data = (string)item.Data;
                    var clothesGender = Convert.ToBoolean(data.Split('_')[2]);
                    if (clothesGender != Main.Players[player].Gender)
                    {
                        var error_gender = (clothesGender) ? "мужская" : "женская";
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Это {error_gender} одежда", 3000);
                        Interface.Dashboard.Close(player);
                        return;
                    }
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2 && Main.Players[player].FractionID != 9) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не можете использовать это сейчас", 3000);
                        Interface.Dashboard.Close(player);
                        return;
                    }
                }

                if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
                {
                    if (item.IsActive)
                    {
                        var wHash = Weapons.GetHash(item.Type.ToString());
                        Trigger.ClientEvent(player, "takeOffWeapon", (int)wHash);
                        Commands.Controller.RPChat("me", player, $"убрал(а) {nInventory.ItemsNames[(int)item.Type]}");
                    }
                    else
                    {
                        var oldwItem = nInventory.Items[UUID].FirstOrDefault(i => (nInventory.WeaponsItems.Contains(i.Type) || nInventory.MeleeWeaponsItems.Contains(i.Type)) && i.IsActive);
                        if (oldwItem != null)
                        {
                            var oldwHash = Weapons.GetHash(oldwItem.Type.ToString());
                            Trigger.ClientEvent(player, "serverTakeOffWeapon", (int)oldwHash);
                            oldwItem.IsActive = false;
                            Interface.Dashboard.Update(player, oldwItem, nInventory.Items[UUID].IndexOf(oldwItem));
                            Commands.Controller.RPChat("me", player, $"убрал(а) {nInventory.ItemsNames[(int)oldwItem.Type]}");
                        }

                        var wHash = Weapons.GetHash(item.Type.ToString());
                        if (Weapons.WeaponsAmmoTypes.ContainsKey(item.Type))
                        {
                            var ammoItem = nInventory.Find(UUID, Weapons.WeaponsAmmoTypes[item.Type]);
                            var ammo = (ammoItem == null) ? 0 : ammoItem.Count;
                            if (ammo > Weapons.WeaponsClipsMax[item.Type]) ammo = Weapons.WeaponsClipsMax[item.Type];
                            if (ammoItem != null) nInventory.Remove(player, ammoItem.Type, ammo);
                            Trigger.ClientEvent(player, "wgive", (int)wHash, ammo, false, true);
                        }
                        else
                        {
                            Trigger.ClientEvent(player, "wgive", (int)wHash, 1, false, true);
                        }

                        Commands.Controller.RPChat("me", player, $"достал(а) {nInventory.ItemsNames[(int)item.Type]}");
                        item.IsActive = true;
                        player.SetData("LastActiveWeap", item.Type);
                        Interface.Dashboard.Update(player, item, index);
                        Interface.Dashboard.Close(player);
                    }
                    return;
                }

                if (nInventory.AmmoItems.Contains(item.Type)) return;

                if (nInventory.AlcoItems.Contains(item.Type))
                {
                    int stage = Convert.ToInt32(item.Type.ToString().Split("Drink")[1]);
                    int curStage = player.GetData<int>("RESIST_STAGE");

                    if (player.HasData("RESIST_BAN"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы пьяны до такой степени, что не можете открыть бутылку", 3000);
                        return;
                    }

                    var stageTimes = new List<int>() { 0, 300, 420, 600 };

                    if (curStage == 0 || curStage == stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }
                    else if (curStage < stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                    }
                    else if (curStage > stage)
                    {
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }

                    if (player.GetData<int>("RESIST_TIME") >= 1500)
                        player.SetData("RESIST_BAN", true);

                    Trigger.ClientEvent(player, "setResistStage", player.GetData<int>("RESIST_STAGE"));
                    BasicSync.AttachObjectToPlayer(player, nInventory.ItemModels[item.Type], 57005, Fractions.AlcoFabrication.AlcoPosOffset[item.Type], Fractions.AlcoFabrication.AlcoRotOffset[item.Type]);

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("amb@world_human_drinking@beer@male@idle_a", "idle_c", 49);
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (player != null)
                            {
                                if (!player.IsInVehicle) player.StopAnimation();
                                else player.SetData("ToResetAnimPhone", true);
                                Main.OffAntiAnim(player);
                                Trigger.ClientEvent(player, "startScreenEffect", "PPFilter", player.GetData<int>("RESIST_TIME") * 1000, false);
                                BasicSync.DetachObject(player);
                            }
                        } catch { }
                    }, 5000);

                    /*if (!player.HasData("RESIST_TIMER"))
                        player.SetData("RESIST_TIMER", Timers.Start(1000, () => Fractions.AlcoFabrication.ResistTimer(player.Name)));*/

                    Commands.Controller.RPChat("me", player, "выпил бутылку " + nInventory.ItemsNames[(int)item.Type]);
                    GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                }

                var gender = Main.Players[player].Gender;
                Log.Debug("item used");
                switch (item.Type)
                {
                    #region Clothes
                    case ItemType.Glasses:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                player.ClearAccessory(1);
                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не можете надеть эти очки с маской", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses = new ComponentItem(variation, texture);
                                player.SetAccessories(1, variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            return;
                        }
                    case ItemType.Hat:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item2)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не можете надеть этот головной убор с маской", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            Customization.SetHat(player, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Texture);
                            return;
                        }
                    case ItemType.Mask:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(Customization.EmtptySlots[gender][1], 0);
                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                                player.SendChatMessage("~r~Маска снята");
                                player.SetSharedData("HideNick", false);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(variation, texture);
                                player.SendChatMessage("~r~Маска одета");
                                player.SetSharedData("HideNick", true);

                                if (Customization.MaskTypes.ContainsKey(variation))
                                {
                                    if (Customization.MaskTypes[variation].Item1)
                                    {
                                        player.SetClothes(2, 0, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item2)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Hat);
                                        Customization.SetHat(player, -1, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item3)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Glasses);
                                        player.ClearAccessory(1);
                                    }
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
                            return;
                        }
                    case ItemType.Gloves:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                if (!Customization.CorrectGloves[gender][variation].ContainsKey(Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation)) return;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(variation, texture);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectGloves[gender][variation][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation], texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(3, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Texture);
                            return;
                        }
                    case ItemType.Leg:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(Customization.EmtptySlots[gender][4], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(4, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Texture);
                            return;
                        }
                    case ItemType.Bag:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(Customization.EmtptySlots[gender][5], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(5, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Texture);
                            return;
                        }
                    case ItemType.Feet:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(Customization.EmtptySlots[gender][6], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(6, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Texture);
                            return;
                        }
                    case ItemType.Jewelry:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(Customization.EmtptySlots[gender][7], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(7, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Texture);
                            return;
                        }
                    case ItemType.Accessories:
                        {
                            var itemData = (string)item.Data;
                            var variation = Convert.ToInt32(itemData.Split('_')[0]);
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);

                            if (item.IsActive)
                            {
                                var watchesSlot = Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches;
                                if (watchesSlot.Variation == variation && watchesSlot.Texture == texture)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(-1, 0);
                                    player.ClearAccessory(6);
                                }
                                else
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(-1, 0);
                                    player.ClearAccessory(7);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches.Variation == -1)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(variation, texture);
                                    player.SetAccessories(6, variation, texture);

                                    nInventory.Items[UUID][index].IsActive = true;
                                    Interface.Dashboard.Update(player, item, index);
                                }
                                else if (Customization.AccessoryRHand[gender].ContainsKey(variation))
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets.Variation == -1)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(Customization.AccessoryRHand[gender][variation], texture);
                                        player.SetAccessories(7, Customization.AccessoryRHand[gender][variation], texture);

                                        nInventory.Items[UUID][index].IsActive = true;
                                        Interface.Dashboard.Update(player, item, index);
                                    }
                                    else
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Заняты обе руки", 3000);
                                        return;
                                    }
                                }
                                else 
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Левая рука занята, а на правой никто часы не носит", 3000);
                                    return;
                                }
                            }
                            return;
                        }
                    case ItemType.Undershit:
                        {
                            var itemData = (string)item.Data;
                            var underwearID = Convert.ToInt32(itemData.Split('_')[0]);
                            var underwear = Customization.Underwears[gender][underwearID];
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);
                            if (item.IsActive)
                            {
                                if (underwear.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation)
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (underwear.Top == -1)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Эту одежду можно одеть только под низ верхней", 3000);
                                        return;
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                    nInventory.UnActiveItem(player, item.Type);
                                    nInventory.Items[UUID][index].IsActive = true;
                                    Interface.Dashboard.Update(player, item, index);
                                }
                                else
                                {
                                    var nowTop = Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    if (nowTop != null)
                                    {
                                        var topType = nowTop.Type;
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Эта одежда несовместима с Вашей верхней одеждой", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        Interface.Dashboard.Update(player, item, index);
                                    }
                                    else
                                    {
                                        if (underwear.Top == -1)
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Эту одежду можно одеть только под низ верхней", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        Interface.Dashboard.Update(player, item, index);
                                    }
                                }
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    case ItemType.BodyArmor:
                        {
                            if (item.IsActive)
                            {
                                item.Data = player.Armor.ToString();
                                player.Armor = 0;
                                player.ResetSharedData("HASARMOR");

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var armor = Convert.ToInt32((string)item.Data);
                                player.Armor = armor;
                                player.SetSharedData("HASARMOR", true);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            return;
                        }
                    case ItemType.Unknown:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(Customization.EmtptySlots[gender][10], 0);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(variation, texture);
                            }
                            player.SetClothes(10, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Texture);
                            return;
                        }
                    case ItemType.Top:
                        {
                            if (item.IsActive)
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                else
                                {
                                    var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                    var underwear = Customization.Underwears[gender][underwearID];
                                    if (underwear.Top == -1)
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                    else
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                Interface.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);

                                if (Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation) != null || Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    else
                                    {
                                        var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                        var underwear = Customization.Underwears[gender][underwearID];
                                        var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture;
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        Log.Debug($"UnderwearID: {underwearID} | TopType: {topType}");
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    }
                                }
                                else
                                {
                                    var underwearID = 0;
                                    var underwear = Customization.Underwears[gender].Values.FirstOrDefault(u => u.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture;
                                    if (underwear != null)
                                    {
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        Log.Debug($"UnderwearID: {underwearID} | TopType: {topType}");
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                Interface.Dashboard.Update(player, item, index);
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    #endregion
                    case ItemType.BagWithDrill:
                    case ItemType.BagWithMoney:
                    case ItemType.Pocket:
                    case ItemType.Cuffs:
                    case ItemType.CarKey:
                        return;
                    case ItemType.KeyRing:
                        List<nItem> items = new List<nItem>();
                        string data = item.Data;
                        List<string> keys = (data.Length == 0) ? new List<string>() : new List<string>(data.Split('/'));
                        if (keys.Count > 0 && string.IsNullOrEmpty(keys[keys.Count - 1]))
                            keys.RemoveAt(keys.Count - 1);

                        foreach (var key in keys)
                            items.Add(new nItem(ItemType.CarKey, 1, key));
                        player.SetData("KEYRING", nInventory.Items[Main.Players[player].UUID].IndexOf(item));
                        Interface.Dashboard.OpenOut(player, items, "Связка ключей", 7);
                        return;
                    case ItemType.Material:
                        Trigger.ClientEvent(player, "board", "close");
                        Interface.Dashboard.isopen[player] = false;
                        Interface.Dashboard.Close(player);
                        Fractions.Manager.OpenGunCraftMenu(player);
                        return;
                    case ItemType.Beer:
                        EatManager.AddWater(player, 12);
                        EatManager.AddEat(player, 2);
                        Commands.Controller.RPChat("me", player, $"выпил(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Burger:
                        player.Health = (player.Health + 30 > 100) ? 100 : player.Health + 30;
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"съел(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.eCola:
                        EatManager.AddWater(player, 15);
                        EatManager.AddEat(player, 2);
                        Commands.Controller.RPChat("me", player, $"выпил(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.HotDog:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 14);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"съел(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Pizza:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 30);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"съел(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Sandwich:
                        EatManager.AddWater(player, -5);
                        EatManager.AddEat(player, 8);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"съел(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Sprunk:
                        EatManager.AddWater(player, 25);
                        EatManager.AddEat(player, 2);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"выпил(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Сrisps:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.Controller.RPChat("me", player, $"съел(а) {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Rod:
                        RodManager.useInventory(player, 1);
                        break;
                    case ItemType.RodUpgrade:
                        RodManager.useInventory(player, 2);
                        break;
                    case ItemType.RodMK2:
                        RodManager.useInventory(player, 3);
                        break;
                    case ItemType.Drugs:
                        if (!player.HasData("USE_DRUGS") || DateTime.Now > player.GetData<DateTime>("USE_DRUGS"))
                        {
                            player.Health = (player.Health + 50 > 100) ? 100 : player.Health + 50;
                            Trigger.ClientEvent(player, "startScreenEffect", "DrugsTrevorClownsFight", 300000, false);
                            Commands.Controller.RPChat("me", player, $"закурил(а) косяк");
                            player.SetData("USE_DRUGS", DateTime.Now.AddMinutes(3));
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.GasCan:
                        if (!player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                            Interface.Dashboard.Close(player);
                            return;
                        }
                        var veh = player.Vehicle;
                        if (!veh.HasSharedData("PETROL")) return;
                        var fuel = veh.GetSharedData<int>("PETROL");
                        if (fuel == VehicleManager.VehicleTank[veh.Class])
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"В машине полный бак", 3000);
                            Interface.Dashboard.Close(player);
                            return;
                        }
                        fuel += 30;
                        if (fuel > VehicleManager.VehicleTank[veh.Class]) fuel = VehicleManager.VehicleTank[veh.Class];
                        veh.SetSharedData("PETROL", fuel);
                        if (player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "GARAGE")
                        {
                            var number = player.Vehicle.NumberPlate;
                            VehicleManager.Vehicles[number].Fuel = fuel;
                        }
                        break;
                    case ItemType.HealthKit:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {
                            player.Health = 100;
                            player.SetData("USE_MEDKIT", DateTime.Now.AddMinutes(5));
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("amb@code_human_wander_texting_fat@female@enter", "enter", 49);
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player == null) return;
                                    if (!player.IsInVehicle) player.StopAnimation();
                                    else player.SetData("ToResetAnimPhone", true);
                                    Main.OffAntiAnim(player);
                                    Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                                } catch { }
                            }, 5000);
                            Commands.Controller.RPChat("me", player, $"использовал(а) аптечку");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;


                    case ItemType.Remka:

                        if (!player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в машине", 3000);
                            return;
                        }
                        {
                            if (player.VehicleSeat == 0)
                            {
                                if (VehicleStreaming.GetVehicleDirt(player.Vehicle) >= 0.0f)
                                {

                                    VehicleManager.RepairCar(player.Vehicle);
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, "Вы отремонтировали транспорт", 3000);
                                }

                            }
                        }
                        break;

                    case ItemType.Lockpick:
                        if (player.GetData<int>("INTERACTIONCHECK") != 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Невозможно использовать в данный момент", 3000);
                            Interface.Dashboard.Close(player);
                            return;
                        }
                        //player.SetData("LOCK_TIMER", Main.StartT(10000, 999999, (o) => SafeMain.lockCrack(player, player.Name), "LOCK_TIMER"));
                        player.SetData("LOCK_TIMER", Timers.StartOnce(10000, () => SafeMain.lockCrack(player, player.Name)));
                        //player.FreezePosition = true;
                        Trigger.ClientEvent(player, "showLoader", "Идёт взлом", 1);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы начали взламывать дверь", 3000);
                        break;
                    case ItemType.ArmyLockpick:
                        if (!player.IsInVehicle || player.Vehicle.DisplayName != "Barracks")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Вы должны находиться в военном перевозчике материалов", 3000);
                            return;
                        }
                        if (VehicleStreaming.GetEngineState(player.Vehicle))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"Машину уже заведена", 3000);
                            return;
                        }
                        var lucky = new Random().Next(0, 5);
                        Log.Debug(lucky.ToString());
                        if (lucky == 5)
                            Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, $"У Вас не получилось завести машину. Попробуйте ещё раз", 3000);
                        else
                        {
                            VehicleStreaming.SetEngineState(player.Vehicle, true);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"У Вас получилось завести машину", 3000);
                        }
                        break;

                    case ItemType.Present:
                        player.Health = (player.Health + 10 > 100) ? 100 : player.Health + 10;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы открыли подарок, в нём были:", 3000);

                        Tuple<int, int> types = nInventory.PresentsTypes[Convert.ToInt32(item.Data)];
                        if (types.Item1 <= 2)
                        {
                            Main.Players[player].EXP += nInventory.TypesCounts[types.Item1];
                            if (Main.Players[player].EXP >= 3 + Main.Players[player].LVL * 3)
                            {
                                Main.Players[player].EXP = Main.Players[player].EXP - (3 + Main.Players[player].LVL * 3);
                                Main.Players[player].LVL += 1;
                            }

                            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"{nInventory.TypesCounts[types.Item1]} EXP", 3000);

                            Finance.Wallet.Change(player, nInventory.TypesCounts[types.Item2]);

                            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"$ {nInventory.TypesCounts[types.Item2]}", 3000);
                        }
                        else
                        {
                            Finance.Wallet.Change(player, nInventory.TypesCounts[types.Item1]);

                            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"$ {nInventory.TypesCounts[types.Item1]}", 3000);

                            Main.Players[player].EXP += nInventory.TypesCounts[types.Item2];
                            if (Main.Players[player].EXP >= 3 + Main.Players[player].LVL * 3)
                            {
                                Main.Players[player].EXP = Main.Players[player].EXP - (3 + Main.Players[player].LVL * 3);
                                Main.Players[player].LVL += 1;
                            }

                            Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"{nInventory.TypesCounts[types.Item2]} EXP", 3000);
                        }

                        Commands.Controller.RPChat("me", player, $"открыл(а) подарок {types.Item1} + {types.Item2}");
                        break;


                }
                nInventory.Remove(player, item.Type, 1);
                Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Вы использовали {nInventory.ItemsNames[item.ID]}", 3000);
                GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                Interface.Dashboard.Close(player);
            }
            catch (Exception e)
            {
                Log.Write($"EXCEPTION AT\"ITEM_USE\"/{item.Type}/{index}/{player.Name}/:\n" + e.ToString(), Nlogs.Type.Error);
            }
        }
        // TO DELETE
        private static readonly List<int> TypesCounts = new List<int>()
        {
            5, 10, 15, 3000, 5000, 10000
        };
        private static readonly List<Tuple<int, int>> PresentsTypes = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0, 5),
            new Tuple<int, int>(1, 4),
            new Tuple<int, int>(2, 3),
            new Tuple<int, int>(5, 0),
            new Tuple<int, int>(4, 1),
            new Tuple<int, int>(3, 2),
        };
        //
        public static void onDrop(Player player, nItem item, dynamic data)
        {
            try
            {
                var rnd = new Random();
                if (data != null && (int)data != 1)
                    Commands.Controller.RPChat("me", player, $"выбросил(а) {nInventory.ItemsNames[(int)item.Type]}");

                GameLog.Items($"player({Main.Players[player].UUID})", "ground", Convert.ToInt32(item.Type), 1, $"{item.Data}");

                if (!nInventory.ClothesItems.Contains(item.Type) && !nInventory.WeaponsItems.Contains(item.Type) && item.Type != ItemType.CarKey && item.Type != ItemType.KeyRing)
                {
                    foreach (var o in NAPI.Pools.GetAllObjects())
                    {
                        if (player.Position.DistanceTo(o.Position) > 2) continue;
                        if (!o.HasSharedData("TYPE") || o.GetSharedData<string>("TYPE") != "DROPPED" || !o.HasData("ITEM")) continue;
                        nItem oItem = o.GetData<nItem>("ITEM");
                        if (oItem.Type == item.Type)
                        {
                            oItem.Count += item.Count;
                            o.SetData("ITEM", oItem);
                            o.SetData("WILL_DELETE", DateTime.Now.AddMinutes(2));
                            return;
                        }
                    }
                }
                item.IsActive = false;

                
                var xrnd = rnd.NextDouble();
                var yrnd = rnd.NextDouble();
                var obj = NAPI.Object.CreateObject(nInventory.ItemModels[item.Type], player.Position + nInventory.ItemsPosOffset[item.Type] + new Vector3(xrnd, yrnd, 0), player.Rotation + nInventory.ItemsRotOffset[item.Type], 255, player.Dimension);
                obj.SetSharedData("TYPE", "DROPPED");
                obj.SetSharedData("PICKEDT", false);
                obj.SetData("ITEM", item);
                var id = rnd.Next(100000, 999999);
                while (ItemsDropped.Contains(id)) id = rnd.Next(100000, 999999);
                obj.SetData("ID", id);
                //obj.SetData("DELETETIMER", Main.StartT(14400000, 99999999, (o) => deleteObject(obj), "ODELETE_TIMER"));
                obj.SetData("DELETETIMER", Timers.StartOnce(14400000, () => deleteObject(obj)));
            }
            catch (Exception e) { Log.Write("onDrop: " + e.Message, Nlogs.Type.Error); }
        }
        public static void onTransfer(Player player, nItem item, dynamic data)
        {
            //
        }
    }
}
