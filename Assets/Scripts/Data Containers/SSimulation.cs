using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    using Data;
    using Managers;
    using Utils;

    [Serializable]
    public class SSimulation
    {
        public long timestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public float revenue;
        public float totalDelay;
        public float totalAudible;
        public float totalEnergy;
        public List<SDrone> drones;
        public List<SRetiredDrone> retiredDrones;
        public List<SBattery> batteries;
        public List<SHub> hubs;
        public List<SJob> completedJobs;
        public List<SJob> incompleteJobs;
        public List<SNoFlyZone> noFlyZones;
        public List<uint> routerQueue;
        public List<uint> schedulerQueue;
        public STime currentTime;


        public SSimulation(SimulationData data)
        {
            revenue = data.revenue;
            totalDelay = data.totalDelay;
            totalAudible = data.totalAudible;
            totalEnergy = data.totalEnergy;
            drones = new List<SDrone>();
            retiredDrones = new List<SRetiredDrone>();
            batteries = new List<SBattery>();
            hubs = new List<SHub>();
            completedJobs = new List<SJob>();
            incompleteJobs = new List<SJob>();
            noFlyZones = new List<SNoFlyZone>();
            currentTime = TimeKeeper.Chronos.Get().Serialize();
            routerQueue = RouteManager.Serialize();
            schedulerQueue = JobManager.Serialize();

            foreach (Drone drone in data.drones.Values)
                drones.Add(drone.Serialize());
            foreach (Hub hub in data.hubs.Values)
                hubs.Add(hub.Serialize());
            foreach (RetiredDrone rDrone in data.retiredDrones.Values)
                retiredDrones.Add(rDrone.Serialize());
            foreach (Battery bat in data.batteries.Values)
                batteries.Add(bat.Serialize());
            foreach (Job job in data.completeJobs.Values)
                completedJobs.Add(job.Serialize());
            foreach (Job job in data.incompleteJobs.Values)
                incompleteJobs.Add(job.Serialize());
            foreach (NoFlyZone nfz in data.noFlyZones.Values)
                noFlyZones.Add(nfz.Serialize());
        }
    }

}