using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Base
{
    public class Base : Application
    {
        public Base(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            // Constructor required for Xamarin.Android
        }

        public override void OnCreate()
        {
            base.OnCreate();

            // Initialization code for your application
        }
    }
}
