using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Concurrent;
using TrendNET.WMS.Device.App;



public delegate void SingleItemEventHandler(string barcode);

public class CustomAutoCompleteAdapter<T> : ArrayAdapter<T>
{
    private LayoutInflater inflater;
    public List<T> originalItems;
    public event SingleItemEventHandler SingleItemEvent;

    public CustomAutoCompleteAdapter(Context context, int textViewResourceId, List<T> objects)
       : base(context, textViewResourceId, new List<T>(objects.Take(100))) // Initialize with the first 1000 items
    {
        inflater = LayoutInflater.From(context);
        originalItems = objects != null ? InitializeOriginalItems(objects) : new List<T>();
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        View view = convertView;
        if (view == null)
        {
            view = inflater.Inflate(WMS.Resource.Layout.custom_dropdown_item, null, false);
        }

        // Customize the appearance of the dropdown item here
        TextView text = view.FindViewById<TextView>(WMS.Resource.Id.textValue);
        text.Text = GetItem(position).ToString();

        return view;
    }
    private List<T> InitializeOriginalItems(List<T> objects)
    {
        if (objects.Count > 1000)
        {
            var bag = new ConcurrentBag<T>();
            Parallel.ForEach(objects, item =>
            {
                bag.Add(item);
            });
            return bag.ToList();
        }
        else
        {
            return new List<T>(objects);
        }
    }

    public ComboBoxItem GetComboBoxItem(int position)
    {
        ComboBoxItem? comboBoxItem = base.GetItem(position) as ComboBoxItem;
        return comboBoxItem ?? new ComboBoxItem();
    }

    public void RaiseOneItemRemaining(string? singleItemString)
    {
       SingleItemEvent?.Invoke(singleItemString);
    }



    public override Filter Filter
    {
        get
        {
            return new CustomFilter<T>(this, originalItems);
        }
    }
}