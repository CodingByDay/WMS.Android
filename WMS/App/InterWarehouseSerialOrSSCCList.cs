using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS.App
{
    class ProductionEnteredPositionViewList { 


       public string Ident { get; set; }

        public string SerialNumber { get; set; }


        public string Location { get; set; }

        public string Qty { get; set; }

        public string Filled { get; set; }

    }
}