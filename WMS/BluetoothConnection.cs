using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using TrendNET.WMS.Device.Services;
using Android.Net;
using System.Net;
using Stream = Android.Media.Stream;
using Android.Util;
using Android.Content;
using Plugin.Settings.Abstractions;
using System.Linq;



// Crashes, analytics and automatic updating.


////////////////////////////////////
using Microsoft.AppCenter;//////////
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using static Android.App.ActionBar;
using Microsoft.AppCenter.Distribute;
using Uri = System.Uri;
using System.Threading.Tasks;
using AlertDialog = Android.App.AlertDialog;
using Aspose.Words.Tables;
using Android.Bluetooth;
using AndroidX.AppCompat.App;
using WMS.App;

using AndroidX.AppCompat.App;




namespace WMS
{
    [Activity(Label = "WMS", MainLauncher = false, Icon = "@drawable/barcode", NoHistory = true)]
    public class BluetoothConnection : AppCompatActivity
    {


        private EditText deviceName;
        private Button makeConnection;
        private BluetoothAdapter bluetoothAdapter;




        protected override void OnCreate(Bundle savedInstanceState)
        {      
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.BluetoothConnection);
            deviceName = FindViewById<EditText>(Resource.Id.tbDeviceName);
            makeConnection = FindViewById<Button>(Resource.Id.btShowStock);
            makeConnection.Click += MakeConnection_Click;
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter == null)
            {
                // Device doesn't support Bluetooth
                Toast.MakeText(this, "Bluetooth is not available on this device.", ToastLength.Long).Show();
                return;
            }
            if (!bluetoothAdapter.IsEnabled)
            {
                // Request to enable Bluetooth if not enabled
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, 1);
            }
        }

        private void MakeConnection_Click(object sender, EventArgs e)
        {
            // Start the BluetoothService with the selected device
            Intent serviceIntent = new Intent(this, typeof(BluetoothService));
            serviceIntent.PutExtra("SelectedDevice", deviceName.Text);
            StartService(serviceIntent);
        }

        private void Listener_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Settings));
            HelpfulMethods.clearTheStack(this);
        }

     
    }
}