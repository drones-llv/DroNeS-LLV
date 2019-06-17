using System.Globalization;
using Drones.Data;
using Drones.Extensions;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;

namespace Drones.UI.Drone
{
    public class RetiredDroneTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var rd = (RetiredDroneData)data;

            Data[0].SetField(Source);
            Data[1].SetField(rd.destroyedTime);
            Data[2].SetField(rd.collisionLocation.ToStringXYZ());
            Data[3].SetField(rd.packageWorth.ToString("C", CultureInfo.CurrentCulture));

        }

        public static RetiredDroneTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<RetiredDroneTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}

