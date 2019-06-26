using Drones.Managers;
using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.Data
{
    using static Managers.SimManager;
    public class HubData : IData
    {
        private readonly Hub _source;
        public uint UID { get; }
        public bool IsDataStatic => false;

        public SecureSortedSet<uint, IDataSource> drones;
        public SecureSortedSet<uint, IDataSource> incompleteJobs;
        public SecureSortedSet<uint, IDataSource> completedJobs;
        public SecureSortedSet<uint, Drone> DronesWithNoJobs;
        public SecureSortedSet<uint, Battery> batteries;
        public SecureSortedSet<uint, Battery> BatteriesWithNoDrones;
        public Vector3 Position => _source.transform.position;
        public int NumberOfDroneCrashes;
        public int ActiveDroneCount;
        public int DelayedCompletedJobs;
        public int FailedJobs;
        public int CompletedJobCount;
        public float Earnings;
        public float TotalDelayOfCompletedJobs;
        public float EnergyConsumption;
        public float AudibleDuration;
        public int NumberOfJobsInQueue;
        public int NumberOfJobsDelayedInQueue;

        public HubData() { }

        public HubData(Hub hub)
        {
            _source = hub;
            UID = hub.UID;
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
                BatteriesWithNoDrones.Remove(bat.UID);
                AllBatteries.Remove(bat.UID);
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