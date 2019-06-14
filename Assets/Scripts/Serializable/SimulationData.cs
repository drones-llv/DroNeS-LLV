using System;

namespace Drones.Data
{
    using Utils;
    using DataStreamer;
    using Serializable;

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
        }

        public SimulationData()
        {
            simulation = DateTime.Now;
            InitializeCollections();
            SetUpCallbacks();
        }

        public SimulationData(SSimulation data)
        {
            InitializeCollections();
        }

        public void Load(SSimulation data)
        {
            simulation = data.simulation;
            revenue = data.revenue;
            totalDelay = data.totalDelay;
            totalAudible = data.totalAudible;
            totalEnergy = data.totalEnergy;
            queuedJobs = data.queuedJobs;
            completedCount = data.completedCount;
            crashes = data.crashes;
            delayedJobs = data.delayedJobs;
            failedJobs = data.failedJobs;
            foreach (var job in data.completedJobs)
            {
                var loaded = new Job(job);
                completeJobs.Add(loaded.UID, loaded);
                jobs.Add(loaded.UID, loaded);
            }
            foreach (var job in data.incompleteJobs)
            {
                var loaded = new Job(job);
                incompleteJobs.Add(loaded.UID, loaded);
                jobs.Add(loaded.UID, loaded);
            }

            foreach (var nfz in data.noFlyZones)
            {
                NoFlyZone.Load(nfz);
            }

            foreach (var rDrone in data.retiredDrones)
            {
                var loaded = new RetiredDrone(rDrone);
            }
            foreach (var hub in data.hubs)
            {
                var h = Hub.Load(hub, data.drones, data.batteries);
            }
            SetUpCallbacks();
        }
    }
}
