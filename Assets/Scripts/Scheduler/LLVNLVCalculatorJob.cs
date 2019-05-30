using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Utils.Scheduler
{
    public struct LLVNLVCalculatorJob : IJobParallelFor
    {
        float totalLosses;
        float totalDuration;
        NativeArray<StrippedJob> allJobs;
        NativeArray<float> potentialLosses;
        NativeArray<float> potentialGains;
        NativeArray<float> nlv;

        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }

}

