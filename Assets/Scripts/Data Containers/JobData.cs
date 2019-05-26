using UnityEngine;

namespace Drones.Data
{
    using Utils;
    using Serializable;

    public class JobData : IData
    {
        private static uint _Count;
        public static void Reset() => _Count = 0;

        public uint UID { get; }
        public bool IsDataStatic { get; set; } = false;

        public uint drone;
        public float earnings;
        public TimeKeeper.Chronos assignment;
        public TimeKeeper.Chronos completed;

        public JobStatus status;
        public Vector3 dropoff;
        public readonly TimeKeeper.Chronos created;
        public readonly TimeKeeper.Chronos deadline;

        public Vector3 pickup;
        public CostFunction costFunction;
        public float packageWeight = 2.25f;
        public float packageXArea = 0.16f;

        public JobData(SJob data)
        {
            UID = ++_Count;
            status = data.status;
            packageWeight = data.packageWeight;
            packageXArea = data.packageXarea;
            created = new TimeKeeper.Chronos(data.createdUnity).SetReadOnly();
            deadline = new TimeKeeper.Chronos(data.deadline).SetReadOnly();
            pickup = data.pickup;
            dropoff = data.destination;
            costFunction = new CostFunction(data.costFunction);

            if (data.status != JobStatus.Assigning)
            {
                assignment = new TimeKeeper.Chronos(data.assignedTime).SetReadOnly();
                if (data.status != JobStatus.Delivering)
                {
                    completed = new TimeKeeper.Chronos(data.completedOn).SetReadOnly();
                }
            }
        }

        public JobData(Hub pickup, Vector3 dropoff, float weight, float penalty) 
        {
            status = JobStatus.Assigning;
            created = TimeKeeper.Chronos.Get().SetReadOnly();
            deadline = created + 1800;
            this.pickup = pickup.Position;
            this.dropoff = LandingZoneIdentifier.Reposition(dropoff);
            packageWeight = weight;
            costFunction = new CostFunction(created, WeightToRev(Pricing.US, weight), penalty);
        }

        private float WeightToRev(Pricing p, float weight)
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

            float oz = UnitConverter.ConvertValue(Mass.oz, weight);
            if (oz <= 10) return Random.value < 0.5f ? 2.41f : 3.19f;
            if (oz <= 16) return Random.value < 0.5f ? 2.49f : 3.28f;

            float lbs = UnitConverter.ConvertValue(Mass.lb, weight);
            if (lbs <= 2) return 4.76f;
            if (lbs <= 3) return 5.26f;
            return 5.26f + (lbs - 3) * 0.38f;
        }


    }
}
