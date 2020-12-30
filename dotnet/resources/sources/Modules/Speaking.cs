using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Modules
{
    class RoomController
    {
        public Dictionary<string, Room> Rooms;
        private static RoomController instance;
        private RoomController()
        {
            Rooms = new Dictionary<string, Room>();
        }
        public static RoomController getInstance()
        {
            if (instance == null)
            {
                instance = new RoomController();
            }
            return instance;
        }
        public void CreateRoom(string name)
        {
            if (!Rooms.ContainsKey(name))
            {
                Rooms.Add(name, new Room(name));
                Console.WriteLine("Room " + name + " created");
            }
        }
        public void RemoveRoom(string name)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];
                room.OnRemove();
                Rooms.Remove(name);

                Console.WriteLine("Room " + name + " removed");
            }
        }
        public bool HasRoom(string name)
        {
            return Rooms.ContainsKey(name);
        }
        public void OnJoin(string name, Player player)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];
                room.OnJoin(player);
            }
        }
        public void OnQuit(string name, Player player)
        {
            if (Rooms.ContainsKey(name))
            {
                Room room = Rooms[name];
                room.OnQuit(player);
            }
        }
    }
    class Room
    {
        public string Name;
        public List<Player> Players;

        public Dictionary<string, object> MetaData { get { return new Dictionary<string, object> { { "name", Name } }; } }

        public Room(string Name)
        {
            this.Name = Name;

            this.Players = new List<Player>();
        }

        public void OnJoin(Player player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));


                Plugins.Trigger.ClientEvent(player, "voice.radioConnect", argsMe.ToArray());
                Plugins.Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioConnect", MetaData, player);

                var tempPlayer = player.GetData<Models.VoiceMetaData>("Voip");
                tempPlayer.RadioRoom = Name;

                player.SetData<Models.VoiceMetaData>("Voip", tempPlayer);
                Players.Add(player);
            }
        }

        public void OnQuit(Player player)
        {
            if (Players.Contains(player))
            {
                var argsMe = new List<object> { MetaData };
                Players.ForEach(_player => argsMe.Add(_player));

                Plugins.Trigger.ClientEvent(player, "voice.radioDisconnect", argsMe.ToArray());
                Plugins.Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData, player);

                player.ResetData("Voip");
                Players.Remove(player);
            }
        }

        public void OnRemove()
        {
            Plugins.Trigger.ClientEventToPlayers(Players.ToArray(), "voice.radioDisconnect", MetaData);
            Players.Clear();
        }
    }
    public class Voice : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Voice");
        public Voice()
        {
            RoomController.getInstance().CreateRoom("VoiceRoom");
        }

        public Player GetPlayerById(int id)
        {
            Player target = null;
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Value == id)
                {
                    target = player;
                    break;
                }
            }
            return target;
        }

        public static void PlayerJoin(Player player)
        {
            try
            {
                Models.VoiceMetaData DefaultVoiceMeta = new Models.VoiceMetaData
                {
                    IsEnabledMicrophone = false,
                    RadioRoom = "",
                    StateConnection = "closed",
                    MicrophoneKey = 78 // N
                };

                Models.VoicePhoneMetaData DefaultVoicePhoneMeta = new Models.VoicePhoneMetaData
                {
                    CallingState = "nothing",
                    Target = null
                };

                player.SetData("Voip", DefaultVoiceMeta);
                player.SetData("PhoneVoip", DefaultVoicePhoneMeta);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void PlayerQuit(Player player, string reson)
        {
            try
            {
                RoomController controller = RoomController.getInstance();
                Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");

                if (controller.HasRoom(voiceMeta.RadioRoom))
                {
                    controller.OnQuit(voiceMeta.RadioRoom, player);
                }

                Models.VoicePhoneMetaData playerPhoneMeta = player.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target != null)
                {
                    Player target = playerPhoneMeta.Target;
                    Models.VoicePhoneMetaData targetPhoneMeta = target.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                    var pSim = Main.Players[player].Sim;
                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();

                    Plugins.Notice.Send(target, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{playerName} завершил вызов", 3000);
                    targetPhoneMeta.Target = null;
                    targetPhoneMeta.CallingState = "nothing";

                    target.ResetData("AntiAnimDown");
                    if (!target.IsInVehicle) target.StopAnimation();
                    else target.SetData("ToResetAnimPhone", true);

                    Globals.BasicSync.DetachObject(target);

                    Plugins.Trigger.ClientEvent(target, "voice.phoneStop");

                    target.SetData("PhoneVoip", targetPhoneMeta);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        [Command("v_reload")]
        public void voiceDebugReload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v1).");
            Plugins.Trigger.ClientEvent(player, "v_reload");
        }

        [Command("v_reload2")]
        public void voiceDebug2Reload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v2).");
            Plugins.Trigger.ClientEvent(player, "v_reload2");
        }

        [Command("v_reload3")]
        public void voiceDebug3Reload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v3).");
            Plugins.Trigger.ClientEvent(player, "v_reload3");
        }

        [RemoteEvent("add_voice_listener")]
        public void add_voice_listener(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Player target = (Player)arguments[0];
                if (!Main.Players.ContainsKey(target)) return;
                player.EnableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [RemoteEvent("remove_voice_listener")]
        public void remove_voice_listener(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Player target = (Player)arguments[0];
                if (!Main.Players.ContainsKey(target)) return;
                player.DisableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // METHODS //

        public static void PhoneCallCommand(Player player, Player target)
        {
            try
            {
                if (player.HasData("AntiAnimDown"))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Невозможно достать мобильный телефон", 3000);
                    return;
                }
                if (target != null && Main.Players.ContainsKey(target))
                {
                    Models.VoicePhoneMetaData targetPhoneMeta = target.GetData<Models.VoicePhoneMetaData>("PhoneVoip");
                    Models.VoicePhoneMetaData playerPhoneMeta = player.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                    if (playerPhoneMeta.Target != null)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "В данный момент Вы уже разговариваете", 3000);
                        return;
                    }

                    var tSim = Main.Players[target].Sim;
                    var pSim = Main.Players[player].Sim;

                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                    var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                    if (targetPhoneMeta.Target != null)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В данный момент {targetName} занят", 3000);
                        Plugins.Notice.Send(target, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{playerName} пытался Вам дозвониться", 3000);
                        return;
                    }

                    targetPhoneMeta.Target = player;
                    targetPhoneMeta.CallingState = "callMe";

                    playerPhoneMeta.Target = target;
                    playerPhoneMeta.CallingState = "callTo";

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    Globals.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                    player.SetData("PhoneVoip", playerPhoneMeta);
                    target.SetData("PhoneVoip", targetPhoneMeta);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

                            Models.VoicePhoneMetaData tPhoneMeta = target.GetData<Models.VoicePhoneMetaData>("PhoneVoip");
                            Models.VoicePhoneMetaData pPhoneMeta = player.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                            if (pPhoneMeta.Target == null || pPhoneMeta.Target != target || pPhoneMeta.CallingState == "talk") return;

                            pPhoneMeta.Target = null;
                            tPhoneMeta.Target = null;

                            pPhoneMeta.CallingState = "nothing";
                            tPhoneMeta.CallingState = "nothing";

                            if (!player.IsInVehicle)
                                player.StopAnimation();
                            else
                                player.SetData("ToResetAnimPhone", true);
                            Globals.BasicSync.DetachObject(player);

                            player.SetData("PhoneVoip", pPhoneMeta);
                            target.SetData("PhoneVoip", tPhoneMeta);

                            player.ResetData("AntiAnimDown");

                            Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{targetName} не отвечает", 3000);
                            Plugins.Notice.Send(target, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{playerName} завершил вызов", 3000);
                        }
                        catch { }

                    }, 20000);

                    Plugins.Notice.Send(target, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"{playerName} звонит Вам. Откройте телефон, чтобы принять/отклонить вызов", 3000);
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Alert, Plugins.PositionNotice.TopCenter, $"Вы звоните {targetName}", 3000);
                }
                else
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Абонент вне зоны действия сети", 3000);
                }

            }
            catch (Exception e)
            {
                Log.Write($"PhoneCall: {e.Message}", Plugins.Logs.Type.Error);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_PlayerExitVehicle(Player player, Vehicle veh)
        {
            try
            {
                if (player.HasData("ToResetAnimPhone"))
                {
                    player.StopAnimation();
                    player.ResetData("ToResetAnimPhone");
                }
            }
            catch { }
        }

        //[Command("ca")]
        public static void PhoneCallAcceptCommand(Player player)
        {
            try
            {
                Models.VoicePhoneMetaData playerPhoneMeta = player.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target == null || playerPhoneMeta.CallingState == "callTo" || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В данный момент Вам никто не звонит", 3000);
                    return;
                }

                Player target = playerPhoneMeta.Target;

                Models.VoicePhoneMetaData targetPhoneMeta = target.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                playerPhoneMeta.CallingState = "talk";
                targetPhoneMeta.CallingState = "talk";

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{playerName} принял Ваш вызов", 3000);
                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы приняли вызов от {targetName}", 3000);

                Main.OnAntiAnim(player);
                player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                Globals.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                Plugins.Trigger.ClientEvent(player, "voice.phoneCall", target, 1);
                Plugins.Trigger.ClientEvent(target, "voice.phoneCall", player, 1);

                player.ResetData("ToResetAnimPhone");
                target.ResetData("ToResetAnimPhone");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallAccept: {e.Message}", Plugins.Logs.Type.Error);
            }
        }

        //[Command("h")]
        public static void PhoneHCommand(Player player)
        {
            try
            {
                Models.VoicePhoneMetaData playerPhoneMeta = player.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target == null || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    if (!player.HasData("IS_DYING") && !player.GetData<bool>("CUFFED")) Plugins.Notice.Send(player, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"В данный момент Вы не говорите по телефону", 3000);
                    return;
                }

                Player target = playerPhoneMeta.Target;
                Models.VoicePhoneMetaData targetPhoneMeta = target.GetData<Models.VoicePhoneMetaData>("PhoneVoip");

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Plugins.Notice.Send(player, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Звонок завершен", 3000);
                Plugins.Notice.Send(target, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"{playerName} завершил звонок", 3000);

                playerPhoneMeta.Target = null;
                targetPhoneMeta.Target = null;

                playerPhoneMeta.CallingState = "nothing";
                targetPhoneMeta.CallingState = "nothing";

                if (!player.IsInVehicle) player.StopAnimation();
                if (!target.IsInVehicle) target.StopAnimation();

                player.ResetData("AntiAnimDown");
                target.ResetData("AntiAnimDown");
                if (player.IsInVehicle) player.SetData("ToResetAnimPhone", true);
                if (player.IsInVehicle) target.SetData("ToResetAnimPhone", true);

                Globals.BasicSync.DetachObject(player);
                Globals.BasicSync.DetachObject(target);

                Plugins.Trigger.ClientEvent(player, "voice.phoneStop");
                Plugins.Trigger.ClientEvent(target, "voice.phoneStop");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallCancel: {e.Message}", Plugins.Logs.Type.Error);
            }
        }

        //[Command("changeroom")]
        public void ChangeRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();
                    Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");

                    if (controller.HasRoom(name))
                    {
                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            player.SendChatMessage("You are already on this room");
                            return;
                        }

                        controller.OnQuit(name, player);
                        controller.OnJoin(name, player);
                    }
                    else
                    {
                        player.SendChatMessage("This room doesn't exist");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("createroom")]
        public void CreateRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (!controller.HasRoom(name))
                    {
                        controller.CreateRoom(name);

                        player.SendChatMessage("You create room - " + name);
                    }
                    else
                    {
                        player.SendChatMessage("Room already created");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("removeroom")]
        public void RemoveRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        controller.RemoveRoom(name);

                        player.SendChatMessage("You has removed room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("leaveroom")]
        public void LeaveRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");

                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            controller.OnQuit(name, player);
                        }

                        player.SendChatMessage("You leave from room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SetVoiceDistance(Player player, float distance)
        {
            player.SetSharedData("voice.distance", distance);
        }

        public float GetVoiceDistance(Player player)
        {
            return player.GetSharedData<float>("voice.distance");
        }

        public bool IsMicrophoneEnabled(Player player)
        {
            Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");

            return voiceMeta.IsEnabledMicrophone;
        }

        public void SetVoiceMuted(Player player, bool isMuted)
        {
            player.SetSharedData("voice.muted", isMuted);
        }

        public bool GetVoiceMuted(Player player)
        {
            return player.GetSharedData<bool>("voice.muted");
        }

        public void SetMicrophoneKey(Player player, int microphoneKey)
        {
            try
            {
                Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");
                voiceMeta.MicrophoneKey = microphoneKey;

                Plugins.Trigger.ClientEvent(player, "voice.changeMicrophoneActivationKey", microphoneKey);
                player.SetData("Voip", voiceMeta);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public int GetMicrophoneKey(Player player)
        {
            Models.VoiceMetaData voiceMeta = player.GetData<Models.VoiceMetaData>("Voip");
            return voiceMeta.MicrophoneKey;
        }
    }
}
