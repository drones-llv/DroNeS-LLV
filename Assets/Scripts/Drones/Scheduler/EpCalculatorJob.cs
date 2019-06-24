using Drones.Objects;
using Drones.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct EpCalculatorJob : IJobParallelFor
    {
        public TimeKeeper.Chronos Time;
        [ReadOnly]
        public NativeArray<EPStruct> Input;
        [WriteOnly]
        public NativeArray<float> Ep;

        public void Execute(int i)
        {
            var n = Input.Length;
            var r = i / n;
            var c = i % n;

            if (r != c) Ep[r * n + c] = Input[r].value + JobScheduler.ExpectedValue(Input[c].job, FinishTime(Input[r].job));
            else Ep[r * n + c] = Input[r].value;
        }

        private TimeKeeper.Chronos FinishTime(StrippedJob job) => CourierService.Inverse(job, JobScheduler.ExpectedValue(job, Time));
    }
}
