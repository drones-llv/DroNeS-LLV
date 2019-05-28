using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Managers
{
    using Utils;
    using Utils.Router;

    public class RouteManager : MonoBehaviour
    {

        private Queue<Drone> _waitingList = new Queue<Drone>();

        private bool _Started;

        private Pathfinder _Router;

        private void Awake()
        {
            _Router = new Raypath();
        }

        private void OnDisable()
        {
            _Started = false;
        }

        private IEnumerator ProcessQueue()
        {
            _Started = true;
            while (true)
            {
                yield return new WaitUntil(() => (_waitingList.Count > 0) && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));

                Drone drone = _waitingList.Dequeue();

                if (drone.InPool) continue;

                var job = drone.GetJob();
                var destination =
                    job == null ? drone.GetHub().Position :
                    job.Status == JobStatus.Pickup ? job.Pickup :
                    job.Status == JobStatus.Delivering ? job.DropOff :
                    drone.GetHub().Position;

                var origin = drone.transform.position;
                var l = _Router.GetRoute(origin, destination, job == null || job.Status == JobStatus.Pickup);
                drone.NavigateWaypoints(l);
                foreach (var i in l) Debug.Log(i);

                if (TimeKeeper.DeltaFrame() > 12) yield return null;
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

        public void LoadQueue(List<uint> data)
        {
            _waitingList = new Queue<Drone>();
            foreach (var i in data)
            {
                AddToQueue((Drone)SimManager.AllDrones[i]);
            }
        }

        public List<uint> Serialize()
        {
            var l = new List<uint>();
            foreach (var d in _waitingList)
            {
                l.Add(d.UID);
            }
            return l;
        }

    }
}
