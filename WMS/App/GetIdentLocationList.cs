using TrendNET.WMS.Device.Services;

namespace WMS.App
{
    public static class GetIdentLocationList
    {
        /// <summary>
        ///   This is a helper method used in tablet classes to get the list of locations for a specific ident in the specified warehouse.
        /// </summary>
        /// <param name="warehouse">The warehouse of the ident.</param>
        /// <param name="ident">Ident code</param>
        /// <returns>Returns a list of TakeoverDocument objects.</returns>

        public static List<TakeoverDocument> fillItemsOfList(string warehouse, string ident)
        {
            List<TakeoverDocument> result = new List<TakeoverDocument>();
            string error;
            var stock = Services.GetObjectList("str", out error, warehouse + "||" + ident);
            //return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())).ToArray());
            stock.Items.ForEach(x =>
            {
                result.Add(new TakeoverDocument
                {
                    ident = x.GetString("Ident"),
                    sscc = x.GetString("SSCC"),
                    serial = x.GetString("Serial"),
                    location = x.GetString("Location"),
                    quantity = x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())
                });
            });

            return result;
        }

        public static List<LocationClass> fillItemsOfListLocationClass(string warehouse, string ident)
        {
            List<LocationClass> result = new List<LocationClass>();
            string error;
            var stock = Services.GetObjectList("str", out error, warehouse + "||" + ident);
            //return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())).ToArray());
            stock.Items.ForEach(x =>
            {
                result.Add(new LocationClass
                {
                    ident = x.GetString("Ident"),
                    location = x.GetString("Location"),
                    quantity = x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())
                });
            });

            return result;
        }
    }
}