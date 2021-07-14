using System;

namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class ConnectionNotFoundException : WebsocketAdapterException
    {
        
        public ConnectionNotFoundException(Guid connectionId): base("connectionNotFound: "+ connectionId.ToString()) { }
    }
}
