using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Managers
{
    using Utils;
    using Utils.Router;

    public class RouteManager : MonoBehaviour
    {
        private static RouteManager Instance { get; set; }

        private Queue<Drone> _waitingList = new Queue<Drone>();

        private bool _Started;

        private Pathfinder _Router;

        private static bool Started
        {
            get => Instance._Started;
            set => Instance._Started = value;
        }

        private void Awake()
        {
            Instance = this;
            _Router = new Raypath();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private IEnumerator ProcessQueue()
        {
            Started = true;
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

                drone.NavigateWaypoints(_Router.GetRoute(origin, destination, job == null));

                if (TimeKeeper.DeltaFrame() > 12) yield return null;
            }
        }

        public static void AddToQueue(Drone drone)
        {
            if (!Started)
            {
                Instance.StartCoroutine(Instance.ProcessQueue());
            }
            if (!Instance._waitingList.Contains(drone))
            {
                Instance._waitingList.Enqueue(drone);
            }
        }

        public static void LoadQueue(List<uint> data)
        {
            Instance._waitingList = new Queue<Drone>();
            foreach (var i in data)
            {
                AddToQueue((Drone)SimManager.AllDrones[i]);
            }
        }

        public static List<uint> Serialize()
        {
            var l = new List<uint>();
            foreach (var d in Instance._waitingList)
            {
                l.Add(d.UID);
            }
            return l;
        }

    }
}
