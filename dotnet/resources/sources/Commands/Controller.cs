using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iTeffa.Commands
{
    public class Controller : Script
    {
        private static readonly Nlogs Log = new Nlogs("Controller Commands");
        private static readonly Random rnd = new Random();
        public static void SendToAdmins(ushort minLVL, string message)
        {
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= minLVL)
                {
                    p.SendChatMessage(message);
                }
            }
        }
        public static string RainbowExploit(Player sender, string message)
        {
            if (message.Contains("!{"))
            {
                foreach (var p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"~y~[CHAT-EXPLOIT] {sender.Name} ({sender.Value}) - {message}");
                    }
                }
                return Regex.Replace(message, "!", string.Empty);
            }
            return message;
        }
        [ServerEvent(Event.ChatMessage)]
        public void API_onChatMessage(Player sender, string message)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                if (Main.Players[sender].Unmute > 0)
                {
                    Plugins.Notice.Send(sender, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Вы замучены еще на {Main.Players[sender].Unmute / 60} минут", 3000);
                    return;
                }
                else if (Main.Players[sender].VoiceMuted)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            Main.Players[sender].VoiceMuted = false;
                            sender.SetSharedData("voice.muted", false);
                        }
                        catch { }
                    });
                }

                message = RainbowExploit(sender, message);
                List<Player> players = Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        int[] id = new int[] { sender.Value };
                        foreach (Player c in players)
                        {
                            Trigger.ClientEvent(c, "sendRPMessage", "chat", "{name}: " + message, id);
                        }

                        if (!sender.HasData("PhoneVoip")) return;
                        Models.VoicePhoneMetaData phoneMeta = sender.GetData<Models.VoicePhoneMetaData>("PhoneVoip");
                        if (phoneMeta.CallingState == "talk" && Main.Players.ContainsKey(phoneMeta.Target))
                        {
                            var pSim = Main.Players[sender].Sim;
                            var contactName = (Main.Players[phoneMeta.Target].Contacts.ContainsKey(pSim)) ? Main.Players[phoneMeta.Target].Contacts[pSim] : pSim.ToString();
                            phoneMeta.Target.SendChatMessage($"[В телефоне] {contactName}: {message}");
                        }
                    }
                    catch (Exception e) { Log.Write("ChatMessage_TaskRun: " + e.Message, Nlogs.Type.Error); }
                });
                return;
            }
            catch (Exception e) { Log.Write("ChatMessage: " + e.Message, Nlogs.Type.Error); }
        }
        #region try
        public static void Try(Player sender, string message)
        {
            try
            {
                int result = rnd.Next(5);
                Log.Debug("Random result: " + result.ToString());
                switch (result)
                {
                    case 3:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#277C6B}" + " удачно", new int[] { sender.Value });
                        return;
                    default:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#FF0707}" + " неудачно", new int[] { sender.Value });
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        #endregion
        #region RP Chat
        public static void RPChat(string cmd, Player sender, string message, Player target = null)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                var names = new int[] { sender.Value };
                if (target != null) names = new int[] { sender.Value, target.Value };
                switch (cmd)
                {
                    case "me":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "me", "!{#ffe666}{name} " + message, names);
                        return;
                    case "todo":
                        var args = message.Split('*');
                        var msg = args[0];
                        var action = args[1];
                        var genderCh = (Main.Players[sender].Gender) ? "" : "а";
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "todo", msg + ",!{#ffe666} - сказал" + genderCh + " {name}, " + action, names);
                        return;
                    case "do":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "do", "!{#ffe666}" + message + " ({name})", names);
                        return;
                    case "s":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                        return;
                    case "b":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                        return;
                    case "m":
                        if (Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9 || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                        var vehicle = sender.Vehicle;
                        if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                        if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "m", "~r~[Мегафон] {name}: " + message, names);
                        return;
                    case "t":
                        if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                        foreach (var p in Main.Players.Keys.ToList())
                        {
                            if (p == null || !Main.Players.ContainsKey(p)) continue;

                            if (Main.Players[p].WorkID == 6)
                            {
                                if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                    p.SendChatMessage($"~y~[Рация дальнобойщиков] [{sender.Name}]: {message}");
                            }
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
        }
        public static Task RPChatAsync(string cmd, Player sender, string message)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(sender)) return;
                    var names = new int[] { sender.Value };
                    switch (cmd)
                    {
                        case "me":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "me", "!{#E066FF}{name} " + message, names);
                            return;
                        case "todo":
                            var args = message.Split('*');
                            var msg = args[0];
                            var action = args[1];
                            var genderCh = Main.Players[sender].Gender ? "" : "а";
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "todo", msg + ",!{#E066FF} - сказал" + genderCh + " {name}, " + action, names);
                            return;
                        case "do":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "do", "!{#E066FF}" + message + " ({name})", names);
                            return;
                        case "s":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                            return;
                        case "b":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                            return;
                        case "m":
                            if ((Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9) || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                            var vehicle = sender.Vehicle;
                            if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                            if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "m", "!{#FF4D4D}[Мегафон] {name}: " + message, names);
                            return;
                        case "t":
                            if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                            foreach (var p in Main.Players.Keys.ToList())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;

                                if (Main.Players[p].WorkID == 6)
                                {
                                    if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                        p.SendChatMessage($"~y~[Рация дальнобойщиков] [{sender.Name}]: {message}");
                                }
                            }
                            return;
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }
            });
            return Task.CompletedTask;
        }
        #endregion
        #region Игру кости
        public static int acceptDiceGame(Player playerTwo)
        {
            try
            {
                Player originPlayer = playerTwo.GetData<Player>("DICE_PLAYER");
                int money = playerTwo.GetData<int>("DICE_VALUE");

                if (money <= 0)
                {
                    Plugins.Notice.Send(playerTwo, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Денежная ценность должна быть выше 0", 3000);
                    Plugins.Notice.Send(originPlayer, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, $"Денежная ценность должна быть выше 0", 3000);

                    return 0;
                }

                int playerOneResult = new Random().Next(1, 6);
                int playerTwoResult = new Random().Next(1, 6);

                while (playerOneResult == playerTwoResult)
                {
                    Plugins.Notice.Send(playerTwo, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Играем снова, потому что у вас тот же кубик ${playerTwoResult}, что и у противника", 3000);
                    Plugins.Notice.Send(originPlayer, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Играем снова, потому что у вас тот же кубик ${playerTwoResult}, что и у противника", 3000);

                    playerOneResult = new Random().Next(1, 6);
                    playerTwoResult = new Random().Next(1, 6);
                }


                Plugins.Notice.Send(originPlayer, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"У вас ${playerOneResult}, а у вашего оппонента ${playerTwoResult}", 3000);
                Plugins.Notice.Send(playerTwo, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"У вас ${playerOneResult}, а у вашего оппонента ${playerTwoResult}", 3000);

                if (playerOneResult > playerTwoResult)
                {
                    Plugins.Notice.Send(originPlayer, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выиграли у соперника ${money}$", 3000);
                    Finance.Wallet.Change(originPlayer, money);
                    Finance.Wallet.Change(playerTwo, -money);
                    return 1;
                }
                else
                {
                    Plugins.Notice.Send(playerTwo, Plugins.TypeNotice.Success, Plugins.PositionNotice.TopCenter, $"Вы выиграли у соперника ${money}$", 3000);
                    Finance.Wallet.Change(originPlayer, -money);
                    Finance.Wallet.Change(playerTwo, money);
                    return 2;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), Nlogs.Type.Error); }

            return 0;
        }
        public static void rejectDiceGame(Player playerTwo)
        {
            Player originPlayer = playerTwo.GetData<Player>("DICE_PLAYER");

            Plugins.Notice.Send(originPlayer, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок (${playerTwo.Value}) отклонил игру", 3000);

            playerTwo.ResetData("DICE_PLAYER");
            playerTwo.ResetData("DICE_VALUE");
        }
        #endregion
    }
}
