using System;
using Drones.Data;
using Drones.JobSystem;
using Drones.Managers;
using Unity.Collections;
using UnityEngine;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Objects
{
    [Serializable]
    public class Battery
    {
        public static uint Count { get; private set; }
        private int _accessIndex;
        public static void Reset() => Count = 0;
        public Battery(Drone drone, Hub hub)
        {
            UID = ++Count;
            _data = new BatteryData(this);
            AssignDrone(drone);
            AssignHub(hub);
        }
        public Battery(Hub hub)
        {
            UID = ++Count;
            _data = new BatteryData(this);
            AssignDrone();
            AssignHub(hub);
        }

        #region Properties
        public string Name => $"B{UID:000000}";

        public BatteryStatus Status => _data.status;

        public float Charge => _data.charge / _data.capacity;

        public float Capacity => _data.capacity / BatteryData.DesignCapacity;
        #endregion

        public uint UID { get; }
        
        private BatteryData _data;

        private Hub GetHub()
        {
            return (Hub)SimManager.AllHubs[_data.hub];
        }

        public bool GetDrone(out Drone drone)
        {
            if (_data.drone == 0)
            {
                drone = null;
                return false;
            }
            drone = (Drone)SimManager.AllDrones[_data.drone];
            return true;
        }

        public bool HasDrone() => _data.drone != 0;

        public void AssignHub(Hub hub) => _data.hub = hub.UID;
        public void AssignDrone(Drone drone) => _data.drone = drone.UID;
        public void AssignDrone() => _data.drone = 0;
        public void SetStatus(BatteryStatus status)
        {
            _data.status = status;   
        }
        
        public void Destroy() => GetHub()?.DestroyBattery(this);
        public EnergyInfo GetEnergyInfo()
        {
            var info = new EnergyInfo();
            if (GetDrone(out var d))
            {
                d.GetEnergyInfo(ref info);
            }
            else 
            {
                info.pkgWgt = 0;
                info.moveType = DroneMovement.Idle;
            }
            
            info.charge = _data.charge;
            info.capacity = _data.capacity;
            info.totalCharge = _data.totalCharge;
            info.totalDischarge = _data.totalDischarge;
            info.cycles = _data.cycles;
            info.status = _data.status;
            info.chargeRate = BatteryData.DesignCapacity/3600f;
            info.designCycles = BatteryData.DesignCycles;
            info.designCapacity = BatteryData.DesignCapacity;
            info.chargeTarget = BatteryData.ChargeTarget;

            return info;
        }

        public void SetEnergyInfo(EnergyInfo info)
        {
            _data.charge = info.charge;
            _data.capacity = info.capacity;
            _data.totalCharge = info.totalCharge;
            _data.totalDischarge = info.totalDischarge;
            _data.cycles = info.cycles;
            if (info.stopCharge == 1)
            {
                GetHub()?.StopCharging(this);
            }
        }
    }

}