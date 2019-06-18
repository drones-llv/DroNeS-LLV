using System;
using Drones.Objects;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.Event_System
{
    public class NoFlyZoneEntry : IEvent
    {
        public NoFlyZoneEntry(IDataSource obj, NoFlyZone nfz)
        {
            Type = EventType.EnteredNoFlyZone;
            OpenWindow = () => { if (ID == obj.ToString()) obj.OpenInfoWindow(); };
            ID = obj.ToString();
            Target = nfz.Position.ToArray();
            Time = TimeKeeper.Chronos.Get();
            Message = Time + " - " + ID + " entered illegal airspace " + nfz.Name;
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
