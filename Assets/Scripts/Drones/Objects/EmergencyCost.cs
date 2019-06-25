using Drones.Scheduler;
using Drones.Utils;
using UnityEngine;

namespace Drones.Objects
{
    public struct EmergencyCost
    {
        public EmergencyCost(TimeKeeper.Chronos startTime, bool isShort)
        {
            Start = startTime;
            Guarantee = isShort ? 7 * 60 : 18 * 60;
        }

        private TimeKeeper.Chronos Start { get; }
        public float Guarantee { get; }

        public float Evaluate(TimeKeeper.Chronos complete) => 1 - (complete - Start) / Guarantee;

        public TimeKeeper.Chronos Inverse(float value) => Start + (1 - value) * Guarantee;
    }
}