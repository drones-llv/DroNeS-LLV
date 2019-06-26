using System.Collections;
using Drones.Objects;
using UnityEngine;
using Utils;

namespace Drones.Utils
{
    public class AudioSensor : MonoBehaviour
    {
        private static readonly WaitUntil _WaitForUnpause = new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
        private bool _active = false;

        private Drone _drone;

        private int _inRadius;

        private TimeKeeper.Chronos _time;

        private readonly WaitForSeconds _wait = new WaitForSeconds(1 / 10f);

        private Drone AssignedDrone
        {
            get
            {
                if (_drone == null)
                {
                    _drone = transform.parent.GetComponent<Drone>();
                }
                return _drone;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("BuildingCollider") &&
                other.gameObject.layer != LayerMask.NameToLayer("TileCollider")) return;
            _inRadius++;
            if (!_active) StartCoroutine(StartTimer());
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("BuildingCollider") &&
                other.gameObject.layer != LayerMask.NameToLayer("TileCollider")) return;
            _inRadius--;
            if (_inRadius > 0) return;
            _inRadius = 0;
            _active = false;
        }

        private IEnumerator StartTimer()
        {
            _active = true;
            _time = TimeKeeper.Chronos.Get();
            yield return _WaitForUnpause;
            while (_active)
            {
                _time.Now();
                yield return _wait;
                var dt = _time.Timer();
                AssignedDrone.UpdateAudible(dt);
            }

        }

        public void SetSensorRadius(float radius)
        {
            var t = transform;
            t.localScale = t.worldToLocalMatrix * Vector3.one * radius;
        }

    }
}

