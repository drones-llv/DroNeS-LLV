using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace Drones.Utils.Scheduler
{
    public struct LLVInitializerJob : IJobParallelFor
    {
        float totalLosses;
        float totalDuration;
        NativeArray<StrippedJob> allJobs;
        NativeArray<float> potentialLosses;

        public void Execute(int i)
        {
            throw new System.NotImplementedException();
        }
    }
}
