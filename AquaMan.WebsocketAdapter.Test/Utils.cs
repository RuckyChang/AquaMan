using Newtonsoft.Json;

namespace AquaMan.WebsocketAdapter.Test
{
    public class Utils
    {
        public static int ParseEventType(string message)
        {
            var eventPkg = JsonConvert.DeserializeObject<Event>(message);

            return eventPkg.EventType;
        }
    }
}
