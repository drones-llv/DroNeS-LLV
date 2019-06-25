using Drones.Utils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drones.JobSystem
{
    public struct Deadlines
    {
        public uint UID;
        public uint Hub;
        public TimeKeeper.Chronos Deadline;
        public float penalty;
    }

    public struct HubRevenue
    {
        public uint UID;
        public float Revenue;
    }
    
    public struct JobFailer : IJobParallelFor
    {
        public TimeKeeper.Chronos CurrentTime; 
        public NativeArray<Deadlines> IncompleteJobs;
        public NativeArray<HubRevenue> HubRevenue;
        public NativeIntPtr.Concurrent SimulationRevenue;
        public void Execute(int index)
        {
            var j = IncompleteJobs[index];
            if (j.Deadline > CurrentTime) return;
            
            for (var i = 0; i < HubRevenue.Length; i++)
            {
                var h = HubRevenue[i];
                if (j.Hub != h.UID) continue;
                
                h.Revenue -= j.penalty;
                SimulationRevenue.Add(-Mathf.FloorToInt(j.penalty)*100);
            }
            
        }
    }
}