using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics.Drawables;
using Android.Graphics;
namespace WMS
{
    [Activity(Label = "TakeOverEnteredPositionsViewTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class TakeOverEnteredPositionsViewTablet : AppCompatActivity
    {
        private TextView lbInfo;

        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNumber;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private ListView dataList;
        private Button btUpdate;
        private Button button4;
        private Button btFinish;
        private Button btDelete;
        private Button button5;
        private int displayedPosition = 0;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObjectList positions = null;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btNext;
        private Button btnNo;
        private List<TakeOverEnteredPositionsViewListItems> data = new List<TakeOverEnteredPositionsViewListItems>();
        private string tempUnit;
        private int selected;
        private int selectedItem = -1;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverEnteredPositionsViewTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNumber = FindViewById<EditText>(Resource.Id.tbSerialNumber);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            dataList = FindViewById<ListView>(Resource.Id.dataList);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            button4 = FindViewById<Button>(Resource.Id.button4);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            button5 = FindViewById<Button>(Resource.Id.button5);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btNext.Click += BtNext_Click;
            dataList.ItemLongClick += DataList_ItemLongClick;
            TakeOverEnteredPositionsViewAdapter adapter = new TakeOverEnteredPositionsViewAdapter(this, data);
            dataList.Adapter = adapter;
            btUpdate.Click += BtUpdate_Click;
            button4.Click += Button4_Click;

            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            button5.Click += Button5_Click;
            dataList.ItemClick += DataList_ItemClick;
            InUseObjects.ClearExcept(new string[] { "MoveHead" });
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }

            LoadPositions();
            fillList();
            dataList.PerformItemClick(dataList, 0, 0);
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



        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));


