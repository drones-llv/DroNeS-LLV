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
        NativeArray<LLVStruct> jobs;
        NativeArray<float> loss;
        NativeArray<float> duration;
        NativeArray<float> lossvalue;
        public LLVScheduler(Queue<Drone> drones, List<StrippedJob> jobs)
        {
            DroneQueue = drones;
            JobQueue = jobs;
        }

        public JobHandle Scheduling { get; private set; }
        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<StrippedJob> JobQueue { get; }
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
                    var initializer = new LLVInitializerJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        results = jobs,
                        totalLosses = loss,
                        totalDuration = duration,
                    };
                    var initJob = initializer.Schedule(jobs.Length, 4);
                    var calculator = new LLVCalculatorJob
                    {
                        time = (ChronoWrapper)TimeKeeper.Chronos.Get(),
                        input = jobs,
                        totalLosses = loss,
                        totalDuration = duration,
                        nlv = lossvalue
                    };
                    var calcjob = calculator.Schedule(jobs.Length, 4, initJob);
                    yield return new WaitUntil(() => calcjob.IsCompleted);
                    calcjob.Complete();
                    var n = FindMin(ref calculator.nlv);
                    Dispose();

                    drone.AssignJob((Job)JobQueue[n]);
                    JobQueue.RemoveAt(n);
                    SimManager.JobDequeued();
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
            lossvalue.Dispose();
        }

        public void Initialize()
        {
            jobs = new NativeArray<LLVStruct>(JobQueue.Count, Allocator.TempJob);
            for (int i = 0; i < JobQueue.Count; i++)
            {
                jobs[i] = new LLVStruct
                {
                    job = JobQueue[i]
                };
            }
            loss = new NativeArray<float>(1, Allocator.TempJob);
            duration = new NativeArray<float>(1, Allocator.TempJob);
            lossvalue = new NativeArray<float>(JobQueue.Count, Allocator.TempJob);
        }

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }

    }
}
