using Drones.Managers;
using Drones.Objects;
using Drones.Utils;
using UnityEngine;

namespace Drones.Scheduler
{
    public struct SchedulingData
    {
        public uint UID;
        public CostFunction Cost;
        public float ExpectedDuration;
        public float StDevDuration;

        public static explicit operator Job(SchedulingData job) => SimManager.AllJobs[job.UID];
    }
}
