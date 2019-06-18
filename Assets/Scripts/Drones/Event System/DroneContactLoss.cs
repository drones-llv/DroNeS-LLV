using System;
using Drones.Objects;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.Event_System
{
    public class DroneContactLoss : IEvent
    {
        public DroneContactLoss(Drone drone)
        {
            var rDrone = new RetiredDrone(drone);
            Type = EventType.DroneContactLoss;
            OpenWindow = rDrone.OpenInfoWindow;
            ID = rDrone.Name;
            Target = rDrone.Location.ToArray();
            Time = TimeKeeper.Chronos.Get();
            Message = Time + " - " + ID + " contact lost";
            //drone.DestroySelf(null);
            drone.Delete();
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
