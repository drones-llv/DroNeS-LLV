using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Scheduler
{
    using Utils.Jobs;
    using Utils;
    using Managers;

    public class JobScheduler : MonoBehaviour
    {
        const int STEPS = 200;
        public static Scheduling ALGORITHM { get; set; } = Scheduling.FCFS;
        [SerializeField]
        private Hub _Owner;
        private Hub Owner
        {
            get
            {
                if (_Owner == null) _Owner = GetComponent<Hub>();
                return _Owner;
            }
        }
        private JobGenerator _Generator;
        private Queue<Drone> _droneQueue = new Queue<Drone>();
        private List<StrippedJob> _jobQueue = new List<StrippedJob>();
        private IScheduler _algorithm;

        private void OnDisable()
        {
            _algorithm.Scheduling.Complete();
        }

        private void OnEnable()
        {
            _Generator = new JobGenerator(Owner, Owner.JobGenerationRate);
            StartCoroutine(_Generator.Generate());
            switch (ALGORITHM)
            {
                case Scheduling.EP:
                    _algorithm = new EPScheduler(_droneQueue, _jobQueue);
                    break;
                case Scheduling.LLV:
                    _algorithm = new LLVScheduler(_droneQueue, _jobQueue);
                    break;
                default:
                    _algorithm = new FCFSScheduler(_droneQueue, _jobQueue);
                    break;
            }
        }

        public void AddToQueue(Drone drone)
        {
            if (!_algorithm.Started)
            {
                StartCoroutine(_algorithm.ProcessQueue());
            }
            if (drone != null && !_droneQueue.Contains(drone))
            {
                _droneQueue.Enqueue(drone);
            }
        }

        public void AddToQueue(Job job)
        {
            _jobQueue.Add((StrippedJob)job);
            SimManager.JobEnqueued();
        }

        public int JobQueueLength => _algorithm.JobQueue.Count;

        public void LoadDroneQueue(List<uint> data)
        {
            _droneQueue = new Queue<Drone>();
            foreach (var i in data) AddToQueue((Drone)SimManager.AllDrones[i]);
        }

        public void LoadJobQueue(List<uint> data)
        {
            _jobQueue = new List<StrippedJob>();
            foreach (var i in data)
                _jobQueue.Add((StrippedJob)SimManager.AllJobs[i]);
        }

        public List<uint> SerializeDrones()
        {
            var l = new List<uint>();
            foreach (var d in _droneQueue)
                l.Add(d.UID);
            return l;
        }

        public List<uint> SerializeJobs()
        {
            var l = new List<uint>();
            foreach (var d in _jobQueue)
                l.Add(d.UID);
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

        public static float ExpectedValue(StrippedJob j, ChronoWrapper time)
        {
            var man = ManhattanDist(j);
            var euc = EuclideanDist(j);

            var mu = (man + euc) / 2 / MovementJob.HSPEED;
            var stdev = (mu - euc) / MovementJob.HSPEED;

            var h = (4 * stdev + mu) / STEPS;
            float expected = CostFunction.Evaluate(j, time) * Normal(0, mu, stdev) / 2;
            expected += CostFunction.Evaluate(j, time + 4 * stdev) * Normal(4 * stdev, mu, stdev) / 2;
            for (int i = 1; i < STEPS; i++)
            {
                expected += CostFunction.Evaluate(j, time + i * h) * Normal(i * h, mu, stdev);
            }
            expected *= h;

            return expected;
        }

        public static float ExpectedDuration(StrippedJob job) => (ManhattanDist(job) + EuclideanDist(job)) / (2 * MovementJob.HSPEED);


    }
}
