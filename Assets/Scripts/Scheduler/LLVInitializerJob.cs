using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static Scheduler;
    [BurstCompile]
    public struct LLVInitializerJob : IJobParallelFor
    {
        ChronoWrapper time;
        [ReadOnly]
        NativeArray<StrippedJob> allJobs;
        [WriteOnly]
        NativeArray<float> totalLosses;
        [WriteOnly]
        NativeArray<float> totalDuration;
        [WriteOnly]
        NativeArray<float> potentialLosses;

        public void Execute(int i)
        {
            potentialLosses[i] = ExpectedValue(allJobs[i], time) - ExpectedValue(allJobs[i], time + ExpectedDuration(allJobs[i]));
            totalLosses[0] += potentialLosses[i];
            totalDuration[0] += ExpectedDuration(allJobs[i]);
        }

    }
}
