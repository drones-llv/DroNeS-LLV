using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Drones.Utils.Scheduler
{
    using static JobScheduler;
    [BurstCompile]
    public struct LLVArrayInitializerJob : IJob
    {


        public void Execute()
        {

        }
    }

}
