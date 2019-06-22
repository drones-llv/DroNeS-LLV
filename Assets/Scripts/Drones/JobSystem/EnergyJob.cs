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
    public struct EnergyInfo
    {
        public float pkgWgt;
        public float energy;
        public DroneMovement moveType;
        public BatteryStatus status;
        public float totalDischarge;
        public float totalCharge;
        public float charge;
        public float capacity;
        public int cycles;
        public float chargeRate;
        public int designCycles;
        public float designCapacity;
        public float chargeTarget;

        public int stopCharge;
    }

    public struct DroneInfo
    {
        public float pkgWgt;
        public DroneMovement moveType;
    }

    [BurstCompile]
    public struct EnergyJob : IJobParallelFor
    {
        private const float DischargeVoltage = 23;
        private const float Mass = 22.5f;
        private const float Cd = 0.1f;
        private const float g = 9.81f;
        private const float A = 0.1f;
        private const float Rho = 1.225f; // air density
        private const float PropellerDiameter = 0.3f; // propeller radius
        private const float NumPropellers = 4; // number of propellers
        private const float Eff = 1f; // efficiency
        private const float Epsilon = 0.001f;
        private const float VSpeed = MovementJob.VSPEED;
        private const float HSpeed = MovementJob.HSPEED;

        public float DeltaTime;
        public NativeArray<EnergyInfo> Energies;

        public void Execute(int i)
        {
            var tmp = Energies[i];
            tmp.stopCharge = 0;
            if (tmp.moveType == DroneMovement.Idle)
            {
                tmp.energy = 0;
                if (tmp.status == BatteryStatus.Charge) Charge(ref tmp);
            }
            else if (tmp.status == BatteryStatus.Discharge)
            {
                var w = (Mass + tmp.pkgWgt) * g;
                var power = NumPropellers * math.sqrt(math.pow(w / NumPropellers, 3) * 2 / Mathf.PI / math.pow(PropellerDiameter, 2) / Rho) / Eff;
                if (Energies[i].moveType != DroneMovement.Hover)
                {
                    switch (Energies[i].moveType)
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
                        case DroneMovement.Hover:
                            power = 0;
                            break;
                        case DroneMovement.Idle:
                            power = 0;
                            break;
                        case DroneMovement.Drop:
                            power = 0;
                            break;
                        default:
                            break;
                    }
                }
                tmp.energy = power * DeltaTime;
                Discharge(ref tmp);
            }
            Energies[i] = tmp;
        }

        private static void Discharge(ref EnergyInfo info)
        {
            var dQ = info.energy / DischargeVoltage;
            info.charge -= dQ;
            if (info.charge > 0.1f) info.totalDischarge += dQ;
            else info.status = BatteryStatus.Dead;
        }

        private void Charge(ref EnergyInfo info)
        {
            var dQ = info.chargeRate * DeltaTime;
            if (info.charge / info.capacity < 0.05f) dQ *= 0.1f;
            else if (info.charge / info.capacity > 0.55f) dQ *= (2 * (1 -  info.charge / info.capacity));
            if (info.charge < info.capacity) { info.totalCharge += dQ; }

            if (math.abs(info.chargeTarget * info.capacity - info.charge) < Epsilon)
            {
                info.stopCharge = 1;
            }

            info.charge += dQ;
            info.charge = math.clamp(info.charge, 0, info.capacity);

            if ((int) (info.totalDischarge / info.capacity) <= info.cycles ||
                (int) (info.totalCharge / info.capacity) <= info.cycles) return;
            info.cycles++;
            SetCap(ref info);
        }

        private static void SetCap(ref EnergyInfo info)
        {
            var x = info.cycles / (float)info.designCycles;

            info.capacity = (-0.7199f * math.pow(x, 3) + 0.7894f * math.pow(x, 2) - 0.3007f * x + 1) * info.designCapacity;
        }
    }
}
