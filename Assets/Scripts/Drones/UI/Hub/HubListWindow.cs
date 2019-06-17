using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.Hub
{
    public class HubListWindow : ObjectListWindow
    {
        public static HubListWindow New() => PoolController.Get(WindowPool.Instance).Get<HubListWindow>(null);

        public override ListElement TupleType { get; } = ListElement.HubList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 650);

        public override void OnNewSource(IDataSource source)
        {
            var element = HubTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }
    }
}