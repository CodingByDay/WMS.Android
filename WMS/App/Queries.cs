using TrendNET.WMS.Device.Services;
using WMS.AsyncServices;

public static class Queries
{ // This static class should be used for specific interaction with the API with the query style of implementation //
#nullable enable


    public static async Task<AutocompleteResponse> DefaultIssueWarehouse(string docType)
    {
        AutocompleteResponse response = new AutocompleteResponse();
        ApiResultSet rs = new ApiResultSet();
        var parameters = new List<Services.Parameter>();
        parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = docType });
        string query = $"SELECT acWarehouse FROM uWMSOrderDocTypeOut WHERE acDocType = @acDocType";
        rs = await AsyncServices.GetObjectListBySqlAsync(query, parameters);
        if (rs != null && rs.Success && rs.Rows.Count > 0 && !string.IsNullOrEmpty(rs.Rows[0].StringValue("acWarehouse")))
        {
            response.warehouse = rs.Rows[0].StringValue("acWarehouse");
            response.main = true;
        }
        else
        {
            response.warehouse = CommonData.GetSetting("DefaultWarehouse");
            response.main = false;
        }
        return response;
    }

    public static async Task<AutocompleteResponse> DefaultTakeoverWarehouse(string docType)
    {
        AutocompleteResponse response = new AutocompleteResponse();
        ApiResultSet rs = new ApiResultSet();
        var parameters = new List<Services.Parameter>();
        parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = docType });
        string query = $"SELECT acWarehouse FROM uWMSOrderDocTypeIn WHERE acDocType = @acDocType";
        rs = await AsyncServices.GetObjectListBySqlAsync(query, parameters);
        if (rs != null && rs.Success && rs.Rows.Count > 0 && !string.IsNullOrEmpty(rs.Rows[0].StringValue("acWarehouse")))
        {
            response.warehouse = rs.Rows[0].StringValue("acWarehouse");
            response.main = true;
        }
        else
        {
            response.warehouse = CommonData.GetSetting("DefaultWarehouse");
            response.main = false;
        }
        return response;
    }


#nullable disable
}