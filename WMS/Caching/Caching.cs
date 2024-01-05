using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendNET.WMS.Device.Services;
using Xamarin.Essentials;

namespace Scanner.Caching
{
    internal static class Caching
    {






        public static List<string> SavedList
        {
            get
            {
                var savedList = Deserialize<List<string>>(Preferences.Get(nameof(SavedList), null));
                return savedList ?? new List<string>();
            }
            set
            {
                var serializedList = Serialize(value);
                Preferences.Set(nameof(SavedList), serializedList);
            }
        }

        static T Deserialize<T>(string serializedObject) {


            if (serializedObject != null) {
                return JsonConvert.DeserializeObject<T>(serializedObject);


           }
            return default(T);

        } 

        static string Serialize<T>(T objectToSerialize) => JsonConvert.SerializeObject(objectToSerialize);
    }
}