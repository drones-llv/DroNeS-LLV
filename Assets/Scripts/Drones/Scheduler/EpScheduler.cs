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
    public class EPScheduler : IScheduler
    {
        public EPScheduler(Queue<Drone> drones, Hub hub)
        {
            _owner = hub;
            DroneQueue = drones;
            JobQueue = new List<Job>();
            _jobs = new NativeList<EPStruct>(Allocator.Persistent);
            _precedence = new NativeList<float>(Allocator.Persistent);
        }

        private Hub _owner;
        private NativeList<EPStruct> _jobs;
        private NativeList<float> _precedence;
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

                    Scheduling.Complete();
                    for (var i = _jobs.Length; i < JobQueue.Count; i++)
                    {
                        _jobs.Add(new EPStruct { job = (StrippedJob)JobQueue[i] });
                    }
                    for (var i = _precedence.Length; i < JobQueue.Count * JobQueue.Count; i++)
                    {
                        _precedence.Add(0);
                    }
                    var num = _jobs.Length;
                    var initializer = new EpInitializerJob
                    {
                        time = TimeKeeper.Chronos.Get(),
                        results = _jobs
                    };
                    var initJob = initializer.Schedule(num, 4);
                    var calculator = new EpCalculatorJob
                    {
                        Time = TimeKeeper.Chronos.Get(),
                        Input = _jobs,
                        Ep = _precedence
                    };
                    Scheduling = calculator.Schedule(num, 1, initJob);
                    yield return new WaitUntil(() => Scheduling.IsCompleted);
                    Scheduling.Complete();
                    var n = FindMax(ref calculator.Ep);
                    var end = _jobs.Length - 1;

                    var j = (Job) _jobs[n].job;
                    if (drone.AssignJob(j))
                    {
                        _jobs.RemoveAtSwapBack(n);
                        JobQueue[n] = JobQueue[end];
                        JobQueue.RemoveAt(end);

                        var sq = end * end;
                        while (_precedence.Length != sq) _precedence.RemoveAtSwapBack(0);
                        
                        _owner.JobDequeued(j.IsDelayed);
                    }

                    yield return null;
                }
            }
        }

        private int FindMax(ref NativeArray<float> ep)
        {
            var maxVal = float.MinValue;
            var maxInt = 0;
            for (var i = 0; i < ep.Length; i++)
            {
                if (!(ep[i] < maxVal)) continue;
                maxVal = ep[i];
                maxInt = i;
            }
            return maxInt;
        }

        public void Dispose()
        {
            _jobs.Dispose();
            _precedence.Dispose();
        }

        public void Complete()
        {
            Scheduling.Complete();
            Dispose();
        }

    }
}
