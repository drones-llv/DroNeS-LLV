using System.Collections;
using Drones.Data;
using Drones.JobSystem;
using Drones.Objects;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utils;

namespace Drones.Managers
{
    public class BatteryManager : MonoBehaviour
    {
        public static JobHandle ConsumptionJobHandle => _instance._consumptionJobHandle;
        public static JobHandle ChargeCountJobHandle => _instance._chargeCountJobHandle;

        private static BatteryManager _instance;
        public static BatteryManager New()
        {
            _instance = new GameObject("BatteryManager").AddComponent<BatteryManager>();
            return _instance;
        }
        
        private JobHandle _consumptionJobHandle;
        private JobHandle _chargeCountJobHandle;
        private TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, Battery> Batteries => SimManager.AllBatteries;
        private static SecureSortedSet<uint, IDataSource> Hubs => SimManager.AllHubs;
        private NativeHashMap<uint, BusyDroneData> _droneInfo;
        private NativeQueue<uint> _dronesToDrop;

        private void OnDisable()
        {
//            EnergyJobHandle.Complete();
            ChargeCountJobHandle.Complete();
            Hub.ChargingBatteryCounts.Dispose();
            Battery.AllData.Dispose();
            _droneInfo.Dispose();
            _dronesToDrop.Dispose();
            _instance = null;
        }

        private void Start()
        {
            Batteries.ItemRemoved += OnBatteryRemove;
            Hubs.ItemRemoved += OnHubRemove;
            Battery.AllData = new NativeList<BatteryData>(Allocator.Persistent);
            Hub.ChargingBatteryCounts = new NativeList<ChargeCount>(Allocator.Persistent);
            _droneInfo = new NativeHashMap<uint, BusyDroneData>(SimManager.AllDrones.Count, Allocator.Persistent);
            _dronesToDrop = new NativeQueue<uint>(Allocator.Persistent);
            _time.Now();
            StartCoroutine(Operate());
        }
        
        private IEnumerator Operate()
        {
            var energyJob = new EnergyConsumptionJob();
            var countingJob = new ChargingCounterJob();
            while (true)
            {
                if (Batteries.Count == 0) yield return null;
                
                for (var j = 0; j < Battery.AllData.Length; j++)
                {
                    var dE = Battery.AllData[j].DeltaEnergy;
                    if (SimManager.AllBatteries[Battery.AllData[j].UID].GetDrone(out var d))
                    {
                        d.UpdateEnergy(dE);
                    }
                }
                UpdateDroneInfo();
                energyJob.Energies = Battery.AllData;
                energyJob.DronesToDrop = _dronesToDrop.ToConcurrent();
                energyJob.DroneInfo = _droneInfo;
                energyJob.DeltaTime = _time.Timer();
                countingJob.HubData = Hub.ChargingBatteryCounts;
                countingJob.BatteryData = Battery.AllData;
                _time.Now();

                _consumptionJobHandle = energyJob.Schedule(Battery.AllData.Length, 32);
                _chargeCountJobHandle =
                    countingJob.Schedule(Hub.ChargingBatteryCounts.Length, 1, _consumptionJobHandle);
                yield return null;
                _chargeCountJobHandle.Complete();
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
            while (_dronesToDrop.Count > 0)
            {
                if (SimManager.AllDrones.TryGet(_dronesToDrop.Dequeue(), out var d))
                {
                    ((Drone)d).Drop();
                }
            }
        }

        private void OnBatteryRemove(Battery removed)
        {
            _chargeCountJobHandle.Complete();
            _droneInfo.Dispose();
            _droneInfo = new NativeHashMap<uint, BusyDroneData>(SimManager.AllDrones.Count, Allocator.Persistent);
            Battery.DeleteData(removed);
            _time.Now();
        }

        private void OnHubRemove(IDataSource removed)
        {
            _chargeCountJobHandle.Complete();
            Hub.DeleteData((Hub)removed);
        }
    }


}
