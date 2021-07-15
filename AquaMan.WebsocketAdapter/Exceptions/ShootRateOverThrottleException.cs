using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaMan.WebsocketAdapter.Exceptions
{
    public class ShootRateOverThrottleException:WebsocketAdapterException
    {
        public ShootRateOverThrottleException(Guid id): base($@"guid: {id}, shoot too fast") { }
    }
}
