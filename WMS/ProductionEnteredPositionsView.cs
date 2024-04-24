using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using AlertDialog = Android.App.AlertDialog;

using AndroidX.AppCompat.App;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace WMS
{
    [Activity(Label = "ProductionEnteredPositionsView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProductionEnteredPositionsView : CustomBaseActivity
    {
        private TextView lbInfo;

        private EditText tbSSCC;
        private EditText tbSerialNumber;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;

        private Button btNext;
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
        private Button btnNo;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);

            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.ProductionEnteredPositionsViewTablet);
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.ProductionEnteredPositionsView);
            }

            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNumber = FindViewById<EditText>(Resource.Id.tbSerialNumber);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);

            btNext = FindViewById<Button>(Resource.Id.btNext);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            button4 = FindViewById<Button>(Resource.Id.button4);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            button5 = FindViewById<Button>(Resource.Id.button5);
            btNext.Click += BtNext_Click;
            btUpdate.Click += BtUpdate_Click;
            button4.Click += Button4_Click;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            button5.Click += Button5_Click;
            /////////////////////
            InUseObjects.ClearExcept(new string[] { "MoveHead" });
            if (moveHead == null)
            { 
                    Thread.Sleep(500);
                    throw new ApplicationException("Error, moveHead");
        
            }
            LoadPositions();
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



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // Setting F2 to method ProccesStock()
                case Keycode.F1:
                    if (btNext.Enabled == true)
                    {
                        BtNext_Click(this, null);
                    }
                    break;

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


                    //return true;



            }
            return base.OnKeyDown(keyCode, e);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDelete_Click(object sender, EventArgs e)
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
  
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{Resources.GetString(Resource.String.s212)}" + result);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                     
                        });



                        Dialog dialog = alert.Create();
                        dialog.Show();
                        positions = null;
                        LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                        }
                    }
                    else
                    {
                    string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
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


        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {
                var headID = moveHead.GetInt("HeadID");
                SelectSubjectBeforeFinish.ShowIfNeeded(headID);

                RunOnUiThread(() =>
                {
                    progress = new ProgressDialogClass();

                    progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s262)}");
                });
                

                try
                {

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
                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
                                    HelpfulMethods.clearTheStack(this);
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
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s266)}" + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
                                    HelpfulMethods.clearTheStack(this);

                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });



                        }
                    }
                    else
                    {

                        RunOnUiThread(() =>
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s218)}" + result);
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        });
                      

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
           StartActivity(typeof(ProductionSerialOrSSCCEntry));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            InUseObjects.Set("MoveItem", item);

            StartActivity(typeof(ProductionSerialOrSSCCEntry));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
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
                        string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                      
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
                lbInfo.Text = $"{Resources.GetString(Resource.String.s92)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

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
                tbCreatedBy.Text = item.GetString("ClerkName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;

                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
            else
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s267)}";

                tbSSCC.Text = "";
                tbSerialNumber.Text = "";
                tbQty.Text = "";
                tbLocation.Text = "";
                tbCreatedBy.Text = "";
                tbCreatedAt.Text = "";


                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;

                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                btNext.Enabled = false;
                btUpdate.Enabled = false;
                btDelete.Enabled = false;
            }
        }

    }
}