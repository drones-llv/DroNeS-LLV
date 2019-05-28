using System;
using UnityEngine;
using System.Globalization;

namespace Drones
{
    using Utils;
    using DataStreamer;
    using UI;
    using Serializable;
    using Managers;
    using Drones.Data;

    public class Job : IDataSource
    {
        public static readonly TimeKeeper.Chronos _EoT = new TimeKeeper.Chronos(int.MaxValue, 23, 59, 59.99f).SetReadOnly();
        public Job(SJob data)
        {
            _Data = new JobData(data);
        }

        public Job(Hub pickup, Vector3 dropoff, float weight, float penalty)
        {
            _Data = new JobData(pickup, dropoff, weight, penalty);
        }

        public uint UID => _Data.UID;
        public string Name => "J" + UID.ToString("00000000");
        public override string ToString() => Name;

        #region IDataSource
        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_Data);

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = PoolController.Get(WindowPool.Instance).Get<JobWindow>(OpenWindows.Transform);
                InfoWindow.Source = this;
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }

        public bool IsDataStatic => _Data.IsDataStatic;
        #endregion

        private readonly JobData _Data;
        public Drone GetDrone() => (Drone)SimManager.AllDrones[_Data.drone];
        public RetiredDrone GetRetiredDrone() => (RetiredDrone)SimManager.AllRetiredDrones[_Data.drone];

        public JobStatus Status => _Data.status;
        public Vector3 DropOff => _Data.dropoff;
        public Vector3 Pickup => _Data.pickup;
        public float Earnings => _Data.earnings;
        public TimeKeeper.Chronos Deadline => _Data.deadline;
        public TimeKeeper.Chronos CompletedOn => _Data.completed;
        public float PackageWeight => _Data.packageWeight;
        public float PackageXArea => _Data.packageXArea;
        public float Loss => -_Data.costFunction.GetPaid(_EoT);

        public void AssignDrone(Drone drone)
        {
            if (Status == JobStatus.Assigning)
            {
                _Data.drone = drone.UID;
            }
        }

        public void FailJob()
        {
            _Data.IsDataStatic = true;
            _Data.status = JobStatus.Failed;
            _Data.completed = _EoT;
            _Data.earnings = -Loss;
            var drone = GetDrone();
            var hub = drone?.GetHub();
            if (hub != null)
            {
                hub.UpdateRevenue(Earnings);
                hub.UpdateFailedCount();
            }
            drone?.AssignJob(null);
            AssignDrone(null);
        }

        public void CompleteJob()
        {
            _Data.completed = TimeKeeper.Chronos.Get().SetReadOnly();
            _Data.status = JobStatus.Complete;
            _Data.IsDataStatic = true;
            _Data.earnings = _Data.costFunction.GetPaid(CompletedOn);

            GetDrone().CompleteJob(this);
            AssignDrone(null);
        }

        public void StartDelivery() => _Data.status = JobStatus.Delivering;

        public float Progress()
        {
            if (Status != JobStatus.Complete)
            {
                if (Status != JobStatus.Delivering) return 0.00f;
                return GetDrone().JobProgress;
            }
            return 1.00f;
        }

        public SJob Serialize() => new SJob(_Data);
    };
}