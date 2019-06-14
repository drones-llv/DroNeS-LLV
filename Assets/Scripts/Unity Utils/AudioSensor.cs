using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    using Managers;
    public class AudioSensor : MonoBehaviour
    {
        private bool _Active = false;

        private Drone _Drone;

        private int _inRadius;

        private TimeKeeper.Chronos _Time;

        private readonly WaitForSeconds _Wait = new WaitForSeconds(1 / 10f);

        public Drone AssignedDrone
        {
            get
            {
                if (_Drone == null)
                {
                    _Drone = transform.parent.GetComponent<Drone>();
                }
                return _Drone;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BuildingCollider") || 
                other.gameObject.layer == LayerMask.NameToLayer("TileCollider"))
            {
                _inRadius++;
                if (!_Active) StartCoroutine(StartTimer());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BuildingCollider") ||
                other.gameObject.layer == LayerMask.NameToLayer("TileCollider"))
            {
                _inRadius--;
                if (_inRadius <= 0)
                {
                    _inRadius = 0;
                    _Active = false;
                }
            }
        }

        private IEnumerator StartTimer()
        {
            _Active = true;
            if (_Time == null) _Time = TimeKeeper.Chronos.Get();
            yield return new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
            while (_Active)
            {
                _Time.Now();
                yield return _Wait;
                float dt = _Time.Timer();
                AssignedDrone.UpdateAudible(dt);
            }
            yield break;

        }

        public void SetSensorRadius(float radius)
        {
            transform.localScale = transform.worldToLocalMatrix * Vector3.one * radius;
        }

    }
}

