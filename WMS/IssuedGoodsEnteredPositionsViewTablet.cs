
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

namespace WMS
{
    [Activity(Label = "IssuedGoodsEnteredPositionsViewTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class IssuedGoodsEnteredPositionsViewTablet : Activity
    {
        private int displayedPosition = 0;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObjectList positions = null;
        private TextView lbInfo;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNumber;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private Dialog popupDialog;
        private Button btUpdate;
        private Button btNew;
        private Button btFinish;
        private Button btDelete;
        private Button btLogout;
        private Button btnYes;
        private Button btnNo;
        private Button btNext;
        private ListView listData;
        private List<IssuedEnteredPositionViewList> data = new List<IssuedEnteredPositionViewList>();
        private string tempUnit;
        private int selected;
        private int selectedItem=-1;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private ProgressDialogClass progress;
        private IssuedEnterAdapter adapterX;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetContentView(Resource.Layout.IssuedGoodsEnteredPositionsViewTablet);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNumber = FindViewById<EditText>(Resource.Id.tbSerialNumber);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            btNew = FindViewById<Button>(Resource.Id.btNew);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btNext.Click += BtNext_Click;
            listData = FindViewById<ListView>(Resource.Id.listData);
            listData.ItemLongClick += ListData_ItemLongClick;
            adapterX = new IssuedEnterAdapter(this, data);
            listData.Adapter = adapterX;
            btUpdate.Click += BtUpdate_Click;
            btNew.Click += BtNew_Click;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            btLogout.Click += BtLogout_Click;
            listData.ItemClick += ListData_ItemClick;

            LoaderManifest.LoaderManifestLoopResources(this);
      
            // app 

            InUseObjects.ClearExcept(new string[] { "MoveHead", "OpenOrder" });
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }

            LoadPositions();

            fillList();

            listData.PerformItemClick(listData, 0, 0);
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
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);

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
                        adapterX.NotifyDataSetChanged();
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


        private void ListData_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            DeleteFromTouch(index);
        }

        private void Select(int postionOfTheItemInTheList)
        {
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }
        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;



            listData.RequestFocusFromTouch();
            listData.SetItemChecked(selected, true);
            listData.SetSelection(selected);
        }

        private async Task fillList()
        {
            await Task.Run(() =>
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
                        //var ident = CommonData.LoadIdent(item.GetString("Ident"));
                        var identName = openIdent.GetString("Name");
                        var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                        RunOnUiThread(() =>
                        {
                            data.Add(new IssuedEnteredPositionViewList
                                {
                                    Ident = item.GetString("Ident").Trim(),
                                    SerialNumber = item.GetString("SerialNo"),
                                    SSCC = item.GetString("SSCC"),
                                    Quantity = tempUnit,
                                    Position = numbering.ToString(),
                                    Name = identName,
                                });
                      
                            adapterX.NotifyDataSetChanged();
                        });                                       
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            string errorWebApp = string.Format("Kritična napaka...");
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        });                     
                    }
                }
            });
            RunOnUiThread(() =>
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            });
        }


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // 
           

                case Keycode.F2:
                    if (btUpdate.Enabled == true)
                    {
                        BtUpdate_Click(this, null);
                    }
                    break;

                case Keycode.F3://
                    if (btNew.Enabled == true)
                    {
                        BtNew_Click(this, null);
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
                    if (btLogout.Enabled == true)
                    {
                        BtLogout_Click(this, null);
                    }
                    break;
                    //return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }
        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloGreenDark);
            // Access Popup layout fields like below
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
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

        }

        private void BtFinish_Click(object sender, EventArgs e)
        {

            popupDialogConfirm = new Dialog(this);
            popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
            popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogConfirm.Show();

            popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialogConfirm.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloRedLight);

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

        private void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            try
            {
             
                    progress = new ProgressDialogClass();
                    progress.ShowDialogSync(this, "Zaključujem več paleta na enkrat.");
            
                var headID = moveHead.GetInt("HeadID");

                string result;
                if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                {
                    if (result.StartsWith("OK!"))
                    {
                        progress.StopDialogSync();
                        var id = result.Split('+')[1];
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Zaključevanje uspešno");
                        alert.SetMessage("Zaključevanje uspešno! Št.izdaje:\r\n" + id);
                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            System.Threading.Thread.Sleep(500);

                            StartActivity(typeof(MainMenuTablet));
                        });
                        Dialog dialog = alert.Create();
                        dialog.Show();


                    }
                    else
                    {
                        progress.StopDialogSync();
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Napaka");
                        alert.SetMessage("Napaka pri zaključevanju: " + result);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                            System.Threading.Thread.Sleep(500);


                        });

                        Dialog dialog = alert.Create();
                        dialog.Show();

                    }
                }
                else
                {
                    Toast.MakeText(this, "Napaka pri klicu web aplikacije: " + result, ToastLength.Long).Show();

                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

        }

        private void BtNew_Click(object sender, EventArgs e)
        {
            if (moveHead.GetBool("ByOrder") && CommonData.GetSetting("UseSingleOrderIssueing") == "1")
            {
                StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
                HelpfulMethods.clearTheStack(this);
            }
            else
            {
                StartActivity(typeof(IssuedGoodsIdentEntryTablet));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            InUseObjects.Set("MoveItem", item);


            try
            {


                string error;
                var openIdent = Services.GetObject("id", item.GetString("Ident"), out error);
                if (openIdent == null)
                {
                    Toast.MakeText(this, "Napaka pri preverjanju ident-a: " + error, ToastLength.Long).Show();

                }
                else
                {
                    item.SetString("Ident", openIdent.GetString("Code"));
                    InUseObjects.Set("OpenIdent", openIdent);
                    StartActivity(typeof(IssuedGoodsSerialOrSSCCEntryTablet));
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
                listData.RequestFocusFromTouch();
                listData.SetSelection(selectedItem);
                listData.SetItemChecked(selectedItem, true);
            }
            else
            {
                selectedItem = 0;
                listData.RequestFocusFromTouch();
                listData.SetSelection(selectedItem);
                listData.SetItemChecked(selectedItem, true);
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

        private void FillDisplayedItem()
        {
            if ((positions != null) && (displayedPosition < positions.Items.Count))
            {
                var item = positions.Items[displayedPosition];
                lbInfo.Text = "Vnešene pozicije na odpremi (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

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
                tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
            else
            {
                lbInfo.Text = "Vnešene pozicije na odpremi (ni)";

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
                tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);


            
                btUpdate.Enabled = false;
                btDelete.Enabled = false;
            }
        }
    }
}