using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Java.Util;
using Handler = Android.OS.Handler;

[Service]
public class BluetoothService : Service
{
    private BluetoothAdapter bluetoothAdapter;
    private BluetoothDevice selectedDevice;
    private BluetoothSocket bluetoothSocket;
    private Stream inputStream;
    private Stream outputStream;
    private BluetoothService service;
    BluetoothDevice targetDevice;
    BluetoothSocket socket;
    public static bool running = false;

    private MyBinder binder;
    private Handler handler;

    public override IBinder OnBind(Intent intent)
    {
        binder = new MyBinder(this);
        return binder;
    }

    public class MyBinder : Binder
    {
        private BluetoothService service;

        public MyBinder(BluetoothService service)
        {
            this.service = service;
        }

        public BluetoothService GetService()
        {
            return service;
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();
        // Initialize BluetoothAdapter and other necessary variables here
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        handler = new Handler();
        ConnectToDevice();
        // Return Sticky to keep the service running until explicitly stopped
        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Cleanup and release resources here
        Disconnect();
    }
    private void ShowToast(string message)
    {
        Toast.MakeText(this, message, ToastLength.Short).Show();
    }
    void ConnectToDevice()
    {
        if (!running)
        {
            try
            {
                BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    return;

                if (!adapter.IsEnabled)
                    return;

                BluetoothDevice device = (from bd in adapter.BondedDevices
                                          where bd.Name == "AMS"
                                          select bd).FirstOrDefault();

                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("f8edf739-676c-464e-9337-0d46feaa61d6"));
                socket.Connect();
                ShowToast("Connection successful");
                running = true;
                // Now you have a connected socket for communication
            }
            catch (Exception)
            {
                ShowToast("Failed connection");
                return;
                // Handle connection errors
            }
        }
    }

    public void SendObject(String data)
    {
        if (socket != null)
        {
            // Get the output stream from the socket
            System.IO.Stream outputStream = socket.OutputStream;
            try
            {
                // Convert your data to bytes (replace "Hello, Bluetooth!" with your actual data)
                byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes(data);
                // Write the data to the output stream
                outputStream.Write(dataToSend, 0, dataToSend.Length);
                // Flush the output stream to ensure the data is sent immediately
                outputStream.Flush();
                // Data sent successfully
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }
        }
    }

    private void Disconnect()
    {
        running = false;
        try
        {
            if (outputStream != null)
                outputStream.Close();

            if (inputStream != null)
                inputStream.Close();

            if (bluetoothSocket != null)
                bluetoothSocket.Close();
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            Toast.MakeText(this, "Failed to disconnect: " + ex.Message, ToastLength.Long).Show();
        }
        finally
        {
            inputStream = null;
            outputStream = null;
            bluetoothSocket = null;
        }
    }
}