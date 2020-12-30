using System;
using System.Threading.Tasks;

namespace iTeffa.Models
{
    public class Timers
    {
        public string ID { get; }
        public int MS { get; set; }
        public DateTime Next { get; private set; }
        public Action action { get; set; }
        public bool isOnce { get; set; }
        public bool isTask { get; set; }
        public bool isFinished { get; set; }
        public Timers(Action action_, string id_, int ms_, bool isonce_ = false, bool istask_ = false)
        {
            action = action_;
            ID = id_;
            MS = ms_;
            Next = DateTime.Now.AddMilliseconds(MS);
            isOnce = isonce_;
            isTask = istask_;
            isFinished = false;
        }
        public void Elapsed()
        {
            try
            {
                if (isFinished) return;

                if (Next <= DateTime.Now)
                {
                    if (isOnce) isFinished = true;
                    Next = DateTime.Now.AddMilliseconds(MS);
                    Settings.Timers.Log.Debug($"Timer.Elapsed.{ID}.Invoke");
                    if (isTask) Task.Run(() => action.Invoke());
                    else action.Invoke();
                    Settings.Timers.Log.Debug($"Timer.Elapsed.{ID}.Completed", Plugins.Logs.Type.Success);
                }
            }
            catch (Exception e)
            {
                Settings.Timers.Log.Write($"Timer.Elapsed.{ID}.Error: {e}", Plugins.Logs.Type.Error);
            }
        }
    }
}
