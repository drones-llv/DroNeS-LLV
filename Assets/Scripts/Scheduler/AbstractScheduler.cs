using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Scheduler
{
    using Jobs;
    using Unity.Collections;

    public struct StrippedJob
    {
        public uint UID;
        public Vector3 pickup;
        public Vector3 dropoff;
        public ChronoWrapper start;
        public float reward;
        public float penalty;
    }

    public abstract class AbstractScheduler
    {
        public abstract List<StrippedJob> Sort(List<StrippedJob> jobs, ChronoWrapper time);

        public static float EuclideanDist(StrippedJob job) => (job.pickup - job.dropoff).magnitude;

        public static float ManhattanDist(StrippedJob job)
        {
            var v = job.pickup - job.dropoff;
            return Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
        }

        public static float Normal(float z, float mu, float stdev)
        {
            return 1 / (Mathf.Sqrt(2 * Mathf.PI) * stdev) * Mathf.Exp(-Mathf.Pow(z - mu, 2) / (2 * stdev * stdev));
        }

        public static float ExpectedValue(StrippedJob j, ChronoWrapper time)
        {
            var man = ManhattanDist(j);
            var euc = EuclideanDist(j);

            var mu = (man + euc) / 2 / MovementJob.HSPEED;
            var stdev = (mu - euc) / MovementJob.HSPEED;

            // Approximate integral as a sum over 2 hours, assume probability trails off after 2 hours

            int steps = 60;
            var h = 7200 / steps;

            float expected = CostFunction.Evaluate(j, time) * Normal(0, mu, stdev) / 2;
            expected += CostFunction.Evaluate(j, time + 7200) * Normal(7200, mu, stdev) / 2;
            for (int i = 0; i <= steps - 1; i++)
            {
                expected += CostFunction.Evaluate(j, time + i * h) * Normal(i * h, mu, stdev) / 2;
            }
            expected *= h;

            return expected;
        }
    }
}
