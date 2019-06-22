using System.Collections.Generic;
using Drones.Data;
using Drones.Managers;
using Drones.Router;
using Drones.Scheduler;
using Drones.UI.Hub;
using Drones.UI.SaveLoad;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Objects
{
    public class Hub : MonoBehaviour, IDataSource, IPoolable
    {
        public static Hub New() => PoolController.Get(ObjectPool.Instance).Get<Hub>(null);
        public static int BatteryPerDrone { get; set; } = 4;
        
        public void GetData(DataLogger logger, TimeKeeper.Chronos time) => logger.SetData(_data, time);

        #region IDataSource
        public override string ToString() => Name;

        public bool IsDataStatic => _data.IsDataStatic;

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_data);

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = HubWindow.New();
                InfoWindow.Source = this;
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }
        #endregion

        public uint UID => _data.UID;

        public string Name => $"H{UID:000000}";

        #region Fields
        private HubData _data;
        [SerializeField]
        private HubCollisionController collisionController;
        [SerializeField]
        private DeploymentPath dronePath;
        [SerializeField]
        private Collider hubCollider;
        [SerializeField]
        private float jobGenerationRate = 0.1f;
        private Pathfinder _router;
        private JobGenerator _jobGenerator;
        private JobScheduler _scheduler;
        #endregion

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);

        public bool InPool { get; private set; }

        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            if (InfoWindow != null) InfoWindow.Close.onClick.Invoke();
            if (_data.drones != null)
            {
                _data.drones.ReSort();
                while (_data.drones.Count > 0)
                    ((Drone)_data.drones.GetMin(false)).SelfDestruct();
            }
            _data = null;
            SimManager.AllHubs.Remove(this);
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
            if (!(_jobGenerator is null))
                StopCoroutine(_jobGenerator.Generate());
            _jobGenerator = null;
            _router = null;
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            _data = new HubData(this);
            SimManager.AllHubs.Add(UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            _jobGenerator = new JobGenerator(this, JobGenerationRate);
            StartCoroutine(_jobGenerator.Generate());
            DataLogger.LogHub(this);
        }
        #endregion

        #region Properties
        public Collider HubCollider
        {
            get
            {
                if (hubCollider == null) hubCollider = GetComponent<Collider>();
                return hubCollider;
            }
        }

        public HubCollisionController CollisionController
        {
            get
            {
                if (collisionController == null)
                {
                    collisionController = GetComponentInChildren<HubCollisionController>();
                }
                return collisionController;
            }
        }

        public DeploymentPath DronePath
        {
            get
            {
                if (dronePath == null)
                    dronePath = transform.GetComponentInChildren<DeploymentPath>();
                return dronePath;
            }
        }
        public Pathfinder Router => _router ?? (_router = new SmartStarpath());

        public JobScheduler Scheduler
        {
            get
            {
                if (_scheduler == null) _scheduler = transform.GetComponentInChildren<JobScheduler>();
                return _scheduler;
            }
        }
        
        public Vector3 Position => transform.position;
        #endregion

        public void JobEnqueued()
        {
            _data.queuedJobs++;
            SimManager.JobEnqueued();
        }

        public void JobDequeued(bool isDelayed)
        {
            _data.queuedJobs--;
            SimManager.JobDequeued();
            if (isDelayed) DequeuedDelay();
        }

        public void InQueueDelayed()
        { 
            _data.inQueueDelayed++;
            SimManager.InQueueDelayed();
        }

        public void DequeuedDelay()
        {
            _data.inQueueDelayed--;
            SimManager.DequeuedDelay();
        } 
        
        public void UpdateEnergy(float dE)
        {
            _data.energyConsumption += dE;
            SimManager.UpdateEnergy(dE);
        }

        internal void DeleteJob(Job job)
        {
            _data.incompleteJobs.Remove(job);
            _data.completedCount++;
            SimManager.UpdateCompleteCount();
            SimManager.AllIncompleteJobs.Remove(job);
            SimManager.AllJobs.Remove(job);
        }

        public void UpdateRevenue(float value)
        {
            _data.revenue += value;
            SimManager.UpdateRevenue(value);
        }
        public void UpdateDelay(float dt)
        {
            _data.delay += dt;
            if (dt > 0) UpdateDelayCount();
            SimManager.UpdateDelay(dt);
        }
        private void UpdateDelayCount() => _data.delayedJobs++;
        public void UpdateFailedCount() 
        { 
            _data.failedJobs++;
            SimManager.UpdateFailedCount();
        }
        public void UpdateCrashCount() 
        {
            _data.crashes++;
            SimManager.UpdateCrashCount();
        }
        public void UpdateAudible(float dt)
        {
            _data.audibility += dt;
            SimManager.UpdateAudible(dt);
        }
        public void JobComplete(Job job) => _data.completedJobs.Add(job.UID, job);
        public SecureSortedSet<uint, IDataSource> Drones => _data.drones;
        public float JobGenerationRate
        {
            get => jobGenerationRate;

            set
            {
                jobGenerationRate = value;
                _jobGenerator.SetLambda(value);
            }
        }
        public void OnJobCreate(params Job[] jobs)
        {
            foreach (var job in jobs)
            {
                _data.incompleteJobs.Add(job.UID, job);
                Scheduler.AddToQueue(job);
            }
        }

        private void Awake() => _data = new HubData();
        
        #region Drone/Battery Interface
        public void AddToDeploymentQueue(Drone drone) => DronePath.AddToDeploymentQueue(drone);
        public void DeployDrone(Drone drone)
        {
            _data.DronesWithNoJobs.Remove(drone);
            if (!GetBatteryForDrone(drone)) return;
            var bat = drone.GetBattery();
            StopCharging(bat);
            bat.SetStatus(BatteryStatus.Discharge);
            drone.Deploy();
        }

        public void OnDroneReturn(Drone drone)
        {
            if (_data.DronesWithNoJobs.Add(drone.UID, drone))
            {
                _data.chargingBatteries.Add(drone.GetBattery().UID, drone.GetBattery());
            }
            drone.WaitForDeployment();
            Scheduler.AddToQueue(drone);
        }

        public void RemoveBatteryFromDrone(Drone drone)
        {
            if (drone.GetHub() != this) return;
            var battery = drone.GetBattery();
            drone.AssignBattery();
            battery.AssignDrone();
            _data.BatteriesWithNoDrones.Add(battery.UID, battery);
            _data.chargingBatteries.Add(battery.UID, battery);
        }

        public void StopCharging(Battery battery) => _data.chargingBatteries.Remove(battery.UID);

        public bool GetBatteryForDrone(Drone drone)
        {
            if (drone.GetBattery() != null) return true;

            if (_data.batteries.Count / _data.drones.Count < BatteryPerDrone)
            {
                drone.AssignBattery(BuyBattery(drone));
            }
            else
            {
                var battery = _data.BatteriesWithNoDrones.GetMax(true);
                if (battery == null) return false;
                drone.AssignBattery(battery);
                battery.AssignDrone(drone);
            }
            return true;
        }

        public Drone BuyDrone()
        {
            var drone = Drone.New();
            _data.drones.Add(drone.UID, drone);
            _data.DronesWithNoJobs.Add(drone.UID, drone);
            GetBatteryForDrone(drone);
            Scheduler.AddToQueue(drone);
            drone.transform.position = transform.position;
            
            while (_data.batteries.Count / _data.drones.Count < BatteryPerDrone)
            {
                BuyBattery();
            }
            return drone;
        }

        public void SellDrone()
        {
            if (_data.DronesWithNoJobs.Count <= 0) return;
            var drone = _data.DronesWithNoJobs.GetMin(false);
            var dd = new RetiredDrone(drone);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            _data.drones.Remove(drone);
            drone.Delete();
        }

        public void DestroyBattery(Battery battery) => _data.batteries.Remove(battery);

        private Battery BuyBattery(Drone drone)
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            var bat = new Battery(drone, this);
            _data.batteries.Add(bat.UID, bat);
            _data.BatteriesWithNoDrones.Add(bat.UID, bat);
            return bat;
        }

        public void BuyBattery()
        {
            var bat = new Battery(this);
            _data.batteries.Add(bat.UID, bat);

            _data.BatteriesWithNoDrones.Add(bat.UID, bat);
        }

        public void SellBattery()
        {
            if (_data.BatteriesWithNoDrones.Count <= 0) return;
            var bat = _data.BatteriesWithNoDrones.GetMin(true);
            _data.batteries.Remove(bat);
        }
        #endregion

    }
}