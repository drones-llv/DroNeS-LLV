using UnityEngine;
using System.Collections.Generic;
using Drones.Objects;
using Drones.Serializable;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.Data
{
    using Utils;
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
            previousPosition = CurrentPosition;
            isWaiting = true;
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
            isWaiting = data.isWaiting;
            movement = data.movement;
            totalDelay = data.totalDelay;
            audibleDuration = data.totalAudibleDuration;
            packageWeight = data.totalPackageWeight;
            distanceTravelled = data.totalDistanceTravelled;
            totalEnergy = data.totalEnergy;
            targetAltitude = data.targetAltitude;
            
            var transform = _source.transform;
            previousPosition = transform.position;
            transform.position = data.position;
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
                if (job == 0) return 0;
                var j = (Job)AllIncompleteJobs[job];
                if (j == null || j.Status != JobStatus.Delivering) return 0;
                
                var a = Vector3.Distance(CurrentPosition, j.Pickup);
                var b = Vector3.Distance(j.Pickup, j.DropOff);
                return Mathf.Clamp(a / b, 0, 1);
            }
        }
        public bool wasGoingDown;
        public bool isGoingDown;
        public bool isWaiting;
        public float targetAltitude;
        public Queue<Vector3> waypoints = new Queue<Vector3>();
        public Vector3 previousWaypoint;
        public Vector3 currentWaypoint;
        public Vector3 previousPosition;
        private Vector3 CurrentPosition => _source.transform.position;
        public Vector3 Direction => Vector3.Normalize(previousPosition - CurrentPosition);
        public bool frequentRequests;
        public float energyOnJobStart;
    }

}