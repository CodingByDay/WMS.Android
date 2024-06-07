using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.Services;

namespace WMS.Printing
{
    internal class PrintingCommon
    {
        public static void SetNVOCommonData(ref NameValueObject nvo)
        {
            nvo.SetInt("UserID", Services.UserID());
            nvo.SetString("DeviceID", App.Settings.ID);
        }

        public static void SendToServer(NameValueObject nvo)
        {
            string error;
            var data = CompactSerializer.Serialize<NameValueObject>(nvo);
            if (WebApp.Post("mode=print", data, out error))
            {

            }
            else
            {
            }
        }
    }
}