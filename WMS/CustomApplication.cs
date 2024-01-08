using Android.Runtime;
using Android.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS
{
    public class CustomApplication : Application
    {
        public CustomApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer) { }

        public override void OnCreate()
        {
            base.OnCreate();

            // Configure network security settings programmatically
            ConfigureNetworkSecurity();
        }

        private void ConfigureNetworkSecurity()
        {
            // Create a network security configuration builder

        }
    }
}
