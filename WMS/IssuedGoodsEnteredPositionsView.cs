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
using Org.Apache.Http;
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
    [Activity(Label = "IssuedGoodsEnteredPositionsView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsEnteredPositionsView : CustomBaseActivity
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
        private Button btNext;
        private Button btUpdate;
        private Button btNew;
        private Button btFinish;
        private Button btDelete;
        private Button btLogout;
        private Button btnYes;
        private Button btnNo;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private string flow;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsEnteredPositionsView);
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
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            btNew = FindViewById<Button>(Resource.Id.btNew);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            btNext.Click += BtNext_Click;
            btUpdate.Click += BtUpdate_Click;
            btNew.Click += BtNew_Click;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            btLogout.Click += BtLogout_Click;

            InUseObjects.ClearExcept(new string[] { "MoveHead", "OpenOrder" });
            if (moveHead == null)
            {
                throw new ApplicationException("Data error");
            }
            LoadPositions();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            GetFlowValue();
        }

        private void GetFlowValue()
        {
            flow = moveHead.GetString("CurrentFlow");
            if (!String.IsNullOrEmpty(flow))
            {
                int result;
                var mode = Int32.TryParse(flow, out result);
                if (mode)
                {
                    Base.Store.modeIssuing = result;
                }
            } 
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
                    // return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
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
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s212)}" + result, ToastLength.Long).Show();
     
                            positions = null;
                            LoadPositions();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        return;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + result, ToastLength.Long).Show();
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

        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {


                if (moveHead != null)
                {
                    try
                    {
                        RunOnUiThread(() =>
                    {
                        progress = new ProgressDialogClass();

                        progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s262)}");
                    });



                        int? headID = moveHead.GetInt("HeadID");

                        if(headID == null)
                        {
                            return;
                        }

                        string result;
                        if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
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
                                        System.Threading.Thread.Sleep(500);
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
                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s218)}" + result);

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
                    catch (Exception err)
                    {
                        RunOnUiThread(() =>
                        {
                            Crashes.TrackError(err);
                            Toast.MakeText(this, err.Message, ToastLength.Short).Show();
                            StartActivity(typeof(MainMenu));
                        });
                      
                    }

                } 
            });
        }



        private void BtNew_Click(object sender, EventArgs e)
        {
     
            if (moveHead.GetBool("ByOrder") && flow == "2")
            {            
                StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                this.Finish();
            } else if (!moveHead.GetBool("ByOrder") && flow == "2") {
                StartActivity(typeof(IssuedGoodsIdentEntry));
                this.Finish();
            }
            else if(flow == "1")
            {
                StartActivity(typeof(IssuedGoodsIdentEntry));
                this.Finish();
            }
            else if (flow == "3")
            {
                StartActivity(typeof(ClientPicking));
                this.Finish();
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
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s229)}" + error, ToastLength.Long).Show();
                }
                else
                {

                    item.SetString("Ident", openIdent.GetString("Code"));
                    if(flow == "3")
                    {
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntryClientPicking));
                        Base.Store.isUpdate = true;
                        InUseObjects.Set("OpenIdent", openIdent);
                        StartActivity(i);
                        HelpfulMethods.clearTheStack(this);

                    } else
                    {
                        Base.Store.isUpdate = true;
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                        InUseObjects.Set("OpenIdent", openIdent);
                        StartActivity(i);
                        HelpfulMethods.clearTheStack(this);

                    } 
                    
                }
            }
            catch(Exception error)
            {
                Crashes.TrackError(error);
            }
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
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + error, ToastLength.Long).Show();

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
                lbInfo.Text = $"{Resources.GetString(Resource.String.s267)}";
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
                btNext.Enabled = false;
                btUpdate.Enabled = false;
                btDelete.Enabled = false;
            }
        }
    }
}