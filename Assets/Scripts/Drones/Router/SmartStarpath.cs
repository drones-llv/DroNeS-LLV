using System;
using System.Collections.Generic;
using System.Globalization;
using Drones.Event_System;
using Drones.Managers;
using Drones.Objects;
using UnityEngine;
using Utils;
// ReSharper disable ForCanBeConvertedToForeach

namespace Drones.Router
{

    public class SmartStarpath : Pathfinder
    {
        #region Constant
        private const float Epsilon = 0.1f; 
        #endregion

        #region Fields
        private float[] _hubAlts;
        private float[] _altitudes;
        private int[] _assigned;
        private readonly PriorityQueue<Vertex> _frontier; // TBA
        private readonly Dictionary<Vertex, float> _costSoFar; // TBA
        private readonly HashSet<Vertex> _invalidVertices;
        private readonly Dictionary<Vertex, Vertex> _cameFrom; // TBA
        private readonly List<Vertex> _neighbours; // TBA
        private readonly HashSet<Obstacle> _visitedObstacles; // TBA
        private bool _hubReturn;
        private Ray _ray;
        private float _chosenAltitude;
        private Vector3 _origin;
        private Vector3 _destination;
        private RaycastHit _info;
        private Vertex _end;
        #endregion

        #region Properties
        private static float DroneCount => SimManager.AllDrones.Count;
        private float[] Altitudes
        {
            get
            {
                if (_altitudes != null) return _altitudes;
                const int size = (int)((MaxAlt - MinAlt) / AltDivision) + 1;
                _altitudes = new float[size];
                for (var i = 0; i < size; i++) _altitudes[i] = MinAlt + i * AltDivision;
                return _altitudes;
            }
        }
        
        private float[] HubAlt
        {
            get
            {
                if (_hubAlts != null) return _hubAlts;
                const int size = (int)((HubMaxAlt - HubMinAlt) / AltDivision) + 1;
                _hubAlts = new float[size];
                for (var i = 0; i < size; i++) _hubAlts[i] = HubMinAlt + i * AltDivision;
                return _hubAlts;
            }
        }
        private int[] Assigned
        {
            get
            {
                if (_assigned != null) return _assigned;
                const int size = (int)((MaxAlt - MinAlt) / AltDivision) + 1;
                _assigned = new int[size];
                for (var i = 0; i < size; i++) _assigned[i] = 0;
                return _assigned;
            }
        }
        #endregion

        public SmartStarpath(uint hub) : base(hub)
        {
            _frontier = new PriorityQueue<Vertex>();
            _costSoFar = new Dictionary<Vertex, float>();
            _cameFrom = new Dictionary<Vertex, Vertex>();
            _invalidVertices = new HashSet<Vertex>();
            _visitedObstacles = new HashSet<Obstacle>();
            _neighbours = new List<Vertex>{Capacity = 128};
            if (Buildings != null) { }
        }

        private int CountAt(int i)
        {
            var count = 0;
            for (var j = 0; j < Drone.ActiveDrones.childCount; j++)
            {
                // ReSharper disable once PossibleLossOfFraction
                if (Altitudes[i] - AltDivision / 2 < Drone.ActiveDrones.GetChild(j).position.y &&
                    // ReSharper disable once PossibleLossOfFraction
                    Altitudes[i] + AltDivision / 2 > Drone.ActiveDrones.GetChild(j).position.y)
                    count++;
            }
            return count;
        }

        private void UpdateGameState()
        {
            for (var i = 0; i < Altitudes.Length; i++) Assigned[i] = CountAt(i);
        }

        private void ChooseAltitude()
        {
            var max = float.MinValue;
            var start = ((_destination - _origin).z > 0) ? 0 : 1; // North bound => even; South bound => odd
            var index = Assigned.Length - 1;
            for (var i = start; i < Assigned.Length; i += 2)
            {
                var tmp = -(Assigned[i] / DroneCount) / (Altitudes[i] / MaxAlt);
                if (tmp <= max) continue;
                max = tmp;
                index = i;
            }
            _chosenAltitude = Altitudes[index];
            _origin.y = _chosenAltitude;
            _destination.y = _chosenAltitude;
        }

        private void MaxAltitude()
        {
            _chosenAltitude = HubMaxAlt;
            _origin.y = _chosenAltitude;
            _destination.y = _chosenAltitude;
        }

        public Queue<Vector3> GetRouteTest(Vector3 origin, Vector3 dest)
        {
            var tmp = GameObject.FindGameObjectsWithTag("NoFlyZone");
            Nfz = new Dictionary<uint, Obstacle>();
            foreach (var i in tmp)
            {
                NoFlyZones.Add(1, new Obstacle(i.transform, Rd));
            }
            
            Path = new Queue<Vector3>();
            _destination = dest;
            _origin = origin;
            _hubReturn = false;
            _chosenAltitude = 250;
            
            try
            {
                var t = System.Diagnostics.Stopwatch.StartNew();
                if (!AStarSearch()) throw new Exception("Failed");
                Debug.Log((t.ElapsedMilliseconds / 1000f).ToString(CultureInfo.CurrentCulture));

                return Path;
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
                return Path;
            }
        }
        
