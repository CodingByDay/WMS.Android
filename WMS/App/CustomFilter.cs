﻿using Android.Runtime;
using Android.Widget;
using Java.Lang;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WMS.ExceptionStore;
using System.Timers;
using Timer = System.Timers.Timer; // Include this for the timer

public class CustomFilter<T> : Filter
{
    private readonly CustomAutoCompleteAdapter<T> adapter;
    private readonly List<T> originalItems;
    private string lastRaisedString = string.Empty;
    private Timer filterTimer;

    public CustomFilter(CustomAutoCompleteAdapter<T> adapter, List<T> originalItems)
    {
        this.adapter = adapter;
        this.originalItems = new List<T>(originalItems);
        // Initialize the timer

    }

    protected override FilterResults PerformFiltering(ICharSequence constraint)
    {
        try
        {
            FilterResults result = new FilterResults();
            if (adapter.originalItems != null && adapter.originalItems.Count > 0 && constraint != null)
            {
               string filterString = constraint.ToString();

                // Use PLINQ with case-insensitive comparison for filtering
                var resultList = originalItems.AsParallel()
                    .Where(item => item.ToString().IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(item => !string.Equals(item.ToString(), filterString, StringComparison.OrdinalIgnoreCase)) // Exact match first
                    .ThenBy(item => item.ToString()) // Optional: sort others alphabetically
                    .Take(10)
                    .ToList();

                result.Values = FromArray(resultList.Select(item => item.ToJavaObject()).ToArray());
                result.Count = resultList.Count;
            }
            else
            {
                // Simply return the first 100 original items if no filtering is required
                var resultList = originalItems.Take(10).ToList();
                result.Values = FromArray(resultList.Select(item => item.ToJavaObject()).ToArray());
                result.Count = resultList.Count;
            }

            return result;
        }
        catch (System.Exception ex)
        {
            FilterResults result = new FilterResults
            {
                Count = 0,
                Values = null
            };
            GlobalExceptions.ReportGlobalException(ex);
            return result;
        }
    }

    // Timer elapsed event handler


    protected override void PublishResults(ICharSequence constraint, FilterResults results)
    {
        try
        {
            List<string> resultsProcessing = new List<string>();

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
           
            var ignore = constraint != null && lastRaisedString.Contains(constraint.ToString()) && constraint.ToString().Count() < lastRaisedString.Count();
            // Check if only one item is left after filtering
            if (adapter.Count == 1 && !ignore)
            {
                T singleItem = adapter.GetItem(0);
                string singleItemString = singleItem.ToString(); // Assuming ToString() gives us the string representation we need
                adapter.RaiseOneItemRemaining(singleItemString);
                lastRaisedString = singleItemString;

            }           
        }
        catch (System.Exception ex)
        {
            GlobalExceptions.ReportGlobalException(ex);
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
