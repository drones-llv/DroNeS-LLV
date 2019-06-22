using System.Collections;
using Drones.Data;
using Drones.JobSystem;
using Drones.Objects;
using Drones.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utils;

namespace Drones.Managers
{
    public class BatteryManager : MonoBehaviour
    {
        public static JobHandle EnergyJobHandle => _instance._energyJobHandle;

        private static BatteryManager _instance;
        public static BatteryManager New()
        {
            _instance = new GameObject("BatteryManager").AddComponent<BatteryManager>();
            return _instance;
        }
        
        private JobHandle _energyJobHandle;
        private TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, Battery> Batteries => SimManager.AllBatteries;
        public static NativeList<BatteryData> BatteryInfo;
        private NativeHashMap<uint, DroneInfo> _droneInfo;
        private NativeQueue<uint> _dronesToDrop;
        private NativeQueue<uint> _chargingInHub;

        private void OnDisable()
        {
            EnergyJobHandle.Complete();
            BatteryInfo.Dispose();
            _droneInfo.Dispose();
            _dronesToDrop.Dispose();
            _chargingInHub.Dispose();
            _instance = null;
        }

        private void Start()
        {
            Batteries.ItemRemoved += OnRemove;
            BatteryInfo = new NativeList<BatteryData>(Allocator.Persistent);
            _droneInfo = new NativeHashMap<uint, DroneInfo>(SimManager.AllDrones.Count, Allocator.Persistent);
            _dronesToDrop = new NativeQueue<uint>(Allocator.Persistent);
            _chargingInHub = new NativeQueue<uint>(Allocator.Persistent);
            _time.Now();
            StartCoroutine(Operate());
        }
        
        private IEnumerator Operate()
        {
            var energyJob = new EnergyJob();
            while (true)
            {
                if (Batteries.Count == 0) yield return null;
                
                for (var j = 0; j < BatteryInfo.Length; j++)
                {
                    var dE = BatteryInfo[j].DeltaEnergy;
                    if (SimManager.AllBatteries[BatteryInfo[j].UID].GetDrone(out var d))
                    {
                        d.UpdateEnergy(dE);
                    }
                }
                UpdateDroneInfo();
                DropDeadDrones();
                UpdateHub();
                energyJob.Energies = BatteryInfo;
                energyJob.DronesToDrop = _dronesToDrop.ToConcurrent();
                energyJob.DroneInfo = _droneInfo;
                energyJob.ChargingInHub = _chargingInHub.ToConcurrent();
                energyJob.DeltaTime = _time.Timer();
                _time.Now();

                _energyJobHandle = energyJob.Schedule(BatteryInfo.Length, 32);
                yield return null;
                _energyJobHandle.Complete();
            }
        }

        private void DropDeadDrones()
        {
            while (_dronesToDrop.Count > 0)
            {
                if (SimManager.AllDrones.TryGet(_dronesToDrop.Dequeue(), out var d))
                {
                    ((Drone)d).Drop();
                }
            }
        }

        private void UpdateDroneInfo()
        {
            _droneInfo.Clear();
            foreach (var dataSource in SimManager.AllDrones.Values)
            {
                var drone = (Drone) dataSource;
                drone.UpdateMovement(ref _droneInfo);
            }
        }

        private void UpdateHub()
        {
            foreach (var dataSource in SimManager.AllHubs.Values)
            {
                var hub = (Hub) dataSource;
                hub.ResetChargingBatteryCount();
            }

            while (_chargingInHub.Count > 0)
            {
                ((Hub)SimManager.AllHubs[_chargingInHub.Dequeue()]).IncrementChargingBattery();
            }
        }

        private void OnRemove(Battery removed)
        {
            _energyJobHandle.Complete();
            _droneInfo.Dispose();
            _droneInfo = new NativeHashMap<uint, DroneInfo>(SimManager.AllDrones.Count, Allocator.Persistent);
            Battery.DeleteData(removed);
            _time.Now();
        }
    }


}
