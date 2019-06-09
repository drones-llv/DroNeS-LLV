using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
namespace Drones.Utils.Scheduler
{
    using Managers;

    public class LLVScheduler : IScheduler
    {
        NativeList<LLVStruct> jobs;
        NativeArray<float> loss;
        NativeArray<float> duration;
        NativeList<float> netlossvalue;
        public LLVScheduler(Queue<Drone> drones)
        {
            DroneQueue = drones;
            JobQueue = new List<Job>();
            jobs = new NativeList<LLVStruct>(Allocator.Persistent);
            loss = new NativeArray<float>(1, Allocator.Persistent);
            duration = new NativeArray<float>(1, Allocator.Persistent);
            netlossvalue = new NativeList<float>(Allocator.Persistent);
        }

        public JobHandle Scheduling { get; private set; }
        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<Job> JobQueue { get; set; }
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

                    for (int i = jobs.Length; i < JobQueue.Count; i++)
                    {
                        jobs.Add(new LLVStruct { job = (StrippedJob)JobQueue[i] });
                        netlossvalue.Add(0);
                    }


                    var num = jobs.Length;
                    var initializer = new LLVInitializerJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        results = jobs
                    };
                    var initJob = initializer.Schedule(num, 4);
                    var summer = new LLVSumJob
                    {
                        jobs = jobs,
                        totalLosses = loss,
                        totalDuration = duration
                    };
                    var sumJob = summer.Schedule(initJob);
                    var calculator = new LLVCalculatorJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        input = summer.jobs,
                        totalLosses = loss,
                        totalDuration = duration,
                        nlv = netlossvalue
                    };
                    Scheduling = calculator.Schedule(num, 4, sumJob);
                    yield return new WaitUntil(() => Scheduling.IsCompleted);
                    Scheduling.Complete();

                    var n = FindMin(ref calculator.nlv);
                    var end = jobs.Length - 1;

                    if (drone.AssignJob((Job)jobs[n].job))
                    {
                        jobs.RemoveAtSwapBack(n);
                        netlossvalue.RemoveAtSwapBack(n);

                        JobQueue[n] = JobQueue[end];
                        JobQueue.RemoveAt(end);
                        SimManager.JobDequeued();
                    }

                    yield return null;
                }
            }
        }

        int FindMin(ref NativeArray<float> nlv)
        {
            float minval = float.MaxValue;
            int minint = 0;
            for (int i = 0; i < nlv.Length; i++)
            {
                if (nlv[i] < minval)
                {
                    minval = nlv[i];
                    minint = i;
                }
            }
            return minint;
        }

        public void Dispose()
        {
            jobs.Dispose();
            loss.Dispose();
            duration.Dispose();
            netlossvalue.Dispose();
        }

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }
    }
}
