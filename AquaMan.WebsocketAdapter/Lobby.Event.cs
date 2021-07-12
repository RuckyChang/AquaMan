namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        public enum EventType
        {
            LogedIn
        }

        public class LogedIn
        {
            public string Token { get; set; }
        }
    }
}
