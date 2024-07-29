using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;

public class SearchableSpinnerAdapter : BaseAdapter<string>, IFilterable
{
    private List<string> originalItems;
    private List<string> filteredItems;
    private readonly Context context;
    private readonly ItemFilter filter;
    public List<string> Items => originalItems;


    public SearchableSpinnerAdapter(Context context, List<string> items)
    {
        this.context = context;
        originalItems = new List<string>(items);
        filteredItems = new List<string>(items);
        filter = new ItemFilter(this);
    }

    public override int Count => filteredItems.Count;

    public override string this[int position] => filteredItems[position];

    public override long GetItemId(int position)
    {
        return position;
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        var view = convertView ?? LayoutInflater.From(context).Inflate(Android.Resource.Layout.SimpleSpinnerItem, parent, false);
        var textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
        textView.Text = filteredItems[position];
        return view;
    }

    public override View GetDropDownView(int position, View convertView, ViewGroup parent)
    {
        var view = convertView ?? LayoutInflater.From(context).Inflate(Android.Resource.Layout.SimpleSpinnerDropDownItem, parent, false);
        var textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
        textView.Text = filteredItems[position];
        return view;
    }

    public Filter Filter => filter;

    private class ItemFilter : Filter
    {
        private readonly SearchableSpinnerAdapter adapter;

        public ItemFilter(SearchableSpinnerAdapter adapter)
        {
            this.adapter = adapter;
        }

        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            var filterResults = new FilterResults();
            if (constraint != null && constraint.Length() > 0)
            {
                var query = constraint.ToString().ToLower();
                var filteredList = adapter.originalItems.Where(item => item.ToLower().Contains(query)).ToList();

                filterResults.Values = FromArray(filteredList.Select(r => r.ToJavaObject()).ToArray());
                filterResults.Count = filteredList.Count;
            }
            else
            {
                filterResults.Values = FromArray(adapter.originalItems.Select(r => r.ToJavaObject()).ToArray());
                filterResults.Count = adapter.originalItems.Count;
            }
            return filterResults;
        }

        protected override void PublishResults(ICharSequence constraint, FilterResults results)
        {
            adapter.filteredItems = results.Values.ToArray<Java.Lang.Object>().Select(r => r.ToString()).ToList();
            adapter.NotifyDataSetChanged();
        }
    }
}
