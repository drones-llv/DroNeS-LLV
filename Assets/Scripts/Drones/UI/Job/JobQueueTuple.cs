using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.UI.Job
{
    public class JobQueueTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var job = (JobData)data;

            Data[0].SetField(job.Pickup.ToStringXZ());
            Data[1].SetField(job.Dropoff.ToStringXZ());
            Data[2].SetField(job.Cost.Start.ToString());
            Data[3].SetField(job.Assignment.ToString());
            Data[4].SetField("D" + job.Drone.ToString("000000"));

        }

        public static JobQueueTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<JobQueueTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}

