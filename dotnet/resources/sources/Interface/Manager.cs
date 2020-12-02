using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using iTeffa.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace iTeffa.Interface
{
    class MenuManager : Script
    {
        public static Dictionary<Entity, Menu> Menus = new Dictionary<Entity, Menu>();
        private static nLog Log = new nLog("MenuControl");

        public static void Event_OnPlayerDisconnected(Player client, DisconnectionType type, string reason)
        {
            try
            {
                if (Menus.ContainsKey(client))
                    Menus.Remove(client);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        #region PhoneCallback
        [RemoteEvent("Phone")]
        public Task PhoneCallback(Player client, params object[] arguments)
        {
            if (client == null || !Main.Players.ContainsKey(client))
                return Task.CompletedTask;
            try
            {
                string eventName = Convert.ToString(arguments[0]);

                Menu menu = Menus[client];
                switch (eventName)
                {
                    case "navigation":
                        string btn = Convert.ToString(arguments[1]);
                        if (btn == "home")
                        {
                            Close(client, false);
                            Main.OpenPlayerMenu(client).Wait();
                        }
                        else if (btn == "back")
                        {
                            menu.BackButton.Invoke(client, menu);
                        }
                        break;
                    case "callback":
                        if (menu == null) return Task.CompletedTask;
                        string ItemID = Convert.ToString(arguments[1]);
                        string Event = Convert.ToString(arguments[2]);
                        //dynamic data = NAPI.Util.FromJson(arguments[3].ToString());
                        dynamic data = JsonConvert.DeserializeObject(arguments[3].ToString());

                        Menu.Item item = menu.Items.FirstOrDefault(i => i.ID == ItemID);
                        if (item == null) return Task.CompletedTask;
                        //await Log.DebugAsync($"app:{menu.ID}; item:{item.ID};");
                        //await Log.DebugAsync($"json:{Convert.ToString(arguments[3])}");
                        menu.Callback.Invoke(client, menu, item, Event, data);
                        return Task.CompletedTask;
                }
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Menu menu = Menus[client];
                Log.Write($"EXCEPTION AT /{menu.ID}/\"PHONE_CALLBACK\":\n" + e.ToString(), nLog.Type.Error);
            }
            return Task.CompletedTask;
        }
        #endregion
        #region Menu Open
        public static void Open(Player client, Menu menu, bool force = false)
        {
            try
            {
                if (Menus.ContainsKey(client))
                {
                    Log.Debug($"Player already have opened Menu! id:{Menus[client].ID}", nLog.Type.Warn);
                    if (!force) return;
                    Menus.Remove(client);
                }
                Menus.Add(client, menu);

                //string data = JsonConvert.SerializeObject(menu);
                string data = menu.getJsonStr();

                if (!client.HasData("Phone"))
                {
                    Trigger.ClientEvent(client, "phoneShow");
                    client.SetData("Phone", true);
                }
                Trigger.ClientEvent(client, "phoneOpen", data);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_OPEN\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static async Task OpenAsync(Player client, Menu menu, bool force = false)
        {
            try
            {
                lock (Menus)
                {
                    if (Menus.ContainsKey(client))
                    {
                        Log.Debug($"Player already have opened Menu! id:{Menus[client].ID}");
                        if (!force) return;
                        Menus.Remove(client);
                    }
                    Menus.Add(client, menu);
                }
                string data = await menu.getJsonStrAsync();

                if (!client.HasData("Phone"))
                {
                    Trigger.ClientEvent(client, "phoneShow");
                    client.SetData("Phone", true);
                }
                Trigger.ClientEvent(client, "phoneOpen", data);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_OPEN\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        #region Menu Close
        public static void Close(Player client, bool hidePhone = true)
        {
            try
            {
                if (Menus.ContainsKey(client))
                    Menus.Remove(client);
                if (hidePhone)
                {
                    Trigger.ClientEvent(client, "phoneHide");
                    client.ResetData("Phone");
                }
                Trigger.ClientEvent(client, "phoneClose");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_CLOSE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static Task CloseAsync(Player client, bool hidePhone = true)
        {
            try
            {
                lock (Menus)
                {
                    if (Menus.ContainsKey(client))
                        Menus.Remove(client);
                }
                if (hidePhone)
                {
                    Trigger.ClientEvent(client, "phoneHide");
                    client.ResetData("Phone");
                }
                Trigger.ClientEvent(client, "phoneClose");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MENUCONTROL_CLOSE\":\n" + e.ToString(), nLog.Type.Error);
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}
