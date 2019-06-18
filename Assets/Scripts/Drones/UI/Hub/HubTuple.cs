using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.UI.Hub
{
    public class HubTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var hub = (HubData)data;
            Data[0].SetField(((Objects.Hub)Source).Name);
            Data[1].SetField(hub.drones.Count.ToString());
            Data[2].SetField(hub.batteries.Count.ToString());
            Data[3].SetField(hub.Position.ToStringXZ());

        }

        public static HubTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<HubTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}
