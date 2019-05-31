using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static Scheduler;
    [BurstCompile]
    public struct EPInitializerJob : IJobParallelFor
    {
        ChronoWrapper time;
        [ReadOnly]
        NativeArray<StrippedJob> allJobs;
        [WriteOnly]
        NativeArray<float> expectedNow;

        public void Execute(int i)
        {
            expectedNow[i] = ExpectedValue(allJobs[i], time);
        }
    }
}
