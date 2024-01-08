namespace WMS.App
{
    internal class MorePallets
    {
        public string SSCC { get; set; }

        public string Ident { get; set; }

        public string Name { get; set; }

        public string Quantity { get; set; }

        public string Serial { get; set; }

        public string Location { get; set; }

        public string friendlySSCC { get; set; }

        public string key { get; set; }
        public int no { get; set; }

        public MorePallets()
        {
        }

        public MorePallets(string SSCC, string Ident, string Name, string Quantity, string Serial, string friendlySSCC)
        {
            this.SSCC = SSCC;
            this.Ident = Ident;
            this.Name = Name;
            this.Quantity = Quantity;
            this.Serial = Serial;
            this.friendlySSCC = friendlySSCC;
        }
    }
}