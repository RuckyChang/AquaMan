namespace AquaMan.WebsocketAdapter
{
    public enum EventType
    {
        Error = -1,
        LoggedIn,
        LoggedOut,
        JoinedGame
    }

    public class EventPayload
    {
        public class Error
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        #region lobby
        public class LoggedIn
        {
            public string Token { get; set; }
        }

        public class LoggedOut
        {

        }
        #endregion

        #region game
        public class JoinedGame
        {
            public string Name { get; set; }
            public int Slot { get; set; }
        }
        #endregion
    }
}
