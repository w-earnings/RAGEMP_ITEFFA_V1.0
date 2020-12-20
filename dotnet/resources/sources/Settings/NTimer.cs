using System;
using System.Threading.Tasks;

namespace iTeffa.Settings
{
    public class NTimer
    {
        public string ID { get; }
        public int MS { get; set; }
        public DateTime Next { get; private set; }

        public Action action { get; set; }

        public bool isOnce { get; set; }
        public bool isTask { get; set; }
        public bool isFinished { get; set; }

        public NTimer(Action action_, string id_, int ms_, bool isonce_ = false, bool istask_ = false)
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

                    Timers.Log.Debug($"Timer.Elapsed.{ID}.Invoke");

                    if (isTask) Task.Run(() => action.Invoke());
                    else action.Invoke();

                    Timers.Log.Debug($"Timer.Elapsed.{ID}.Completed", nLog.Type.Success);
                }

            }
            catch (Exception e)
            {
                Timers.Log.Write($"Timer.Elapsed.{ID}.Error: {e}", nLog.Type.Error);
            }
        }

    }
}
