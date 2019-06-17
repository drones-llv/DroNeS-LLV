using Drones.Objects;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct EPCalculatorJob : IJobParallelFor
    {
        public ChronoWrapper time;
        [ReadOnly]
        public NativeArray<EPStruct> input;
        [WriteOnly]
        public NativeArray<float> ep;

        public void Execute(int i)
        {
            int n = input.Length;
            int r = i / n;
            int c = i % n;

            if (r != c) ep[r * n + c] = input[r].value + JobScheduler.ExpectedValue(input[c].job, FinishTime(input[r].job));
            else ep[r * n + c] = input[r].value;
        }

        ChronoWrapper FinishTime(StrippedJob job) => CostFunction.Inverse(job, JobScheduler.ExpectedValue(job, time));
    }
}
