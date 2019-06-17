using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Drones.Serializable
{
    [Serializable]
    public class SchedulerPayload
    {
        public uint requester;
        public float revenue;
        public float delay;
        public float audible;
        public float energy;
        public Dictionary<uint, StrippedDrone> drones;
        public Dictionary<uint, SHub> hubs;
        public Dictionary<uint, SBattery> batteries;
        public Dictionary<uint, SJob> incompleteJobs;
        public Dictionary<uint, StaticObstacle> noFlyZones;
        public STime currentTime;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

}
