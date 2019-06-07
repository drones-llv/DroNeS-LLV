using System;
using Drones.Utils;

namespace Drones.Serializable
{
    using Drones.Data;

    [Serializable]
    public class SCostFunction
    {
        public STime start;
        public float penalty; // seconds
        public float reward;
    }

    [Serializable]
    public class SJob
    {
        // Generator set
        public uint uid;
        public JobStatus status;
        public long creationTime;
        public string content;
        public float packageWeight;
        public float packageXarea;
        public SCostFunction costFunction;
        public SVector3 pickup;
        public SVector3 destination;
        // Scheduler set
        public uint droneUID;
        public string custom; 
        // => if (!string.IsNullOrEmpty(custom)) WriteToConsole(new customItem(SJob.custom));
        /* join the message together i.e. custom = <item> from <name> */

        // Unity set
        public STime deadline;
        public STime completedOn;
        public STime createdUnity;
        public STime assignedTime;

        public SJob(JobData data)
        {
            uid = data.UID;
            packageWeight = data.packageWeight;
            costFunction = data.costFunction?.Serialize();
            completedOn = data.completed?.Serialize();
            deadline = data.deadline?.Serialize();
            status = data.status;
            pickup = data.pickup;
            destination = data.dropoff;
            droneUID = data.drone;
        }

    }
}
