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

        private JobHandle _movementJobHandle = new JobHandle();
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private TransformAccessArray _transforms;
        private NativeArray<MovementInfo> _movementInfoArray;

        private void OnDisable()
        {
            MovementJobHandle.Complete();
            if (_transforms.isCreated) _transforms.Dispose();
            if (_movementInfoArray.IsCreated) _movementInfoArray.Dispose();
            _instance = null;
        }

        private void Initialise()
        {
            _transforms = new TransformAccessArray(0);
            _movementInfoArray = new NativeArray<MovementInfo>(_transforms.length, Allocator.Persistent);
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
            var movementJob = new MovementJob();
            while (true)
            {
                if (_transforms.length == 0) yield return null;
                var j = 0;

                foreach (var dataSource in Drones.Values)
                {
                    var drone = (Drone) dataSource;
                    drone.PreviousPosition = drone.transform.position;
                    _movementInfoArray[j] = drone.GetMovementInfo(_movementInfoArray[j]);
                    j++;
                }

                movementJob.nextMove = _movementInfoArray;
                movementJob.deltaTime = _time.Timer();
                _time.Now();

                _movementJobHandle = movementJob.Schedule(_transforms);

                yield return null;
                MovementJobHandle.Complete();
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
            _movementInfoArray = new NativeArray<MovementInfo>(_transforms.length, Allocator.Persistent);

            var j = 0;
            foreach (var dataSource in Drones.Values)
            {
                var drone = (Drone) dataSource;
                _movementInfoArray[j] = new MovementInfo();
                _movementInfoArray[j] = drone.GetMovementInfo(_movementInfoArray[j]);
                j++;
            }

        }

        public static void ForceDroneCountChange()
        {
            _instance._movementJobHandle.Complete();
            _instance._transforms.Dispose();
            _instance._transforms = new TransformAccessArray(0);
            foreach (var dataSource in Drones.Values)
            {
                var drone = (Drone) dataSource;
                _instance._transforms.Add(drone.transform);
            }
            _instance._movementInfoArray.Dispose();
            _instance._movementInfoArray = new NativeArray<MovementInfo>(_instance._transforms.length, Allocator.Persistent);

            var j = 0;
            foreach (var dataSource in Drones.Values)
            {
                var drone = (Drone) dataSource;
                _instance._movementInfoArray[j] = new MovementInfo();
                _instance._movementInfoArray[j] = drone.GetMovementInfo(_instance._movementInfoArray[j]);
                j++;
            }

        }

    }


}
