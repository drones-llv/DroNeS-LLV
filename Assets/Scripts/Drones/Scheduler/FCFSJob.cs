﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drones.Scheduler
{
    [BurstCompile]
    public struct FCFSJob : IJob
    {
        [WriteOnly]
        public NativeList<StrippedJob> queue;

        public void Execute()
        {
            for (int i = 1; i < queue.Length; i++)
                queue[i - 1] = queue[i];

            queue.RemoveAtSwapBack(queue.Length - 1);
        }
    }

}
