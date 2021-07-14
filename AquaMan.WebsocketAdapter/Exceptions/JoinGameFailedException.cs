namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class JoinGameFailedException: WebsocketAdapterException
    {
        public JoinGameFailedException(string message): base(message) { }
    }
}
