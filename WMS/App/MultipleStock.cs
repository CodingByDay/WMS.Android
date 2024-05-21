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
        public string Location { get; set; }

        public double Quantity { get; set; }
        public override string ToString()
        {
            return Location + " ( " +  Quantity + " ) ";
        }
    }



}
