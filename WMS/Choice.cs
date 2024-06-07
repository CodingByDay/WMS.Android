using Android.Views;
using WMS.App;

namespace WMS
{
    [Activity(Label = "choiceProduction")]
    public class Choice : CustomBaseActivity
    {
        private Button production;
        private Button rapid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.Choice);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // Create your application here
            production = FindViewById<Button>(Resource.Id.production);
            rapid = FindViewById<Button>(Resource.Id.rapid);
            production.Click += Production_Click;
            rapid.Click += Rapid_Click;
        }

        private void Rapid_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(TakeOver2Main));
            HelpfulMethods.clearTheStack(this);
        }

        private void Production_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(UnfinishedTakeoversView));
            HelpfulMethods.clearTheStack(this);
        }

        public override void OnBackPressed()
        {
            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // In smartphone.
                case Keycode.F2:
                    Production_Click(this, null);
                    break;
                // Return true;

                case Keycode.F3:
                    Rapid_Click(this, null);
                    break;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}