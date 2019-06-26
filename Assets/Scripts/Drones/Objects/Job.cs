using Drones.Data;
using Drones.Managers;
using Drones.Scheduler;
using Drones.UI.Job;
using Drones.UI.SaveLoad;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using System.Collections;
using Utils;

namespace Drones.Objects
{
    public class Job : IDataSource
    {
        private static readonly TimeKeeper.Chronos _EoT = new TimeKeeper.Chronos(int.MaxValue - 100, 23, 59, 59.99f);
        private static TimeKeeper.Chronos _clock = TimeKeeper.Chronos.Get();
        
        public Job(Hub pickup, Vector3 dropoff)
        {
            _data = new JobData(pickup, dropoff);
            if (SimManager.Mode == SimulationMode.Delivery)
            {
                _waitingForTheEnd = new WaitUntil(() => Status == JobStatus.Delivering || Deadline < _clock.Now());
                GetHub().StartCoroutine(Tracker());
            }
            else
            {
                _waitingForTheEnd = new WaitUntil(() => Status == JobStatus.Complete || Deadline < _clock.Now());
                GetHub().StartCoroutine(Failer());
            }
        }
        private readonly WaitUntil _waitingForTheEnd;
        public uint UID => _data.UID;
        public string Name => $"J{UID:00000000}";
        public override string ToString() => Name;
        #region IDataSource
        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_data);

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = PoolController.Get(WindowPool.Instance).Get<JobWindow>(UIManager.Transform);
                InfoWindow.Source = this;
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }

        public bool IsDataStatic => _data.IsDataStatic;
        #endregion

        private readonly JobData _data;
        private Drone GetDrone() => (Drone)SimManager.AllDrones[_data.Drone];
        private Hub GetHub() => (Hub) SimManager.AllHubs[_data.Hub];
        public RetiredDrone GetRetiredDrone() => (RetiredDrone)SimManager.AllRetiredDrones[_data.Drone];
        
        public JobStatus Status => _data.Status;
        public Vector3 DropOff => _data.Dropoff;
        public Vector3 Pickup => _data.Pickup;
        public float Earnings => _data.Earnings;
        public float Guarantee => _data.Cost.Guarantee;
        public TimeKeeper.Chronos Deadline => _data.Deadline;
        public TimeKeeper.Chronos CompletedOn => _data.Completed;
        public float PackageWeight => _data.PackageWeight;
        public float Loss => _data.Cost.Penalty;

        public float ExpectedDuration => _data.ExpectedDuration;

        public bool IsDelayed { get; private set; }
        public void AssignDrone(Drone drone)
        {
            if (Status != JobStatus.Assigning) return;
            _data.Drone = drone.UID;
            _data.Assignment = TimeKeeper.Chronos.Get();
        }

        public void FailJob()
        {
            _data.IsDataStatic = true;
            _data.Status = JobStatus.Failed;
            _data.Completed = _EoT;
            _data.Earnings = Loss;

            if (_data.Hub != 0 && !IsDelayed)
            {
                var hub = GetHub();
                hub.UpdateRevenue(Earnings);
                hub.UpdateFailedCount();
            }

            if (_data.Drone != 0)
            {
                var drone = GetDrone();
                _data.EnergyUse = drone.DeltaEnergy();
                drone.AssignJob();
                _data.Drone = 0;
            }
            
            DataLogger.LogJob(_data);
        }

        public void CompleteJob()
        {
            _data.IsDataStatic = true;
            _data.Status = JobStatus.Complete;
            _data.Completed = TimeKeeper.Chronos.Get();
            _data.Earnings = CostFunction.Evaluate(in _data.Cost, in _data.Completed);

            var drone = GetDrone();
            drone.CompleteJob(this);
            var hub = drone.GetHub();

            if (!IsDelayed) hub.UpdateRevenue(Earnings);
            
            drone.UpdateDelay(Deadline.Timer());
            _data.EnergyUse = drone.DeltaEnergy();
            drone.AssignJob();
            _data.Drone = 0;
            DataLogger.LogJob(_data);
        }

        public void StartDelivery() => _data.Status = JobStatus.Delivering;
        public void SetAltitude(float alt) => _data.DeliveryAltitude = alt;

        private IEnumerator Tracker()
        {
            yield return _waitingForTheEnd;
            if (Status == JobStatus.Delivering)
            {
                IsDelayed = false;
                yield break;
            }
            IsDelayed = true;
            GetHub().InQueueDelayed();
            GetHub().UpdateRevenue(Loss);
        }

        private IEnumerator Failer()
        {
            yield return _waitingForTheEnd;
            if (Status == JobStatus.Complete) yield break;
            GetHub().Scheduler.FailedInQueue();
            FailJob();
        }
        
        public float Progress()
        {
            if (Status != JobStatus.Complete)
            {
                return Status != JobStatus.Delivering ? 0.00f : GetDrone().JobProgress;
            }
            return 1.00f;
        }

        public static explicit operator SchedulingData(Job job)
        {
            var j = new SchedulingData
            {
                UID = job.UID,
                Cost = job._data.Cost,
                ExpectedDuration = job._data.ExpectedDuration,
                StDevDuration = job._data.StDevDuration
            };
            return j;
        }
    };
}