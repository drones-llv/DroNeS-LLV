using System.Collections;
using Drones.JobSystem;
using Drones.Objects;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Utils;

namespace Drones.Managers
{
    public class DroneManager : MonoBehaviour
    {
        public static JobHandle MovementJobHandle => _instance._movementJobHandle;
        private static DroneManager _instance;
        public static DroneManager New()
        {
            _instance = new GameObject("DroneManager").AddComponent<DroneManager>();
            return _instance;
        }
        private JobHandle _movementJobHandle;
        private TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private TransformAccessArray _transforms;
        private NativeArray<DroneMovementInfo> _movementInfoArray;

        private void OnDisable()
        {
            MovementJobHandle.Complete();
            _transforms.Dispose();
             _movementInfoArray.Dispose();
            _instance = null;
        }

        private void Initialise()
        {
            _transforms = new TransformAccessArray(0);
            _movementInfoArray = new NativeArray<DroneMovementInfo>(_transforms.length, Allocator.Persistent);
            _time.Now();
        }

        private void Start()
        {
            Drones.SetChanged += (obj) => OnDroneCountChange();
            Initialise();
            StartCoroutine(Operate());
        }

        private IEnumerator Operate()
        {
            var movementJob = new DroneMovementJob();
            while (true)
            {
                if (_transforms.length == 0) yield return null;
                var j = 0;

                foreach (var dataSource in Drones.Values)
                {
                    var drone = (Drone) dataSource;
                    drone.PreviousPosition = drone.transform.position;
                    _movementInfoArray[j] = drone.GetMovementInfo();
                    j++;
                }

                movementJob.NextMove = _movementInfoArray;
                movementJob.DeltaTime = _time.Timer();
                _time.Now();

                _movementJobHandle = movementJob.Schedule(_transforms);

                yield return null;
                _movementJobHandle.Complete();
            }
        }

        private void OnDroneCountChange()
        {
            _movementJobHandle.Complete();
            _transforms.Dispose();
            _transforms = new TransformAccessArray(0);
            foreach (var dataSource in Drones.Values)
            {
                var drone = (Drone) dataSource;
                _transforms.Add(drone.transform);
            }
            _movementInfoArray.Dispose();
            _movementInfoArray = new NativeArray<DroneMovementInfo>(_transforms.length, Allocator.Persistent);

            var j = 0;
            foreach (var dataSource in Drones.Values)
            {
                var drone = (Drone) dataSource;
                _movementInfoArray[j] = drone.GetMovementInfo();
                j++;
            }

        }
    }


}
