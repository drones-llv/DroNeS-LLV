using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

namespace Drones.Utils.Scheduler
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

    }
}
