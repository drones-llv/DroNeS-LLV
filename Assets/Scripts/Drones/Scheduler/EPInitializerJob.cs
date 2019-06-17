using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct EPInitializerJob : IJobParallelFor
    {
        public ChronoWrapper time;

        public NativeArray<EPStruct> results;

        public void Execute(int i)
        {
            var tmp = results[i];
            tmp.value = JobScheduler.ExpectedValue(tmp.job, time);
            results[i] = tmp;
        }
    }
}
