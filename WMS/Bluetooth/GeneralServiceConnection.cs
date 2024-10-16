// MyServiceConnection.cs
using Android.Content;
using Android.OS;
using WMS;

public class GeneralServiceConnection : Java.Lang.Object, IServiceConnection
{
    private IssuedGoodsIdentEntryWithTrail ActivityIssuedGoodsTrail;
    private Settings ActivitySettings;
    private IssuedGoodsSerialOrSSCCEntry ActivityIssuedGoodsSerialOrSSCCEntry;
    private MainMenu ActivityMainMenu;
    private TakeOverIdentEntry ActivityTakeoverIdentEntry;
    private TakeOverSerialOrSSCCEntry ActivityTakeOverSerialOrSSCCEntry;

    public GeneralServiceConnection(IssuedGoodsIdentEntryWithTrail activity)
    {
        this.ActivityIssuedGoodsTrail = activity;
    }

    public GeneralServiceConnection(Settings activity)
    {
        this.ActivitySettings = activity;
    }

    public GeneralServiceConnection(IssuedGoodsSerialOrSSCCEntry activity)
    {
        this.ActivityIssuedGoodsSerialOrSSCCEntry = activity;
    }
    public GeneralServiceConnection(MainMenu activity)
    {
        this.ActivityMainMenu = activity;
    }

    public GeneralServiceConnection(TakeOverIdentEntry activity)
    {
        this.ActivityTakeoverIdentEntry = activity;
    }

    public GeneralServiceConnection(TakeOverSerialOrSSCCEntry activity)
    {
        this.ActivityTakeOverSerialOrSSCCEntry = activity;
    }

    public void OnServiceConnected(ComponentName name, IBinder service)
    {
        BluetoothService.MyBinder binder = (BluetoothService.MyBinder)service;

        if (ActivityIssuedGoodsTrail != null)
        {
            ActivityIssuedGoodsTrail.OnServiceBindingComplete(binder.GetService());
            ActivityIssuedGoodsTrail.binder = binder;
            ActivityIssuedGoodsTrail.isBound = true;
        }
        else if (ActivitySettings != null)
        {
            ActivitySettings.OnServiceBindingComplete(binder.GetService());
            ActivitySettings.binder = binder;
            ActivitySettings.isBound = true;
        } else if(ActivityIssuedGoodsSerialOrSSCCEntry != null)
        {
            ActivityIssuedGoodsSerialOrSSCCEntry.OnServiceBindingComplete(binder.GetService());
            ActivityIssuedGoodsSerialOrSSCCEntry.binder = binder;
            ActivityIssuedGoodsSerialOrSSCCEntry.isBound = true;
        } else if(ActivityMainMenu != null)
        {
            ActivityMainMenu.OnServiceBindingComplete(binder.GetService());
            ActivityMainMenu.binder = binder;
            ActivityMainMenu.isBound = true;
        } else if (ActivityTakeoverIdentEntry != null)
        {
            ActivityTakeoverIdentEntry.OnServiceBindingComplete(binder.GetService());
            ActivityTakeoverIdentEntry.binder = binder;
            ActivityTakeoverIdentEntry.isBound = true;
        } else if (ActivityTakeOverSerialOrSSCCEntry != null)
        {
            ActivityTakeOverSerialOrSSCCEntry.OnServiceBindingComplete(binder.GetService());
            ActivityTakeOverSerialOrSSCCEntry.binder = binder;
            ActivityTakeOverSerialOrSSCCEntry.isBound = true;
        }
    }

    public void OnServiceDisconnected(ComponentName name)
    {
        ActivityIssuedGoodsTrail.isBound = false;
    }
}