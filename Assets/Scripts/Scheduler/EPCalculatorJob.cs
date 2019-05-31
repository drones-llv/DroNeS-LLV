using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static Scheduler;
    [BurstCompile]
    public struct EPCalculatorJob : IJobParallelFor
    {
        ChronoWrapper time;
        [ReadOnly]
        NativeArray<StrippedJob> allJobs;
        [ReadOnly]
        NativeArray<float> expectedNow;
        [WriteOnly]
        NativeArray<float> ep;

        public void Execute(int i)
        {
            int n = allJobs.Length;
            int r = i / n;
            int c = i % n;
            if (r == c) return;

            ep[r * n + c] = expectedNow[r] + ExpectedValue(allJobs[c], FinishTime(allJobs[r]));
        }

        public ChronoWrapper FinishTime(StrippedJob job)
        {
            return CostFunction.Inverse(job, ExpectedValue(job, time));
        }
    }
}
