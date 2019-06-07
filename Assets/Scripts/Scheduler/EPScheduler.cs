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
        public EPScheduler(Queue<Drone> drones)
        {
            DroneQueue = drones;
            JobQueue = new List<Job>();
            jobs = new NativeList<EPStruct>(Allocator.Persistent);
            precedence = new NativeList<float>(Allocator.Persistent);
        }
        NativeList<EPStruct> jobs;
        NativeList<float> precedence;
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

                    Scheduling.Complete();
                    for (int i = jobs.Length; i < JobQueue.Count; i++)
                    {
                        jobs.Add(new EPStruct { job = (StrippedJob)JobQueue[i] });
                    }
                    for (int i = precedence.Length; i < JobQueue.Count * JobQueue.Count; i++)
                    {
                        precedence.Add(0);
                    }
                    var num = jobs.Length;
                    var initializer = new EPInitializerJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        results = jobs
                    };
                    var initJob = initializer.Schedule(num, 4);
                    var calculator = new EPCalculatorJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        input = jobs,
                        ep = precedence
                    };
                    Scheduling = calculator.Schedule(num, 1, initJob);
                    yield return new WaitUntil(() => Scheduling.IsCompleted);
                    Scheduling.Complete();
                    var n = FindMax(ref calculator.ep);
                    var end = jobs.Length - 1;

                    if (drone.AssignJob((Job)jobs[n].job))
                    {
                        jobs.RemoveAtSwapBack(n);
                        JobQueue[n] = JobQueue[end];
                        JobQueue.RemoveAt(end);

                        var sq = end * end;
                        while (precedence.Length != sq) precedence.RemoveAtSwapBack(0);

                        SimManager.JobDequeued();
                    }

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

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }

    }
}
