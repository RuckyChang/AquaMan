namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class NoSuchTokenException: WebsocketAdapterException
    {
        public NoSuchTokenException(string token) : base($"Account not found: {token}"){}
    }
}
