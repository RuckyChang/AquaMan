using System;

namespace AquaMan.Domain.GameEvent
{
    public abstract class Event
    {
        public string ID { get; }
        public DateTime OccurredAt { get; }
        public Event()
        {
            Guid obj = new Guid();
            ID = obj.ToString();

            OccurredAt = DateTime.Now;
        }
    }
}
