using UnityEngine;
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

    public class Drone : MonoBehaviour, IDataSource, IPoolable
    {
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
            }
            if (_Data.hub != 0) SetWaypoints(GetHub().Router.GetRoute(this));
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
        public MovementInfo GetMovementInfo(MovementInfo info)
        {
            info.moveType = _Data.movement;
            info.height = Waypoint.y;
            info.waypoint = _Data.currentWaypoint;
            info.isWaiting = _Data.isWaiting ? 1 : 0;
            info.prev_pos = PreviousPosition;

            return info;
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

        private void NextWaypoint()
        {
            _Data.distanceTravelled += Vector3.Distance(_Data.previousWaypoint, _Data.currentWaypoint);
            _Data.previousWaypoint = _Data.currentWaypoint;
            _Data.currentWaypoint = _Data.waypoints.Dequeue();
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = _Data.currentWaypoint;
            a.y = b.y = 0;
            return Vector3.Distance(a, b) < 0.25f;
        }

        private bool ReachedAltitude()
        {
            return _Data.movement == DroneMovement.Ascend && transform.position.y >= Waypoint.y ||
                _Data.movement == DroneMovement.Descend && transform.position.y <= Waypoint.y;
        }

        public void SetWaypoints(Queue<Vector3> waypoints)
        {
            _Data.waypoints = waypoints;

            if (InHub) GetHub().AddToDeploymentQueue(this);

            //_Data.movement = DroneMovement.Hover;
            StartCoroutine(Horizontal());
        }

        public void Drop()
        {
            _Data.movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject)
                AbstractCamera.ActiveCamera.BreakFollow();
        }

        IEnumerator Horizontal(bool load = false)
        {
            var wait = new WaitUntil(() => ReachedWaypoint());
            while (_Data.waypoints.Count > 0)
            {
                if (!load) NextWaypoint();
                if (Mathf.Abs(transform.position.y - Waypoint.y) > 0.5f)
                {
                    _Data.movement = (transform.position.y > Waypoint.y) ? DroneMovement.Descend : DroneMovement.Ascend;
                    StartCoroutine(Vertical());
                    yield break;
                }
                _Data.movement = DroneMovement.Horizontal;
                yield return wait;
                _Data.movement = DroneMovement.Hover;
            }
            if (InHub)
            {
                _Data.movement = DroneMovement.Horizontal;
                yield return wait;
                _Data.movement = DroneMovement.Idle;
                GetHub().OnDroneReturn(this);
            }
        }

        IEnumerator Vertical()
        {
            yield return new WaitUntil(() => ReachedAltitude());
            _Data.movement = DroneMovement.Hover;
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
            d.y = transform.position.y;
            return Vector3.Distance(d, transform.position) < 0.25f;
        }

        public SDrone Serialize() => new SDrone(_Data, this);

        public StrippedDrone Strip() => new StrippedDrone(_Data, this);

        public Drone LoadState(SDrone data)
        {
            _Data = new DroneData(data, this);
            InPool = false;
            if (_Data.battery != 0) GetBattery().AssignDrone(this);
            StartCoroutine(Horizontal(true));
            if (data.isActive) transform.SetParent(ActiveDrones);
            return this;
        }

    };
}
