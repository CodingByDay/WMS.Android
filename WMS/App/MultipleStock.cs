using Android.Content;

namespace WMS.App
{
    public class MultipleStock
    {
        public enum Showing
        {
            SSCC,
            Serial,
            Ordinary
        }
        public void ConfigurationMethod(Showing showing, Context context)
        {
            this.showing = showing;
            this.context = context;
        }

        public Showing showing = Showing.Ordinary;
        private Context context { get; set; }
        public string SSCC { get; set; }
        public string Serial { get; set; }
        public string Location { get; set; }
        public double Quantity { get; set; }



        public int GetResourceIdString(string resourceName)
        {
            int id = context.Resources.GetIdentifier(resourceName, "string", context.PackageName);
            // This method gets the corresponding id only for strings.
            return id;
        }


        public override string ToString()
        {
            if (this.showing == Showing.Ordinary)
            {
                return Location + " ( " + Quantity + " ) ";
            }
            else if (this.showing == Showing.SSCC)
            {
                return Location + " ( " + Quantity + " | " + SSCC + " ) ";

            }
            else if (this.showing == Showing.Serial)
            {
                return Location + " ( " + Quantity + " | " + Serial + " ) ";

            }
            else
            {
                return Location + " ( " + Quantity + " ) ";
            }
        }
    }



}
