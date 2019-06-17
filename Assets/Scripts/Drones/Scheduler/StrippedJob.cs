using Drones.Managers;
using Drones.Objects;
using UnityEngine;

namespace Drones.Scheduler
{
    public struct StrippedJob
    {
        public uint UID;
        public Vector3 pickup;
        public Vector3 dropoff;
        public ChronoWrapper start;
        public float reward;
        public float penalty;
        public float expectedDuration;
        public float stDevDuration;

        public static explicit operator Job(StrippedJob job) => SimManager.AllJobs[job.UID];
    }
}
