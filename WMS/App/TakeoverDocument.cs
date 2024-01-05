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
   public class TakeoverDocument
    {
        public string ident { get; set; }

        public string sscc { get; set; }

        public string serial { get; set; }


        public string quantity { get; set; }

        public string location { get; set; }
    }
}