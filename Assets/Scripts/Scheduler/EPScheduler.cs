using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

namespace Drones.Utils.Scheduler
{
    using Managers;

    public class EPScheduler : IScheduler
    {
        public EPScheduler(Queue<Drone> drones, List<StrippedJob> jobs)
        {
            DroneQueue = drones;
            JobQueue = jobs;
        }
        NativeArray<EPStruct> jobs;
        NativeArray<float> precedence;
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

                    Initialize();
                    var initializer = new EPInitializerJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        results = jobs
                    };
                    var initJob = initializer.Schedule(JobQueue.Count, 4);
                    var calculator = new EPCalculatorJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        input = jobs,
                        ep = precedence
                    };
                    Scheduling = calculator.Schedule(JobQueue.Count, 1, initJob);
                    yield return new WaitUntil(() => Scheduling.IsCompleted);
                    Scheduling.Complete();
                    var n = FindMax(ref calculator.ep);
                    Dispose();

                    drone.AssignJob((Job)JobQueue[n]);
                    JobQueue.RemoveAt(n);
                    SimManager.JobDequeued();
                    yield return null;
                }
            }
        }

        int FindMax(ref NativeArray<float> ep)
        {
            float maxval = float.MinValue;
            int maxint = 0;
            for (int i = 0; i < ep.Length; i++)
            {
                if (ep[i] < maxval)
                {
                    maxval = ep[i];
                    maxint = i;
                }
            }
            return maxint;
        }

        public void Dispose()
        {
            jobs.Dispose();
            precedence.Dispose();
        }

        public void Initialize()
        {
            jobs = new NativeArray<EPStruct>(JobQueue.Count, Allocator.TempJob);
            for (int i = 0; i < JobQueue.Count; i++) 
            {
                jobs[i] = new EPStruct
                {
                    job = JobQueue[i]
                };
            }

            precedence = new NativeArray<float>(JobQueue.Count * JobQueue.Count, Allocator.TempJob);
        }

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }

    }
}
