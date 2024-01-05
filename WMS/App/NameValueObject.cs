using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TrendNET.WMS.Core.Data {
    [Serializable]
    public class NameValueObject {
        public string ObjectName { get; set; }
        public NameValueList Properties { get; set; }

        public NameValueObject()
        {
            this.ObjectName = "";
            this.Properties = new NameValueList();
        }

        public NameValueObject(string name)
        {
            this.ObjectName = name;
            this.Properties = new NameValueList();
        }

        public static NameValueObject FromObject<T>(T obj)
        {
            Type t = typeof(T);
            var nvo = new NameValueObject(t.Name);
            var props = t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            props.ToList().ForEach(p =>
            {
                var val = p.GetValue(obj, null);
                if ((val is Int32) || (val is Int16))
                {
                    nvo.Properties.Add(p.Name, (Int32)val);
                }
                else if (val is string)
                {
                    var value = (string)val;
                    if (value.Contains("\0")) { value = value.Replace("\0", ""); }
                    nvo.Properties.Add(p.Name, value == null ? "" : value.Trim());
                }
                else if (val is bool)
                {
                    nvo.Properties.Add(p.Name, (bool)val);
                }
                else if (val is double)
                {
                    nvo.Properties.Add(p.Name, (double)val);
                }
                else if (val is DateTime)
                {
                    nvo.Properties.Add(p.Name, (DateTime)val);
                }
            });
            return nvo;
        }

        public string GetString(string property)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            var val = obj == null ? "" : (obj.StringValue == null ? "" : obj.StringValue.Trim());
            return val.Replace("\0", "");
        }

        public void SetString(string property, string value)
        {
            if ((value != null) && value.Contains("\0")) { value = value.Replace("\0", ""); }
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            if (obj != null)
            {
                obj.StringValue = value == null ? "" : value.Trim ();
            }
            else
            {
                Properties.Items.Add(new NameValue { Name = property, StringValue = value == null ? "" : value.Trim() });
            }
        }

        public int GetInt(string property)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            return obj == null ? 0 : (int)obj.IntValue;
        }

        public void SetInt(string property, int value)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            if (obj != null)
            {
                obj.IntValue = value;
            }
            else
            {
                Properties.Items.Add(new NameValue { Name = property, IntValue = value });
            }
        }

        public bool GetBool(string property)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            return obj == null ? false : (bool)obj.BoolValue;
        }

        public void SetBool(string property, bool value)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            if (obj != null)
            {
                obj.BoolValue = value;
            }
            else
            {
                Properties.Items.Add(new NameValue { Name = property, BoolValue = value });
            }
        }

        public double GetDouble(string property)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            return obj == null ? 0.0 : (double)obj.DoubleValue;
        }

        public void SetDouble(string property, double value)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            if (obj != null)
            {
                obj.DoubleValue = value;
            }
            else
            {
                Properties.Items.Add(new NameValue { Name = property, DoubleValue = value });
            }
        }

        public DateTime? GetDateTime(string property)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            return obj == null ? null : obj.DateTimeValue;
        }

        public void SetDateTime(string property, DateTime? value)
        {
            var obj = Properties.Items.FirstOrDefault(x => x.Name == property);
            if (obj != null)
            {
                obj.DateTimeValue = value;
            }
            else
            {
                Properties.Items.Add(new NameValue { Name = property, DateTimeValue = value });
            }
        }
    }
}
