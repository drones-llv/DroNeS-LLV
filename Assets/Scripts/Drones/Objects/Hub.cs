using System.Collections.Generic;
using Drones.Data;
using Drones.Managers;
using Drones.Router;
using Drones.Scheduler;
using Drones.Serializable;
using Drones.UI.Hub;
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

        public string Name => "H" + UID.ToString("000000");

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
        [SerializeField]
        private Pathfinder router;
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
            router = null;
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
        public Pathfinder Router
        {
            get
            {
                if (router == null)
                    router = new Raypath();
                return router;
            }
        }
        public JobScheduler Scheduler
        {
            get
            {
                if (_scheduler == null) _scheduler = transform.GetComponentInChildren<JobScheduler>();
                return _scheduler;
            }
        }

        public void AddToDeploymentQueue(Drone drone) => DronePath.AddToDeploymentQueue(drone);
        public Vector3 Position => transform.position;
        #endregion

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
        public void DeployDrone(Drone drone)
        {
            _data.freeDrones.Remove(drone);
            GetBatteryForDrone(drone);
            var bat = drone.GetBattery();
            StopCharging(bat);
            bat.SetStatus(BatteryStatus.Discharge);
            drone.Deploy();
        }

        public void OnDroneReturn(Drone drone)
        {
            if (_data.freeDrones.Add(drone.UID, drone))
            {
                _data.chargingBatteries.Add(drone.GetBattery().UID, drone.GetBattery());
            }
            drone.WaitForDeployment();
            Scheduler.AddToQueue(drone);
        }

        private void RemoveBatteryFromDrone(Drone drone)
        {
            var battery = drone.GetBattery();
            if (drone.GetHub() != this || !_data.freeBatteries.Add(battery.UID, battery)) return;
            _data.chargingBatteries.Add(battery.UID, battery);
            drone.AssignBattery();
            battery.AssignDrone();
        }

        public void StopCharging(Battery battery) => _data.chargingBatteries.Remove(battery.UID);

        private void GetBatteryForDrone(Drone drone)
        {
            if (drone.GetBattery() != null) return;

            if (_data.drones.Count >= _data.batteries.Count)
            {
                drone.AssignBattery(BuyBattery(drone));
            }
            else
            {
                drone.AssignBattery(_data.freeBatteries.GetMax(true));
                drone.GetBattery().AssignDrone(drone);
            }
        }

        public Drone BuyDrone()
        {
            var drone = Drone.New();
            Scheduler.AddToQueue(drone);
            drone.transform.position = transform.position;
            GetBatteryForDrone(drone);
            _data.drones.Add(drone.UID, drone);
            _data.freeDrones.Add(drone.UID, drone);
            return drone;
        }

        public void SellDrone()
        {
            if (_data.freeDrones.Count <= 0) return;
            var drone = _data.freeDrones.GetMin(false);
            var dd = new RetiredDrone(drone);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            _data.drones.Remove(drone);
            drone.Delete();
        }

        public void DestroyBattery(Battery battery) => _data.batteries.Remove(battery);

        public Battery BuyBattery(Drone drone = null)
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            var bat = new Battery(drone, this);
            _data.batteries.Add(bat.UID, bat);

            if (drone is null) _data.freeBatteries.Add(bat.UID, bat);
            return bat;
        }

        public void SellBattery()
        {
            if (_data.freeBatteries.Count <= 0) return;
            var bat = _data.freeBatteries.GetMin(true);
            _data.batteries.Remove(bat);
        }
        #endregion

        public SHub Serialize() => new SHub(_data, this);

        public static Hub Load(SHub data, List<SDrone> drones, List<SBattery> batteries)
        {
            Hub hub = PoolController.Get(ObjectPool.Instance).Get<Hub>(null, true);
            hub.transform.position = data.position;
            hub.transform.SetParent(null);
            hub.gameObject.SetActive(true);
            hub.InPool = false;
            SimManager.AllHubs.Add(data.uid, hub);
            hub._data = new HubData(data, hub, drones, batteries);
            hub._jobGenerator = new JobGenerator(hub, data.generationRate);
            hub.JobGenerationRate = data.generationRate;
            hub.StartCoroutine(hub._jobGenerator.Generate());
            foreach (var d in hub._data.freeDrones.Values)
            {
                hub.Scheduler.AddToQueue(d);
            }
            return hub;
        }

    }
}