// MyServiceConnection.cs
using Android.Content;
using Android.OS;
using WMS;

public class IssuedGoodsServiceConnection : Java.Lang.Object, IServiceConnection
{
    private IssuedGoodsIdentEntryWithTrail activityIssuedGoods;
    private Settings activitySettings;

    public IssuedGoodsServiceConnection(IssuedGoodsIdentEntryWithTrail activity)
    {
        this.activityIssuedGoods = activity;
    }

    public IssuedGoodsServiceConnection(Settings activity)
    {
        this.activitySettings = activity;
    }

    public void OnServiceConnected(ComponentName name, IBinder service)
    {
        BluetoothService.MyBinder binder = (BluetoothService.MyBinder)service;

        if (activityIssuedGoods != null)
        {
            activityIssuedGoods.OnServiceBindingComplete(binder.GetService());
            activityIssuedGoods.binder = binder;
            activityIssuedGoods.isBound = true;
        }
        else if (activitySettings != null)
        {
            activitySettings.OnServiceBindingComplete(binder.GetService());
            activitySettings.binder = binder;
            activitySettings.isBound = true;
        }
    }

    public void OnServiceDisconnected(ComponentName name)
    {
        activityIssuedGoods.isBound = false;
    }
}