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
    class UnfinishedIssuedList
    {
        public string Document { get; set; }

        public string Orderer { get; set; }

        public string Date { get; set; }

        public string NumberOfPositions { get; set; }
    }
}