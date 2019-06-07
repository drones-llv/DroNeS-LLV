using System;
using System.Collections;
using UnityEngine;

namespace Drones
{
    using Drones.Data;
    using Drones.Managers;
    using Drones.Serializable;
    using Drones.Utils;
    using Drones.Utils.Jobs;

    [Serializable]
    public class Battery
    {
        private readonly static WaitForSeconds _Wait = new WaitForSeconds(1 / 30f);
        public static uint Count { get; private set; }
        public static void Reset() => Count = 0;
        public static int designCycles = 500;
        public static float designCapacity = 576000f; // 576,000 Coulombs = 160,000 mAh
        public static float chargeTarget = 1;

        public Battery(Drone drone, Hub hub)
        {
            _Data = new BatteryData();
            AssignDrone(drone);
            AssignHub(hub);
        }

        public Battery(SBattery data)
        {
            _Data = new BatteryData(data);
        }

        #region Properties
        public string Name => "B" + UID.ToString("000000");

        public BatteryStatus Status => _Data.status;

        public float Charge => _Data.charge / _Data.capacity;

        public float Capacity => _Data.capacity / BatteryData.designCapacity;
        #endregion

        public uint UID => _Data.UID;
        private readonly BatteryData _Data;

        public Hub GetHub() => (Hub)SimManager.AllHubs[_Data.hub];
        public Drone GetDrone() => (Drone)SimManager.AllDrones[_Data.drone];
        public void AssignHub(Hub hub) => _Data.hub = hub.UID;
        public void AssignDrone(Drone drone) => _Data.drone = (drone == null) ? 0 : drone.UID;
        public void Destroy() => GetHub()?.DestroyBattery(this);
        public void SetStatus(BatteryStatus status) => _Data.status = status;

        public EnergyInfo GetEnergyInfo(EnergyInfo info)
        {
            var d = GetDrone();
            if (d != null) d.GetEnergyInfo(ref info);
            else 
            {
                info.pkgWgt = 0;
                info.moveType = DroneMovement.Idle;
            }
            info.charge = _Data.charge;
            info.capacity = _Data.capacity;
            info.totalCharge = _Data.totalCharge;
            info.totalDischarge = _Data.totalDischarge;
            info.cycles = _Data.cycles;
            info.status = _Data.status;
            info.chargeRate = designCapacity/3600f;
            info.dischargeVoltage = 23f;
            info.chargeVoltage = 3.7f;
            info.designCycles = BatteryData.designCycles;
            info.designCapacity = BatteryData.designCapacity;
            info.chargeTarget = BatteryData.chargeTarget;

            return info;
        }

        public void SetEnergyInfo(EnergyInfo info)
        {
            _Data.charge = info.charge;
            _Data.capacity = info.capacity;
            _Data.totalCharge = info.totalCharge;
            _Data.totalDischarge = info.totalDischarge;
            _Data.cycles = info.cycles;
            _Data.dischargeVoltage = info.dischargeVoltage;
            _Data.chargeVoltage = info.chargeVoltage;
            if (info.stopCharge == 1)
            {
                GetHub()?.StopCharging(this);
            }
        }

        public SBattery Serialize() => new SBattery(_Data);
    }

}