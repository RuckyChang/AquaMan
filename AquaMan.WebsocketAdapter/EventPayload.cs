using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaMan.WebsocketAdapter
{
    public enum EventType
    {
        Error = -1,
        LoggedIn,
        LoggedOut,
        ListedGame,
        JoinedGame
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
