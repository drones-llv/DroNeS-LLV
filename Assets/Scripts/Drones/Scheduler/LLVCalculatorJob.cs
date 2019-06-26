using Drones.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct LLVCalculatorJob : IJobParallelFor
    {
        public TimeKeeper.Chronos time;
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
            var pLost = totalLosses[0] - input[i].loss;
            var mean = (totalDuration[0] - input[i].job.ExpectedDuration) / (input.Length - 1);
            var pGain = JobScheduler.ExpectedValue(input[i].job, time) - JobScheduler.ExpectedValue(input[i].job, time + mean);
            nlv[i] = pLost - pGain;
        }
    }

}

