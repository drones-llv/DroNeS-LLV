using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct LLVInitializerJob : IJobParallelFor
    {
        public ChronoWrapper time;
        public NativeArray<LLVStruct> results;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.loss = JobScheduler.ExpectedValue(tmp.job, time) - JobScheduler.ExpectedValue(tmp.job, time + tmp.job.expectedDuration);
            results[i] = tmp;
        }

    }
}
