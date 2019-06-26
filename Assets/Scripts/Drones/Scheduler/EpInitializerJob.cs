using Drones.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct EpInitializerJob : IJobParallelFor
    {
        public TimeKeeper.Chronos Time;

        public NativeArray<EPStruct> Results;

        public void Execute(int i)
        {
            var tmp = Results[i];
            tmp.value = JobScheduler.ExpectedValue(in tmp.job, in Time);
            Results[i] = tmp;
        }
    }
}
