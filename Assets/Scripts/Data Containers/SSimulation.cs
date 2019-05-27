﻿using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    using Data;
    using Managers;
    using Utils;

    [Serializable]
    public class SSimulation
    {
        public long timestamp;
        public long simulation;
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
        public List<uint> schedulerDroneQueue;
        public List<uint> schedulerJobQueue;
        public STime currentTime;
        internal int failedJobs;
        internal int delayedJobs;
        internal int crashes;

        public SSimulation(SimulationData data)
        {
            timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            simulation = data.simulation;
            revenue = data.revenue;
            totalDelay = data.totalDelay;
            totalAudible = data.totalAudible;
            totalEnergy = data.totalEnergy;
            crashes = data.crashes;
            delayedJobs = data.delayedJobs;
            failedJobs = data.failedJobs;
            drones = new List<SDrone>();
            retiredDrones = new List<SRetiredDrone>();
            batteries = new List<SBattery>();
            hubs = new List<SHub>();
            completedJobs = new List<SJob>();
            incompleteJobs = new List<SJob>();
            noFlyZones = new List<SNoFlyZone>();
            currentTime = TimeKeeper.Chronos.Get().Serialize();
            routerQueue = RouteManager.Serialize();
            schedulerDroneQueue = JobManager.SerializeDrones();
            schedulerJobQueue = JobManager.SerializeJobs();

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