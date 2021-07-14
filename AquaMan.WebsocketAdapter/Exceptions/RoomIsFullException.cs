namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class RoomIsFullException:WebsocketAdapterException
    {
        public RoomIsFullException(string message): base(message) { }
    }
}
