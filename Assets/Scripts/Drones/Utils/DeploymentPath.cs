using System.Collections;
using System.Collections.Generic;
using Drones.Objects;
using UnityEngine;
using Utils;

namespace Drones.Utils
{
    public class DeploymentPath : MonoBehaviour
    {
        [SerializeField]
        private HubCollisionController controller;
        [SerializeField]
        private Hub owner;
        [SerializeField]
        private Collider pathCollider;
        public Collider PathCollider
        {
            get
            {
                if (pathCollider == null) pathCollider = GetComponent<Collider>();
                return pathCollider;
            }
        }

        private HubCollisionController Controller
        {
            get
            {
                if (controller == null)
                {
                    controller = transform.parent.GetComponent<HubCollisionController>();
                }
                return controller;
            }
        }

        private Hub Owner
        {
            get
            {
                if (owner == null)
                {
                    owner = transform.parent.GetComponent<Hub>();
                }
                return owner;
            }
        }

        public float period = 0.1f;
        private static int AltitudeCompare(Drone a, Drone b)
        {
            if (a.Waypoint.y < b.Waypoint.y) return -1;
            return 1;
        }

        private readonly MinHeap<Drone> _deploymentQueue = new MinHeap<Drone>(AltitudeCompare);
        private readonly HashSet<uint> _inQueue = new HashSet<uint>();

        public void AddToDeploymentQueue(Drone drone)
        {
            if (!_started) StartCoroutine(DeployDrone());
            if (_inQueue.Contains(drone.UID)) return;
            _deploymentQueue.Add(drone);
            _inQueue.Add(drone.UID);
        }
        private bool _started;
        public void Stop()
        {
            _started = false;
            StopCoroutine(DeployDrone());
        }

        private void OnEnable() => StartCoroutine(DeployDrone());

        private void OnDisable() => _deploymentQueue.Clear();

        private int _intersects;

        public Vector3 Direction { get; private set; }

        public bool IsClear => _intersects == 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 12) return;
            _intersects++;
            Vector3 v;
            if (Vector3.Distance(transform.position, other.transform.position) < 0.1f)
            {
                v = Random.insideUnitSphere;
                v.y = 0;
                transform.parent.position += v;
            }
            v = (transform.position - other.transform.position).normalized;
            v.y = 0;
            Direction += v;
            StartCoroutine(Controller.Reposition(Vector3.zero));
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != 12) return;
            _intersects--;
            if (_intersects == 0)
                Direction = Vector3.zero;
        }

        public IEnumerator DeployDrone()
        {
            if (_started) yield break;
            var time = TimeKeeper.Chronos.Get();
            _started = true;
            while (true)
            {
                time.Now();
                while (time.Timer() < period) yield return null;
                while (!IsClear || _deploymentQueue.Count <= 0) yield return null;
                var outgoing = _deploymentQueue.Remove();
                _inQueue.Remove(outgoing.UID);
                if (outgoing.InPool) continue;

                Owner.DeployDrone(outgoing);
            }
        }

    }
}
