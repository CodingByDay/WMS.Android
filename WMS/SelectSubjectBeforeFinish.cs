using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "SelectSubjectBeforeFinish", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SelectSubjectBeforeFinish : AppCompatActivity
    {
        private int HeadID;
        private Spinner cbSubject;
        private Button btConfirm;
        List<ComboBoxItem> objectSubjects = new List<ComboBoxItem>();
        private int temporaryPositionReceive;
        public static void ShowIfNeeded(int headID)
        {
            if ((CommonData.GetSetting("WorkOrderFinishWithSubject") ?? "0") == "1")
            {
                NameValueObjectList data;

          
              
                try
                {
                    string error;
                    data = Services.GetObjectList("hs", out error, headID.ToString());
                    if (data == null)
                    {
                        string errorWebApp = string.Format("Napaka pri pridobivanju možnih subjektov: " + error);
                      
                        

                        return;
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }

                if (data.Items.Count == 0) { return; }

                var form = new SelectSubjectBeforeFinish();
                form.SetHeadID(headID);
                form.objectSubjects.Clear();
                form.objectSubjects.Add(new ComboBoxItem { Text = ""});
                data.Items.ForEach(i => form.objectSubjects.Add(new ComboBoxItem {Text = i.GetString("Subject") }));
                form.cbSubject.SetSelection(1);
                form.ShowDialog(1, null);

            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectSubjectBeforeFinish);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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
            new IntentFilter(ConnectivityManager.ConnectivityAction));
        }
        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;

        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            if (IsOnline())
            {
                
                try
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
                catch (Exception err)
                {
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void BtConfirm_Click(object sender, EventArgs e)
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
                        string errorWebApp = string.Format("Napaka pri nastavljanje subjekta" + error);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                     
                    }
                    else
                    {
               
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }
            }
            

        }




        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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

        private void CbSubject_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
            {
                Spinner spinner = (Spinner)sender;



                temporaryPositionReceive = e.Position;
            }


        public void SetHeadID(int headID)
        {
            HeadID = headID;
        }
    }


    }
