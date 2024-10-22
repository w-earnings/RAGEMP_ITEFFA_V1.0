﻿using System;
using System.Data;
using GTANetworkAPI;
using MySqlConnector;
using iTeffa.Settings;
using System.Collections.Generic;
using System.Linq;

namespace iTeffa.Globals
{
    class ReportSys : Script
    {
        private class Report
        {
            public int ID { get; set; }
            public string Author { get; set; }
            public string Question { get; set; }
            public string Response { get; set; }
            public string BlockedBy { get; set; }
            public DateTime OpenedDate { get; set; }
            public DateTime ClosedDate { get; set; }
            public bool Status { get; set; }

            public void Send(Player someone = null)
            {
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].AdminLVL < adminLvL) continue;
                        Plugins.Trigger.ClientEvent(target, "addreport", ID, Author, Question);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].AdminLVL < adminLvL) return;
                    Plugins.Trigger.ClientEvent(someone, "addreport", ID, Author, Question);
                }
            }
        }
        private static Dictionary<int, Report> Reports;
        private static readonly Plugins.Logs Log = new Plugins.Logs("ReportSys");
        private static readonly Config conf = new Config("ReportSys");
        private static readonly byte adminLvL = conf.TryGet<byte>("AdminLvL", 1);
        public static void Init()
        {
            try
            {
                Reports = new Dictionary<int, Report>();
                string cmd = @" CREATE TABLE IF NOT EXISTS `questions` (`ID` int(12) unsigned NOT NULL AUTO_INCREMENT, `Author` varchar(40) NOT NULL, `Question` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL, `Respondent` varchar(40) DEFAULT NULL, `Response` varchar(150) CHARACTER SET utf8 COLLATE utf8_bin DEFAULT NULL, `Opened` datetime NOT NULL, `Closed` datetime DEFAULT NULL, `Status` tinyint(4) DEFAULT 0, PRIMARY KEY (`ID`)) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1; 
                SELECT * FROM questions;";
                DataTable result = Database.QueryRead(cmd);
                if (result is null) return;
                foreach (DataRow row in result.Rows)
                {
                    if (Convert.ToBoolean((sbyte)row[7]) != false) continue;

                    Reports.Add((int)row[0], new Report
                    {
                        ID = (int)row[0],
                        Author = row[1].ToString(),
                        Question = Main.BlockSymbols(row[2].ToString()),
                        BlockedBy = row[3].ToString(),
                        Response = Main.BlockSymbols(row[4].ToString()),
                        OpenedDate = (DateTime)row[5],
                        ClosedDate = (DateTime)row[6],
                        Status = Convert.ToBoolean((sbyte)row[7])
                    });
                }

            }
            catch (Exception e)
            {
                Log.Write("Init: " + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        public static void onAdminLoad(Player client)
        {
            try
            {
                foreach (Report report in Reports.Values)
                {
                    report.Send(client);
                }

            }
            catch (Exception e)
            {
                Log.Write("onAdminLoad: " + e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        #region Remote Events
        [RemoteEvent("takereport")]
        public void ReportTake(Player client, int id, bool retrn = false)
        {
            if (Main.Players[client].AdminLVL <= 0) return;
            Log.Debug($"Report take: {id} {retrn}");
            if (!Reports.ContainsKey(id))
            {
                Remove(id, client);
                return;
            }

            if (Reports[id].Status)
            {
                Remove(id, client);
                return;
            }

            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(target)) continue;
                if (Main.Players[target].AdminLVL < adminLvL) continue;
                if (retrn) Plugins.Trigger.ClientEvent(target, "setreport", id, "");
                else Plugins.Trigger.ClientEvent(target, "setreport", id, client.Name);
            }
        }
        [RemoteEvent("sendreport")]
        public void ReportSend(Player player, int ID, string answer)
        {
            if (Main.Players[player].AdminLVL <= 0) return;
            Log.Debug($"Report send: {ID} {answer}");
            if (!Reports.ContainsKey(ID)) return;
            if (!Reports[ID].Status)
            {
                AddAnswer(player, ID, answer);
            }
            else
            {
                player.SendChatMessage("Diese Beschwerde kann nicht mehr geändert werden.");
                Remove(ID, player);
            }
        }
        #endregion
        public static void AddReport(Player player, string question)
        {
            try
            {
                question = Main.BlockSymbols(question);
                player.SetData("NEXT_REPORT", DateTime.Now.AddMinutes(2));
                Plugins.Notice.Send(player, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Sie haben eine Frage gesendet: {question}", 3000);
                player.SetData("IS_REPORT", true);

                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `questions` (`Author`,`Question`,`Opened`,`Closed`) VALUES (@pn,@q,@time,@ntime); SELECT LAST_INSERT_ID();"
                };
                cmd.Parameters.AddWithValue("@pn", player.Name);
                cmd.Parameters.AddWithValue("@q", question);
                cmd.Parameters.AddWithValue("@time", Database.ConvertTime(DateTime.Now));
                cmd.Parameters.AddWithValue("@ntime", Database.ConvertTime(DateTime.MinValue));
                DataTable dt = Database.QueryRead(cmd);
                int id = Convert.ToInt32(dt.Rows[0][0]);
                Report report = new Report
                {
                    ID = id,
                    Author = player.Name,
                    Question = question,
                    BlockedBy = "",
                    Response = "",
                    Status = false,
                    OpenedDate = DateTime.Now,
                    ClosedDate = DateTime.MinValue
                };
                report.Send();
                Reports.Add(id, report);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        private static void AddAnswer(Player player, int repID, string response)
        {
            try
            {
                response = Main.BlockSymbols(response);

                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < adminLvL) return;

                if (!Reports.ContainsKey(repID)) return;

                DateTime now = DateTime.Now;

                try
                {
                    Player target = NAPI.Player.GetPlayerFromName(Reports[repID].Author);
                    if (target is null)
                    {
                        Plugins.Notice.Send(player, Plugins.TypeNotice.Warning, Plugins.PositionNotice.TopCenter, "Spieler nicht gefunden!", 3000);
                    }
                    else
                    {
                        target.SendChatMessage($"~r~Antwort von { player.Name} ({player.Value}): {response}");
                        Plugins.Notice.Send(target, Plugins.TypeNotice.Info, Plugins.PositionNotice.TopCenter, $"Antwort von { player.Name}: {response}", 5000);
                        foreach (var p in Main.Players.Keys.ToList())
                        {
                            if (Main.Players[p].AdminLVL >= adminLvL)
                            {
                                p.SendChatMessage($"~y~[ANSWER] {player.Name}({player.Value})->{target.Name}({target.Value}): {response}");
                            }
                        }
                        Loggings.Admin(player.Name, $"answer({response})", target.Name);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write($"PlayerAnswer:\n" + ex.ToString(), Plugins.Logs.Type.Error);
                }

                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "UPDATE questions SET Respondent=@resp,Response=@res,Status=@st,Closed=@time WHERE ID=@repid LIMIT 1"
                };
                cmd.Parameters.AddWithValue("@resp", player.Name);
                cmd.Parameters.AddWithValue("@res", response);
                cmd.Parameters.AddWithValue("@st", true);
                cmd.Parameters.AddWithValue("@time", Database.ConvertTime(now));
                cmd.Parameters.AddWithValue("@repid", repID);
                Database.Query(cmd);

                Reports[repID].Author = player.Name;
                Reports[repID].Response = response;
                Reports[repID].ClosedDate = now;
                Reports[repID].Status = true;

                Remove(repID);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Plugins.Logs.Type.Error);
            }
        }
        private static void Remove(int ID_, Player someone = null)
        {
            try
            {
                Log.Debug($"Remove {ID_}");
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].AdminLVL < adminLvL) continue;
                        Plugins.Trigger.ClientEvent(target, "delreport", ID_);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].AdminLVL < adminLvL) return;

                    Plugins.Trigger.ClientEvent(someone, "delreport", ID_);
                }
                Reports.Remove(ID_);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), Plugins.Logs.Type.Error);
            }
        }
    }
}
