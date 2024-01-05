using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

public class CustomAutoCompleteAdapter<T> : ArrayAdapter<T>
{
    private LayoutInflater inflater;

    public CustomAutoCompleteAdapter(Context context, int textViewResourceId, List<T> objects)
        : base(context, textViewResourceId, objects)
    {
        inflater = LayoutInflater.From(context);
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        View view = convertView;
        if (view == null)
        {
            view = inflater.Inflate(Scanner.Resource.Layout.custom_dropdown_item, null, false);
        }

        // Customize the appearance of the dropdown item here
        TextView text = view.FindViewById<TextView>(Scanner.Resource.Id.textValue);
        text.Text = GetItem(position).ToString();
   
        return view;
    }
}
