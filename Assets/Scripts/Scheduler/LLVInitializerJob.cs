using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static JobScheduler;
    [BurstCompile]
    public struct LLVInitializerJob : IJobParallelFor
    {
        public ChronoWrapper time;
        public NativeArray<LLVStruct> results;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.loss = ExpectedValue(tmp.job, time) - ExpectedValue(tmp.job, time + tmp.job.expectedDuration);
            results[i] = tmp;
        }

    }
}
