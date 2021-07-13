namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        public enum EventType
        {
            LogedIn,
            Log
        }

        public class EventPayload
        {
            public class LoggedIn
            {
                public string Token { get; set; }
            }

            public class LoggedOut
            {
            
            }
        }
    }
}
