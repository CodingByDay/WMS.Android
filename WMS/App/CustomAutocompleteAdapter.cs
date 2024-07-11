using Android.Content;
using Android.Views;
using Android.Widget;
using TrendNET.WMS.Device.App;



public delegate void SingleItemEventHandler(string barcode);

public class CustomAutoCompleteAdapter<T> : ArrayAdapter<T>
{
    private LayoutInflater inflater;
    public List<T> originalItems;
    public event SingleItemEventHandler SingleItemEvent;

    public CustomAutoCompleteAdapter(Context context, int textViewResourceId, List<T> objects)
        : base(context, textViewResourceId, objects)
    {
        inflater = LayoutInflater.From(context);
        originalItems = objects != null ? new List<T>(objects) : new List<T>();
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