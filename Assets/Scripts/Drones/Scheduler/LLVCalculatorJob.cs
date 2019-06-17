using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct LLVCalculatorJob : IJobParallelFor
    {
        public ChronoWrapper time;
        [ReadOnly]
        public NativeArray<float> totalLosses;
        [ReadOnly]
        public NativeArray<float> totalDuration;
        [ReadOnly]
        public NativeArray<LLVStruct> input;
        [WriteOnly]
        public NativeArray<float> nlv;

        public void Execute(int i)
        {
            float plost = totalLosses[0] - input[i].loss;
            float mean = (totalDuration[0] - input[i].job.expectedDuration) / (input.Length - 1);
            float pgain = JobScheduler.ExpectedValue(input[i].job, time) - JobScheduler.ExpectedValue(input[i].job, time + mean);
            nlv[i] = plost - pgain;
        }
    }

}

