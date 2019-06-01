using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;

namespace Drones.Utils.Scheduler
{
    public interface IScheduler
    {
        IEnumerator ProcessQueue();

        void Dispose();

        void Initialize();

        bool Started { get; }

        JobHandle Scheduling { get; }

        void Complete();

        Queue<Drone> DroneQueue { get; }

        List<StrippedJob> JobQueue { get; }

    }
}
