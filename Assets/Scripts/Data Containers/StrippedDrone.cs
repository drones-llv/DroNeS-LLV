using System.Collections;
using System.Collections.Generic;
using System;
namespace Drones.Serializable
{
    using Data;
    using Utils;
    [Serializable]
    public class StrippedDrone
    {
        public uint uid;
        public bool isWaiting;
        public bool inHub;
        public DroneMovement movement;
        public FlightStatus status;
        public float targetAltitude;
        public List<SVector3> waypointsQueue;
        public SVector3 position;
        public SVector3 previousWaypoint;
        public SVector3 waypoint;
        public SVector3 hubPosition;
        public float charge;
        public uint job;
        public uint hub;
        public uint battery;

        public StrippedDrone(DroneData data, Drone drone)
        {
            uid = data.UID;
            isWaiting = data.isWaiting;
            inHub = data.inHub;
            movement = data.movement;
            status = data.state;
            targetAltitude = data.targetAltitude;
            waypointsQueue = new List<SVector3>();
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
