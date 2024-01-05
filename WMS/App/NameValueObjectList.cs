using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrendNET.WMS.Core.Data {
    public class NameValueObjectList {
        public string ObjectName { get; set; }
        public List<NameValueObject> Items { get; set; }

        public static NameValueObjectList FromCollection<T> (List<T> col) {
            Type t = typeof (T);
            var nvol = new NameValueObjectList ();
            nvol.ObjectName = t.Name;
            nvol.Items = new List<NameValueObject> ();
            col.ForEach (i => nvol.Items.Add (NameValueObject.FromObject<T> (i)));
            return nvol;
        }
       
    }
}
