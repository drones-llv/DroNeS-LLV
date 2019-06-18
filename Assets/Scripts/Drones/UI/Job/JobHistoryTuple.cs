using System.Globalization;
using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.UI.Job
{
    public class JobHistoryTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var job = (JobData)data;

            Data[0].SetField(job.pickup.ToStringXZ());
            Data[1].SetField(job.dropoff.ToStringXZ());
            Data[2].SetField((job.deadline is null) ? "" : UnitConverter.Convert(Chronos.min, job.deadline.Timer()));
            Data[3].SetField(job.earnings.ToString("C", CultureInfo.CurrentCulture));

        }

        public static JobHistoryTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<JobHistoryTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }

    }
}

