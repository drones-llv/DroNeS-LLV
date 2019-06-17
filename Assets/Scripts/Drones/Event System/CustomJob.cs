using System;
using Drones.Managers;
using Drones.Objects;
using Drones.Serializable;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.Event_System
{
    public class CustomJob : IEvent
    {
        public CustomJob(SJob job)
        {
            ID = job.uid.ToString();
            Message = Time + " - " + job.custom;
            Server = (Drone)SimManager.AllDrones[job.droneUID];
            OpenWindow = delegate {
                var j = SimManager.AllJobs[uint.Parse(ID)];
                j?.OpenInfoWindow();

                if (!Server.InPool)
                {
                    AbstractCamera.Followee = Server.gameObject;
                }
            };
        }

        public EventType Type => EventType.CustomJob;

        public string ID { get; }

        public float[] Target => null;

        public Action OpenWindow { get; }

        public TimeKeeper.Chronos Time => TimeKeeper.Chronos.Get().SetReadOnly();

        public string Message { get; }

        public Drone Server { get; }

    }
}
