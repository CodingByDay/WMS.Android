using TrendNET.WMS.Device.Services;

namespace WMS.App
{
    public static class AdapterStore
    {


       /* public static List<TakeoverDocument> fillItemsOfList(string warehouse, string ident)
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
       */
        public static async Task<List<LocationClass>> getStockForWarehouseAndIdent(string ident, string warehouse)
        {
            List<LocationClass> result = new List<LocationClass>();
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
            parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
            string sql = "SELECT * FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
            var sqlResult = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
            if(sqlResult!=null && sqlResult.Success)
            {
                if(sqlResult.Rows.Count > 0)
                {
                    sqlResult.Rows.ForEach(x =>
                    {

                        result.Add(new LocationClass
                        {
                            ident = x.StringValue("acIdent"),
                            location = x.StringValue("aclocation"),
                            quantity = x.DoubleValue("anQty").ToString(),
                            serial = x.StringValue("acSerialNo"),
                            sscc = x.StringValue("acSSCC")
                        });


                    });

                }
            } 


            return result;
        }
    }
}