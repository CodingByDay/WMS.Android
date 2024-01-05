using System;
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
using Scanner.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "InventoryOpenDocument", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryOpenDocument : Activity
    {
        private Spinner cbWarehouse;
        private EditText dtDate;
        private Button select;
        private Button confirm;
        private Button logout;   
        private List<ComboBoxItem> warehousesAdapter = new List<ComboBoxItem>();
        private int temporaryPositionWarehouse;
        private DateTime datex;
      
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InventoryOpenDocument);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            dtDate = FindViewById<EditText>(Resource.Id.dtDate);
            select = FindViewById<Button>(Resource.Id.select);
            confirm = FindViewById<Button>(Resource.Id.confirm);
            logout = FindViewById<Button>(Resource.Id.logout);
            select.Click += Select_Click;
            confirm.Click += Confirm_Click;
            logout.Click += Logout_Click;                     
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;              
            dtDate.Text = DateTime.Now.ToShortDateString();
            var warehouses = CommonData.ListWarehouses();
            warehouses.Items.ForEach(wh =>
            {
                warehousesAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            var adapter = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
             Android.Resource.Layout.SimpleSpinnerItem, warehousesAdapter);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapter;
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

        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }
        private void Confirm_Click(object sender, EventArgs e)
        {
            var warehouse = warehousesAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, "Skladišče ni izbrano!", ToastLength.Long).Show();
                return;
            }
            try
            {
                var date = datex;
                var moveHead = new NameValueObject("MoveHead");
                moveHead.SetString("Wharehouse", warehouse.ID.ToString());
                moveHead.SetDateTime("Date", date);
                moveHead.SetString("Type", "N");
                moveHead.SetString("LinkKey", "");
                moveHead.SetInt("LinkNo", 0);
                moveHead.SetInt("Clerk", Services.UserID());

                string error;
                if (!WebApp.Get("mode=canInsertInventory&wh=" + warehouse.ID.ToString(), out error))
                {
                    Toast.MakeText(this, "Napaka pri preverjanju zapisa inventure: " + error, ToastLength.Long).Show();
                    return;
                }
                if (error == "OK!")
                {
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        Toast.MakeText(this, "Napaka pri zapisu inventure: " + error, ToastLength.Long).Show();
                        return;
                    }
                    Toast.MakeText(this, "Dokument inventure shranjen!", ToastLength.Long).Show();
                    StartActivity(typeof(InventoryMenu));
                }
                else
                {
                    Toast.MakeText(this, "Napaka pri preverjanju zapisa inventure: " + error, ToastLength.Long).Show();
                    return;
                }
            }
            catch(Exception error)
            {
                Crashes.TrackError(error);
                return;
            }
        }
        private void Select_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            DatePickerDialog dialog = new DatePickerDialog(this, (sender, args) =>
            {
                DateTime selectedDate = args.Date;
                if (selectedDate >= today)
                {
                    dtDate.Text = selectedDate.ToShortDateString();
                    datex = selectedDate;
                }
                else
                {
                    Toast.MakeText(this, "Prosimo izberite pravilen datum.", ToastLength.Short).Show();
                }
            }, today.Year, today.Month - 1, today.Day);
            DatePicker datePicker = dialog.DatePicker;
            DateTime tomorrow = today.AddDays(0);
            long minDate = (long)(tomorrow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            datePicker.MinDate = minDate;
            dialog.Show();
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            temporaryPositionWarehouse = e.Position;
        }
    }
}