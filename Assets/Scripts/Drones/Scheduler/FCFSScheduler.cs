using System.Collections;
using System.Collections.Generic;
using Drones.Managers;
using Drones.Objects;
using Drones.Utils;
using Unity.Jobs;
using UnityEngine;
using Utils;

namespace Drones.Scheduler
{
    public class FCFSScheduler : IScheduler
    {
        private Hub _owner;

        public FCFSScheduler(Queue<Drone> drones, Hub owner)
        {
            _owner = owner;
            DroneQueue = drones;
            JobQueue = new List<DeliveryJob>();
        }
        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<DeliveryJob> JobQueue { get; set; }
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
                    var drone = DroneQueue.Dequeue();
                    if (drone.InPool) continue;
                    
                    if (drone.AssignJob(JobQueue[0]))
                    {
                        _owner.JobDequeued(JobQueue[0].IsDelayed);
                        JobQueue.RemoveAt(0);
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
