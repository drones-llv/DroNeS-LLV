using System.Collections.Generic;

using UnityEngine;

namespace Drones
{
    using Managers;
    using Utils;
    using DataStreamer;
    using UI;
    using Interface;
    using Serializable;
    using Data;
    using Utils.Scheduler;
    using Utils.Router;
    using System;

    public class Hub : MonoBehaviour, IDataSource, IPoolable
    {
        public static Hub New() => PoolController.Get(ObjectPool.Instance).Get<Hub>(null);

        #region IDataSource
        public override string ToString() => Name;

        public bool IsDataStatic => _Data.IsDataStatic;

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_Data);

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

        public uint UID => _Data.UID;

        public string Name => "H" + UID.ToString("000000");

        #region Fields
        private HubData _Data;
        [SerializeField]
        private HubCollisionController _CollisionController;
        [SerializeField]
        private DeploymentPath _DronePath;
        [SerializeField]
        private Collider _Collider;
        [SerializeField]
        private float _jobGenerationRate = 0.1f;
        private JobGenerator _jobGenerator;
        [SerializeField]
        private Pathfinder _Router;
        private JobScheduler _Scheduler;
        #endregion

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);

        public bool InPool { get; private set; }

        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            InfoWindow?.Close.onClick.Invoke();
            if (_Data.drones != null)
            {
                _Data.drones.ReSort();
                while (_Data.drones.Count > 0)
                    ((Drone)_Data.drones.GetMin(false)).SelfDestruct();
            }
            _Data = null;
            SimManager.AllHubs.Remove(this);
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
            if (!(_jobGenerator is null))
                StopCoroutine(_jobGenerator.Generate());
            _jobGenerator = null;
            _Router = null;
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            _Data = new HubData(this);
            SimManager.AllHubs.Add(UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            _jobGenerator = new JobGenerator(this, JobGenerationRate);
            StartCoroutine(_jobGenerator.Generate());
        }
        #endregion

        #region Properties
        public Collider Collider
        {
            get
            {
                if (_Collider == null) _Collider = GetComponent<Collider>();
                return _Collider;
            }
        }

        public HubCollisionController CollisionController
        {
            get
            {
                if (_CollisionController == null)
                {
                    _CollisionController = GetComponentInChildren<HubCollisionController>();
                }
                return _CollisionController;
            }
        }

        public DeploymentPath DronePath
        {
            get
            {
                if (_DronePath == null)
                    _DronePath = transform.GetComponentInChildren<DeploymentPath>();
                return _DronePath;
            }

        }
        public Pathfinder Router
        {
            get
            {
                if (_Router == null)
                    _Router = new Raypath();
                return _Router;
            }
        }
        public JobScheduler Scheduler
        {
            get
            {
                if (_Scheduler == null) _Scheduler = transform.GetComponentInChildren<JobScheduler>();
                return _Scheduler;
            }
        }

        public void AddToDeploymentQueue(Drone drone) => DronePath.AddToDeploymentQueue(drone);
        public Vector3 Position => transform.position;
        #endregion

        public void UpdateEnergy(float dE)
        {
            _Data.energyConsumption += dE;
            SimManager.UpdateEnergy(dE);
        }

        internal void DeleteJob(Job job)
        {
            _Data.incompleteJobs.Remove(job);
            _Data.completedCount++;
            SimManager.UpdateCompleteCount();
            SimManager.AllIncompleteJobs.Remove(job);
            SimManager.AllJobs.Remove(job);
        }

        public void UpdateRevenue(float value)
        {
            _Data.revenue += value;
            SimManager.UpdateRevenue(value);
        }
        public void UpdateDelay(float dt)
        {
            _Data.delay += dt;
            if (dt > 0) UpdateDelayCount();
            SimManager.UpdateDelay(dt);
        }
        private void UpdateDelayCount() => _Data.delayedJobs++;
        public void UpdateFailedCount() 
        { 
            _Data.failedJobs++;
            SimManager.UpdateFailedCount();
        }
        public void UpdateCrashCount() 
        {
            _Data.crashes++;
            SimManager.UpdateCrashCount();
        }
        public void UpdateAudible(float dt)
        {
            _Data.audibility += dt;
            SimManager.UpdateAudible(dt);
        }
        public void JobComplete(Job job) => _Data.completedJobs.Add(job.UID, job);
        public SecureSortedSet<uint, IDataSource> Drones => _Data.drones;
        public float JobGenerationRate
        {
            get => _jobGenerationRate;

            set
            {
                _jobGenerationRate = value;
                _jobGenerator.SetLambda(value);
            }
        }
        public void OnJobCreate(params Job[] jobs)
        {
            foreach (Job job in jobs)
            {
                _Data.incompleteJobs.Add(job.UID, job);
                Scheduler.AddToQueue(job);
            }
        }
        void Awake() => _Data = new HubData();

        #region Drone/Battery Interface
        public void DeployDrone(Drone drone)
        {
            _Data.freeDrones.Remove(drone);
            GetBatteryForDrone(drone);
            var bat = drone.GetBattery();
            StopCharging(bat);
            bat.SetStatus(BatteryStatus.Discharge);
            drone.Deploy();
        }

        public void OnDroneReturn(Drone drone)
        {
            if (drone != null && _Data.freeDrones.Add(drone.UID, drone))
            {
                _Data.chargingBatteries.Add(drone.GetBattery().UID, drone.GetBattery());
            }
            drone.WaitForDeployment();
            Scheduler.AddToQueue(drone);
        }

        private void RemoveBatteryFromDrone(Drone drone)
        {
            if (drone.GetHub() == this && _Data.freeBatteries.Add(drone.GetBattery().UID, drone.GetBattery()))
            {
                _Data.chargingBatteries.Add(drone.GetBattery().UID, drone.GetBattery());
                drone.AssignBattery(null);
            }
        }

        public void ReassignDrone(Drone drone, Hub hub)
        {
            _Data.drones.Remove(drone);
            _Data.batteries.Remove(drone.GetBattery());
            drone.AssignHub(hub);
            hub._Data.drones.Add(drone.UID, drone);
            hub._Data.batteries.Add(drone.GetBattery().UID, drone.GetBattery());
            drone.GetBattery().AssignHub(hub);
        }

        public void StopCharging(Battery battery) => _Data.chargingBatteries.Remove(battery.UID);

        private void GetBatteryForDrone(Drone drone)
        {
            if (drone.GetBattery() != null) return;

            if (_Data.drones.Count >= _Data.batteries.Count)
            {
                drone.AssignBattery(BuyBattery(drone));
            }
            else
            {
                drone.AssignBattery(_Data.freeBatteries.GetMax(true));
                drone.GetBattery().AssignDrone(drone);
            }
        }

        public Drone BuyDrone()
        {
            Drone drone = Drone.New();
            Scheduler.AddToQueue(drone);
            drone.transform.position = transform.position;
            GetBatteryForDrone(drone);
            _Data.drones.Add(drone.UID, drone);
            _Data.freeDrones.Add(drone.UID, drone);
            return drone;
        }

        public void SellDrone()
        {
            if (_Data.freeDrones.Count > 0)
            {
                Drone drone = _Data.freeDrones.GetMin(false);
                var dd = new RetiredDrone(drone);
                SimManager.AllRetiredDrones.Add(dd.UID, dd);
                _Data.drones.Remove(drone);
                drone.Delete();
            }
        }

        public void DestroyBattery(Battery battery) => _Data.batteries.Remove(battery);

        public Battery BuyBattery(Drone drone = null)
        {
            var bat = new Battery(drone, this);
            _Data.batteries.Add(bat.UID, bat);

            if (drone is null) _Data.freeBatteries.Add(bat.UID, bat);
            return bat;
        }

        public void SellBattery()
        {
            if (_Data.freeBatteries.Count > 0)
            {
                var bat = _Data.freeBatteries.GetMin(true);
                _Data.batteries.Remove(bat);
            }
        }
        #endregion

        public SHub Serialize() => new SHub(_Data, this);

        public static Hub Load(SHub data, List<SDrone> drones, List<SBattery> batteries)
        {
            Hub hub = PoolController.Get(ObjectPool.Instance).Get<Hub>(null, true);
            hub.transform.position = data.position;
            hub.transform.SetParent(null);
            hub.gameObject.SetActive(true);
            hub.InPool = false;
            SimManager.AllHubs.Add(data.uid, hub);
            hub._Data = new HubData(data, hub, drones, batteries);
            hub._jobGenerator = new JobGenerator(hub, data.generationRate);
            hub.JobGenerationRate = data.generationRate;
            hub.StartCoroutine(hub._jobGenerator.Generate());
            foreach (var d in hub._Data.freeDrones.Values)
            {
                hub.Scheduler.AddToQueue(d);
            }
            return hub;
        }

    }
}