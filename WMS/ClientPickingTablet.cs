using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Java.Util.Concurrent;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;


namespace WMS
{
    [Activity(Label = "ClientPicking", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ClientPickingTablet : AppCompatActivity, IBarcodeResult
    {

        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ClientPickingAdapter adapter;
        private List<ClientPickingPosition> positions = new List<ClientPickingPosition>();
        private List<string> distinctClients = new List<string>();
        private NameValueObjectList data = new NameValueObjectList();
        private ListView ivTrail;
        private EditText tbClient;
        private EditText tbIdentFilter;
        private EditText tbLocationFilter;
        SoundPool soundPool;
        int soundPoolId;
        private MyOnItemLongClickListener listener;
        private ClientPickingPosition chosen;
        /*
        This object contains the information about the current flow of the issueing process
        it must have a value always
        String CurrentFlow possible values: 0, 1, 2, string.Empty.
        */
        private NameValueObject CurrentFlow = (NameValueObject)InUseObjects.Get("CurrentClientFlow");
        private Button btConfirm;
        private Button btDisplayPositions;
        private Button btBack;
        private Button btLogout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.ClientPickingTablet);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // Fields
            ivTrail = FindViewById<ListView>(Resource.Id.ivTrail);
            tbClient = FindViewById<EditText>(Resource.Id.tbClient);
            tbIdentFilter = FindViewById<EditText>(Resource.Id.tbIdentFilter);
            tbLocationFilter = FindViewById<EditText>(Resource.Id.tbLocationFilter);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            btDisplayPositions = FindViewById<Button>(Resource.Id.btDisplayPositions);
            btBack = FindViewById<Button>(Resource.Id.btBack);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            // Flow methods
            initializeView(Initialize());
            SetUpScanningFields();
            SetUpView();
            tbIdentFilter.AfterTextChanged += TbIdentFilter_AfterTextChanged;
            tbLocationFilter.AfterTextChanged += TbLocationFilter_AfterTextChanged;
            ivTrail.ItemClick += IvTrail_ItemClick;
            btConfirm.Click += BtConfirm_Click;
            btDisplayPositions.Click += BtDisplayPositions_Click;
            btBack.Click += BtBack_Click;
            btLogout.Click += BtLogout_Click;
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtBack_Click(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private void BtDisplayPositions_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (adapter.returnSelected() != null)
            {
                if (adapter.returnSelected().locationQty.Count == 0)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Ta izdelek ni mogoče izdati ker nima zalogo.", ToastLength.Long).Show();

                    });
                    return;
                }
                else
                {
                    InUseObjects.Set("OpenOrder", data.Items.ElementAt(adapter.returnSelected().originalIndex));
                }

            }
            else
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Izdelek ni izbran?.", ToastLength.Long);
                });

                return;
            }
            if (SaveMoveHead())
            {

                var obj = adapter.returnSelected();
                var ident = obj.Ident;
                var qty = obj.Quantity;
                Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                i.PutExtra("ident", ident);
                i.PutExtra("qty", qty);
                i.PutExtra("selected", ClientPickingPosition.Serialize(obj));
                StartActivity(i);
                this.Finish();

            }
        }

        private bool SaveMoveHead()
        {
            var obj = adapter.returnSelected();
            var ident = obj.Ident;
            var location = obj.Location;
            var qty = Convert.ToDouble(obj.Quantity);
            var extraData = new NameValueObject("ExtraData");
            extraData.SetString("Location", location);
            extraData.SetDouble("Qty", qty);
            InUseObjects.Set("ExtraData", extraData);
            string error;
            try
            {

                var openIdent = Services.GetObject("id", ident, out error);
                if (openIdent == null)
                {
                    string WebError = string.Format("Napaka pri preverjanju identa." + error);
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    return false;
                }
                InUseObjects.Set("OpenIdent", openIdent);
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);

            }

            if (!moveHead.GetBool("Saved"))
            {

                try
                {
                    var test = openOrder.GetString("No");
                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("CurrentFlow", CurrentFlow.GetString("CurrentFlow"));
                    moveHead.SetString("Type", "P");
                    moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                    moveHead.SetString("LinkNo", openOrder.GetString("No"));
                    moveHead.SetString("Document1", openOrder.GetString("Document1"));
                    moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                    moveHead.SetString("Note", openOrder.GetString("Note"));
                    string testDocument1 = openOrder.GetString("Document1");
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                    }
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije." + error);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        return true;
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }

        private void IvTrail_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            adapter.setSelected(e.Position);
            var order = data.Items.ElementAt(adapter.returnSelected().originalIndex);
            InUseObjects.Set("OpenOrder", order);
            openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
            chosen = adapter.returnSelected();
        }

        private NameValueObjectList Initialize()
        {
            NameValueObjectList oodtw = new NameValueObjectList();
            if (moveHead != null)
            {
                adapter = new ClientPickingAdapter(this, positions);
                ivTrail.Adapter = adapter;
                string error;
                oodtw = Services.GetObjectList("oodtw", out error, moveHead.GetString("DocumentType") + "|" + moveHead.GetString("Wharehouse") + "|" + "");
            }
            return oodtw;
        }


        private void SetUpScanningFields()
        {
            tbIdentFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocationFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void SetUpView()
        {
            if (moveHead != null)
            {
                tbClient.Text = moveHead.GetString("Receiver");
            }
        }

        private void initializeView(NameValueObjectList dataParameter)
        {
            if (moveHead != null)
            {
                int counter = 0;
                data.Items = dataParameter.Items.Where(x => x.GetString("Receiver") == moveHead.GetString("Receiver")).ToList();
                data.Items.ForEach(i =>
                {

                    var ident = i.GetString("Ident");
                    var location = i.GetString("Location");
                    var name = i.GetString("Name");
                    var no = i.GetString("Key");
                    var lvi = new ClientPickingPosition();
                    lvi.Order = i.GetString("Key");
                    lvi.Ident = ident;
                    lvi.Location = location;
                    lvi.Quantity = i.GetDouble("OpenQty").ToString("###,##0.00");
                    lvi.No = i.GetInt("No");
                    lvi.originalIndex = counter;
                    counter += 1;
                    positions.Add(lvi);
                });

                var unique = positions.Select(o => o.Ident).Distinct();
                string joined = string.Join(",", unique);
                string error;
                var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + joined);

                foreach (var item in stock.Items)
                {
                    foreach (var singular in positions.Where(x => x.Ident == item.GetString("Ident")))
                    {
                        singular.locationQty.Add(item.GetString("Location"), item.GetDouble("RealStock"));
                    }
                }

                foreach (var change in positions)
                {
                    if (change.locationQty.Count == 0)
                    {
                        change.Location = "Ni zaloge";
                        change.Quantity = string.Empty;
                    }
                    else if (change.locationQty.Count == 1)
                    {
                        change.Location = change.locationQty.ElementAt(0).Key;

                    }
                    else if (change.locationQty.Count > 2)
                    {
                        change.Location = "Na več";
                        change.Quantity = string.Empty;
                    }
                }
                adapter.NotifyDataSetChanged();
                adapter.Filter(positions, true, string.Empty, false);
                listener = new MyOnItemLongClickListener(this, adapter.returnData(), adapter);
                ivTrail.OnItemLongClickListener = listener;

            }
        }
        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        private void TbLocationFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            adapter.Filter(positions, false, tbLocationFilter.Text, false);
            listener.updateData(adapter.returnData());
        }

        private void TbIdentFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            adapter.Filter(positions, true, tbIdentFilter.Text, true);
            listener.updateData(adapter.returnData());
        }
        public void GetBarcode(string barcode)
        {
            if (barcode != "Scan fail" && barcode != "")
            {
                if (tbIdentFilter.HasFocus)
                {
                    Sound();
                    tbIdentFilter.Text = barcode;
                    adapter.Filter(positions, true, tbIdentFilter.Text, true);
                    if (adapter.returnNumberOfItems() == 0)
                    {
                        tbIdentFilter.Text = string.Empty;
                    }
                }
                else if (tbLocationFilter.HasFocus)
                {
                    Sound();
                    tbLocationFilter.Text = barcode;
                    adapter.Filter(positions, false, tbLocationFilter.Text, false);
                    if (adapter.returnNumberOfItems() == 0)
                    {
                        tbIdentFilter.Text = string.Empty;
                    }
                }
            }
            listener.updateData(adapter.returnData());
        }


        // Class for handling long click 
        public class MyOnItemLongClickListener : Java.Lang.Object, AdapterView.IOnItemLongClickListener
        {
            public Context context_;
            public List<ClientPickingPosition> data_;
            public ClientPickingAdapter adapter_;


            public void updateData(List<ClientPickingPosition> data)
            {
                data_ = data;
            }

            public MyOnItemLongClickListener(Context context, List<ClientPickingPosition> data, ClientPickingAdapter adapter)
            {
                context_ = context;
                data_ = data;
                adapter_ = adapter;
            }

            public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
            {
                adapter_.setSelected(position);
                ClientPickingPosition selected = data_.ElementAt(position);
                AlertDialog.Builder builder = new AlertDialog.Builder(context_);
                builder.SetTitle("Podrobnosti");
                builder.SetMessage($"Ident: {selected.Ident}\nLokacija: {selected.Location}\nNaročilo: {selected.Order}\nNaziv: {selected.Name}");
                builder.SetPositiveButton("OK", (s, args) =>
                {
                });
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();
                return true; // Return true to consume the long click event
            }
        }
    }
}