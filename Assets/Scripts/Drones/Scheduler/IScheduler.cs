using System.Collections;
using System.Collections.Generic;
using Drones.Objects;
using Unity.Jobs;

namespace Drones.Scheduler
{
    public interface IScheduler
    {
        IEnumerator ProcessQueue();

        void Dispose();

        bool Started { get; }

        JobHandle Scheduling { get; }

        void Complete();

        Queue<Drone> DroneQueue { get; }

        List<Job> JobQueue { get; set; }

        int FailedInQueue { get; set; }

    }
}
