using System;
using System.Collections.Generic;
using System.Threading;

namespace iTeffa.Settings
{
    public static class Timers
    {
        public static Dictionary<string, Models.Timers > timers = new Dictionary<string, Models.Timers>();
        public static Nlogs Log = new Nlogs("nTimer", false);
        private static readonly Config config = new Config("Timers");
        private static Thread thread;
        private static int delay;
        private static int clearDelay;

        public static void Init()
        {
            delay = config.TryGet<int>("delay", 100);
            clearDelay = config.TryGet<int>("clearDelay", 300000);

            thread = new Thread(Logic)
            {
                IsBackground = true,
                Name = "nTimer"
            };
            thread.Start();

            Timers.Start(clearDelay, () =>
            {
                lock (Timers.timers)
                {
                    List<Models.Timers> timers_ = new List<Models.Timers>(Timers.timers.Values);
                    foreach (Models.Timers t in timers_)
                    {
                        if (t.isFinished) Timers.timers.Remove(t.ID);
                    }
                }
            });
        }
        private static void Logic()
        {
            while (true)
            {
                try
                {
                    if (timers.Count < 1) continue;

                    List<Models.Timers> timers_ = new List<Models.Timers>(timers.Values);

                    foreach (Models.Timers timer in timers_)
                    {
                        timer.Elapsed();
                    }
                    Thread.Sleep(delay);

                }
                catch (Exception e)
                {
                    Log.Write($"Timers.Logic: {e.ToString()}", Nlogs.Type.Error);
                }
            }
        }

        public static Models.Timers Get(string id)
        {
            if (timers.ContainsKey(id))
                return timers[id];
            return null;
        }

        public static string Start(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new Models.Timers(action, id, interval));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string Start(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new Models.Timers(action, id, interval));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartOnce(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new Models.Timers(action, id, interval, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartOnce(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new Models.Timers(action, id, interval, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartTask(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new Models.Timers(action, id, interval, false, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new Models.Timers(action, id, interval, false, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartOnceTask(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new Models.Timers(action, id, interval, true, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static string StartOnceTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new Models.Timers(action, id, interval, true, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", Nlogs.Type.Error);
                return null;
            }
        }

        public static void Stop(string id)
        {
            if (id is null) throw new Exception("Trying to stop timer with NULL ID");
            if (timers.ContainsKey(id))
            {
                timers[id].isFinished = true;
                timers.Remove(id);
            }
        }

        public static void Stats()
        {
            string timers_ = "";
            foreach (Models.Timers t in timers.Values)
            {
                string state = (t.isFinished) ? "stopped" : "active";
                timers_ += $"{t.ID}:{state} ";
            }

            Log.Write(
                $"\nThread State = {thread.ThreadState.ToString()}" +
                $"\nTimers Count = {timers.Count}" +
                $"\nTimers = {timers_}" +
                $"\n");
        }
    }
}
