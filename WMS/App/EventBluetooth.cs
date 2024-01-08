using WMS.App;

public class EventBluetooth
{
    /// <summary>
    ///  Tale objekt bo serializiran in poslan na AR očala v obliki json.
    /// </summary>
    ///

    public EventBluetooth()
    {
    }

    public enum EventType
    {
        TakeoverList, // Prevzem blaga seznam
        TakeOverPosition, // Prevzem pozicij
        IssuedList, // Izdaja seznam
        IssuedPosition // Izdaja pozicij
    }

    public string orderNumber { get; set; }
    public string clientName { get; set; }

    public Trail chosenPosition { get; set; } // Pozicije naročila, če je event = IssuedPosition || TakeOverPosition

    public List<Position> positions { get; set; } // Pozicije naročila, če je event = IssuedList || TakeOverList
    public EventType eventType { get; set; } // switch (eventType)
    public bool isRefreshCallback { get; set; } // Če je true ponovno naložiti positions array

    public class Position
    {
        public string Ident { get; set; }
        public string Location { get; set; }
        public string Qty { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }
}