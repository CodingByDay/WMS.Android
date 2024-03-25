using Android;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;

namespace WMS
{
    public class CustomBaseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

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
