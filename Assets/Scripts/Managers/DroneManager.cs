using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones.Managers
{
    using DataStreamer;
    using Utils.Jobs;
    using Utils;

    public class DroneManager : MonoBehaviour
    {
        public static JobHandle MovementJobHandle => _Instance._movementJobHandle;
        private static DroneManager _Instance;
        public static DroneManager New()
        {
            _Instance = new GameObject("DroneManager").AddComponent<DroneManager>();
            return _Instance;
        }

        private JobHandle _movementJobHandle = new JobHandle();
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private TransformAccessArray _Transforms;
        private NativeArray<MovementInfo> _MovementInfoArray;

        private void OnDisable()
        {
            MovementJobHandle.Complete();
            if (_Transforms.isCreated) _Transforms.Dispose();
            if (_MovementInfoArray.IsCreated) _MovementInfoArray.Dispose();
            _Instance = null;
        }

        private void Initialise()
        {
            _Transforms = new TransformAccessArray(0);
            _MovementInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);
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
            MovementJob _movementJob = new MovementJob();
            while (true)
            {
                if (_Transforms.length == 0) yield return null;
                int j = 0;

                foreach (Drone drone in Drones.Values)
                {
                    drone.PreviousPosition = drone.transform.position;
                    _MovementInfoArray[j] = drone.GetMovementInfo(_MovementInfoArray[j]);
                    j++;
                }

                _movementJob.nextMove = _MovementInfoArray;
                _movementJob.deltaTime = _time.Timer();
                _time.Now();

                _movementJobHandle = _movementJob.Schedule(_Transforms);

                yield return new WaitUntil(() => MovementJobHandle.IsCompleted);
                MovementJobHandle.Complete();
                yield return null;
            }
        }

        public void OnDroneCountChange()
        {
            _movementJobHandle.Complete();
            _Transforms.Dispose();
            _Transforms = new TransformAccessArray(0);
            foreach (Drone drone in Drones.Values)
            {
                _Transforms.Add(drone.transform);
            }
            _MovementInfoArray.Dispose();
            _MovementInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);

            int j = 0;
            foreach (Drone drone in Drones.Values)
            {
                _MovementInfoArray[j] = new MovementInfo();
                _MovementInfoArray[j] = drone.GetMovementInfo(_MovementInfoArray[j]);
                j++;
            }

        }

    }


}
