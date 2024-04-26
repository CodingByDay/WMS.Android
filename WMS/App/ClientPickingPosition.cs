using System.Runtime.Serialization;
using System.Text.Json;

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
    
        }


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
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        // Helper method to deserialize a byte array to an object
        public static T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}