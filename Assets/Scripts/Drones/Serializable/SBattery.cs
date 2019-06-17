﻿using System;
using Drones.Data;
using Utils;

namespace Drones.Serializable
{
    [Serializable]
    public class SBattery
    {
        public BatteryStatus status;
        public uint count;
        public uint uid;
        public float charge;
        public float capacity;
        public int cycles;
        public uint drone;
        public uint hub;
        public int designCycles;
        public float dischargeVoltage;
        public float designCapacity;
        public float chargeRate;
        public float chargeTarget;
        public float chargeVoltage;
        public float totalCharge;
        public float totalDischarge;

        public SBattery(BatteryData data)
        {
            count = BatteryData.Count;
            uid = data.UID;
            status = data.status;
            capacity = data.capacity;
            charge = data.charge;
            cycles = data.cycles;
            drone = data.drone;
            hub = data.hub;
            designCycles = BatteryData.DesignCycles;
            designCapacity = BatteryData.DesignCapacity;
            chargeTarget = BatteryData.ChargeTarget;
            chargeRate = data.chargeRate;
            chargeVoltage = data.chargeVoltage;
            dischargeVoltage = data.dischargeVoltage;
            totalCharge = data.totalCharge;
            totalDischarge = data.totalDischarge;

        }
    }

}

