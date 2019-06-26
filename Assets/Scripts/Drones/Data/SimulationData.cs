using System;
using Drones.Objects;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.Data
{
    public class SimulationData : IData
    {
        public uint UID { get; } = 0;

        public bool IsDataStatic { get; } = false;
        public DateTime simulation;
        public SimulationStatus status;
        public SecureSortedSet<uint, IDataSource> drones;
        public SecureSortedSet<uint, IDataSource> hubs;
        public SecureSortedSet<uint, IDataSource> noFlyZones;
        public SecureSortedSet<uint, IDataSource> incompleteJobs;
        public SecureSortedSet<uint, IDataSource> completeJobs;
        public SecureSortedSet<uint, IDataSource> retiredDrones;
        public SecureSortedSet<uint, Battery> batteries;
        public SecureSortedSet<uint, Job> jobs;
        public float revenue;
        public float totalDelay;
        public float totalAudible;
        public float totalEnergy;
        public int queuedJobs;
        public int crashes;
        public int delayedJobs;
        public int failedJobs;
        public int completedCount;
        public int inQueueDelayed;

        private void InitializeCollections()
        {
            retiredDrones = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is RetiredDrone
            };

            drones = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Drone
            };

            hubs = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Hub
            };

            noFlyZones = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is NoFlyZone
            };

            incompleteJobs = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Job && ((Job)item).Status != JobStatus.Complete
            };

            completeJobs = new SecureSortedSet<uint, IDataSource>
            {
                MemberCondition = (item) => item is Job
            };

            batteries = new SecureSortedSet<uint, Battery>();

            jobs = new SecureSortedSet<uint, Job>();
        }

        private void SetUpCallbacks()
        {
            drones.ItemRemoved += (obj) =>
            {
                var i = (Drone)obj;
                i.GetHub()?.Drones.Remove(i);
            };
            completeJobs.ItemAdded += (item) => { incompleteJobs.Remove(item); };
            jobs.ItemRemoved += (item =>
            {
                incompleteJobs.Remove(item);
                completeJobs.Remove(item);
            });
        }

        public SimulationData()
        {
            simulation = DateTime.Now;
            InitializeCollections();
            SetUpCallbacks();
        }
    }
}