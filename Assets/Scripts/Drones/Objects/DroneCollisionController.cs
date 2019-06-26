using System.Collections;
using Drones.Managers;
using Drones.Utils;
using UnityEngine;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Objects
{
    public class DroneCollisionController : MonoBehaviour
    {
        [SerializeField]
        private Drone owner;
        [SerializeField]
        private TrailRenderer trail;
        private DeploymentPath Descent => DroneHub.DronePath;
        private Hub _hub;
        private Hub DroneHub
        {
            get
            {
                if (_hub == null) _hub = owner.GetHub();
                return _hub;
            }
        }

        private bool _collisionOn;
        public bool InHub => !_collisionOn && Vector3.Distance(DroneHub.Position, transform.position) < 0.5f;

        private void Awake()
        {
            if (owner == null) owner = GetComponent<Drone>();
            if (trail == null) trail = GetComponent<TrailRenderer>();
        }

        private void OnEnable()
        {
            trail.enabled = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            var otherLayer = other.gameObject.layer;
            
            if (otherLayer == LayerMask.NameToLayer("IgnoreCollision")) return;
            
            if (other.CompareTag("Drone")) return;

            if (otherLayer == LayerMask.NameToLayer("Hub"))
            {
                _collisionOn = other.GetComponent<DeploymentPath>() != Descent
                                && other.GetComponent<Hub>() != DroneHub;
            }

            if (!_collisionOn) return;
            DroneManager.MovementJobHandle.Complete();
            Collide(other);

        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            _collisionOn |= (other.gameObject.layer == LayerMask.NameToLayer("Hub")
                && (other.GetComponent<DeploymentPath>() == Descent || other.GetComponent<Hub>() == DroneHub));
        }

        private void Collide(Collider other)
        {
            owner.GetHub().UpdateCrashCount();
            owner.GetJob()?.FailJob();
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();
            Explosion.New(transform.position);
            var dd = new RetiredDrone(owner, other);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            owner.Delete();
        }
    }

}
