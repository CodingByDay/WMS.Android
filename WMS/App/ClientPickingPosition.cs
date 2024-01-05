using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using static Java.Util.Jar.Attributes;

namespace WMS.App
{
    [Serializable]
    public class ClientPickingPosition
    {
        public string Ident { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Quantity { get; set; }
        public string Order { get; set; }
        public int No { get; set; }
        public int originalIndex { get; set; }
        public Dictionary<string, double> locationQty { get; set; } = new Dictionary<string, double>();




        public void recalculatePickingPositions()
        {
            // change property locatioQty //
            // 
        }



        /// <summary>
        ///  Empty constructor.
        /// </summary>
        public ClientPickingPosition()
        {

        }

        public ClientPickingPosition(SerializationInfo info, StreamingContext context)
        {
            Ident = info.GetString("Ident");
            Location = info.GetString("Location");
            Quantity = info.GetString("Quantity");
            Name = info.GetString("Name");
            Order = info.GetString("Key");
            No = info.GetInt32("No");
            locationQty = (Dictionary<string, double>)info.GetValue("locationQty", typeof(Dictionary<string, double>));
            originalIndex = info.GetInt32("originalIndex");
        }

 

        // Method to perform serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Ident", Ident);
            info.AddValue("Location", Location);
            info.AddValue("Qty", Quantity);
            info.AddValue("Name", Name);
            info.AddValue("Key", Order);
            info.AddValue("No", No);
            info.AddValue("locationQty", locationQty, typeof(Dictionary<string, double>));
            info.AddValue("originalIndex", originalIndex);
        }

        // Helper method to serialize an object to a byte array
        public static byte[] Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        // Helper method to deserialize a byte array to an object
        public static T Deserialize<T>(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(memoryStream);
            }
        }


    }
}