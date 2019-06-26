using System.Runtime.CompilerServices;
using Drones.Data;
using Drones.Utils;
using Unity.Collections;
using Unity.Jobs;
using Utils;

namespace Drones.JobSystem
{
    public struct ChargeCount
    {
        public uint Uid;
        public int Count;
    }

    public struct ChargingCounterJob : IJobParallelFor
    {
        public NativeArray<ChargeCount> HubData;
        [ReadOnly] public NativeList<BatteryData> BatteryData;
        public void Execute(int index)
        {
            var h = HubData[index];
            var count = 0;
            for (var i = 0; i < BatteryData.Length; i++)
            {
                var bd = BatteryData[i];
                if (bd.hub == h.Uid && bd.status == BatteryStatus.Charge)
                {
                    count++;
                }
            }
            h.Count = count;
            HubData[index] = h;
        }
    }
    
}