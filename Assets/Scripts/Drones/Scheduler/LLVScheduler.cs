﻿using System.Collections;
using System.Collections.Generic;
using Drones.Managers;
using Drones.Objects;
using Drones.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utils;

namespace Drones.Scheduler
{
    public class LLVScheduler : IScheduler
    {
        private Hub _owner;
        private NativeList<LLVStruct> _jobs;
        private NativeArray<float> _loss;
        private NativeArray<float> _duration;
        private NativeList<float> _netLossValue;
        public LLVScheduler(Queue<Drone> drones, Hub owner)
        {
            _owner = owner;
            DroneQueue = drones;
            JobQueue = new List<DeliveryJob>();
            _jobs = new NativeList<LLVStruct>(Allocator.Persistent);
            _loss = new NativeArray<float>(1, Allocator.Persistent);
            _duration = new NativeArray<float>(1, Allocator.Persistent);
            _netLossValue = new NativeList<float>(Allocator.Persistent);
        }

        public JobHandle Scheduling { get; private set; }
        public bool Started { get; private set; }
        public Queue<Drone> DroneQueue { get; }
        public List<DeliveryJob> JobQueue { get; set; }
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

                    for (var i = _jobs.Length; i < JobQueue.Count; i++)
                    {
                        _jobs.Add(new LLVStruct { job = (StrippedJob)JobQueue[i] });
                        _netLossValue.Add(0);
                    }


                    var num = _jobs.Length;
                    var initializer = new LLVInitializerJob
                    {
                        Time = TimeKeeper.Chronos.Get(),
                        Results = _jobs
                    };
                    var initJob = initializer.Schedule(num, 4);
                    var summer = new LLVSumJob
                    {
                        jobs = _jobs,
                        totalLosses = _loss,
                        totalDuration = _duration
                    };
                    var sumJob = summer.Schedule(initJob);
                    var calculator = new LLVCalculatorJob
                    {
                        time = TimeKeeper.Chronos.Get(),
                        input = summer.jobs,
                        totalLosses = _loss,
                        totalDuration = _duration,
                        nlv = _netLossValue
                    };
                    Scheduling = calculator.Schedule(num, 4, sumJob);
                    yield return new WaitUntil(() => Scheduling.IsCompleted);
                    Scheduling.Complete();

                    var n = FindMin(ref calculator.nlv);
                    var end = _jobs.Length - 1;
                    var j = (DeliveryJob) _jobs[n].job;
                    if (drone.AssignJob(j))
                    {
                        _jobs.RemoveAtSwapBack(n);
                        _netLossValue.RemoveAtSwapBack(n);

                        JobQueue[n] = JobQueue[end];
                        JobQueue.RemoveAt(end);
                        _owner.JobDequeued(j.IsDelayed);
                    }

                    yield return null;
                }
            }
        }

        private int FindMin(ref NativeArray<float> nlv)
        {
            var minVal = float.MaxValue;
            var minInt = 0;
            for (var i = 0; i < nlv.Length; i++)
            {
                if (!(nlv[i] < minVal)) continue;
                minVal = nlv[i];
                minInt = i;
            }
            return minInt;
        }

        public void Dispose()
        {
            _jobs.Dispose();
            _loss.Dispose();
            _duration.Dispose();
            _netLossValue.Dispose();
        }

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }
    }
}
