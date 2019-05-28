using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    using Data;
    [Serializable]
    public class SHub
    {
        public uint count;
        public List<uint> batteries;
        public List<uint> freeBatteries;
        public List<uint> chargingBatteries;
        public List<uint> drones;
        public List<uint> freeDrones;
        public List<uint> exitingDrones;
        public List<uint> incompleteJobs;
        public List<uint> completedJobs;
        public List<uint> routeQueue;
        public List<uint> schedulerJobQueue;
        public List<uint> schedulerDroneQueue;
        public SVector3 position;
        public uint uid;
        public int crashes;
        public int delayedJobs;
        public int failedJobs;
        public float energy;
        public float revenue;
        public float delay;
        public float audibility;


        public SHub(HubData data, Hub hub)
        {
            count = HubData.Count;
            uid = data.UID;
            batteries = new List<uint>();
            freeBatteries = new List<uint>();
            chargingBatteries = new List<uint>();
            drones = new List<uint>();
            freeDrones = new List<uint>();
            position = hub.transform.position;
            exitingDrones = new List<uint>();
            completedJobs = new List<uint>();
            incompleteJobs = new List<uint>();
            schedulerJobQueue = hub.Scheduler.SerializeJobs();
            schedulerDroneQueue = hub.Scheduler.SerializeDrones();
            routeQueue = hub.Router.Serialize();
            energy = data.energyConsumption;
            revenue = data.revenue;
            delay = data.delay;
            audibility = data.audibility;
            crashes = data.crashes;
            delayedJobs = data.delayedJobs;
            failedJobs = data.failedJobs;

            foreach (var bat in data.batteries.Keys)
                batteries.Add(bat);
            foreach (var bat in data.freeBatteries.Keys)
                freeBatteries.Add(bat);
            foreach (var bat in data.chargingBatteries.Keys)
                chargingBatteries.Add(bat);
            foreach (var d in data.drones.Keys)
                drones.Add(d);
            foreach (var j in data.incompleteJobs.Keys)
                incompleteJobs.Add(j);
            foreach (var j in data.completedJobs.Keys)
                completedJobs.Add(j);
            foreach (var d in data.freeDrones.Keys)
                freeDrones.Add(d);
            foreach (var d in data.deploymentQueue)
                exitingDrones.Add(d.UID);
        }
    }

}
