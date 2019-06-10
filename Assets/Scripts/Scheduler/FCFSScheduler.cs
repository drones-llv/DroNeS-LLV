using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

namespace Drones.Utils.Scheduler
{
    using Managers;


    public class FCFSScheduler : IScheduler
    {
        public FCFSScheduler(Queue<Drone> drones)
        {
            DroneQueue = drones;
            JobQueue = new List<Job>();
        }
        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<Job> JobQueue { get; set; }
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

                    if (drone.AssignJob(JobQueue[0]))
                    {
                        JobQueue.RemoveAt(0);
                        SimManager.JobDequeued();
                    }
                    yield return null;
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
