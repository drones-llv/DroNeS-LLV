using Drones.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct EpInitializerJob : IJobParallelFor
    {
        public TimeKeeper.Chronos time;

        public NativeArray<EPStruct> results;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.value = JobScheduler.ExpectedValue(tmp.job, time);
            results[i] = tmp;
        }
    }
}
