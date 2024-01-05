using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

public class LazyAdapter : BaseAdapter<string>
{
    private List<string> items;
    private Context context;
    private List<string> allItems;

    public LazyAdapter(Context context, List<string> allItems)
    {
        this.context = context;
        this.items = new List<string>();
        this.allItems = allItems;
    }

    public void LoadMore(List<string> newItems)
    {
        items.AddRange(newItems);
        NotifyDataSetChanged();
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        // Implement view creation logic here for the item at the given position
        // You can use LayoutInflater to inflate a custom layout for each item
        var itemView = convertView ?? LayoutInflater.From(context).Inflate(Android.Resource.Layout.SimpleSpinnerItem, parent, false);
        var textView = itemView.FindViewById<TextView>(Android.Resource.Id.Text1);
        textView.Text = items[position];
        return itemView;
    }

    public override int Count => items.Count;

    public override long GetItemId(int position) => position;

    public override string this[int position] => items[position];
}