            // Access Popup layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += (e, ev) => { Yes(index); };
            btnNo.Click += (e, ev) => { No(index); };
        }




        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }


        private void Yes(int index)
        {
            var item = positions.Items[index];
            var id = item.GetInt("HeadID");


            try
            {

                string result;
                if (WebApp.Get("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), out result))
                {
                    if (result == "OK!")
                    {
                        positions = null;
                        LoadPositions();
                        data.Clear();
                        fillList();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string errorWebAppIssued = string.Format("Napaka pri brisanju pozicije " + result);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        positions = null;
                        LoadPositions();

                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string errorWebAppIssued = string.Format("Napaka pri dostopu web aplikacije: " + result);
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();

                    return;
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

            string errorWebApp = string.Format("Pozicija uspešno zbrisana.");
            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
        }
        private void DataList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {

            var index = e.Position;
            DeleteFromTouch(index);



        }

        private void DataList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
            dataList.RequestFocusFromTouch();
            dataList.SetItemChecked(selected, true);
            dataList.SetSelection(selected);
        }
        private void Select(int postionOfTheItemInTheList)
        {
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void fillList()
        {

            for (int i = 0; i < positions.Items.Count; i++)
            {
                if (i < positions.Items.Count && positions.Items.Count > 0)
                {
                    var item = positions.Items.ElementAt(i);
                    var created = item.GetDateTime("DateInserted");
                    var numbering = i + 1;
                    bool setting;

                    if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                    {
                        setting = false;
                    }
                    else
                    {
                        setting = true;
                    }
                    if (setting)
                    {
                        tempUnit = item.GetDouble("Qty").ToString();

                    }
                    else
                    {
                        tempUnit = item.GetDouble("Factor").ToString();
                    }
                    string error;
                    var ident = item.GetString("Ident").Trim();
                    var openIdent = Services.GetObject("id", ident, out error);

                    var identName = openIdent.GetString("Name");
                    var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                    var friendlySSCC = item.GetString("SSCC");
                    
                    data.Add(new TakeOverEnteredPositionsViewListItems
                    {
                        Ident = item.GetString("Ident"),
                        SerialNumber = item.GetString("SerialNo"),
                        SSCC = HelpfulMethods.lastReturn(item.GetString("SSCC"), 5),              
                        Quantity = tempUnit,
                        Position = numbering.ToString(),
                        Name = identName.Trim(),


                    });
                    ;
                }
                else
                {
                    string errorWebApp = string.Format("Kritična napaka...");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }

            }
        }
        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += BtnYes_Click;
            btnNo.Click += BtnNo_Click;
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {

            {
                var item = positions.Items[displayedPosition];
                var id = item.GetInt("ItemID");


                try
                {

                    string result;
                    if (WebApp.Get("mode=delMoveItem&item=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), out result))
                    {
                        if (result == "OK!")
                        {
                            positions = null;
                            LoadPositions();
                            data.Clear();
                            fillList();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        }
                        else
                        {
                            Toast.MakeText(this, "Napaka pri brisanju pozicije: " + result, ToastLength.Long).Show();
                            positions = null;
                            LoadPositions();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                            return;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "Napaka pri dostopu do web aplikacije: " + result, ToastLength.Long).Show();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                finally
                {
                    popupDialog.Dismiss();
                    popupDialog.Hide();
                }

            }

        }



        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {
                RunOnUiThread(() =>
                {
                    progress = new ProgressDialogClass();

                    progress.ShowDialogSync(this, "Zaključujem");
                });



                try
                {

                    var headID = moveHead.GetInt("HeadID");

                    string result;
                    if (WebApp.Get("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                    {
                        if (result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();
                                var id = result.Split('+')[1];


                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle("Uspešno zaključevanje");
                                alert.SetMessage("Zaključevanje uspešno! Št. prevzema:\r\n" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenuTablet));
                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });
                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle("Napaka");
                                alert.SetMessage("Napaka pri zaključevanju: " + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenuTablet));

                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "Napaka pri klicu web aplikacije:  " + result, ToastLength.Long).Show();
                    }
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        progress.StopDialogSync();
                    });
                }
            });

        }
        private async void BtFinish_Click(object sender, EventArgs e)
        {
            popupDialogConfirm = new Dialog(this);
            popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
            popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogConfirm.Show();

            popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialogConfirm.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

            // Access Popup layout fields like below
            btnYesConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnYes);
            btnNoConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnNo);
            btnYesConfirm.Click += BtnYesConfirm_Click;
            btnNoConfirm.Click += BtnNoConfirm_Click;
        }

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            await FinishMethod();
        }

        private void Button4_Click(object sender, EventArgs e)
        {

            if (CommonData.GetSetting("UseDirectTakeOver") == "1")
            {
                InUseObjects.Set("MoveHead", moveHead);
                InUseObjects.Set("MoveItem", null);
                StartActivity(typeof(TakeOver2MainTablet));
                HelpfulMethods.clearTheStack(this);

                return;
            }
            else

                StartActivity(typeof(TakeOverIdentEntryTablet));
                HelpfulMethods.clearTheStack(this);

        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {

            var item = positions.Items[displayedPosition];
            InUseObjects.Set("MoveItem", item);

            if (CommonData.GetSetting("UseDirectTakeOver") == "1")
            {
                InUseObjects.Set("MoveHead", moveHead);
                StartActivity(typeof(TakeOver2Main));
                HelpfulMethods.clearTheStack(this);

                return;
            }


            try
            {
         

                string error;
                var openIdent = Services.GetObject("id", item.GetString("Ident"), out error);
                if (openIdent == null)
                {

                    Toast.MakeText(this, "Napaka pri preverjanju identa.  " + error, ToastLength.Long).Show();

                }
                else
                {
                    item.SetString("Ident", openIdent.GetString("Code"));
                    InUseObjects.Set("OpenIdent", openIdent);
                    StartActivity(typeof(TakeOverSerialOrSSCCEntryTablet));
                    HelpfulMethods.clearTheStack(this);

                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            selectedItem++;

            if (selectedItem <= (positions.Items.Count - 1))
            {
                dataList.RequestFocusFromTouch();
                dataList.SetSelection(selectedItem);
                dataList.SetItemChecked(selectedItem, true);
            }
            else
            {
                selectedItem = 0;
                dataList.RequestFocusFromTouch();
                dataList.SetSelection(selectedItem);
                dataList.SetItemChecked(selectedItem, true);
            }

            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private void LoadPositions()
        {

            try
            {

                if (positions == null)
                {
                    var error = "";

                    if (positions == null)
                    {
                        positions = Services.GetObjectList("mi", out error, moveHead.GetInt("HeadID").ToString());
                        InUseObjects.Set("TakeOverEnteredPositions", positions);
                    }
                    if (positions == null)
                    {
                        Toast.MakeText(this, "Napaka pri dostopu do web aplikacije: " + error, ToastLength.Long).Show();

                        return;
                    }
                }

                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
           

                case Keycode.F2:
                    if (btUpdate.Enabled == true)
                    {
                        BtUpdate_Click(this, null);
                    }
                    break;

                case Keycode.F3://
                    if (button4.Enabled == true)
                    {
                        Button4_Click(this, null);
                    }
                    break;

                case Keycode.F4:
                    if (btFinish.Enabled == true)
                    {
                        BtFinish_Click(this, null);
                    }
                    break;

                case Keycode.F5:
                    if (btDelete.Enabled == true)
                    {
                        BtDelete_Click(this, null);
                    }
                    break;

                case Keycode.F6:
                    if (button5.Enabled == true)
                    {
                        Button5_Click(this, null);
                    }
                    break;


                    // return true;



            }
            return base.OnKeyDown(keyCode, e);
        }

        private void FillDisplayedItem()
        {
            if ((positions != null) && (displayedPosition < positions.Items.Count))
            {
                var item = positions.Items[displayedPosition];
                lbInfo.Text = "Vnešene pozicije na naročilu (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

                tbIdent.Text = item.GetString("IdentName");
                tbSSCC.Text = item.GetString("SSCC");
                tbSerialNumber.Text = item.GetString("SerialNo");
                if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                {
                    tbQty.Text = item.GetDouble("Factor").ToString() + " x " + item.GetDouble("Packing").ToString();
                }
                else
                {
                    tbQty.Text = item.GetDouble("Qty").ToString();
                }
                tbLocation.Text = item.GetString("LocationName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedBy.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.") + " " + item.GetString("ClerkName");



                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;


                tbIdent.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);


                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
            else
            {
                lbInfo.Text = "Vnešene pozicije na naročilu (ni)";

                tbIdent.Text = "";
                tbSSCC.Text = "";
                tbSerialNumber.Text = "";
                tbQty.Text = "";
                tbLocation.Text = "";
                tbCreatedBy.Text = "";


                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;


                tbIdent.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);

                btUpdate.Enabled = false;
                btDelete.Enabled = false;


            }




          
        }
    }
}