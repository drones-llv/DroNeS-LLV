using System.Collections.Generic;
using System;

namespace Drones.Serializable
{
    using Data;
    [Serializable]
    public class SRetiredDrone
    {
        public uint uid;
        public bool isDroneCollision;
        public string hub;
        public float packageworth;
        public uint assignedJob;
        public STime destroyed;
        public SVector3 waypoint;
        public SVector3 location;
        public List<uint> completedJobs;
        public string otherDroneName;
        public uint otherUID;
        public float charge;


        public SRetiredDrone(RetiredDroneData data)
        {
            uid = data.UID;
            isDroneCollision = data.isDroneCollision;
            hub = data.hub;
            assignedJob = data.job;
            packageworth = data.packageWorth;
            destroyed = data.destroyedTime.Serialize();
            waypoint = data.waypoint;
            location = data.collisionLocation;
            completedJobs = new List<uint>();
            otherDroneName = data.otherDrone;
            otherUID = data.otherUID;
            charge = data.batteryCharge;
            foreach (var job in data.completedJobs.Keys)
                completedJobs.Add(job);
        }
    }
}
