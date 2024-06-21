using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.ExceptionStore
{
    public static class GlobalExceptions
    {
        public static void ReportGlobalException(Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }
}
