using TrendNET.WMS.Device.Services;
using WMS.ExceptionStore;

namespace WMS.App
{
    public static class AdapterStore
    {


     
        public static async Task<List<LocationClass>> GetStockForWarehouseAndIdent(string ident, string warehouse)
        {
            try
            {
                List<LocationClass> result = new List<LocationClass>();
                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
                string sql = "SELECT acIdent, aclocation, anQty, acSerialNo, acSSCC FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
                var sqlResult = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
                if (sqlResult != null && sqlResult.Success)
                {
                    if (sqlResult.Rows.Count > 0)
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<LocationClass>();
            }
        }
    }
}