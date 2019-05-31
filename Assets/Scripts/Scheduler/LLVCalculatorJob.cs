using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static Scheduler;
    [BurstCompile]
    public struct LLVCalculatorJob : IJobParallelFor
    {
        ChronoWrapper time;
        [ReadOnly]
        NativeArray<float> totalLosses;
        [ReadOnly]
        NativeArray<float> totalDuration;
        [ReadOnly]
        NativeArray<float> potentialLosses;
        [ReadOnly]
        NativeArray<StrippedJob> allJobs;
        [WriteOnly]
        NativeArray<float> nlv;

        public void Execute(int i)
        {
            float plost = totalLosses[0] - potentialLosses[i];
            float mean = (totalDuration[0] - ExpectedDuration(allJobs[i])) / (allJobs.Length - 1);
            float pgain = ExpectedValue(allJobs[i], time) - ExpectedValue(allJobs[i], time + mean);
            nlv[i] = plost - pgain;
        }
    }

}

