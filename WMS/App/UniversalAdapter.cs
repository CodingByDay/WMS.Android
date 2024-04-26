using Android.Content;
using Android.Graphics;
using Android.Views;

public class UniversalAdapter<T> : BaseAdapter<T>
{
    private readonly Context context;
    private readonly List<T> items;
    private readonly int layoutResource;
    private readonly LayoutInflater inflater;
    private readonly Action<View, T> bindAction;
    private int selectedPosition = -1;

    public UniversalAdapter(Context context, List<T> items, int layoutResource, Action<View, T> bindAction)
    {
        this.context = context;
        this.items = items;
        this.layoutResource = layoutResource;
        this.bindAction = bindAction;
        this.inflater = LayoutInflater.From(context);
    }



    public override T this[int position] => items[position];

    public override int Count => items.Count;

    public override long GetItemId(int position) => position;

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        View view = convertView ?? inflater.Inflate(layoutResource, parent, false);
        var item = items[position];
        bindAction?.Invoke(view, item);

        // Set background color for selected item
        if (position == selectedPosition)
        {
            view.SetBackgroundColor(Color.Argb(128, 169, 169, 169)); // Gray with transparency
        }
        else
        {
            view.SetBackgroundColor(Color.Transparent);
        }

        return view;
    }

    public void SetSelected(int position)
    {
        selectedPosition = position;
        NotifyDataSetChanged();
    }

    public T GetSelectedItem()
    {
        if (selectedPosition >= 0 && selectedPosition < items.Count)
        {
            return items[selectedPosition];
        }
        else
        {
            return default(T);
        }
    }

    public int GetSelectedIndex()
    {
        return selectedPosition;
    }
}
