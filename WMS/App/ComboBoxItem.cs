using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Android.Widget;

namespace TrendNET.WMS.Device.App
{
    [Serializable]
    public class ComboBoxItem : ISerializable
    {
        public string ID { get; set; }
        public string Text { get; set; }

        public ComboBoxItem() { }

        protected ComboBoxItem(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ID = info.GetString("ID");
            Text = info.GetString("Text");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("ID", ID);
            info.AddValue("Text", Text);
        }

        public override string ToString()
        {
            return Text;
        }

        public static void Select(Spinner important, List<ComboBoxItem> obj, string id)
        {
            for (int i = 0; i < obj.Count; i++)
            {
                if (obj[i].ID == id)
                {
                    int selected = (int)important.SelectedItemId;
                    if (selected != i)
                    {
                        important.SetSelection(i, true);
                    }
                    return;
                }
            }
            if (important.SelectedItemId != -1)
            {
                important.SetSelection(-1);
            }
        }




        public static void SelectAutocomplete(CustomAutoCompleteTextView important, List<ComboBoxItem> obj, string id)
        {
            for (int i = 0; i < obj.Count; i++)
            {
                if (obj[i].ID == id)
                {
                    important.Text = id;
                }
            }
        }



    }
}
