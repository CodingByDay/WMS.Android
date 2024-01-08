using Android.Content;
using Android.OS;
using System.Timers;
using WMS.App;
using Timer = System.Timers.Timer;

namespace WMS.Background
{
    [Service]
    public class Inactivity : Service
    {
        private static Timer timer = new Timer();

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Timer timer1 = new Timer
            {
                Interval = 64800000
            };
            timer1.Enabled = true;
            timer1.Elapsed += Timer1_Elapsed;
            return StartCommandResult.NotSticky;
        }

        private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            settings.restart = true;
        }
    }
}