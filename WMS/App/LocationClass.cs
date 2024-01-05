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
    public class LocationClass
    {
        public string ident { get; set; }
        public string quantity { get; set; }

        public string  location { get; set; }

        public LocationClass()
        {
            
        }

        /// <summary>
        ///  Just to make it easier to insert into the list from the async methods.
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="quantity"></param>
        /// <param name="location"></param>
        public LocationClass(string ident, string quantity, string location)
        {
            this.ident = ident;
            this.quantity = quantity;
            this.location = location;
        }
    }
}