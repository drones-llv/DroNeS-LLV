using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Drones.Utils.Jobs
{
    public struct EnergyInfo
    {
        public float pkgWgt;
        public float pkgXArea;
        public float energy;
        public DroneMovement moveType;
    }

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
        public const float vSpeed = MovementJob.VSPEED;
        public const float hSpeed = MovementJob.HSPEED;
        public float deltaTime;
        public NativeArray<EnergyInfo> energies;

        public void Execute(int i)
        {
            var tmp = energies[i];
            if (energies[i].moveType == DroneMovement.Idle)
            {
                tmp.energy = 0;
                energies[i] = tmp;
                return;
            }

            float m = mass + energies[i].pkgWgt;
            float w = m * g;

            float power = n_Prop * Mathf.Sqrt(Mathf.Pow(w / n_Prop, 3) * 2 / Mathf.PI / Mathf.Pow(Prop_D, 2) / rho) / eff;
            if (energies[i].moveType != DroneMovement.Hover)
            {
                if (energies[i].moveType == DroneMovement.Ascend)
                {
                    power += 0.5f * rho * Mathf.Pow(vSpeed, 3) * Cd * A;
                    power += mass * g * vSpeed;
                }
                else if (energies[i].moveType == DroneMovement.Descend)
                {
                    power += 0.5f * rho * Mathf.Pow(vSpeed, 3) * Cd * A;
                    power -= mass * g * vSpeed;
                }
                else
                {
                    power += 0.5f * rho * Mathf.Pow(hSpeed, 3) * Cd * A;
                }
            }
            tmp.energy = power * deltaTime;
            energies[i] = tmp;
        }
    }
}
