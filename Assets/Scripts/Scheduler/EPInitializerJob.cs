using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static JobScheduler;
    [BurstCompile]
    public struct EPInitializerJob : IJobParallelFor
    {
        public ChronoWrapper time;

        public NativeArray<EPStruct> results;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.value = ExpectedValue(tmp.job, time);
            results[i] = tmp;
        }
    }
}
