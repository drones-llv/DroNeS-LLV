using System;
using Drones.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.JobSystem
{
    public struct BusyDroneData
    {
        public float pkgWgt;
        public DroneMovement moveType;
    }

    [BurstCompile]
    public struct EnergyConsumptionJob : IJobParallelFor
    {
        #region Constants
        private const float Mass = 22.5f;
        private const float Cd = 0.1f;
        private const float g = 9.81f;
        private const float A = 0.1f;
        private const float Rho = 1.225f; // air density
        private const float PropellerDiameter = 0.3f; // propeller radius
        private const float NumPropellers = 4; // number of propellers
        private const float Eff = 1f; // efficiency
        private const float VSpeed = DroneMovementJob.VerticalSpeed;
        private const float HSpeed = DroneMovementJob.HorizontalSpeed;
        #endregion
        
        public float DeltaTime;
        public NativeArray<BatteryData> Energies;
        [ReadOnly] public NativeHashMap<uint, BusyDroneData> DroneInfo;
        [WriteOnly] public NativeQueue<uint>.Concurrent DronesToDrop;

        public void Execute(int i)
        {
            var battery = Energies[i];
            if (!DroneInfo.TryGetValue(battery.UID, out var info) || info.moveType == DroneMovement.Idle)
            {
                battery.status = BatteryStatus.Idle;
                battery.DeltaEnergy = 0;
                Charge(ref battery);
            }
            else
            {
                battery.status = BatteryStatus.Discharge;
                var w = (Mass + info.pkgWgt) * g;
                var power = NumPropellers * math.sqrt(math.pow(w / NumPropellers, 3) * 2 / Mathf.PI / math.pow(PropellerDiameter, 2) / Rho) / Eff;
                switch (info.moveType)
                {
                    case DroneMovement.Ascend:
                        power += 0.5f * Rho * Mathf.Pow(VSpeed, 3) * Cd * A;
                        power += w * VSpeed;
                        break;
                    case DroneMovement.Descend:
                        power += 0.5f * Rho * Mathf.Pow(VSpeed, 3) * Cd * A;
                        power -= w * VSpeed;
                        break;
                    case DroneMovement.Horizontal:
                        power += 0.5f * Rho * Mathf.Pow(HSpeed, 3) * Cd * A;
                        break;
                    case DroneMovement.Drop:
                        power = 0;
                        break;
                    case DroneMovement.Hover:
                        break;
                    case DroneMovement.Idle:
                        power = 0;
                        break;
                    default:
                        break;
                }
                battery.DeltaEnergy = power * DeltaTime;
                Discharge(ref battery);
            }
            Energies[i] = battery;
        }
        
        private void Discharge(ref BatteryData info)
        {
            var dQ = info.DeltaEnergy / BatteryData.DischargeVoltage;
            info.charge -= dQ;
            if (info.charge > 0.1f) info.totalDischarge += dQ;
            else
            {
                info.status = BatteryStatus.Dead;
                DronesToDrop.Enqueue(info.drone);
            }
        }

        private void Charge(ref BatteryData info)
        {
            if (info.charge > BatteryData.ChargeTarget * info.capacity)
            {
                return;
            }
            info.status = BatteryStatus.Charge;
            var dQ = BatteryData.ChargeRate * DeltaTime;
            if (info.charge / info.capacity < 0.05f) dQ *= 0.1f;
            else if (info.charge / info.capacity > 0.55f) dQ *= (2 * (1 -  info.charge / info.capacity));
            if (info.charge < info.capacity) { info.totalCharge += dQ; }
            info.charge += dQ;
            info.charge = math.clamp(info.charge, 0, info.capacity);

            if ((int) (info.totalDischarge / info.capacity) <= info.cycles ||
                (int) (info.totalCharge / info.capacity) <= info.cycles) return;
            info.cycles++;
            SetCap(ref info);
        }

        private static void SetCap(ref BatteryData info)
        {
            var x = info.cycles / (float)BatteryData.DesignCycles;

            info.capacity = (-0.7199f * math.pow(x, 3) + 0.7894f * math.pow(x, 2) - 0.3007f * x + 1) * BatteryData.DesignCapacity;
        }
    }
}
