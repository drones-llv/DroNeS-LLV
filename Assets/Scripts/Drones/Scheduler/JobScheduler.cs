using System;
using System.Collections.Generic;
using Drones.JobSystem;
using Drones.Managers;
using Drones.Objects;
using Drones.Utils;
using UnityEngine;
using Utils;

namespace Drones.Scheduler
{
    public class JobScheduler : MonoBehaviour
    {
        const int STEPS = 200;
        public static Scheduling ALGORITHM { get; set; } = Scheduling.FCFS;
        [SerializeField]
        private Hub owner;
        private Hub Owner
        {
            get
            {
                if (owner == null) owner = GetComponent<Hub>();
                return owner;
            }
        }
        private JobGenerator _generator;
        private readonly Queue<Drone> _droneQueue = new Queue<Drone>();
        
        private IScheduler _algorithm;

        private void OnDisable()
        {
            _algorithm.Complete();
        }

        private void OnEnable()
        {
            _generator = new JobGenerator(Owner, Owner.JobGenerationRate);
            StartCoroutine(_generator.Generate());
            NewAlgorithm();
        }

        private void NewAlgorithm()
        {
            switch (ALGORITHM)
            {
                case Scheduling.EP:
                    _algorithm = new EPScheduler(_droneQueue, owner);
                    break;
                case Scheduling.LLV:
                    _algorithm = new LLVScheduler(_droneQueue, owner);
                    break;
                case Scheduling.FCFS:
                    _algorithm = new FcfsScheduler(_droneQueue, owner);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AddToQueue(Drone drone)
        {
            if (!_algorithm.Started)
            {
                StartCoroutine(_algorithm.ProcessQueue());
            }
            if (!_droneQueue.Contains(drone))
            {
                _droneQueue.Enqueue(drone);
            }
        }

        public void AddToQueue(Job job)
        {
            _algorithm.JobQueue.Add(job);
            owner.JobEnqueued();
        }

        public int JobQueueLength => _algorithm.JobQueue.Count;

        private static float Normal(float z, float mu, float sigma)
        {
            return 1 / (Mathf.Sqrt(2 * Mathf.PI) * sigma) * Mathf.Exp(-Mathf.Pow(z - mu, 2) / (2 * sigma * sigma));
        }

        public static float ExpectedValue(in SchedulingData j, in TimeKeeper.Chronos time)
        {
            var mu = j.ExpectedDuration;
            var std = j.StDevDuration;
            
            var h = (4 * std + mu) / STEPS;
            var expected = CostFunction.Evaluate(j.Cost, time) * Normal(0, mu, std) / 2;
            expected += CostFunction.Evaluate(j.Cost,time + 4 * std) * Normal(4 * std, mu, std) / 2;
            for (var i = 1; i < STEPS; i++)
            {
                expected += CostFunction.Evaluate(j.Cost,time + i * h) * Normal(i * h, mu, std);
            }
            expected *= h;

            return expected;
        }

        public void FailedInQueue() => _algorithm.FailedInQueue++;
    }
}
