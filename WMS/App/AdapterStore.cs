﻿using TrendNET.WMS.Device.Services;
using WMS.ExceptionStore;

namespace WMS.App
{
    public static class AdapterStore
    {


     
        public static async Task<List<LocationClass>> GetStockForWarehouseAndIdent(string ident, string warehouse)
        {
            try
            {
                string error;
                List<LocationClass> result = new List<LocationClass>();
                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
                string sql = "SELECT acIdent, anQty, acLocation FROM uWMSStockByWarehouseSUM WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
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
                                location = x.StringValue("acLocation"),
                                quantity = x.DoubleValue("anQty").ToString(),
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



        public static async Task<List<LocationClass>> GetStockForIdent(string ident)
        {
            try
            {
                string error;
                List<LocationClass> result = new List<LocationClass>();
                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                string sql = "SELECT acIdent, anQty, acLocation, acWarehouse FROM uWMSStockByWarehouseSUM WHERE acIdent = @acIdent;";
                var sqlResult = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
                if (sqlResult != null && sqlResult.Success)
                {
                    if (sqlResult.Rows.Count > 0)
                    {
                        sqlResult.Rows.ForEach(x =>
                        {
                            result.Add(new LocationClass
                            {
                                ident = x.StringValue("acWarehouse"),
                                location = x.StringValue("acLocation"),
                                quantity = x.DoubleValue("anQty").ToString(),
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