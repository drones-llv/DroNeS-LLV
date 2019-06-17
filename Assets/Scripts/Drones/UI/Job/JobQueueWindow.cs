using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.Job
{
    public class JobQueueWindow : ObjectListWindow
    {
        public static JobQueueWindow New() => PoolController.Get(WindowPool.Instance).Get<JobQueueWindow>(null);

        public override ListElement TupleType { get; } = ListElement.JobQueue;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1180, 500);

        public override void OnNewSource(IDataSource source)
        {
            var element = JobQueueTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }
    }
}