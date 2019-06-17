using Drones.Data;
using Drones.Extensions;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;

namespace Drones.UI.Job
{
    public class JobQueueTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var job = (JobData)data;

            Data[0].SetField(job.pickup.ToStringXZ());
            Data[1].SetField(job.dropoff.ToStringXZ());
            Data[2].SetField(job.created?.ToString());
            Data[3].SetField(job.assignment?.ToString());
            Data[4].SetField("D" + job.drone.ToString("000000"));

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

