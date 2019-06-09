using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    public class DeploymentPath : MonoBehaviour
    {
        [SerializeField]
        private HubCollisionController _controller;
        [SerializeField]
        private Hub _Owner;
        [SerializeField]
        private Collider _Collider;
        public Collider Collider
        {
            get
            {
                if (_Collider == null) _Collider = GetComponent<Collider>();
                return _Collider;
            }
        }

        private HubCollisionController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller = transform.parent.GetComponent<HubCollisionController>();
                }
                return _controller;
            }
        }

        private Hub Owner
        {
            get
            {
                if (_Owner == null)
                {
                    _Owner = transform.parent.GetComponent<Hub>();
                }
                return _Owner;
            }
        }

        public float PERIOD = 2.0f;
        private static int AltitudeCompare(Drone a, Drone b)
        {
            if (a.Waypoint.y < b.Waypoint.y) return -1;
            return 1;
        }

        private readonly MinHeap<Drone> _deploymentQueue = new MinHeap<Drone>(AltitudeCompare);
        private readonly HashSet<uint> _inQueue = new HashSet<uint>();

        public void AddToDeploymentQueue(Drone drone)
        {
            if (!_Started) StartCoroutine(DeployDrone());
            if (!_inQueue.Contains(drone.UID))
            {
                _deploymentQueue.Add(drone);
                _inQueue.Add(drone.UID);

            }
        }
        private bool _Started;
        public void Stop()
        {
            _Started = false;
            StopCoroutine(DeployDrone());
        }

        private void OnEnable() => StartCoroutine(DeployDrone());

        private void OnDisable() => _deploymentQueue.Clear();

        private int _Intersects;

        public Vector3 Direction { get; private set; }

        public bool IsClear => _Intersects == 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 12)
            {
                _Intersects++;
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
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 12)
            {
                _Intersects--;
                if (_Intersects == 0)
                    Direction = Vector3.zero;
            }
        }

        public IEnumerator DeployDrone()
        {
            if (_Started) yield break;
            var time = TimeKeeper.Chronos.Get();
            Drone outgoing;
            _Started = true;
            while (true)
            {
                time.Now();
                while (time.Timer() < PERIOD) yield return null;
                if (IsClear && _deploymentQueue.Count > 0)
                {
                    outgoing = _deploymentQueue.Remove();
                    _inQueue.Remove(outgoing.UID);
                    if (outgoing.InPool) continue;

                    Owner.DeployDrone(outgoing);
                }
            }
        }

    }
}
