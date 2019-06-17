﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Drones
{
    using Managers;
    using DataStreamer;
    using Interface;
    using Serializable;
    using UI;
    using Utils;
    using Data;
    using Utils.Jobs;
    using Drones.Utils.Scheduler;
    using System;

    public class Drone : MonoBehaviour, IDataSource, IPoolable
    {
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static Transform _ActiveDrones;
        public static Transform ActiveDrones
        {
            get
            {
                if (_ActiveDrones == null)
                {
                    _ActiveDrones = new GameObject
                    {
                        name = "ActiveDrones"
                    }.transform;
                    DontDestroyOnLoad(_ActiveDrones.gameObject);
                }
                return _ActiveDrones;
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
            _Data = new DroneData();
        }
        public void OnRelease()
        {
            StopAllCoroutines();
            SimManager.AllDrones.Remove(this);
            InPool = true;
            InfoWindow?.Close.onClick.Invoke();
            GetBattery()?.Destroy();
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
        }

        public void OnGet(Transform parent = null)
        {
            _Data = new DroneData(this);
            SimManager.AllDrones.Add(_Data.UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            InPool = false;
        }

        public bool InPool { get; private set; }
        #endregion

        #region IDataSource
        public bool IsDataStatic => _Data.IsDataStatic;

        public AbstractInfoWindow InfoWindow { get; set; }

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_Data);

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
        public uint UID => _Data.UID;
        public string Name => "D" + _Data.UID.ToString("000000");

        public bool AssignJob(Job job)
        {
            if (job == null)
            {
                _Data.job = 0;
            }
            else
            {
                var j = (StrippedJob)job;
                var t = j.expectedDuration;
                if (Mathf.Min(t, 0.9f * CostFunction.GUARANTEE) > GetBattery().Charge * CostFunction.GUARANTEE)
                {
                    GetHub().Scheduler.AddToQueue(this);
                    return false;
                }
                _Data.job = job.UID;
                job.AssignDrone(this);
                job.StartDelivery();
                if (_Data.hub != 0) StartCoroutine(DeliverySequence());
            }
            return true;
        }

        public void AssignBattery(Battery battery)
        {
            if (battery == null)
            {
                _Data.batterySwaps++;
                _Data.battery = 0;
            }
            else
                _Data.battery = battery.UID;
        }

        public void AssignHub(Hub hub)
        {
            if (hub == null) return;
            _Data.hubsAssigned++;
            _Data.hub = hub.UID;
        }

        public void CompleteJob(Job job)
        {
            //JobHistory.Add(_Data.job, job);
            GetHub().DeleteJob(job);
            UpdateDelay(job.Deadline.Timer());
            GetHub().UpdateRevenue(job.Earnings);
            AssignJob(null);
        }

        public Job GetJob() => (Job)SimManager.AllIncompleteJobs[_Data.job];
        public Hub GetHub() => (Hub)SimManager.AllHubs[_Data.hub];
        public Battery GetBattery() => SimManager.AllBatteries[_Data.battery];
        public void WaitForDeployment() => _Data.isWaiting = true;
        public void Deploy() => _Data.isWaiting = false;
        public void UpdateDelay(float dt)
        {
            _Data.totalDelay += dt;
            GetHub().UpdateDelay(dt);
        }
        public void UpdateEnergy(float dE)
        {
            _Data.totalEnergy += dE;
            GetHub().UpdateEnergy(dE);
        }
        public void UpdateAudible(float dt)
        {
            _Data.audibleDuration += dt;
            GetHub().UpdateAudible(dt);
        }
        public EnergyInfo GetEnergyInfo(ref EnergyInfo info)
        {
            info.moveType = _Data.movement;
            info.pkgWgt = (_Data.job == 0) ? 0 : GetJob().PackageWeight;

            return info;
        }

        #region Fields
        private DroneData _Data;
        [SerializeField]
        private DroneCollisionController _CollisionController;
        #endregion

        #region Drone Properties
        public DroneCollisionController CollisionController
        {
            get
            {
                if (_CollisionController == null)
                {
                    _CollisionController = GetComponent<DroneCollisionController>();
                }
                return _CollisionController;
            }
        }
        public bool InHub => CollisionController.InHub;
        public DroneMovement Movement => _Data.movement;
        public Vector3 Direction => _Data.Direction;
        public float JobProgress => _Data.JobProgress;
        public SecureSortedSet<uint, IDataSource> JobHistory => _Data.completedJobs;
        public Vector3 Waypoint => _Data.currentWaypoint;
        public Vector3 PreviousPosition
        {
            get => _Data.previousPosition;
            set => _Data.previousPosition = value;
        }
        #endregion

        public void SelfDestruct()
        {
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();

            Explosion.New(transform.position);
            var dd = new RetiredDrone(this);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            Delete();
        }

        #region Movement Engine
        IEnumerator DeliverySequence()
        {
            if (InHub)
            {
                GetHub().AddToDeploymentQueue(this);
                yield return StartCoroutine(LeaveHub());
            }
            yield return StartCoroutine(NavigateToJob());
            yield return StartCoroutine(DropOff());
            yield return StartCoroutine(ReturnToHub());
        }

        public void Drop()
        {
            StopAllCoroutines();
            _Data.movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject)
                AbstractCamera.ActiveCamera.BreakFollow();
        }

        IEnumerator LeaveHub()
        {
            yield return new WaitUntil(() => !_Data.isWaiting);
            _Data.movement = DroneMovement.Descend;
            while (transform.position.y < Constants.cruisingAltitude)
            {
                transform.Translate(Vector3.down * Constants.droneVerticalSpeed * Time.deltaTime * TimeKeeper.Timescale);
                yield return null;
            }
            // Cap the position
            transform.position = new Vector3(transform.position.x, Constants.cruisingAltitude, transform.position.z);
        }

        IEnumerator NavigateToJob()
        {
            _Data.movement = DroneMovement.Hover;
            Vector3 dest = GetJob().DropOff;
            dest.y = transform.position.y;

            _Data.velocity = (dest - transform.position).normalized * Constants.droneHorizontalSpeed;
            _Data.acceleration = new Vector3(0, 0, 0);

            while (!ReachedJob())
            {
                MoveTo(dest);
                yield return null;
            }
            transform.position = new Vector3(GetJob().DropOff.x, transform.position.y, GetJob().DropOff.z);
        }

        IEnumerator DropOff()
        {
            // Descend the drone
            _Data.movement = DroneMovement.Descend;
            while (transform.position.y >= Constants.droneDescendLevel)
            {
                float step = - Constants.droneVerticalSpeed * Time.deltaTime * TimeKeeper.Timescale;
                transform.Translate(0, step, 0);
                yield return null;
            }
            // Complete the job
            GetJob().CompleteJob();

            // Reascend to the return-to-hub altitude
            _Data.movement = DroneMovement.Ascend;
            while (transform.position.y <= Constants.returnToHubAltitude)
            {
                float step = Constants.droneVerticalSpeed * Time.deltaTime * TimeKeeper.Timescale;
                transform.Translate(0, step, 0);
                yield return null;
            }
            // Cap this position
            transform.position = new Vector3(transform.position.x, Constants.returnToHubAltitude, transform.position.z);
        }

        IEnumerator ReturnToHub()
        {
            _Data.movement = DroneMovement.Hover;
            Vector3 hub = GetHub().Position;
            hub.y = transform.position.y;

            while (!ReachedHub())
            {
                MoveTo(hub);
                yield return null;
            }
            _Data.movement = DroneMovement.Idle;
            GetHub().OnDroneReturn(this);
        }

        private bool ReachedJob()
        {
            var d = GetJob().DropOff;
            d.y = transform.position.y;
            return Vector3.Distance(d, transform.position) < 0.25f;
        }

        private bool ReachedHub()
        {
            var d = GetHub().Position;
            d.y = transform.position.y;
            return Vector3.Distance(d, transform.position) < 0.25f;
        }
        #endregion

        #region Obstacle Avoidance
        private int CheckSensors()
        {
            int avoidDirection = 0;
            Ray frontSensor = new Ray(transform.position, _Data.velocity);
            Ray leftSensor = new Ray(transform.position, Quaternion.Euler(0, -45, 0) * _Data.velocity);
            Ray rightSensor = new Ray(transform.position, Quaternion.Euler(0, 45, 0) * _Data.velocity);
            RaycastHit info;

            // Left sensor
            if (Physics.Raycast(leftSensor, out info, Constants.droneLeftSensorRange))
            {
                if (!info.collider.CompareTag("Drone"))
                {
                    Debug.DrawLine(transform.position, info.point, Color.red);
                    avoidDirection += 1;
                }
            }

            // Right sensor
            if (Physics.Raycast(rightSensor, out info, Constants.droneRightSensorRange))
            {
                if (!info.collider.CompareTag("Drone"))
                {
                    Debug.DrawLine(transform.position, info.point, Color.red);
                    avoidDirection -= 1;
                }
            }

            // Front sensor
            if (Physics.Raycast(frontSensor, out info, Constants.droneFrontSensorRange))
            {
                if (!info.collider.CompareTag("Drone"))
                {
                    Debug.DrawLine(transform.position, info.point, Color.red);
                    avoidDirection =
                        avoidDirection != 0 ? avoidDirection * 2 :
                        info.normal.x < 0 ? 1 : -1;
                }
            }
            return avoidDirection;
        }

        private Vector3 Seek(Vector3 target)
        {
            Vector3 desired = target - transform.position;
            float distance = desired.magnitude;
            desired = desired.normalized * Constants.droneHorizontalSpeed;

            if (distance < Constants.droneApproachRadius)
            {
                desired *= distance / Constants.droneApproachRadius;
            }

            Vector3 steer = desired - _Data.velocity;

            if (steer.magnitude > Constants.droneSeekForce)
            {
                steer = steer.normalized * Constants.droneSeekForce;
            }
            return steer;
        }

        private void MoveTo(Vector3 target)
        {
            int m = CheckSensors();
            if (m == 0)
            {
                _Data.acceleration = Seek(target);
            }
            else
            {
                _Data.acceleration = Avoid(m);
            }

            // Increment the velocity vector
            _Data.velocity += _Data.acceleration * Time.deltaTime * TimeKeeper.Timescale;
            // Normalize if above the max speed
            if (_Data.velocity.magnitude > Constants.droneHorizontalSpeed)
            {
                _Data.velocity = _Data.velocity.normalized * Constants.droneHorizontalSpeed;
            }
            // Increment the position vector
            transform.position += _Data.velocity * Time.deltaTime * TimeKeeper.Timescale;
            transform.rotation = Quaternion.LookRotation(_Data.velocity);
        }

        private Vector3 Avoid(int m)
        {
            Vector3 steer = Quaternion.Euler(0, 45 * m, 0) * _Data.velocity * Constants.droneAvoidanceStrenth;
            return steer;
        }
        #endregion

        public SDrone Serialize() => new SDrone(_Data, this);

        public StrippedDrone Strip() => new StrippedDrone(_Data, this);

        public Drone LoadState(SDrone data)
        {
            throw new NotImplementedException();
        }
    }
}
