using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.UI.Drone
{
    public class DroneTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var drone = (Objects.Drone)Source;
            Data[0].SetField(drone.Name);
            Data[1].SetField(drone.GetHub().Name);
            var job = drone.GetJob();
            if (job != null)
            {
                Data[2].SetField(job.Pickup.ToStringXZ());
                Data[3].SetField(job.DropOff.ToStringXZ());
            }
            else
            {
                Data[2].SetField("");
                Data[3].SetField("");
            }
        }

        public static DroneTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<DroneTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}
