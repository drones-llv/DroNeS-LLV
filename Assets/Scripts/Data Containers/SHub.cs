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
        public List<uint> jobQueue;
        public SVector3 position;
        public uint uid;
        public float energy;


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
            jobQueue = new List<uint>(data.jobQueue);
            energy = data.energyConsumption;

            foreach (var bat in data.batteries.Keys)
                batteries.Add(bat);
            foreach (var bat in data.freeBatteries.Keys)
                freeBatteries.Add(bat);
            foreach (var bat in data.chargingBatteries.Keys)
                chargingBatteries.Add(bat);
            foreach (var d in data.drones.Keys)
                drones.Add(d);
            foreach (var d in data.freeDrones.Keys)
                freeDrones.Add(d);
            foreach (var d in data.deploymentQueue)
                exitingDrones.Add(d.UID);
        }
    }

}
