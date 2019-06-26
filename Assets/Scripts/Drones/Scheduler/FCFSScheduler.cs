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
    public class FcfsScheduler : IScheduler
    {
        private readonly Hub _owner;
        public FcfsScheduler(Queue<Drone> drones, Hub owner)
        {
            _owner = owner;
            DroneQueue = drones;
            JobQueue = new List<Job>();
        }
        public int FailedInQueue { get; set; }
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
                    var drone = DroneQueue.Dequeue();
                    if (drone.InPool) continue;
                    var j = JobQueue[0];
                    if (j.Status == JobStatus.Failed || drone.AssignJob(j))
                    {
                        _owner.JobDequeued(j.IsDelayed);
                        JobQueue.RemoveAt(0);
                    }

                    for (var i = JobQueue.Count - 1; i >= 0 && FailedInQueue > 0; i--)
                    {
                        if (JobQueue[i].Status != JobStatus.Failed) continue;
                        _owner.JobDequeued(JobQueue[i].IsDelayed);
                        JobQueue.RemoveAt(i);
                        FailedInQueue--;
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
