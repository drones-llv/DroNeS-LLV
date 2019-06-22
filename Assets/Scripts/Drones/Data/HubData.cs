using System.Collections;
using System.Collections.Generic;
using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Data
{
    using Utils;
    using static Managers.SimManager;
    public class HubData : IData
    {
        private static uint Count { get; set; }
        public static void Reset() => Count = 0;
        public const float DeploymentPeriod = 0.75f;
        private readonly Hub _source;

        public uint UID { get; }

        public bool IsDataStatic => false;

        public SecureSortedSet<uint, IDataSource> drones;
        public SecureSortedSet<uint, IDataSource> incompleteJobs;
        public SecureSortedSet<uint, IDataSource> completedJobs;
        public SecureSortedSet<uint, Drone> DronesWithNoJobs;
        public SecureSortedSet<uint, Battery> batteries;
        public SecureSortedSet<uint, Battery> chargingBatteries;
        public SecureSortedSet<uint, Battery> BatteriesWithNoDrones;
        public Vector3 Position => _source.transform.position;
        public int crashes;
        public int delayedJobs;
        public int failedJobs;
        public int completedCount;
        public float revenue;
        public float delay;
        public float energyConsumption;
        public float audibility;
        public int queuedJobs;
        public int inQueueDelayed;

        public HubData() { }

        public HubData(Hub hub)
        {
            _source = hub;
            UID = ++Count;
            InitializeCollections();
            SetUpCollectionEvents();
        }

        private void SetUpCollectionEvents()
        {
            batteries.ItemAdded += delegate (Battery bat)
            {
                AllBatteries.Add(bat.UID, bat);
                bat.AssignHub(_source);
            };
            batteries.ItemRemoved += delegate (Battery bat)
            {
                chargingBatteries.Remove(bat.UID);
                BatteriesWithNoDrones.Remove(bat.UID);
                AllBatteries.Remove(bat.UID);
            };
            chargingBatteries.ItemAdded += delegate (Battery bat)
            {
                bat.SetStatus(BatteryStatus.Charge);
            };
            chargingBatteries.ItemRemoved += delegate (Battery bat)
            {
                if (bat.Status == BatteryStatus.Charge)
                    bat.SetStatus(BatteryStatus.Idle);
            };
            drones.ItemAdded += delegate (IDataSource drone)
            {
                ((Drone)drone).AssignHub(_source);
                AllDrones.Add(drone.UID, drone);
                DronesWithNoJobs.Add(drone.UID, (Drone)drone);
            };
            drones.ItemRemoved += delegate (IDataSource drone)
            {
                AllDrones.Remove(drone);
                DronesWithNoJobs.Remove((Drone)drone);
            };
            DronesWithNoJobs.ItemAdded += (drone) =>
            {
                drone.transform.SetParent(_source.transform);
            };
            DronesWithNoJobs.ItemRemoved += (drone) =>
            {
                drone.transform.SetParent(Drone.ActiveDrones);
            };
            completedJobs.ItemAdded += delegate (IDataSource job)
            { 
                incompleteJobs.Remove(job);
                AllCompleteJobs.Add(job.UID, job);
            };
            completedJobs.ItemRemoved += (job) => AllCompleteJobs.Remove(job);
            incompleteJobs.ItemAdded += delegate (IDataSource job)
            {
                AllJobs.Add(job.UID, (Job)job);
                AllIncompleteJobs.Add(job.UID, job);
            };
            incompleteJobs.ItemRemoved += (job) => AllIncompleteJobs.Remove(job);
        }

        private void InitializeCollections()
        {
            batteries = new SecureSortedSet<uint, Battery>();

            BatteriesWithNoDrones = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
            {
                MemberCondition = (obj) => batteries.Contains(obj) && !obj.HasDrone()
            };
            chargingBatteries = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
            {
                MemberCondition = (obj) => batteries.Contains(obj)
            };
            drones = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (obj) => obj is Drone
            };
            DronesWithNoJobs = new SecureSortedSet<uint, Drone>
            {
                MemberCondition = (drone) => drones.Contains(drone) && drone.GetJob() == null
            };
            incompleteJobs = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Job && ((Job)item).Status != JobStatus.Complete
            };
            completedJobs = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Job
            };
        }

    }

}