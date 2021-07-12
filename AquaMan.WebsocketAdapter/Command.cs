namespace AquaMan.WebsocketAdapter
{
    public class Command
    {
        public int CommandType { get; set; }
    }

    public class Command<TPayload>
    {
        public int CommandType { get; set; }

        public TPayload? Payload { get; set; }
    }
}
