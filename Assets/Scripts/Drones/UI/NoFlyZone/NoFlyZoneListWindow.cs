using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.NoFlyZone
{
    public class NoFlyZoneListWindow : ObjectListWindow
    {
        public static NoFlyZoneListWindow New() => PoolController.Get(WindowPool.Instance).Get<NoFlyZoneListWindow>(null);

        public override ListElement TupleType { get; } = ListElement.NFZList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(730, 600);

        public override void OnNewSource(IDataSource source)
        {
            var element = NFZTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }
    }
}
