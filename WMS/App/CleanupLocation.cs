namespace WMS.App
{
    public class CleanupLocation
    {
        public string Name { get; set; }
        public string Ident { get; set; }
        public string Location { get; set; }
        public string SSCC { get; set; }
        public string Serial { get; set; }


        public CleanupLocation(string Name, string Ident, string Location, string SSCC, string Serial)
        {
            this.Name = Name;
            this.Ident = Ident;
            this.Location = Location;
            this.SSCC = SSCC;
            this.Serial = Serial;
        }
    }
}