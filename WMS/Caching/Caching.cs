using Newtonsoft.Json;
using Xamarin.Essentials;
namespace WMS.Caching
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

        static T Deserialize<T>(string serializedObject)
        {
            if (serializedObject != null)
            {
                return JsonConvert.DeserializeObject<T>(serializedObject);
            }
            return default(T);
        }

        static string Serialize<T>(T objectToSerialize) => JsonConvert.SerializeObject(objectToSerialize);
    }
}