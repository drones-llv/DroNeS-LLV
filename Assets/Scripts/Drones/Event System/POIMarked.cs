using System;
using Drones.Utils;
using Drones.Utils.Interfaces;
using EventType = Utils.EventType;

namespace Drones.Event_System
{
    public class POIMarked : IEvent
    {
        public POIMarked(string name, float[] target)
        {
            Type = EventType.POIMarked;
            OpenWindow = () => { };
            ID = name;
            Target = target;
            Time = TimeKeeper.Chronos.Get();
            Message = Time.ToString() + " - POI " + ID + " Marked";
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
