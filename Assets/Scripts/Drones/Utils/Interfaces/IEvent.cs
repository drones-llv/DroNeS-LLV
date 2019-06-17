using System;
using EventType = Utils.EventType;

namespace Drones.Utils.Interfaces
{
    public interface IEvent
    {
        EventType Type { get; }
        string ID { get; }
        float[] Target { get; }
        Action OpenWindow { get; }
        TimeKeeper.Chronos Time { get; }
        string Message { get; }
    }
}
