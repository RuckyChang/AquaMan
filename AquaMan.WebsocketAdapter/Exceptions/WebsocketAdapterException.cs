using System;

namespace AquaMan.WebsocketAdapter.Exceptions
{
    public abstract class WebsocketAdapterException: Exception
    {
        public WebsocketAdapterException(string message): base(message) { }
    }
}
