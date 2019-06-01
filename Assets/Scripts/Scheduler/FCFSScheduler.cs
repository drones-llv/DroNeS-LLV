using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Drones.Utils.Scheduler
{
    using Managers;

    public class FCFSScheduler : IScheduler
    {
        public FCFSScheduler(Queue<Drone> drones, List<StrippedJob> jobs)
        {
            DroneQueue = drones;
            JobQueue = jobs;
        }

        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<StrippedJob> JobQueue { get; }
        public JobHandle Scheduling { get; private set; }
        public IEnumerator ProcessQueue()
        {
            Started = true;
            var wait = new WaitUntil(() => (DroneQueue.Count > 0) && JobQueue.Count > 0 && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));
            while (true)
            {
                yield return wait;
                while (DroneQueue.Count > 0 && JobQueue.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone = DroneQueue.Dequeue();
                    if (drone.InPool) continue;

                    drone.AssignJob(SimManager.AllJobs[JobQueue[0].UID]);
                    JobQueue.RemoveAt(0);
                    SimManager.JobDequeued();
                }
            }
        }

        public void Dispose()
        {
            return;
        }

        public void Initialize()
        {
            return;
        }

        public void Complete()
        {
            return;
        }
    }
}
