namespace WMS.App
{
    public class OpenOrder
    {
        public string Ident { get; set; }
        public string? Order { get; set; }
        public int? Position { get; set; }
        public string? Client { get; set; }
        public DateTime? Date { get; set; }
        public double? Quantity { get; set; }
        public double? Packaging { get; set; }

    }
}
