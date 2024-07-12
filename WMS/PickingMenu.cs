using Android.Content.PM;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using WMS.ExceptionStore;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class PickingMenu : CustomBaseActivity
    {
        private Button ident;
        private Button order;
        private Button client;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private bool isMobile;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.PickingMenuTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.PickingMenu);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);

                isMobile = !App.Settings.tablet;
                ident = FindViewById<Button>(Resource.Id.ident);
                order = FindViewById<Button>(Resource.Id.order);
                client = FindViewById<Button>(Resource.Id.client);
                // Click events
                ident.Click += Ident_Click;
                order.Click += Order_Click;
                client.Click += Client_Click;
                // Flow events
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void Client_Click(object sender, EventArgs e)
        {
            try
            {
                Base.Store.modeIssuing = 3;
                StartActivity(typeof(IssuedGoodsBusinessEventSetupClientPicking));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Order_Click(object sender, EventArgs e)
        {
            try
            {
                Base.Store.modeIssuing = 2;
                StartActivity(typeof(IssuedGoodsBusinessEventSetup));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Ident_Click(object sender, EventArgs e)
        {
            try
            {
                Base.Store.modeIssuing = 1;
                StartActivity(typeof(IssuedGoodsBusinessEventSetup));
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
                    // In smartphone
                    case Keycode.F1:
                        Ident_Click(this, null);
                        break;
                    case Keycode.F2:
                        Order_Click(this, null);
                        break;
                    case Keycode.F3:
                        Client_Click(this, null);
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