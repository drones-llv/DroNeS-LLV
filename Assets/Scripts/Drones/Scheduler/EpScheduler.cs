using System.Collections;
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

        public int FailedInQueue { get; set; }
        private readonly Hub _owner;
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
                if (DroneQueue.Count <= 0 || JobQueue.Count <= 0 || TimeKeeper.TimeSpeed == TimeSpeed.Pause)
                    yield return wait;
                
                var drone = DroneQueue.Dequeue();
                if (drone.InPool) continue;
                
                Scheduling.Complete();
                for (var i = _jobs.Length; i < JobQueue.Count; i++)
                {
                    _jobs.Add(new EPStruct { job = (SchedulingData)JobQueue[i] });
                }
                for (var i = _precedence.Length; i < JobQueue.Count * JobQueue.Count; i++)
                {
                    _precedence.Add(0);
                }
                var num = _jobs.Length;
                var initializer = new EpInitializerJob
                {
                    Time = TimeKeeper.Chronos.Get(),
                    Results = _jobs
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


                var j = JobQueue[n];
                var b = j.Status == JobStatus.Failed;
                if (b || drone.AssignJob(j))
                {
                    if (b) DroneQueue.Enqueue(drone);
                    _jobs.RemoveAtSwapBack(n);
                    JobQueue.RemoveAtSwapBack(n);
                    
                    var sq = end * end;
                    while (_precedence.Length != sq) _precedence.RemoveAtSwapBack(0);
                        
                    _owner.JobDequeued(j.IsDelayed);
                }
                // Clean Queue
                for (var i = JobQueue.Count - 1; i >= 0 && FailedInQueue > 0; i--)
                {
                    if (JobQueue[i].Status != JobStatus.Failed) continue;
                    _owner.JobDequeued(JobQueue[i].IsDelayed);
                    JobQueue.RemoveAtSwapBack(i);
                    _jobs.RemoveAtSwapBack(i);
                    var x = _jobs.Length * _jobs.Length;
                    while (_precedence.Length != x) _precedence.RemoveAtSwapBack(0);
                    FailedInQueue--;
                }
                yield return null;
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
