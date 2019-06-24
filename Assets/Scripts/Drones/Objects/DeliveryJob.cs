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
    public class DeliveryJob : IDataSource
    {
        private static readonly TimeKeeper.Chronos _EoT = new TimeKeeper.Chronos(int.MaxValue - 100, 23, 59, 59.99f);
        private static TimeKeeper.Chronos _clock = TimeKeeper.Chronos.Get();

        public DeliveryJob(Hub pickup, Vector3 dropoff, float weight, float penalty)
        {
            _data = new DeliveryData(pickup, dropoff, weight, penalty);
            GetHub().StartCoroutine(Tracker());
        }

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

        private readonly DeliveryData _data;
        private Drone GetDrone() => (Drone)SimManager.AllDrones[_data.Drone];
        private Hub GetHub() => (Hub) SimManager.AllHubs[_data.Hub];

        public JobStatus Status => _data.Status;
        public Vector3 DropOff => _data.Dropoff;
        public Vector3 Pickup => _data.Pickup;
        public float Earnings => _data.Earnings;
        public TimeKeeper.Chronos Deadline => _data.Deadline;
        public TimeKeeper.Chronos CompletedOn => _data.Completed;
        public float PackageWeight => _data.PackageWeight;
        public float Loss => -_data.CourierService.GetPaid(_EoT);

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
            _data.Earnings = -Loss;
            var drone = GetDrone();
            var hub = drone != null ? drone.GetHub() : null;
            if (hub != null && !IsDelayed)
            {
                hub.UpdateRevenue(Earnings);
                hub.UpdateFailedCount();
            }
            _data.EnergyUse = drone.DeltaEnergy();
            drone.AssignJob();
            _data.Drone = 0;
            DataLogger.LogJob(_data);
        }

        public void CompleteJob()
        {
            _data.Completed = TimeKeeper.Chronos.Get();
            _data.Status = JobStatus.Complete;
            _data.IsDataStatic = true;
            _data.Earnings = _data.CourierService.GetPaid(CompletedOn);

            var drone = GetDrone();
            var hub = drone.GetHub();
            
            hub.DeleteJob(this);
            
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
            yield return new WaitUntil(() => Status == JobStatus.Delivering || Deadline < _clock.Now());
            if (Status == JobStatus.Delivering)
            {
                IsDelayed = false;
                yield break;
            }
            IsDelayed = true;
            GetHub().InQueueDelayed();
            GetHub().UpdateRevenue(-Loss);
        }
        
        public float Progress()
        {
            if (Status != JobStatus.Complete)
            {
                return Status != JobStatus.Delivering ? 0.00f : GetDrone().JobProgress;
            }
            return 1.00f;
        }

        public static explicit operator StrippedJob(DeliveryJob deliveryJob)
        {
            var j = new StrippedJob
            {
                UID = deliveryJob.UID,
                pickup = deliveryJob.Pickup,
                dropoff = deliveryJob.DropOff,
                start = deliveryJob._data.Created,
                reward = deliveryJob._data.CourierService.Reward,
                penalty = -deliveryJob._data.CourierService.Penalty,
                expectedDuration = deliveryJob._data.ExpectedDuration,
                stDevDuration = deliveryJob._data.StDevDuration
            };
            return j;
        }
    };
}