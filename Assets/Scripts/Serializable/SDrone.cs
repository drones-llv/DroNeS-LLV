using System.Collections;
using System.Collections.Generic;
using System;
namespace Drones.Serializable
{
    using Data;
    using Utils;
    [Serializable]
    public class SDrone
    {
        public uint count;
        public uint uid;
        public uint totalDeliveryCount;
        public uint totalBatterySwaps;
        public uint totalHubHandovers;
        public bool isWaiting;
        public bool inHub;
        public bool isActive;
        public DroneMovement movement;
        public FlightStatus status;
        public List<uint> completedJobs;
        public List<SVector3> waypointsQueue;
        public SVector3 position;
        public SVector3 previousWaypoint;
        public SVector3 waypoint;
        public SVector3 hubPosition;
        public uint job;
        public uint hub;
        public uint battery;
        public float charge;
        public float totalDelay;
        public float totalAudibleDuration;
        public float totalPackageWeight;
        public float totalDistanceTravelled;
        public float totalEnergy;
        public float targetAltitude;

        public SDrone(DroneData data, Drone drone)
        {
            count = DroneData.Count;
            isActive = (drone.transform.parent == Drone.ActiveDrones);
            uid = data.UID;
            totalDeliveryCount = data.DeliveryCount;
            totalBatterySwaps = data.batterySwaps;
            totalHubHandovers = data.hubsAssigned;
            isWaiting = data.isWaiting;
            inHub = drone.InHub;
            movement = data.movement;
            totalDelay = data.totalDelay;
            totalAudibleDuration = data.audibleDuration;
            totalPackageWeight = data.packageWeight;
            totalDistanceTravelled = data.distanceTravelled;
            totalEnergy = data.totalEnergy;
            targetAltitude = data.targetAltitude;
            waypointsQueue = new List<SVector3>();
            completedJobs = new List<uint>(data.completedJobs.Keys);
            position = drone.transform.position;
            previousWaypoint = data.previousWaypoint;
            waypoint = data.currentWaypoint;
            job = data.job;
            hub = data.hub;
            battery = data.battery;
            charge = drone.GetBattery().Charge;
            foreach (var point in data.waypoints)
                waypointsQueue.Add(point);
        }

    }


}