        public override void GetRoute(Drone drone)
        {
            UpdateGameState();
            Path = drone.WaypointsQueue;
            var job = drone.GetJob();
            _destination =
                job == null || job.Status == JobStatus.Pickup ? GetHub().Position :
                job.Status == JobStatus.Delivering ? job.DropOff :
                GetHub().Position;
            _origin = drone.transform.position;
            _hubReturn = job == null || job.Status == JobStatus.Pickup;
            
            ChooseAltitude();
            var i = 0;
            while (i < 2)
            {
                if (AStarSearch())
                {
                    drone.StartMoving();
                    return;
                }
                MaxAltitude();
                i++;
            }
            // Log Failed
            FailSafe(drone, job);
        }

        private void FailSafe(Drone drone, Job job)
        {
            if (!_hubReturn)
            {
                if (job != null)
                {
                    DebugLog.New($"Failed to route {drone}, failing job {job.Name}");
                    job.FailJob();
                }
                GetHub().GetNewJob(drone);
            }
            DebugLog.New($"Failed to route {drone}, not on job");
            MaxAltitude();
            GeneratePath(ref _end);
            drone.StartMoving();
        }

        private void Initialize()
        {
            Path.Clear();
            _frontier.Clear();
            _costSoFar.Clear();
            _cameFrom.Clear();
            _invalidVertices.Clear();
            _origin.y = _chosenAltitude;
            _destination.y = _chosenAltitude;
            _end = new Vertex{point = _destination, index = -1, owner = int.MinValue};
            _frontier.Enqueue(_end, 0);
            _costSoFar.Add(_end, 0);
        }

        private bool AStarSearch()
        {
            Initialize();
            while (!_frontier.IsEmpty())
            {
                _neighbours.Clear();
                _visitedObstacles.Clear();
                var current = _frontier.Dequeue();
                if (IsClear(current.point, _origin))
                {
                    if (_hubReturn) GeneratePathToHub(ref current); 
                    else GeneratePathToJob(ref current);
                    return true;
                }
                AddNeighbours();

                for (var i = 0; i < _neighbours.Count; i++)
                {
                    var next = _neighbours[i];
                    if (_invalidVertices.Contains(next)) continue;
                    float cost = - 1;
                    if (IsClear(current.point, next.point)) cost = _costSoFar[current] + Vector3.Distance(next.point, current.point) * 100;
                    else 
                    {
                        if (!IsClear(next)) _invalidVertices.Add(next);
                        AddNeighbours();
                    }
                    if (cost < 0) continue;

                    var b = _costSoFar.ContainsKey(next);
                    if (b && cost >= _costSoFar[next]) continue;
                    
                    if (b) _costSoFar[next] = cost;
                    else _costSoFar.Add(next, cost);

                    if (_cameFrom.ContainsKey(next)) _cameFrom[next] = current;
                    else _cameFrom.Add(next, current);
                    
                    _frontier.Enqueue(next, Mathf.FloorToInt(cost + Vector3.Distance(next.point, _origin) * 100));
                }
            }
            return false;
        }

        private void AddNeighbours()
        {
            if (_neighbours.Count > _neighbours.Capacity - 4) return;
            var o = Obstacle.Accessor[_info.collider];
            if (_visitedObstacles.Contains(o)) return;
            _visitedObstacles.Add(o);
            for (var j = 0; j < 4; j++)
            {
                var vertex = o.vertices[j];
                if (_invalidVertices.Contains(vertex) || Vector3.Distance(_ray.origin, vertex.point) < Epsilon) continue;
                vertex.point.y = _chosenAltitude;
                _neighbours.Add(vertex);
            }
        }

        private void GeneratePathToHub(ref Vertex current)
        {
            Path.Enqueue(_origin);
            Vector3 i;
            if (current != _end)
            {
                while (_cameFrom[current] != _end)
                {
                    Path.Enqueue(current.point);
                    current = _cameFrom[current];
                }
                Path.Enqueue(current.point);
                i = _destination - current.point;
                i -= i.normalized * 5.5f;
                i = current.point + i;
            }
            else
            {
                i = _destination - _origin;
                i -= i.normalized * 5.5f;
                i = _origin + i;
            }
            Path.Enqueue(i);
            i.y = 500;
            Path.Enqueue(i);
            _destination.y = 500;
            Path.Enqueue(_destination);
        }
        
        private void GeneratePathToJob(ref Vertex current)
        {
            var i = _origin;
            i.y = 500;
            i += Vector3.Normalize(current.point - _origin) * 3;
            Path.Enqueue(i);
            i.y = current.point.y;
            Path.Enqueue(i);
            Path.Enqueue(current.point);
            while (_cameFrom.TryGetValue(current, out current))
            {
                Path.Enqueue(current.point);
            }
            _destination.y = 5;
            Path.Enqueue(_destination);
        }
        
        private void GeneratePath(ref Vertex current)
        {
            Path.Enqueue(_origin);
            Path.Enqueue(current.point);
            while (_cameFrom.TryGetValue(current, out current))
            {
                Path.Enqueue(current.point);
            }
            var v = _destination;
            v.y = _hubReturn ? 500 : 5;
            Path.Enqueue(v);
        }

        private bool IsIntersect(float distance)
        {
            return Physics.SphereCast(_ray, 0.5f, out _info, distance, 1 << 12 | 1 << 15);
        }

        private bool IsClear(Vector3 start, Vector3 end)
        {
            var d = end - start;
            _ray.origin = start;
            _ray.direction = d.normalized;
            return !IsIntersect(d.magnitude);
        }

        private bool IsClear(Vertex p)
        {
            var s = p.point;
            s.y = 1000;
            return !Physics.SphereCast(new Ray(s,Vector3.down), 0.5f, 1000 - _chosenAltitude + 1, 1 << 12 | 1 << 15);
        }
    }

}