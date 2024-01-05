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
    class TakeOverIdentList
    {

        public string Ident { get; set; }
        public string Ordered { get; set; }

        public string Received{ get; set; }

        public string Open { get; set; }
        public string Name { get; set; }
 
    }
}