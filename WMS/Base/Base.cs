using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Base
{
    [Application]
    public class Base : Application
    {
        public Bitmap cachedImage;

        public Base(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        
        }

        public override void OnCreate()
        {
            base.OnCreate();

            // Initialization code for your application
        }
    }
}
