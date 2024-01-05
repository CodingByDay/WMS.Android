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

namespace WMS.App
{
    class UnfinishedTakeoverList
    {
        public string Document { get; set; }
        public string Issuer { get; set; }
        public string Date { get; set; }

        public string NumberOfPositions { get; set; }



    }
}