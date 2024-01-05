using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanner.App
{
    /// <summary>
    ///  Helpful class for serializing the object into a j son string.
    /// </summary>
    class moveItem
    {
        public int HeadID { get; set; }

        public string LinkKey { get; set; }

        public int LinkNo { get; set; }

        public string Ident { get; set; }

        public string SSCC { get; set; }

        public string SerialNo { get; set; }

        public double Packing { get; set; }

        public double Factor { get; set; }

        public double Qty { get; set; }

        public int Clerk { get; set; }

        public string Location { get; set; }

        public string IssueLocation { get; set; }

        public string Pallete { get; set; }
    }
}