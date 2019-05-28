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
        public static JobHandle MovementJobHandle => Instance._movementJobHandle;
        public static JobHandle EnergyJobHandle => Instance._energyJobHandle;
        private static DroneManager Instance { get; set; }
        private JobHandle _movementJobHandle = new JobHandle();
        private JobHandle _energyJobHandle = new JobHandle();
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private TransformAccessArray _Transforms;
        private NativeArray<MovementInfo> _MovementInfoArray;
        private NativeArray<EnergyInfo> _EnergyInfoArray;
        private NativeArray<Vector3> _PreviousPositions;

        private void OnDisable()
        {
            MovementJobHandle.Complete();
            EnergyJobHandle.Complete();
            if (_Transforms.isCreated) _Transforms.Dispose();
            if (_MovementInfoArray.IsCreated) _MovementInfoArray.Dispose();
            if (_EnergyInfoArray.IsCreated) _EnergyInfoArray.Dispose();
            if (_PreviousPositions.IsCreated) _PreviousPositions.Dispose();
        }

        private void Initialise()
        {
            _Transforms = new TransformAccessArray(0);
            _MovementInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);
            _EnergyInfoArray = new NativeArray<EnergyInfo>(_Transforms.length, Allocator.Persistent);
            _PreviousPositions = new NativeArray<Vector3>(_Transforms.length, Allocator.Persistent);
            _time.Now();
        }

        private IEnumerator Start()
        {
            Instance = this;
            yield return new WaitUntil(() => SimManager.Instance != null && SimManager.Initialized);
            Drones.SetChanged += (obj) => OnDroneCountChange();
            Initialise();
            StartCoroutine(Operate());
        }

        private IEnumerator Operate()
        {
            MovementJob _movementJob = new MovementJob();
            EnergyJob _energyJob = new EnergyJob();
            while (true)
            {
                if (_Transforms.length == 0) yield return null;
                int j = 0;

                foreach (Drone drone in Drones.Values)
                {
                    var dE = _EnergyInfoArray[j].energy;
                    drone.UpdateEnergy(dE);
                    drone.GetBattery().DischargeBattery(dE);

                    _PreviousPositions[j] = drone.PreviousPosition;
                    drone.PreviousPosition = drone.transform.position;
                   
                    _MovementInfoArray[j] = drone.GetMovementInfo(_MovementInfoArray[j]);
                    _EnergyInfoArray[j] = drone.GetEnergyInfo(_EnergyInfoArray[j]);
                    j++;
                }

                _movementJob.nextMove = _MovementInfoArray;
                _movementJob.rt_dt = _PreviousPositions;
                _movementJob.deltaTime = _time.Timer();

                _energyJob.energies = _EnergyInfoArray;
                _energyJob.deltaTime = _time.Timer();
                _time.Now();

                _movementJobHandle = _movementJob.Schedule(_Transforms);
                _energyJobHandle = _energyJob.Schedule(_Transforms.length, 1);

                yield return null;
                EnergyJobHandle.Complete();
                MovementJobHandle.Complete();
            }
        }

        public void OnDroneCountChange()
        {
            _movementJobHandle.Complete();
            _energyJobHandle.Complete();
            _Transforms.Dispose();
            _Transforms = new TransformAccessArray(0);
            foreach (Drone drone in Drones.Values)
            {
                _Transforms.Add(drone.transform);
            }
            _MovementInfoArray.Dispose();
            _MovementInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);
            _EnergyInfoArray.Dispose();
            _EnergyInfoArray = new NativeArray<EnergyInfo>(_Transforms.length, Allocator.Persistent);
            _PreviousPositions.Dispose();
            _PreviousPositions = new NativeArray<Vector3>(_Transforms.length, Allocator.Persistent);
            int j = 0;
            foreach (Drone drone in Drones.Values)
            {
                _PreviousPositions[j] = drone.PreviousPosition;

                _MovementInfoArray[j] = new MovementInfo();
                _MovementInfoArray[j] = drone.GetMovementInfo(_MovementInfoArray[j]);
                _EnergyInfoArray[j] = new EnergyInfo();
                _EnergyInfoArray[j] = drone.GetEnergyInfo(_EnergyInfoArray[j]);
                j++;
            }

        }

    }


}
