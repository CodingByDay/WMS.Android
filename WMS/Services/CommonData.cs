using Microsoft.AppCenter.Crashes;
using TrendNET.WMS.Core.Data;

namespace TrendNET.WMS.Device.Services
{
    public class CommonData
    {
        public static string Version = "1.0.73f";
        private static Dictionary<string, NameValueObjectList> warehouses = new Dictionary<string, NameValueObjectList>();
        private static NameValueObjectList shifts = null;
        private static NameValueObjectList subjects = null;
        private static NameValueObjectList allIdents = null;
        private static Dictionary<string, bool> locations = new Dictionary<string, bool>();
        private static Dictionary<string, NameValueObjectList> docTypes = new Dictionary<string, NameValueObjectList>();
        private static Dictionary<string, NameValueObject> idents = new Dictionary<string, NameValueObject>();
        private static Dictionary<string, string> settings = new Dictionary<string, string>();

        private static string qtyPicture = null;

        public static string GetQtyPicture()
        {
            if (qtyPicture == null)
            {
                var digStr = GetSetting("QtyDigits");
                if (string.IsNullOrEmpty(digStr)) { digStr = "2"; }
                var digits = Convert.ToInt32(digStr);
                qtyPicture = "###,###,##0.";
                for (int i = 1; i <= digits; i++) { qtyPicture += "0"; }
            }
            return qtyPicture;
        }

        public static string GetSetting(string name)
        {
            if (settings.ContainsKey(name))
            {
                return settings[name];
            }

            try
            {
                string error;
                var value = Services.GetObject("sg", name, out error);
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    var val = value.GetString("Value");
                    settings.Add(name, val);
                    return val == null ? "" : val;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetNextSSCC()
        {
            try
            {
                string error;
                var value = Services.GetObject("ns", "", out error);
                if (value == null)
                {
                    throw new ApplicationException("Napaka pri pridobivanju SSCC kode: " + error);
                }
                else
                {
                    var sscc = value.GetString("SSCC");
                    if (string.IsNullOrEmpty(sscc))
                    {
                        return string.Empty;
                    }
                    return sscc;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static NameValueObject GetWarehouse(string warehouse)
        {
            var wh = ListWarehouses()
                .Items
                .FirstOrDefault(x => x.GetString("Subject") == warehouse);
            if (wh == null)
            {
            }
            return wh;
        }

        public static NameValueObjectList ListWarehouses()
        {
            var userID = Services.UserID().ToString();
            if (warehouses.ContainsKey(userID))
            {
                return warehouses[userID];
            }
            try
            {
                string error;
                var whs = Services.GetObjectList("wh", out error, userID);
                if (whs == null)
                {
                    return null;
                }
                warehouses.Add(userID, whs);
                return whs;
            }
            catch (Exception err)
            {
                Crashes.TrackError(err);
                return null;
            }
        }

        public static NameValueObjectList ListSubjects()
        {
            string error;
            subjects = Services.GetObjectList("su", out error, "");
            try
            {
                if (subjects == null)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
            return subjects;
        }

        public static NameValueObjectList ListReprintSubjects()
        {
            string error;
            subjects = Services.GetObjectList("surl", out error, "");
            try
            {
                if (subjects == null)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
            return subjects;
        }

        public static bool IsValidLocation(string warehouse, string location)
        {
            var key = warehouse + "|" + location;
            if (locations.ContainsKey(key))
            {
                return locations[key];
            }
            try
            {
                string error;
                var loc = Services.GetObject("lo", key, out error);
                if (loc != null)
                {
                    locations[key] = true;
                }
                return loc != null;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return false;
            }
        }

        public static NameValueObjectList ListDocTypes(string pars)
        {
            if (docTypes.ContainsKey(pars))
            {
                return docTypes[pars];
            }
            try
            {
                string error;
                var dts = Services.GetObjectList("dt", out error, pars);
                if (dts == null)
                {
                    return null;
                }
                docTypes.Add(pars, dts);
                return dts;
            }
            catch (Exception err)
            {
                Crashes.TrackError(err);
                return null;
            }
        }

        public static NameValueObject LoadIdent(string ident)
        {
            if (idents.ContainsKey(ident))
            {
                return idents[ident];
            }

            try
            {
                string error;
                var openIdent = Services.GetObject("id", ident, out error);
                if (openIdent == null)
                {
                    return null;
                }
                else
                {
                    var code = openIdent.GetString("Code");
                    var secCode = openIdent.GetString("SecondaryCode");
                    if (!string.IsNullOrEmpty(code) && !idents.ContainsKey(code))
                    {
                        idents.Add(code, openIdent);
                    }
                    if (!string.IsNullOrEmpty(secCode) && !idents.ContainsKey(secCode))
                    {
                        idents.Add(secCode, openIdent);
                    }
                    return openIdent;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}