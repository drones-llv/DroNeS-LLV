using UnityEngine;
using System.Collections.Generic;
using Drones.Objects;
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
            DeliveryCount = 0;
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
        public uint DeliveryCount;
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
        
        public bool isWaiting;
        public Queue<Vector3> waypoints = new Queue<Vector3>();
        public Vector3 previousWaypoint;
        public Vector3 currentWaypoint;
        public Vector3 previousPosition;
        private Vector3 CurrentPosition => _source.transform.position;
        public Vector3 Direction => Vector3.Normalize(previousPosition - CurrentPosition);
        public float energyOnJobStart;
    }

}