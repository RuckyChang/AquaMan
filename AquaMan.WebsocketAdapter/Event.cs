namespace AquaMan.WebsocketAdapter
{
    public class Event
    {
        public int EventType { get; set; }
    }

    public class Event<TPayload>
    {
        public int EventType { get; set; }
        public TPayload? Payload { get; set; }
    }
}
