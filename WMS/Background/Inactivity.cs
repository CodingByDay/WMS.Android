using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Scanner.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Scanner.Background
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
