using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Crashes;
using Scanner.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "PickingMenu", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class PickingMenuTablet : Activity
    {
        private Button ident;
        private Button order;
        private Button client;
        private NameValueObject choice = (NameValueObject)InUseObjects.Get("CurrentClientFlow");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private bool isMobile;

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PickingMenuTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            // Fields
            isMobile = !settings.tablet;
            ident = FindViewById<Button>(Resource.Id.ident);
            order = FindViewById<Button>(Resource.Id.order);
            client = FindViewById<Button>(Resource.Id.client);

            // Click events

            ident.Click += Ident_Click;
            order.Click += Order_Click;
            client.Click += Client_Click;
            // Flow events
        }








        private void Client_Click(object sender, EventArgs e)
        {
            choice.SetString("CurrentFlow", "2");
            InUseObjects.Set("CurrentClientFlow", choice);
            StartActivity(typeof(IssuedGoodsBusinessEventSetupClientPickingTablet));
        }

        private void Order_Click(object sender, EventArgs e)
        {
            choice.SetString("CurrentFlow", "1");
            InUseObjects.Set("CurrentClientFlow", choice);
            StartActivity(typeof(IssuedGoodsBusinessEventSetupTablet));

        }

        private void Ident_Click(object sender, EventArgs e)
        {
            choice.SetString("CurrentFlow", "0");
            InUseObjects.Set("CurrentClientFlow", choice);
            StartActivity(typeof(IssuedGoodsBusinessEventSetupTablet));
        }
    }
}