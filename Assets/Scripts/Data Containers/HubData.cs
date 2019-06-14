using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Data
{
    using Serializable;
    using Utils;
    using DataStreamer;
    using static Managers.SimManager;
    public class HubData : IData
    {
        public static uint Count { get; private set; }
        public static void Reset() => Count = 0;
        public const float deploymentPeriod = 0.75f;
        private readonly Hub _source;

        public uint UID { get; }

        public bool IsDataStatic => false;

        public Queue<Drone> deploymentQueue;

        public SecureSortedSet<uint, IDataSource> drones;
        public SecureSortedSet<uint, IDataSource> incompleteJobs;
        public SecureSortedSet<uint, IDataSource> completedJobs;
        public SecureSortedSet<uint, Drone> freeDrones;
        public SecureSortedSet<uint, Battery> batteries;
        public SecureSortedSet<uint, Battery> chargingBatteries;
        public SecureSortedSet<uint, Battery> freeBatteries;
        public Vector3 Position => _source.transform.position;
        public int crashes;
        public int delayedJobs;
        public int failedJobs;
        public int completedCount;
        public float revenue;
        public float delay;
        public float energyConsumption;
        public float audibility;

        public HubData() { }

        public HubData(Hub hub)
        {
            _source = hub;
            UID = ++Count;
            InitializeCollections();
            SetUpCollectionEvents();
        }

        public HubData(SHub data, Hub hub, List<SDrone> droneData, List<SBattery> batteryData)
        {
            _source = hub;
            UID = data.count;
            energyConsumption = data.energy;
            revenue = data.revenue;
            delay = data.delay;
            audibility = data.audibility;
            crashes = data.crashes;
            delayedJobs = data.delayedJobs;
            failedJobs = data.failedJobs;
            InitializeCollections();
            LoadAssignments(data, droneData, batteryData);
            hub.Scheduler.LoadJobQueue(data.schedulerJobQueue);
            hub.Scheduler.LoadDroneQueue(data.schedulerDroneQueue);
            SetUpCollectionEvents();
        }

        private void LoadAssignments(SHub hubData, List<SDrone> droneData, List<SBattery> batteryData)
        {
            var fd = new HashSet<uint>(hubData.freeDrones);
            var fb = new HashSet<uint>(hubData.freeBatteries);
            var cb = new HashSet<uint>(hubData.chargingBatteries);
            foreach (uint i in hubData.completedJobs) 
                completedJobs.Add(i, AllJobs[i]);
            foreach (uint i in hubData.incompleteJobs)
                incompleteJobs.Add(i, AllJobs[i]);

            for (int i = batteryData.Count - 1; i >= 0; i--)
            {
                if (LoadBattery(batteryData[i], fb, cb)) 
                    batteryData.RemoveAt(i);
            }
            for (int i = droneData.Count - 1; i >= 0; i--)
            {
                if (LoadDrone(droneData[i], fd)) 
                    droneData.RemoveAt(i);
            }
            foreach(uint uid in hubData.exitingDrones)
            {
                deploymentQueue.Enqueue((Drone)drones[uid]);
            }
        }

        private bool LoadBattery(SBattery data, HashSet<uint> free, HashSet<uint> charging)
        {
            if (data.hub == UID)
            {
                var bat = new Battery(data);
                AllBatteries.Add(bat.UID, bat);
                batteries.Add(bat.UID, bat);
                if (free.Contains(bat.UID)) freeBatteries.Add(bat.UID, bat);
                if (charging.Contains(bat.UID)) chargingBatteries.Add(bat.UID, bat);
                return true;
            }
            return false;
        }

        private bool LoadDrone(SDrone data, HashSet<uint> free)
        {
            if (data.hub == UID)
            {
                Drone drone = Drone.Load(data);
                AllDrones.Add(drone.UID, drone);
                drones.Add(drone.UID, drone);
                if (!data.inHub) drone.transform.SetParent(Drone.ActiveDrones);
                if (free.Contains(drone.UID)) freeDrones.Add(drone.UID, drone);
                return true;
            }
            return false;
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
                freeBatteries.Remove(bat.UID);
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
                freeDrones.Add(drone.UID, (Drone)drone);
            };
            drones.ItemRemoved += delegate (IDataSource drone)
            {
                AllDrones.Remove(drone);
                freeDrones.Remove((Drone)drone);
            };
            freeDrones.ItemAdded += (drone) =>
            {
                drone.transform.SetParent(_source.transform);
            };
            freeDrones.ItemRemoved += (drone) =>
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
            deploymentQueue = new Queue<Drone>();
            batteries = new SecureSortedSet<uint, Battery>();

            freeBatteries = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
            {
                MemberCondition = (Battery obj) => { return batteries.Contains(obj) && obj.GetDrone() == null; }
            };
            chargingBatteries = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
            {
                MemberCondition = (Battery obj) => { return batteries.Contains(obj); }
            };
            drones = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (obj) => { return obj is Drone; }
            };
            freeDrones = new SecureSortedSet<uint, Drone>
            {
                MemberCondition = (drone) => { return drones.Contains(drone) && drone.GetJob() == null; }
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