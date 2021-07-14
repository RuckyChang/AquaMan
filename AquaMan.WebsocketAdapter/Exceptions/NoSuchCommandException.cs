
namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class NoSuchCommandException : WebsocketAdapterException
    {
        public NoSuchCommandException(int command) : base($"Command not defined: {command}") { }
    }
}
