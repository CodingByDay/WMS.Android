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
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "InventoryOpen", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryOpen : AppCompatActivity
    {
        private Spinner cbWarehouse;
        private EditText dtInventory;
        private Button btChoose;
        private EditText tbItems;
        private EditText tbConfirmedBy;
        private EditText tbConfirmationDate;
        private List<ComboBoxItem> warehousesObjectsAdapter = new List<ComboBoxItem>();
        private Button btOpen;
        private Button button2;
        private int temporaryPositionWarehouse;
        private NameValueObject moveHead = null;
        private string lastError = null;
        private DateTime dateX;
        private TextView warehouseLabel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.InventoryOpen);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            btChoose = FindViewById<Button>(Resource.Id.btChoose);
            tbItems = FindViewById<EditText>(Resource.Id.tbItems);
            tbConfirmedBy = FindViewById<EditText>(Resource.Id.tbConfirmedBy);
            tbConfirmationDate = FindViewById<EditText>(Resource.Id.tbConfirmationDate);
            dtInventory = FindViewById<EditText>(Resource.Id.dtInventory);
            btOpen = FindViewById<Button>(Resource.Id.btOpen);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += Button2_Click;
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
            btChoose.Click += BtChoose_Click;
            btOpen.Click += BtOpen_Click;
            dtInventory.Text = DateTime.Today.ToShortDateString();
            warehouseLabel = FindViewById<TextView>(Resource.Id.warehouseLabel);

            var warehouses = CommonData.ListWarehouses();
            if (warehouses != null)
            {
                warehouses.Items.ForEach(wh =>
                {
                    warehousesObjectsAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });
            }
            var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
             Android.Resource.Layout.SimpleSpinnerItem, warehousesObjectsAdapter);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapterWarehouse;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            dateX = DateTime.Today;
            UpdateFields();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
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
        private void UpdateFields()
        {
            var warehouse = warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                ClearFields();
                return;
            }
            else
            {
                var date = dateX;

       
                try
                {

                    string result;
                    if (WebApp.Get("mode=getConfirmedInventoryHead&wh=" + warehouse + "&date=" + date.ToString("s"), out result))
                    {
                        var moveHeadID = Convert.ToInt32(result);
                        if (moveHeadID < 0)
                        {
                            ClearFieldsError("Inventurni dokument za skladišče in datum ne obstaja!");
                            return;
                        }
                        else
                        {
                            moveHead = Services.GetObject("mh", moveHeadID.ToString(), out result);
                            if (moveHead == null)
                            {
                                ClearFieldsError("Inventurni dokument za skladišče in datum ne obstaja!");
                                return;
                            }
                            else
                            {
                                tbItems.Text = moveHead.GetInt("ItemCount").ToString();
                                tbConfirmedBy.Text = "???";
                                tbConfirmationDate.Text = "???";
                                btOpen.Enabled = true;
                                lastError = null;
                            }
                        }
                    }
                    else
                    {
                        ClearFieldsError("Napaka pri preverjanju inventure: " + result);
                        return;
                    }
                }
                catch(Exception ex) 
                {
                   Crashes.TrackError(ex);
                }
            }
        }

        private void ClearFields()
        {
            tbItems.Text = "";
            tbConfirmedBy.Text = "";
            tbConfirmationDate.Text = "";
            btOpen.Enabled = false;
        }
        private void ClearFieldsError(string err)
        {
            ClearFields();
            Toast.MakeText(this, err, ToastLength.Long).Show();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                
                case Keycode.F3:
                    if (btOpen.Enabled == true)
                    {
                        BtOpen_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    if (button2.Enabled == true)
                    {
                        StartActivity(typeof(MainMenu));
                        HelpfulMethods.clearTheStack(this);
                    }
                    break;
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtOpen_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastError))
            {
                Toast.MakeText(this, lastError, ToastLength.Long).Show();
         
                return;
            }

            var warehouse = warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, "Skladišče ni izbrano!", ToastLength.Long).Show();

                return;
            }

  
            try
            {
            

                var date = dateX;
                string error;
                if (!WebApp.Get("mode=canInsertInventory&wh=" + warehouse.ID.ToString(), out error))
                {
                    DialogHelper.ShowDialogError(this, this, "Napaka pri preverjanju zapisa inventure: " + error);
         
                    return;
                }
                if (error == "OK!")
                {
                    if (!WebApp.Get("mode=reopenInventory&id=" + moveHead.GetInt("HeadID").ToString(), out error))
                    {
                        DialogHelper.ShowDialogError(this, this, "Napaka pri odpiranju inventure: " + error);
                     
                        return;
                    }
                    else
                    {
                        Toast.MakeText(this, "Inventura odprta!", ToastLength.Long).Show();
              
                        StartActivity(typeof(InventoryConfirm));
                        HelpfulMethods.clearTheStack(this);
                    }
                }
                else
                {
                    DialogHelper.ShowDialogError(this, this, "Napaka pri preverjanju zapisa inventure: " + error);
                    return;
                }
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
                return;
            }
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            temporaryPositionWarehouse = e.Position;
            warehouseLabel.Text = "Skladišče: " + warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            UpdateFields();

        }



        private void BtChoose_Click(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                dtInventory.Text = time.ToShortDateString();
                dateX = time;
                UpdateFields();
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
    }
}