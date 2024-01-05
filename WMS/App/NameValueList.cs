using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrendNET.WMS.Core.Data {
    [Serializable]
    public class NameValueList {
        public List<NameValue> Items { get; set; }

        public NameValueList()
        {
            Items = new List<NameValue>();
        }

        public NameValue Get (string name) {
            var item = Items.FirstOrDefault (x => x.Name == name);
            if (item == null) {
                item = new NameValue { Name = name };
            }
            return item;
        }

        private void CheckItems () {
            if (Items == null) { Items = new List<NameValue> (); }
        }

        public void Add (string name, string value) {
            CheckItems ();
            Items.Add (new NameValue { Name = name, StringValue = value });
        }

        public void Add (string name, int value) {
            CheckItems ();
            Items.Add (new NameValue { Name = name, IntValue = value });
        }

        public void Add (string name, bool value) {
            CheckItems ();
            Items.Add (new NameValue { Name = name, BoolValue = value });
        }

        public void Add(string name, double value)
        {
            CheckItems();
            Items.Add(new NameValue { Name = name, DoubleValue = value });
        }

        public void Add(string name, DateTime value)
        {
            CheckItems();
            Items.Add(new NameValue { Name = name, DateTimeValue = value });
        }
    }
}
