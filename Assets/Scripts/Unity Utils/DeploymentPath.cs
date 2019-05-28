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

        public const float PERIOD = 0.75f;
        private readonly Queue<Drone> _deploymentQueue = new Queue<Drone>();

        public void AddToDeploymentQueue(Drone drone) => _deploymentQueue.Enqueue(drone);

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
            var time = TimeKeeper.Chronos.Get();
            WaitUntil _DroneReady = new WaitUntil(() => time.Timer() > PERIOD);
            Drone outgoing = null;
            while (true)
            {
                if (!IsClear) yield return null;
                if (_deploymentQueue.Count > 0)
                {
                    outgoing = _deploymentQueue.Dequeue();
                    if (outgoing.InPool) continue;

                    Owner.DeployDrone(outgoing);
                    outgoing.GetBattery().SetStatus(BatteryStatus.Discharge);
                    outgoing.Deploy();
                }
                yield return _DroneReady;
                time.Now();
            }
        }

    }
}
