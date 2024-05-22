using Android.Content;
using Android.Content.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WMS.App
{
    public class MultipleStock
    {
        public void ConfigurationMethod(bool excludeSSCCSerial, Context context)
        {
            this.excludeSSCCSerial = excludeSSCCSerial;
            this.context = context;
        }
        public bool excludeSSCCSerial = true;
        private Context context { get; set; }
        public string SSCC { get; set; }
        public string Serial { get; set; }
        public string Location { get; set; }
        public double Quantity { get; set; }



        public int GetResourceIdString(string resourceName)
        {
            int id = context.Resources.GetIdentifier(resourceName, "string", context.PackageName);
            // This method gets the corresponding id only for strings.
            return id;
        }


        public override string ToString()
        {
            if(this.excludeSSCCSerial)
            {
                return Location + " ( " + Quantity + " ) ";
            }
            else
            {
                string quantity = context.GetString(GetResourceIdString("s348"));
                string serial = context.GetString(GetResourceIdString("s349"));
                string sscc = context.GetString(GetResourceIdString("s350"));

                return Location + " ( " +
                    quantity + Quantity + " " +
                    serial + (Serial.Length >= 4 ? Serial.Substring(Serial.Length - 4) : Serial) + " " +
                    sscc + (SSCC.Length >= 4 ? SSCC.Substring(SSCC.Length - 4) : SSCC) +
                " ) ";
            }
        }
    }



}
