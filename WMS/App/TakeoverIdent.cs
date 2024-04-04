using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.App
{
    public class TakeoverIdent
    {
        public string Ident { get; set; }
        public string? Order { get; set; }
        public int? Position { get; set; }
        public string? Client { get; set; }
        public DateTime? Date { get; set; }
        public double? Quantity { get; set; }
    }
}
