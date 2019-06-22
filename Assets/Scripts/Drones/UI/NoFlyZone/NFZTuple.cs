using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Utils;

namespace Drones.UI.NoFlyZone
{
    public class NFZTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var nfz = (NfzData)data;

            Data[0].SetField(nfz.Position.ToStringXZ());
            Data[1].SetField(nfz.droneEntryCount);
            Data[2].SetField(nfz.hubEntryCount);
        }

        public static NFZTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<NFZTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}
