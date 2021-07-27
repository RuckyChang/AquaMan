using System;

namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class QuitGameFailedException: Exception
    {
        public QuitGameFailedException(string reason): base(reason)
        {

        }
    }
}
