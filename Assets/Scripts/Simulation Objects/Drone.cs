using UnityEngine;
using System.Collections.Generic;

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
            GetJob()?.FailJob();
            AssignJob(null);
            GetBattery()?.Destroy();
            _Data = null;
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
        }

        public void OnGet(Transform parent = null)
        {
            _Data = new DroneData(this);
            SimManager.AllDrones.Add(_Data.UID, this);
            Trail.enabled = true;
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

        public void AssignJob(Job job)
        {
            if (_Data.job != 0 && job == null)
            {
                _Data.job = 0;
            }
            else if (_Data.job == 0 && job != null)
            {
                _Data.job = job.UID;
                job.AssignDrone(this);
                Pickup(job);
            }
            GetHub()?.Router.AddToQueue(this);
        }

        private void Pickup(Job job) 
        {
            if (InHub)
            {
                var d = Vector3.Normalize(GetJob().DropOff - transform.position) * 4;
                d.y = 0;
                transform.position += d;
                job.StartDelivery();
            }
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
            _Data.completedJobs.Add(_Data.job, job);
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
            info.height = _Data.targetAltitude;
            info.waypoint = _Data.currentWaypoint;
            info.isWaiting = _Data.isWaiting ? 1 : 0;

            return info;
        }
        public EnergyInfo GetEnergyInfo(EnergyInfo info)
        {
            info.moveType = _Data.movement;
            info.pkgXArea = (_Data.job == 0) ? 1 : GetJob().PackageXArea;
            info.pkgWgt = (_Data.job == 0) ? 0 : GetJob().PackageWeight;

            return info;
        }

        #region Fields
        private TrailRenderer _Trail;
        private DroneData _Data;
        #endregion

        #region Drone Properties
        public TrailRenderer Trail
        {
            get
            {
                if (_Trail == null)
                {
                    _Trail = GetComponent<TrailRenderer>();
                }
                return _Trail;
            }
        }
        public bool InHub => _Data.inHub;
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

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer != LayerMask.NameToLayer("Hub") && _Data.collisionOn)
            {
                DroneManager.MovementJobHandle.Complete();
                DestroySelf(other);
            } 
            else if (other.GetComponent<Hub>() == GetHub())
            {
                _Data.inHub = true;
                _Data.collisionOn = false;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Hub")
                && other.GetComponent<Hub>() == GetHub())
            {
                _Data.collisionOn = true;
                _Data.inHub = false;
            }
        }

        private void ChangeAltitude(float height)
        {
            if (_Data.movement == DroneMovement.Hover)
            {
                _Data.movement = (transform.position.y > height) ? DroneMovement.Descend : DroneMovement.Ascend;
                _Data.targetAltitude = height;
            }
        }

        private void MoveTo(Vector3 waypoint)
        {
            if (_Data.movement == DroneMovement.Hover)
            {
                _Data.movement = DroneMovement.Horizontal;
                _Data.distanceTravelled += Vector3.Distance(_Data.previousWaypoint, _Data.currentWaypoint);
                _Data.previousWaypoint = _Data.currentWaypoint;
                _Data.currentWaypoint = waypoint;
            }
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = _Data.currentWaypoint;
            a.y = b.y = 0;

            return Vector3.Distance(a, b) < 0.25f;
        }

        public void NavigateWaypoints(Queue<Vector3> waypoints)
        {
            _Data.waypoints = waypoints;

            if (InHub)
            {
                GetHub().AddToDeploymentQueue(this);
            }

            _Data.movement = DroneMovement.Hover;
            _Data.state = FlightStatus.PreparingHeight;
            ChangeAltitude(_Data.waypoints.Peek().y);
        }

        private void DestroySelf(Collider other)
        {
            GetHub().UpdateCrashCount();
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();

            Explosion.New(transform.position);
            var dd = new RetiredDrone(this, other);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            Delete();
        }

        public void DestroySelf()
        {
            if (gameObject == AbstractCamera.Followee)
                AbstractCamera.ActiveCamera.BreakFollow();

            Explosion.New(transform.position);
            var dd = new RetiredDrone(this);
            SimManager.AllRetiredDrones.Add(dd.UID, dd);
            Delete();
        }

        void ChangeState()
        {
            Job job = GetJob();
            if (_Data.state == FlightStatus.PreparingHeight)
            {
                if (transform.position.y < 15f)
                {
                    if (job != null && job.Status == JobStatus.Delivering && _Data.isGoingDown != _Data.wasGoingDown)
                    {
                        job.CompleteJob();
                        GetHub().Scheduler.AddToQueue(this);
                    }

                    GetHub().Router.AddToQueue(this);
                    return;
                }
                _Data.state = FlightStatus.AwaitingWaypoint;
            }

            if (_Data.state != FlightStatus.AwaitingWaypoint && _Data.state != FlightStatus.Cruising) return;

            if (_Data.waypoints.Count > 0)
            {
                _Data.state = FlightStatus.Cruising;
                _Data.currentWaypoint = _Data.waypoints.Dequeue();
                MoveTo(_Data.currentWaypoint);
                return;
            }

            if (InHub && job == null)
            {
                if (Vector3.Distance(_Data.currentWaypoint, GetHub().Position) < 2.5f)
                {
                    _Data.state = FlightStatus.Idle;
                    _Data.movement = DroneMovement.Idle;
                    GetHub().OnDroneReturn(this);
                }
                return;
            }

            if (job != null)
            {
                if (job.Status != JobStatus.Pickup && job.Status != JobStatus.Delivering) return;
                if (job.Status == JobStatus.Pickup)
                {
                    Pickup(job);
                }
                Vector3 destination =
                    job.Status == JobStatus.Pickup ? job.Pickup :
                    job.Status == JobStatus.Delivering ? job.DropOff :
                    Vector3.zero;

                destination.y = transform.position.y;

                if (Vector3.Distance(transform.position, destination) < 0.1f)
                {
                    destination.y = 10;
                    var q = new Queue<Vector3>();
                    q.Enqueue(destination);
                    NavigateWaypoints(q);
                }
                else
                {
                    GetHub().Router.AddToQueue(this);
                }
            }
        }

        void Update()
        {
            _Data.wasGoingDown = _Data.isGoingDown;
            _Data.isGoingDown = _Data.movement == DroneMovement.Descend;
            if (_Data.movement == DroneMovement.Ascend && transform.position.y >= _Data.targetAltitude ||
                _Data.movement == DroneMovement.Descend && transform.position.y <= _Data.targetAltitude ||
                _Data.movement == DroneMovement.Horizontal && ReachedWaypoint())
            {
                _Data.movement = DroneMovement.Hover;
            }

            if (_Data.movement == DroneMovement.Hover) ChangeState();

            if (_Data.movement != DroneMovement.Idle && GetBattery().Status == BatteryStatus.Dead) Drop();
        }

        void Drop()
        {
            Trail.enabled = false;
            _Data.movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject)
                AbstractCamera.ActiveCamera.BreakFollow();
        }

        public SDrone Serialize() => new SDrone(_Data, this);

        public StrippedDrone Strip() => new StrippedDrone(_Data, this);

        public Drone LoadState(SDrone data)
        {
            _Data = new DroneData(data, this);
            InPool = false;
            transform.position = data.position;
            if (_Data.battery != 0) GetBattery().AssignDrone(this);

            return this;
        }

    };
}
