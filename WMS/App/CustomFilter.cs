using Android.Runtime;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;

public class CustomFilter<T> : Filter
{
    private readonly CustomAutoCompleteAdapter<T> adapter;
    private readonly List<T> originalItems;

    public CustomFilter(CustomAutoCompleteAdapter<T> adapter, List<T> originalItems)
    {
        this.adapter = adapter;
        this.originalItems = new List<T>(originalItems);
    }

    protected override FilterResults PerformFiltering(ICharSequence constraint)
    {
        FilterResults result = new FilterResults();

        if (adapter.originalItems != null && adapter.originalItems.Count > 0 && constraint != null)
        {
            List<T> filteredList = new List<T>();

            foreach (var originalItem in adapter.originalItems)
            {
                string itemText = originalItem.ToString();

                // Implement your filter logic here (example: case-insensitive contains)
                if (itemText.ToLowerInvariant().Contains(constraint.ToString().ToLowerInvariant()))
                {
                    filteredList.Add(originalItem);
                }
            }

            result.Count = filteredList.Count;
            result.Values = FromArray(filteredList.Select(item => item.ToJavaObject()).ToArray());
        }
        else
        {
            // If originalItems is null or empty, provide empty results
            result.Count = 0;
            result.Values = null;
        }

        return result;
    }



    protected override void PublishResults(ICharSequence constraint, FilterResults results)
    {
        adapter.Clear();

        if (results != null && results.Count > 0 && results.Values != null)
        {
            var resultValues = results.Values.ToArray<Java.Lang.Object>();

            foreach (var item in resultValues)
            {
                T typedItem = item.ToNetObject<T>();
                adapter.Add(typedItem);
            }

            adapter.NotifyDataSetChanged();
        }
        else
        {
            adapter.NotifyDataSetInvalidated();
        }

        // Check if only one item is left after filtering
        if (adapter.Count == 1)
        {
            T singleItem = adapter.GetItem(0);
            string singleItemString = singleItem.ToString(); // Assuming ToString() gives us the string representation we need
        }
    }


}
public static class ObjectExtensions
{
    public static T ToNetObject<T>(this Java.Lang.Object obj)
    {
        var wrapper = obj.JavaCast<JavaObjectWrapper<T>>();
        return wrapper != null ? wrapper.Instance : default(T);
    }


    public static Java.Lang.Object ToJavaObject<T>(this T obj)
    {
        return new JavaObjectWrapper<T>(obj);
    }
}



public class JavaObjectWrapper<T> : Java.Lang.Object
{
    public T Instance { get; private set; }

    public JavaObjectWrapper(T instance)
    {
        Instance = instance;
    }
}
