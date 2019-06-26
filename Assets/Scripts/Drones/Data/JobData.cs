using Drones.JobSystem;
using Drones.Managers;
using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.Data
{
    using Utils;

    public class JobData : IData
    {
        private static uint _count;
        public static void Reset() => _count = 0;

        public uint UID { get; }
        public bool IsDataStatic { get; set; }
        public float EnergyUse { get; set; }
        public uint Drone;
        public readonly uint Hub;
        public JobStatus Status;
        public readonly float PackageWeight;
        public Vector3 Pickup;
        public Vector3 Dropoff;
        public float Earnings;
        public TimeKeeper.Chronos Assignment;
        public TimeKeeper.Chronos Completed;
        public readonly TimeKeeper.Chronos Deadline;
        public readonly CostFunction Cost;
        public readonly float ExpectedDuration;
        public readonly float StDevDuration;
        public float DeliveryAltitude;

        public JobData(Hub pickup, Vector3 dropoff) 
        {
            UID = ++_count;
            
            Hub = pickup.UID;
            Status = JobStatus.Assigning;
            Pickup = pickup.Position;
            Dropoff = LandingZoneIdentifier.Reposition(dropoff);
            PackageWeight = Random.Range(0.1f, 2.5f);
            
            Cost = new CostFunction(WeightToRev(Pricing.US, PackageWeight));
            ExpectedDuration = (LateralManhattan() + LateralEuclidean()) / (2 * DroneMovementJob.HorizontalSpeed) + (Pickup.y-dropoff.y) / DroneMovementJob.VerticalSpeed;
            StDevDuration = LateralManhattan() / DroneMovementJob.HorizontalSpeed - ExpectedDuration + (Pickup.y - Dropoff.y) / DroneMovementJob.VerticalSpeed;
            
            Deadline = Cost.Start + Cost.Guarantee;
        }

        private float LateralManhattan()
        {
            var v = Pickup - Dropoff;
            return Mathf.Abs(v.x) + Mathf.Abs(v.z);
        }
        private float LateralEuclidean() 
        {
            var v = Pickup - Dropoff;
            v.y = 0;
            return v.magnitude;
        }
        private static float WeightToRev(Pricing p, float weight)
        {
            if (p == Pricing.UK)
            {
                if (weight <= 0.25) return 2.02f;
                if (weight <= 0.5) return 2.14f;
                if (weight <= 1) return 2.30f;
                if (weight <= 1.5) return 2.45f;
                if (weight <= 2) return 2.68f;
                if (weight <= 4) return 3.83f;
            }

            var oz = UnitConverter.ConvertValue(Mass.oz, weight);
            if (oz <= 10) return Random.value < 0.5f ? 2.41f : 3.19f;
            if (oz <= 16) return Random.value < 0.5f ? 2.49f : 3.28f;

            var lbs = UnitConverter.ConvertValue(Mass.lb, weight);
            if (lbs <= 2) return 4.76f;
            if (lbs <= 3) return 5.26f;
            return 5.26f + (lbs - 3) * 0.38f;
        }


    }
}
