using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TrendNET.WMS.Device.App
{
    public class InUseObjects
    {
        private static Dictionary<string, object> inUseObjects = new Dictionary<string, object>();

        public static void Clear()
        {
            inUseObjects = new Dictionary<string, object>();
        }

        public static void ClearExcept(string[] keys)
        {
            var temp = new Dictionary<string, object>();
            foreach (var k in keys)
            {
                var o = Get(k);
                if (o != null)
                {
                    temp.Add(k, o);
                }
            }
            inUseObjects = temp;
        }

        public static object Get(string key)
        {
            return inUseObjects.ContainsKey(key) ? inUseObjects[key] : null;
        }

        public static void Set(string key, object o)
        {
            if (inUseObjects.ContainsKey(key))
            {
                inUseObjects[key] = o;
            }
            else
            {
                inUseObjects.Add(key, o);
            }
        }

        public static void Invalidate(string key)
        {
            if (inUseObjects.ContainsKey(key))
            {
                inUseObjects.Remove (key);
            }
        }
    }
}
