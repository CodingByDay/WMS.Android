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
    class UnfinishedPackagingList
    {

        public string SerialNumber { get; set; }
        public string ssccCode { get; set; }

        public string CreatedBy { get; set; }
        public string NumberOfPositions { get; set; }

     
    }
}