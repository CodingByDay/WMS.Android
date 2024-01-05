using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrendNET.WMS.Core.Data {
    [Serializable]
    public class NameValue {
        public string Name { get; set; }
        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public double? DoubleValue { get; set; }
        public bool? BoolValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
    }
}
