using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Org.Apache.Commons.Logging;

namespace WMS
{
    public class SearchableSpinner : LinearLayout
    {
        public EditText spinnerTextValueField { get; set; }

        private ImageView icon;
        private List<string> items;
        private Context context;
        private string selectedItem;

        public SearchableSpinner(Context context) : base(context)
        {
            this.context = context;
            Initialize(context);
        }

        public SearchableSpinner(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.context = context;
            Initialize(context);
        }

        public SearchableSpinner(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            this.context = context;
            Initialize(context);
        }

        protected SearchableSpinner(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            this.context = Context;
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            Orientation = Orientation.Horizontal;

            // Inflate your custom layout
            Inflate(context, Resource.Layout.SearchableSpinnerRepresentation, this);

            spinnerTextValueField = FindViewById<EditText>(Resource.Id.editText);
            icon = FindViewById<ImageView>(Resource.Id.icon);

            icon.Click += (s, e) =>
            {
                ShowPopupDialog();
            };

        }

        private void ShowPopupDialog()
        {
            var builder = new AlertDialog.Builder(context);
            var view = LayoutInflater.From(context).Inflate(Resource.Layout.SearchableSpinner, null);

            var searchViewInDialog = view.FindViewById<SearchView>(Resource.Id.searchView);
            var listViewInDialog = view.FindViewById<ListView>(Resource.Id.listView);

            // Create an ArrayAdapter with the list of items
            var popupAdapter = new ArrayAdapter<string>(context, Android.Resource.Layout.SimpleListItem1, items);
            listViewInDialog.Adapter = popupAdapter;

            // Filter items based on search query
            searchViewInDialog.QueryTextChange += (s, e) =>
            {
                var filteredItems = string.IsNullOrEmpty(e.NewText)
                    ? items
                    : items.FindAll(item => item.ToLower().Contains(e.NewText.ToLower()));
                popupAdapter.Clear();
                popupAdapter.AddAll(filteredItems);
                popupAdapter.NotifyDataSetChanged();
            };

            // Create the dialog
            var dialog = builder.SetView(view)
                                .SetNegativeButton("Cancel", (s, e) => {
                                    // The dialog will be dismissed automatically
                                })
                                .Create();

            // Handle item selection
            listViewInDialog.ItemClick += (s, e) =>
            {
                selectedItem = popupAdapter.GetItem(e.Position);
                spinnerTextValueField.Text = selectedItem; // Update EditText with the selected item
                dialog.Dismiss(); // Close the dialog
            };

            // Show the dialog
            dialog.Show();
        }


        public void SetItems(List<string> items)
        {
            this.items = items;
        }

        public void ColorTheRepresentation(int colorChoice)
        {
           switch(colorChoice)
            {
                case 1:
                    spinnerTextValueField.SetBackgroundColor(Android.Graphics.Color.Aqua);
                    break;
            }
        }
    }
}
