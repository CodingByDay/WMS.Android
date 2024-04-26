using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

public class UniversalAdapter<T> : BaseAdapter<T>
{
    private readonly Context context;
    private readonly List<T> items;
    private readonly int layoutResource;
    private readonly LayoutInflater inflater;
    private readonly Action<View, T> bindAction;

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
        return view;
    }
}
