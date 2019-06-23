﻿using System;
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
        private Queue<Drone> _droneQueue = new Queue<Drone>();
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
                    _algorithm = new FCFSScheduler(_droneQueue, owner);
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

        public void LoadDroneQueue(IEnumerable<uint> data)
        {
            _droneQueue = new Queue<Drone>();
            foreach (var i in data) AddToQueue((Drone)SimManager.AllDrones[i]);
        }

        public List<uint> SerializeDrones()
        {
            var l = new List<uint>();
            foreach (var d in _droneQueue)
                l.Add(d.UID);
            return l;
        }

        public void LoadJobQueue(IEnumerable<uint> data)
        {
            _algorithm.Complete();
            NewAlgorithm();
            foreach (var i in data)
                _algorithm.JobQueue.Add(SimManager.AllJobs[i]);
            StartCoroutine(_algorithm.ProcessQueue());
        }

        public List<uint> SerializeJobs()
        {
            var l = new List<uint>();
            foreach (var job in _algorithm.JobQueue)
            {
                l.Add(job.UID);
            }

            return l;
        }

        public static float EuclideanDist(StrippedJob job) => (job.pickup - job.dropoff).magnitude;

        public static float ManhattanDist(StrippedJob job)
        {
            var v = job.pickup - job.dropoff;
            return Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
        }

        public static float Normal(float z, float mu, float stdev)
        {
            return 1 / (Mathf.Sqrt(2 * Mathf.PI) * stdev) * Mathf.Exp(-Mathf.Pow(z - mu, 2) / (2 * stdev * stdev));
        }

        public static float ExpectedValue(StrippedJob j, TimeKeeper.Chronos time)
        {
            var mu = j.expectedDuration;
            var stdev = j.stDevDuration;

            var h = (4 * stdev + mu) / STEPS;
            var expected = CostFunction.Evaluate(j, time) * Normal(0, mu, stdev) / 2;
            expected += CostFunction.Evaluate(j, time + 4 * stdev) * Normal(4 * stdev, mu, stdev) / 2;
            for (var i = 1; i < STEPS; i++)
            {
                expected += CostFunction.Evaluate(j, time + i * h) * Normal(i * h, mu, stdev);
            }
            expected *= h;

            return expected;
        }

        public static float ExpectedDuration(StrippedJob job) => (ManhattanDist(job) + EuclideanDist(job)) / (2 * DroneMovementJob.HSPEED);
    }
}
