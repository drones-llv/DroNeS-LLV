﻿using System.Collections;
using System.Collections.Generic;
using Drones.Data;
using Drones.JobSystem;
using Drones.Managers;
using Drones.Scheduler;
using Drones.Serializable;
using Drones.UI.Drone;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Drones.Objects
{
    public class Drone : MonoBehaviour, IDataSource, IPoolable
    {
        private static Transform _activeDrones;
        public static Transform ActiveDrones
        {
            get
            {
                if (_activeDrones != null) return _activeDrones;
                _activeDrones = new GameObject
                {
                    name = "ActiveDrones"
                }.transform;
                DontDestroyOnLoad(_activeDrones.gameObject);
                return _activeDrones;
            }
        }

        public static Drone New() => PoolController.Get(ObjectPool.Instance).Get<Drone>(null);

        public static Drone Load(SDrone data)
        {
            var d = PoolController.Get(ObjectPool.Instance).Get<Drone>(null, true);
            d.gameObject.SetActive(true);

            return d.LoadState(data);
        }

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public void Delete() => PC().Release(GetType(), this);
        public void Awake()
        {
            _data = new DroneData();
        }
        public void OnRelease()
        {
            StopAllCoroutines();
            SimManager.AllDrones.Remove(this);
            InPool = true;
            if (InfoWindow != null) InfoWindow.Close.onClick.Invoke();
            GetBattery()?.Destroy();
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
        }

        public void OnGet(Transform parent = null)
        {
            _data = new DroneData(this);
            SimManager.AllDrones.Add(_data.UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            InPool = false;
        }

        public bool InPool { get; private set; }
        #endregion

        #region IDataSource
        public bool IsDataStatic => _data.IsDataStatic;

        public AbstractInfoWindow InfoWindow { get; set; }

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_data);

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = DroneWindow.New();
                InfoWindow.Source = this;
            }
            else
                InfoWindow.transform.SetAsLastSibling();
        }
        #endregion

        public override string ToString() => Name;
        public uint UID => _data.UID;
        public string Name => $"D{_data.UID:000000}";

        public bool AssignJob(Job job)
        {
            var j = (StrippedJob)job;
            var t = j.expectedDuration;
            if (Mathf.Min(t, 0.9f * CostFunction.GUARANTEE) > GetBattery().Charge * CostFunction.GUARANTEE)
            {
                GetHub().Scheduler.AddToQueue(this);
                return false;
            }
            _data.job = job.UID;
            job.AssignDrone(this);
            job.StartDelivery();
            if (_data.hub != 0) GetHub().Router.GetRoute(this, ref _data.waypoints);
            StartDelivery();
            return true;
        }

        public bool AssignJob()
        {
            _data.job = 0;
            if (_data.hub != 0) GetHub().Router.GetRoute(this, ref _data.waypoints);
            StartDelivery();
            return true;
        }

        public void AssignBattery(Battery battery) =>_data.battery = battery.UID;
        public void AssignBattery()
        {
            _data.batterySwaps++;
            _data.battery = 0;
        }
        
        public void AssignHub(Hub hub) 
        {
            _data.hubsAssigned++;
            _data.hub = hub.UID;
        }

        public void CompleteJob(Job job)
        {
            //JobHistory.Add(_Data.job, job);
            GetHub().DeleteJob(job);
            UpdateDelay(job.Deadline.Timer());
            GetHub().UpdateRevenue(job.Earnings);
            AssignJob();
        }

        public Job GetJob() => (Job)SimManager.AllIncompleteJobs[_data.job];
        public Hub GetHub() => (Hub)SimManager.AllHubs[_data.hub];
        public Battery GetBattery() => SimManager.AllBatteries[_data.battery];
        public void WaitForDeployment() => _data.isWaiting = true;
        public void Deploy() => _data.isWaiting = false;

        private void UpdateDelay(float dt)
        {
            _data.totalDelay += dt;
            GetHub().UpdateDelay(dt);
        }
        public void UpdateEnergy(float dE)
        {
            _data.totalEnergy += dE;
            GetHub().UpdateEnergy(dE);
        }
        public void UpdateAudible(float dt)
        {
            _data.audibleDuration += dt;
            GetHub().UpdateAudible(dt);
        }
        public MovementInfo GetMovementInfo(MovementInfo info)
        {
            info.moveType = _data.movement;
            info.height = Waypoint.y;
            info.waypoint = _data.currentWaypoint;
            info.isWaiting = _data.isWaiting ? 1 : 0;
            info.prev_pos = PreviousPosition;

            return info;
        }
        public void GetEnergyInfo(ref EnergyInfo info)
        {
            info.moveType = _data.movement;
            info.pkgWgt = (_data.job == 0) ? 0 : GetJob().PackageWeight;
        }

        #region Fields
        private DroneData _data;
        [FormerlySerializedAs("_CollisionController")] [SerializeField]
        private DroneCollisionController collisionController;
        #endregion

        #region Drone Properties
        public DroneCollisionController CollisionController
        {
            get
            {
                if (collisionController == null)
                {
                    collisionController = GetComponent<DroneCollisionController>();
                }
                return collisionController;
            }
        }
        public bool InHub => CollisionController.InHub;
        public DroneMovement Movement => _data.movement;
        public Vector3 Direction => _data.Direction;
        public float JobProgress => _data.JobProgress;
        public SecureSortedSet<uint, IDataSource> JobHistory => _data.completedJobs;
        public Vector3 Waypoint => _data.currentWaypoint;
        public Vector3 PreviousPosition
        {
            get => _data.previousPosition;
            set => _data.previousPosition = value;
        }
        #endregion

        public void SelfDestruct(bool explosion = false)
        {
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();

            if (explosion) Explosion.New(transform.position);
            var dd = new RetiredDrone(this);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            Delete();
        }

        private void NextWaypoint()
        {
            _data.distanceTravelled += Vector3.Distance(_data.previousWaypoint, _data.currentWaypoint);
            _data.previousWaypoint = _data.currentWaypoint;
            _data.currentWaypoint = _data.waypoints.Dequeue();
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = _data.currentWaypoint;
            a.y = b.y = 0;
            return Vector3.Distance(a, b) < 0.25f;
        }

        private bool ReachedAltitude()
        {
            var position = transform.position;
            return _data.movement == DroneMovement.Ascend && position.y >= Waypoint.y ||
                _data.movement == DroneMovement.Descend && position.y <= Waypoint.y;
        }

        private void StartDelivery()
        {
            if (InHub) GetHub().AddToDeploymentQueue(this);
            StartCoroutine(Horizontal());
        }

        public void Drop()
        {
            _data.movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject)
                AbstractCamera.ActiveCamera.BreakFollow();
        }

        private IEnumerator Horizontal(bool load = false)
        {
            var wait = new WaitUntil(ReachedWaypoint);
            while (_data.waypoints.Count > 0)
            {
                if (!load) NextWaypoint();
                if (Mathf.Abs(transform.position.y - Waypoint.y) > 0.5f)
                {
                    _data.movement = (transform.position.y > Waypoint.y) ? DroneMovement.Descend : DroneMovement.Ascend;
                    StartCoroutine(Vertical());
                    yield break;
                }
                _data.movement = DroneMovement.Horizontal;
                yield return wait;
                _data.movement = DroneMovement.Hover;
            }
            if (!InHub) yield break;
            _data.movement = DroneMovement.Horizontal;
            yield return wait;
            _data.movement = DroneMovement.Idle;
            GetHub().OnDroneReturn(this);
        }

        private IEnumerator Vertical()
        {
            yield return new WaitUntil(ReachedAltitude);
            _data.movement = DroneMovement.Hover;
            if (!InHub)
            {
                if (transform.position.y < 10f && ReachedJob()) 
                    GetJob().CompleteJob();
                else
                    StartCoroutine(Horizontal());
                yield break;
            }
            StartCoroutine(Horizontal());
        }

        private bool ReachedJob()
        {
            var d = GetJob().DropOff;
            var position = transform.position;
            d.y = position.y;
            return Vector3.Distance(d, position) < 0.25f;
        }

        public SDrone Serialize() => new SDrone(_data, this);

        public StrippedDrone Strip() => new StrippedDrone(_data, this);

        private Drone LoadState(SDrone data)
        {
            _data = new DroneData(data, this);
            InPool = false;
            if (_data.battery != 0) GetBattery().AssignDrone(this);
            StartCoroutine(Horizontal(true));
            if (data.isActive) transform.SetParent(ActiveDrones);
            return this;
        }

    };
}
