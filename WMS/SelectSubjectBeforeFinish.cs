using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
namespace WMS
{
    [Activity(Label = "SelectSubjectBeforeFinish", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SelectSubjectBeforeFinish : CustomBaseActivity
    {
        private int HeadID;
        private Spinner cbSubject;
        private Button btConfirm;
        List<ComboBoxItem> objectSubjects = new List<ComboBoxItem>();
        private int temporaryPositionReceive;
        public static async Task ShowIfNeeded(int headID)
        {
            try
            {
                if ((await CommonData.GetSettingAsync("WorkOrderFinishWithSubject") ?? "0") == "1")
                {
                    NameValueObjectList data;
                    try
                    {
                        string error;
                        data = Services.GetObjectList("hs", out error, headID.ToString());
                        if (data == null)
                        {
                            return;
                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }

                    if (data.Items.Count == 0) { return; }

                    var form = new SelectSubjectBeforeFinish();
                    form.SetHeadID(headID);
                    form.objectSubjects.Clear();
                    form.objectSubjects.Add(new ComboBoxItem { Text = "" });
                    data.Items.ForEach(i => form.objectSubjects.Add(new ComboBoxItem { Text = i.GetString("Subject") }));
                    form.cbSubject.SetSelection(1);
                    form.ShowDialog(1, null);

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                // Create your application here
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.SelectSubjectBeforeFinishTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.SelectSubjectBeforeFinish);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                cbSubject = FindViewById<Spinner>(Resource.Id.cbSubject);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                cbSubject.ItemSelected += CbSubject_ItemSelected;
                btConfirm.Click += BtConfirm_Click;



                var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                      Android.Resource.Layout.SimpleSpinnerItem, objectSubjects);

                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                cbSubject.Adapter = adapterWarehouse;

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
            {
                if (IsOnline())
                {

                    try
                    {
                        LoaderManifest.LoaderManifestLoopStop(this);
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                    }
                }
                else
                {
                    LoaderManifest.LoaderManifestLoop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                var subject = objectSubjects.ElementAt(temporaryPositionReceive).ToString();
                if (!string.IsNullOrEmpty(subject))
                {


                    try
                    {
                        NameValueObject data = new NameValueObject("SetHeadSubject");
                        data.SetInt("HeadID", HeadID);
                        data.SetString("Subject", subject);
                        string error;

                        var result = Services.SetObject("hs", data, out error);
                        if (result == null)
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s247)}" + error);
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();

                        }
                        else
                        {

                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                }
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
                    // Setting F2 to method ProccesStock()
                    case Keycode.F1:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
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

        private void CbSubject_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                Spinner spinner = (Spinner)sender;
                temporaryPositionReceive = e.Position;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }

        }


        public void SetHeadID(int headID)
        {
            try
            {
                HeadID = headID;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }


}
