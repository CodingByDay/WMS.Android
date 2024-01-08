using TrendNET.WMS.Device.Services;

public static class Queries
{ // This static class should be used for specific interaction with the API through query style new implementation //
#nullable enable

    public static AutocompleteResponse DefaultIssueWarehouse(string docType)
    {
        AutocompleteResponse response = new AutocompleteResponse();
        ApiResultSet rs = new ApiResultSet();
        string query = $"SELECT acWarehouse FROM uWMSOrderDocTypeOut WHERE acDocType = '{docType}'";
        rs = Services.GetObjectListBySql(query);
        if (rs.Success && rs.Rows.Count > 0 && !string.IsNullOrEmpty(rs.Rows[0].StringValue("acWarehouse")))
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

    public static AutocompleteResponse DefaultTakeoverWarehouse(string docType)
    {
        AutocompleteResponse response = new AutocompleteResponse();
        ApiResultSet rs = new ApiResultSet();
        string query = $"SELECT acWarehouse FROM uWMSOrderDocTypeIn WHERE acDocType = '{docType}'";
        rs = Services.GetObjectListBySql(query);
        if (rs.Success && rs.Rows.Count > 0 && !string.IsNullOrEmpty(rs.Rows[0].StringValue("acWarehouse")))
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