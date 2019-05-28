using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Managers
{
    using Utils;

    public class JobManager : MonoBehaviour
    {

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


        private Queue<Drone> _waitingList = new Queue<Drone>();
        private Queue<Job> _jobQueue = new Queue<Job>();

        private void OnDisable()
        {
            _Started = false;
        }

        private void OnEnable()
        {
            _Generator = new JobGenerator(Owner, Owner.JobGenerationRate);
            StartCoroutine(_Generator.Generate());
        }

        private bool _Started;

        private IEnumerator ProcessQueue()
        {
            _Started = true;
            var wait = new WaitUntil(() => (_waitingList.Count > 0) && _jobQueue.Count > 0 && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));
            while (true)
            {
                yield return wait;

                while (_waitingList.Count > 0 && _jobQueue.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone = _waitingList.Dequeue();
                    if (drone.InPool) continue;
                    drone.AssignJob(_jobQueue.Dequeue());
                    SimManager.JobDequeued();
                }
            }
        }

        public void AddToQueue(Drone drone)
        {
            if (!_Started)
            {
                StartCoroutine(ProcessQueue());
            }
            if (!_waitingList.Contains(drone))
            {
                _waitingList.Enqueue(drone);
            }
        }

        public void AddToQueue(Job job)
        {
            _jobQueue.Enqueue(job);
            SimManager.JobEnqueued();
        }

        public int JobQueueLength => _jobQueue.Count;

        public void LoadDroneQueue(List<uint> data)
        {
            _waitingList = new Queue<Drone>();
            foreach (var i in data)
            {
                AddToQueue((Drone)SimManager.AllDrones[i]);
            }
        }

        public void LoadJobQueue(List<uint> data)
        {
            _jobQueue = new Queue<Job>();
            foreach (var i in data)
            {
                _jobQueue.Enqueue((Job)SimManager.AllIncompleteJobs[i]);
            }
        }

        public List<uint> SerializeDrones()
        {
            var l = new List<uint>();
            foreach (var d in _waitingList)
            {
                l.Add(d.UID);
            }
            return l;
        }

        public List<uint> SerializeJobs()
        {
            var l = new List<uint>();
            foreach (var d in _jobQueue)
            {
                l.Add(d.UID);
            }
            return l;
        }
    }
}
