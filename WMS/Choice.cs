using Android.Views;
using WMS.App;
using WMS.ExceptionStore;

namespace WMS
{
    [Activity(Label = "choiceProduction")]
    public class Choice : CustomBaseActivity
    {
        private Button production;
        private Button rapid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Rapid_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(TakeOver2Main));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Production_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(UnfinishedTakeoversView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
    }
}