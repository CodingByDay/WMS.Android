using Android.Content;
using Android.Views;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TrendNET.WMS.Device.Services;
using WMS.ExceptionStore;

namespace WMS.App
{
    public static class HelperMethods
    {
        public static View FindNextFocusableView(View currentView)
        {
            if (currentView == null)
                return null;

            ViewGroup parentView = (ViewGroup)currentView.Parent;
            if (parentView == null)
                return null;

            View nextFocusableView = null;
            bool foundCurrent = false;
            int childCount = parentView.ChildCount;

            for (int i = 0; i < childCount; i++)
            {
                View child = parentView.GetChildAt(i);

                if (foundCurrent && IsFocusable(child))
                {
                    nextFocusableView = child;
                    break;
                }

                if (child == currentView)
                {
                    foundCurrent = true;
                }
            }

            // If not found in this parent, recursively search in the parent's parent
            if (nextFocusableView == null && parentView.Parent is ViewGroup)
            {
                nextFocusableView = FindNextFocusableView(parentView);
            }

            return nextFocusableView;
        }

        public static bool IsFocusable(View view)
        {
            return view != null && view.Focusable && view.Visibility == ViewStates.Visible;
        }

        public static bool is2D(string code)
        {
            if (code.Contains("1T") && code.Contains("K") && code.Contains("4Q"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string lastReturn(string input, int noCharacters)
        {
            if (noCharacters > input.Length)
            {
                return input;
            }
            else
            {
                string lastReturn;
                lastReturn = input.Substring(input.Length - noCharacters);
                return lastReturn;
            }
        }

        public static  List<string> DeserializeJsonStream(string json)
        {
            List<string> items = new List<string>();

            using (StringReader stringReader = new StringReader(json))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                JArray jsonArray = JArray.Load(jsonReader);

                foreach (var item in jsonArray)
                {
                    items.Add(item.ToString());
                }
            }

            return items;
        }




        public static void StartActivityWithAlias(Context context, Type activityClass, string aliasName)
        {
            try
            {
                bool useAlias = App.Settings.tablet;
                Intent intent = new Intent();
                intent.SetAction(Intent.ActionMain);
                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.NewTask);

                if (useAlias)
                {
                    // Define the component name for the activity alias
                    ComponentName cn = new ComponentName(context.ApplicationContext, aliasName);
                    intent.SetComponent(cn);
                }
                else
                {
                    // Use the main activity class directly
                    intent.SetClass(context, activityClass);
                }

                // Start the activity
                context.StartActivity(intent);
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public static async Task<List<String>> GetLocationsForGivenWarehouse(string warehouse)
        {
            try
            {
                List<string> result = new List<string>();

                await Task.Run(() =>
                {
                    string error;
                    var locations = Services.GetObjectList("lo", out error, warehouse);
                    if (locations != null)
                    {
                        locations.Items.ForEach(x =>
                        {
                            var location = x.GetString("LocationID");
                            result.Add(location);
                            // Notify the adapter state change!
                        });
                    }

                });
                return result;
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<string>();
            }
        }

        /// <summary>
        /// Additional method for allowing companies to go over the ordered quantity.
        /// </summary>
        /// <param name="Stock">Stock value</param>
        /// <param name="Max">Maximum allowed transaction</param>
        /// <param name="WantedQty">The inputed value</param>
        /// <returns></returns>
        public static QuantityProcessing IsOverTheLimitTransactionAllowed(double Stock, double Max, double WantedQty)
        {
            try
            {
                if (WantedQty <= Stock)
                {
                    if (WantedQty <= Max)
                    {
                        return QuantityProcessing.GoodToGo;
                    }
                    return QuantityProcessing.OverTheOrdered;
                }
                return QuantityProcessing.OverTheStock;
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return QuantityProcessing.OtherError;
            }
        }



        public static void SelectPositionProgramaticaly(ListView control, int index)
        {
      
                if (control != null)
                {
                    control.RequestFocusFromTouch();
                    control.SetSelection(index);
                    control.SetItemChecked(index, true);
                }
  
        }


    }
}