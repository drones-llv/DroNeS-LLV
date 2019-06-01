using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static JobScheduler;
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

            if (r != c) ep[r * n + c] = input[r].value + ExpectedValue(input[c].job, FinishTime(input[r].job));
            else ep[r * n + c] = input[r].value;
        }

        ChronoWrapper FinishTime(StrippedJob job) => CostFunction.Inverse(job, ExpectedValue(job, time));
    }
}
