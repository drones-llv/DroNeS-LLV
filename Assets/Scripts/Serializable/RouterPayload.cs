using System;
using System.Collections.Generic;
using Drones.Utils;
using Newtonsoft.Json;

namespace Drones.Serializable
{
    [Serializable]
    public class RouterPayload
    {
        public uint requester;
        public SVector3 origin;
        public SVector3 destination;
        public bool onJob;
        public JobStatus status;
        public List<StaticObstacle> noFlyZones;
        public Dictionary<uint, int> drone;
        public List<SVector3> dronePositions;
        public List<SVector3> droneDirections;

        public RouterPayload()
        {
            noFlyZones = new List<StaticObstacle>();
            drone = new Dictionary<uint, int>();
            dronePositions = new List<SVector3>();
            droneDirections = new List<SVector3>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

    }

}
