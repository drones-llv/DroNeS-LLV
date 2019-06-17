using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.Drone
{
    public class DroneListWindow : ObjectListWindow
    {
        public static DroneListWindow New() => PoolController.Get(WindowPool.Instance).Get<DroneListWindow>(null);

        public override ListElement TupleType { get; } = ListElement.DroneList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 650);

        public override void OnNewSource(IDataSource source)
        {
            var element = DroneTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }
    }
}