namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        public enum EventType
        {
            Error = -1,
            LoggedIn,
            LoggedOut
        }

        public class EventPayload
        {
            public class Error
            {
                public string ErrorCode { get; set; }
            }
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
