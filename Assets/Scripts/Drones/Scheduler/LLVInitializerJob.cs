using Drones.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct LLVInitializerJob : IJobParallelFor
    {
        public TimeKeeper.Chronos Time;
        public NativeArray<LLVStruct> Results;

        public void Execute(int i)
        {
            var tmp = Results[i];
            tmp.loss = JobScheduler.ExpectedValue(tmp.job, Time) - JobScheduler.ExpectedValue(tmp.job, Time + tmp.job.ExpectedDuration);
            Results[i] = tmp;
        }

    }
}
