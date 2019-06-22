using System;
using Drones.Data;
using Drones.JobSystem;
using Drones.Managers;
using Drones.Serializable;
using Unity.Collections;
using UnityEngine;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Objects
{
    [Serializable]
    public class Battery
    {
        private static NativeHashMap<uint, BatteryData> _allData;
        public Battery(Drone drone, Hub hub)
        {
            var data = new BatteryData(this);
            _allData.TryAdd(data.UID, data);
            UID = data.UID;
            if (drone is null) AssignDrone(); 
            else AssignDrone(drone);
            AssignHub(hub);
        }
        public Battery(SBattery data)
        {
            _allData.TryAdd(data.uid, new BatteryData(data));
            UID = data.uid;
        }

        #region Properties
        public string Name => $"B{UID:000000}";

        public BatteryStatus Status => Data.status;

        public float Charge => Data.charge / Data.capacity;

        public float Capacity => Data.capacity / BatteryData.DesignCapacity;
        #endregion

        public uint UID { get; }
        private BatteryData Data => _allData[UID];

        public Hub GetHub()
        {
            return (Hub)SimManager.AllHubs[_allData[UID].hub];
        } 
        public Drone GetDrone() => (Drone)SimManager.AllDrones[Data.drone];
        public bool GetDrone(out Drone drone)
        {
            if (Data.drone == 0)
            {
                drone = null;
                return false;
            }
            drone = (Drone)SimManager.AllDrones[Data.drone];
            return true;
        }

        public void AssignHub(Hub hub)
        {
            // Complete Job here
            var tmp = _allData[UID]; 
            tmp.hub = hub.UID;
            _allData.Remove(UID);
            _allData.TryAdd(UID, tmp);
        }
        public void AssignDrone(Drone drone)
        {
            var tmp = _allData[UID]; 
            tmp.drone = drone.UID;
            _allData.Remove(UID);
            _allData.TryAdd(UID, tmp);
        } 
        public void AssignDrone()
        {
            var tmp = _allData[UID]; 
            tmp.drone = 0;
            _allData.Remove(UID);
            _allData.TryAdd(UID, tmp);
        }
        public void SetStatus(BatteryStatus status)
        {
            Data.status = status;   
        }
        public void Destroy() => GetHub()?.DestroyBattery(this);
        public EnergyInfo GetEnergyInfo()
        {
            var info = new EnergyInfo();
            if (Data.drone != 0)
            {
                GetDrone().GetEnergyInfo(ref info);
            }
            else 
            {
                info.pkgWgt = 0;
                info.moveType = DroneMovement.Idle;
            }
            info.charge = Data.charge;
            info.capacity = Data.capacity;
            info.totalCharge = Data.totalCharge;
            info.totalDischarge = Data.totalDischarge;
            info.cycles = Data.cycles;
            info.status = Data.status;
            info.chargeRate = BatteryData.DesignCapacity/3600f;
            info.designCycles = BatteryData.DesignCycles;
            info.designCapacity = BatteryData.DesignCapacity;
            info.chargeTarget = BatteryData.ChargeTarget;

            return info;
        }

        public void SetEnergyInfo(EnergyInfo info)
        {
            Data.charge = info.charge;
            Data.capacity = info.capacity;
            Data.totalCharge = info.totalCharge;
            Data.totalDischarge = info.totalDischarge;
            Data.cycles = info.cycles;
            if (info.stopCharge == 1)
            {
                GetHub()?.StopCharging(this);
            }
        }

        public SBattery Serialize() => new SBattery(Data);
    }

}