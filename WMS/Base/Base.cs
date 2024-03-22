using Android.Content;
using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrendNET.WMS.Core.Data;
using Android.Content;
using Android.Content.Res;
using Java.Util;
using Android.OS;
using Android.Util;
using Android.Views;
using System.Globalization;
using WMS.App;
using Java.Lang;
namespace WMS
{
    /// <summary>
    /// This is a very important new functionality. 
    /// It allows us to save the global state directly
    /// here instead of in the old InUseData which needs more steps do be initialized and used.
    /// Define all global states here with the public modifiers and use them in the application.
    /// Document properties and uses. - Janko Jovičić 14.03.2024
    /// </summary>
    [Application]
    public class Base : Application
    {
        // This property is used for showing what is the choice of the user in regards to issuing mode.
        public int modeIssuing { get; set; } = 1;
        // This property is used for showing whether the current action is for updating the position or creating it.
        public bool isUpdate { get; set; } = false;
        // Open order object.
        public static Base Store { get; set; }
        public IssueIdent OpenOrder { get; set; }

        public string language = string.Empty;

        // Reset method using reflection
        public void ResetValues()
        {
            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    object defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                    property.SetValue(this, defaultValue);
                }
            }
        }





        public Base(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }


        public override void OnCreate()
        {
            base.OnCreate();
            Store = this;

        }

    




    }
}
