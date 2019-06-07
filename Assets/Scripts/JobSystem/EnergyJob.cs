using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Drones.Data;

namespace Drones.Utils.Jobs
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
        public float dischargeVoltage;
        public float chargeVoltage;
        public int designCycles;
        public float designCapacity;
        public float chargeTarget;

        public int stopCharge;
    }

    [BurstCompile]
    public struct EnergyJob : IJobParallelFor
    {
        public const float mass = 22.5f;
        public const float Cd = 0.1f;
        public const float g = 9.81f;
        public const float A = 0.1f;
        public const float rho = 1.225f; // air density
        public const float Prop_D = 0.3f; // propeller radius
        public const float n_Prop = 4; // number of propellers
        public const float eff = 1f; // efficiency
        public const float EPSILON = 0.001f;
        public const float vSpeed = MovementJob.VSPEED;
        public const float hSpeed = MovementJob.HSPEED;

        public float deltaTime;
        public NativeArray<EnergyInfo> energies;

        public void Execute(int i)
        {
            var tmp = energies[i];
            tmp.stopCharge = 0;
            if (tmp.moveType == DroneMovement.Idle)
            {
                tmp.energy = 0;
                if (tmp.status == BatteryStatus.Charge) Charge(ref tmp);
            }
            else if (tmp.status == BatteryStatus.Discharge)
            {
                float w = (mass + tmp.pkgWgt) * g;
                float power = n_Prop * math.sqrt(math.pow(w / n_Prop, 3) * 2 / Mathf.PI / math.pow(Prop_D, 2) / rho) / eff;
                if (energies[i].moveType != DroneMovement.Hover)
                {
                    if (energies[i].moveType == DroneMovement.Ascend)
                    {
                        power += 0.5f * rho * Mathf.Pow(vSpeed, 3) * Cd * A;
                        power += w * vSpeed;
                    }
                    else if (energies[i].moveType == DroneMovement.Descend)
                    {
                        power += 0.5f * rho * Mathf.Pow(vSpeed, 3) * Cd * A;
                        power -= w * vSpeed;
                    }
                    else
                    {
                        power += 0.5f * rho * Mathf.Pow(hSpeed, 3) * Cd * A;
                    }
                }
                tmp.energy = power * deltaTime;
                Discharge(ref tmp);
            }
            energies[i] = tmp;
        }

        private void Discharge(ref EnergyInfo info)
        {
            var dQ = info.energy / info.dischargeVoltage;
            info.charge -= dQ;
            if (info.charge > 0.1f) info.totalDischarge += dQ;
            else info.status = BatteryStatus.Dead;
        }

        private void Charge(ref EnergyInfo info)
        {
            var dQ = info.chargeRate * deltaTime;
            if (info.charge / info.capacity < 0.05f) dQ *= 0.1f;
            else if (info.charge / info.capacity > 0.55f) dQ *= (2 * (1 -  info.charge / info.capacity));
            if (info.charge < info.capacity) { info.totalCharge += dQ; }

            if (math.abs(info.chargeTarget * info.capacity - info.charge) < EPSILON)
            {
                info.stopCharge = 1;
            }

            info.charge += dQ;
            info.charge = math.clamp(info.charge, 0, info.capacity);

            if ((int)(info.totalDischarge / info.capacity) > info.cycles && (int)(info.totalCharge / info.capacity) > info.cycles)
            {
                info.cycles++;
                SetCap(ref info);
            }
        }

        private void SetCap(ref EnergyInfo info)
        {
            float x = info.cycles / info.designCycles;

            info.capacity = (-0.7199f * math.pow(x, 3) + 0.7894f * math.pow(x, 2) - 0.3007f * x + 1) * info.designCapacity;
        }
    }
}
