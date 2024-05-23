using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Sentry;

namespace WMS
{
    public class CustomBaseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

         /*
            // Set up a global exception handler
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                LogUnhandledException(args.Exception);
                args.Handled = true; // Mark exception as handled to prevent default crash behavior
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    LogUnhandledException(ex);
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                LogUnhandledException(args.Exception);
                args.SetObserved();
            };
         */
        }

     /*

        private void LogUnhandledException(Exception exception)
        {
           var test = SentryXamarin.

           string parting = "yes";
        }

    */
        public override void StartActivity(Intent intent)
        {
            base.StartActivity(intent);
            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);
        }

        public override void StartActivity(Intent intent, Bundle options)
        {
            base.StartActivity(intent, options);
            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);
        }
    }
}
