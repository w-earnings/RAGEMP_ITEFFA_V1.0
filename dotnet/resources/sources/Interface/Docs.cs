﻿using GTANetworkAPI;
using iTeffa.Settings;
using System;
using System.Collections.Generic;

namespace iTeffa.Interface
{
    class Docs : Script
    {
        private static readonly Plugins.Logs Log = new Plugins.Logs("Docs");
        [RemoteEvent("passport")]
        public static void Event_Passport(Player player, params object[] arguments)
        {
            try
            {
                Player to = (Player)arguments[0];
                Log.Debug(to.Name.ToString());
                Passport(player, to);
            } catch(Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_PASSPORT\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        [RemoteEvent("licenses")]
        public static void Event_Licenses(Player player, params object[] arguments)
        {
            try
            {
                Player to = (Player)arguments[0];
                Licenses(player, to);
            } catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_LICENSES\":\n" + e.ToString(), Plugins.Logs.Type.Error);
            }
        }

        public static void Passport(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Plugins.Notice.Send(from, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptPass");
            to.SetData("IS_REQUESTED", true);
            Plugins.Notice.Send(to, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({from.Value}) хочет показать паспорт. Y/N - принять/отклонить", 3000);
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void Licenses(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Plugins.Notice.Send(from, Plugins.TypeNotice.Error, Plugins.PositionNotice.TopCenter, "Игрок находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptLics");
            to.SetData("IS_REQUESTED", true);
            Plugins.Notice.Send(to, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, $"Игрок ({from.Value}) хочет показать лицензии. Y/N - принять/отклонить", 3000);
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void AcceptPasport(Player player)
        {
            Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";
            string work = (acc.WorkID > 0) ? Working.WorkManager.JobStats[acc.WorkID] : "Безработный";
            List<object> data = new List<object>
                    {
                        acc.UUID,
                        acc.FirstName,
                        acc.LastName,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        fraction,
                        work
                    };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({from.Value}) показал Вам паспорт", 5000);
            Plugins.Notice.Send(from, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы показали паспорт игроку ({player.Value})", 5000);
            Log.Debug(json);
            Plugins.Trigger.ClientEvent(player, "passport", json);
            Plugins.Trigger.ClientEvent(player, "newPassport", from, acc.UUID);
        }
        public static void AcceptLicenses(Player player)
        {
            Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            
            var lic = "";
            for (int i = 0; i < acc.Licenses.Count; i++)
                if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
            if (lic == "") lic = "Отсутствуют";

            List<string> data = new List<string>
                    {
                        acc.FirstName,
                        acc.LastName,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        lic
                    };

            Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Игрок ({from.Value}) показал Вам лицензии", 5000);
            Plugins.Notice.Send(from, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Вы показали лицензии игроку ({player.Value})", 5000);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Plugins.Trigger.ClientEvent(player, "licenses", json);
        }
    }
}
