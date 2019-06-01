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
        public NativeArray<float> totalLosses;
        public NativeArray<float> totalDuration;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.loss = ExpectedValue(tmp.job, time) - ExpectedValue(tmp.job, time + ExpectedDuration(tmp.job));
            totalLosses[0] += tmp.loss;
            totalDuration[0] += ExpectedDuration(tmp.job);
            results[i] = tmp;
        }

    }
}
