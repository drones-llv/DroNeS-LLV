using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static JobScheduler;
    [BurstCompile]
    public struct LLVSumJob : IJob
    {
        public NativeArray<float> totalLosses;
        public NativeArray<float> totalDuration;
        [ReadOnly]
        public NativeArray<LLVStruct> jobs;

        public void Execute()
        {
            totalLosses[0] = 0;
            totalDuration[0] = 0;
            for (int i = 0; i < jobs.Length; i++)
            {
                totalLosses[0] += jobs[i].loss;
                totalDuration[0] += jobs[i].job.expectedDuration;
            }

        }
    }

}

