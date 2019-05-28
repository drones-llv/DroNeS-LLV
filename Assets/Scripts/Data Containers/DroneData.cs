using UnityEngine;
using System.Collections.Generic;

namespace Drones.Data
{
    using DataStreamer;
    using Utils;
    using Serializable;
    using static Managers.SimManager;
    public class DroneData : IData
    {
        public static uint Count { get; private set; }
        public static void Reset() => Count = 0;

        private readonly Drone _source;
        public DroneData() { }
        public DroneData(Drone src)
        {
            _source = src;
            UID = ++Count;
            completedJobs.ItemAdded += (obj) => _source.GetHub().JobComplete((Job)obj);
            completedJobs.ItemAdded += (obj) => packageWeight += ((Job)obj).PackageWeight;
            movement = DroneMovement.Idle;
            state = FlightStatus.Idle;
            collisionOn = false;
            inHub = true;
            previousPosition = CurrentPosition;
        }

        public DroneData(SDrone data, Drone src)
        {
            _source = src;
            Count = data.count;
            UID = data.uid;
            battery = data.battery;
            job = data.job;
            hub = data.hub;
            batterySwaps = data.totalBatterySwaps;
            hubsAssigned = data.totalHubHandovers;
            collisionOn = data.collisionOn;
            isWaiting = data.isWaiting;
            movement = data.movement;
            state = data.status;
            totalDelay = data.totalDelay;
            audibleDuration = data.totalAudibleDuration;
            packageWeight = data.totalPackageWeight;
            distanceTravelled = data.totalDistanceTravelled;
            totalEnergy = data.totalEnergy;
            targetAltitude = data.targetAltitude;
            previousPosition = _source.transform.position;
            waypoints = new Queue<Vector3>();
            foreach (Vector3 point in data.waypointsQueue)
            {
                waypoints.Enqueue(point);
            }
            currentWaypoint = data.waypoint;
            previousWaypoint = data.previousWaypoint;

            foreach (uint id in data.completedJobs)
                completedJobs.Add(id, AllCompleteJobs[id]);
           
        }
        public uint UID { get; }
        public bool IsDataStatic { get; set; } = false;
        public uint job;
        public uint hub;
        public uint battery;
        public SecureSortedSet<uint, IDataSource> completedJobs = new SecureSortedSet<uint, IDataSource>
            ((x, y) => (((Job)x).CompletedOn >= ((Job)y).CompletedOn) ? -1 : 1)
        {
            MemberCondition = (IDataSource obj) => { return obj is Job; }
        };
        public FlightStatus state;
        public DroneMovement movement;
        public uint DeliveryCount => (uint)completedJobs.Count;
        public float packageWeight;
        public float distanceTravelled;
        public uint batterySwaps;
        public uint hubsAssigned;
        public float totalDelay;
        public float audibleDuration;
        public float totalEnergy;
        public float JobProgress
        {
            get
            {
                if (job != 0)
                {
                    Job j = (Job)AllIncompleteJobs[job];
                    if (j != null && j.Status == JobStatus.Delivering)
                    {
                        float a = Vector3.Distance(CurrentPosition, j.Pickup);
                        float b = Vector3.Distance(j.Pickup, j.DropOff);
                        return Mathf.Clamp(a / b, 0, 1);
                    }
                }
                return 0;
            }
        }
        public bool wasGoingDown;
        public bool isGoingDown;
        public bool inHub;
        public bool collisionOn;
        public bool isWaiting;
        public float targetAltitude;
        public Queue<Vector3> waypoints = new Queue<Vector3>();
        public Vector3 previousWaypoint;
        public Vector3 currentWaypoint;
        public Vector3 previousPosition;
        private Vector3 CurrentPosition => _source.transform.position;
        public Vector3 Direction => Vector3.Normalize(previousPosition - CurrentPosition);
        public bool frequentRequests;
    }

}