﻿using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    using Managers;
    public class DroneCollisionController : MonoBehaviour
    {
        [SerializeField]
        private Drone _Owner;
        [SerializeField]
        private TrailRenderer _Trail;

        private bool CollisionOn => !InHub;
        public bool InHub => _Owner.GetHub().Collider.bounds.Contains(transform.position);

        void Awake()
        {
            if (_Owner == null) _Owner = GetComponent<Drone>();
            if (_Trail == null) _Trail = GetComponent<TrailRenderer>();
        }

        private void OnEnable()
        {
            _Trail.enabled = true;
            StartCoroutine(Gravity());
        }

        private IEnumerator Gravity()
        {
            var battery = _Owner.GetBattery();
            while (true)
            {
                if (_Owner.Movement != DroneMovement.Idle && battery != null && battery.Status == BatteryStatus.Dead)
                {
                    _Trail.enabled = false;
                    _Owner.Drop();
                    yield break;
                }
                yield return null;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (CollisionOn)
            {
                DroneManager.MovementJobHandle.Complete();
                Collide(other);
            }
        }

        private void Collide(Collider other)
        {
            _Owner.GetHub().UpdateCrashCount();
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();

            Explosion.New(transform.position);
            var dd = new RetiredDrone(_Owner, other);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            _Owner.Delete();
        }
    }

}
