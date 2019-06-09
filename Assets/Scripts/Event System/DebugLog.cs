using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.EventSystem
{
    using UI;
    using Utils;
    public class DebugLog : IEvent
    {
        private DebugLog(object msg)
        {
            Message = msg.ToString();
            ConsoleLog.WriteToConsole(this);
        }

        public static void New(object msg) => new DebugLog(msg);

        public EventType Type => EventType.DebugLog;

        public string ID => null;

        public float[] Target => null;

        public Action OpenWindow => null;

        public TimeKeeper.Chronos Time => null;

        public string Message { get; }

    }
}
