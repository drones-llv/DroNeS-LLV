using System;
using System.Collections.Generic;
using Drones.Managers;
using Drones.Objects;
using UnityEngine;
using Utils;
// ReSharper disable PossibleLossOfFraction

namespace Drones.Router
{
    public struct Neighbour
    {
        public Vector2Int node;
        public int isValid;
        public bool IsValid() => isValid != 0;
    }

    public class Starpath : Pathfinder
    {
        #region Constants
        private readonly Vector2Int _null = new Vector2Int(-1, -1);
        private const float BitmapHeightScale = 500f;
        #endregion

        #region Fields
        private float[] _altitudes;
        private int[] _assigned;
        private float[] _hubAlts;
        private readonly Texture2D _map; // TBA
        private readonly int _height; // TBA
        private readonly int _width; // TBA
        private readonly PriorityQueue<Vector2Int> _frontier; // TBA
        private readonly Dictionary<Vector2Int, float> _costSoFar; // TBA
        private readonly Dictionary<Vector2Int, Vector2Int> _cameFrom; // TBA
        private readonly List<Vector3> _reversePath; // TBA
        private Neighbour[] _neighbours; // TBA
        private bool _hubReturn;
        private Ray _casted;
        private Neighbour _replacer;
        private float _chosenAltitude;
        private Vector3 _origin;
        private Vector3 _destination;
        private Vector2Int _start;
        private Vector2Int _end;
        private int _meter;
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

        public Starpath(int meter, uint hub) : base(hub)
        {
            _meter = meter;
            _map = Resources.Load($"Textures/height_bitmap_test_{_meter}m") as Texture2D;
            if (_map != null)
            {
                _height = _map.height;
                _width = _map.width;
            }
            else
            {
                throw new ApplicationException("Cannot find texture file!");
            }
            _frontier = new PriorityQueue<Vector2Int>();
            _costSoFar = new Dictionary<Vector2Int, float>();
            _cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            _reversePath = new List<Vector3>();
            _neighbours = new Neighbour[4];
        }


        private int CountAt(int i)
        {
            var count = 0;
            for (var j = 0; j < Drone.ActiveDrones.childCount; j++)
            {
                if (Altitudes[i] - AltDivision / 2 < Drone.ActiveDrones.GetChild(j).position.y &&
                    Altitudes[i] + AltDivision / 2 > Drone.ActiveDrones.GetChild(j).position.y)
                    count++;
            }
            return count;
        }

        private void UpdateGameState()
        {
            for (var i = 0; i < Altitudes.Length; i++)
            {
                Assigned[i] = CountAt(i);
            }
        }

        private bool IsNeighbourValid(in Vector2Int origin, in Vector2Int dir) 
        {
            _replacer.node = origin + dir;
            if (OutOfRange(in _replacer.node))
            {
                _replacer.isValid = 0;
                return false;
            }
            foreach (var nfz in NoFlyZones.Values)
            {
                if (!nfz.Contains(ToPosition(in _replacer.node))) continue;
                _replacer.isValid = 0;
                return false;
            }
            foreach (var h in Hubs.Values)
            {
                if (!h.Contains(ToPosition(in _replacer.node))) continue;
                _replacer.isValid = 0;
                return false;
            }
            if (_map.GetPixel(_replacer.node.x, _replacer.node.y).r > (_chosenAltitude - 1) / BitmapHeightScale)
            {
                _replacer.isValid = 0;
                return false;
            }

            _replacer.isValid = 1;
            return true;
        } 

        private static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        private int ChooseAltitude(Vector3 origin, Vector3 dest)
        {
            float max = 0;
            var start = ((dest - origin).z > 0) ? 0 : 1; // North bound => even; South bound => odd

            var maxIndex = Assigned.Length - 1;
            for (var i = start; i < Assigned.Length; i += 2)
            {
                // maximise altitude, minimize traffic, + 1 to prevent singularity
                var tmp = Altitudes[i] / MaxAlt / (Assigned[i] / DroneCount + 1);
                if (!(tmp > max)) continue;
                max = tmp;
                maxIndex = i;
            }

            Assigned[maxIndex]++;
            return maxIndex;
        }

        private Vector2Int ToPixel(Vector3 v)
        {
            return new Vector2Int((int)v.x / _meter + _width / 2 , (int)v.z / _meter + _height / 2);
        }

        private bool OutOfRange(in Vector2Int pixel)
        {
            return pixel.x < 0 || pixel.y < 0 || pixel.x > _width || pixel.y > _height;
        }

        public Queue<Vector3> GetRouteTest(Vector3 origin, Vector3 dest)
        {
            var tmp = GameObject.FindGameObjectsWithTag("NoFlyZone");
            Nfz = new Dictionary<uint, Obstacle>();
            foreach (var i in tmp)
            {
                NoFlyZones.Add(1, new Obstacle(i.transform, Rd));
            }

            _frontier.Clear();
            _costSoFar.Clear();
            _cameFrom.Clear();
            _reversePath.Clear();
            _destination = dest;
            _origin = origin;
            _hubReturn = false;

            _chosenAltitude = 250;
            try
            {
                var v = _destination;
                v.y = _hubReturn ? 500 : 5;
                var t = System.Diagnostics.Stopwatch.StartNew();
                if (!Navigate()) throw new Exception("Failed");
                
                ProcessPath();
                var q = new Queue<Vector3>();
                for (var i = _reversePath.Count - 1; i >= 0; i--)
                    q.Enqueue(_reversePath[i]);

                q.Enqueue(v);

                return q;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return new Queue<Vector3>();
            }

        }

        public override void GetRoute(Drone drone)
        {
            UpdateGameState();
            Path = drone.WaypointsQueue;
            var job = drone.GetJob();
            _destination =
                job == null || job.Status == JobStatus.Pickup ? drone.GetHub().Position :
                job.Status == JobStatus.Delivering ? job.DropOff :
                drone.GetHub().Position;
            _origin = drone.transform.position;
            _hubReturn = job == null || job.Status == JobStatus.Pickup;
            _chosenAltitude = _hubReturn ? HubAlt[(_destination - _origin).z > 0 ? 0 : 1] :
                    Altitudes[ChooseAltitude(_origin, _destination)];

            var v = _destination;
            v.y = _hubReturn ? 500 : 5;
            if (Navigate())
            {
                ProcessPath();
                for (var i = _reversePath.Count - 1; i >= 0; i--)
                    Path.Enqueue(_reversePath[i]);
                    
                Path.Enqueue(v);
                return;
            }
            var y = _hubReturn ? HubAlt[0] : MaxAlt;
            _origin.y = y;
            _destination.y = y;
            Path.Enqueue(_origin);
            Path.Enqueue(_destination);
            Path.Enqueue(v);
        }

        private int Neighbours(in Vector2Int v)
        {
            var c = 0;
            if (IsNeighbourValid(v, Vector2Int.up)) c++;
            _neighbours[0] = _replacer;
            if (IsNeighbourValid(v, Vector2Int.right)) c++;
            _neighbours[1] = _replacer;
            if (IsNeighbourValid(v, Vector2Int.down)) c++;
            _neighbours[2] = _replacer;
            if (IsNeighbourValid(v, Vector2Int.left)) c++;
            _neighbours[3] = _replacer;
            var si = 0;
            var ei = 3;
            while (si < ei && si < c)
            {
                if (_neighbours[si].IsValid()) si++;
                else
                {
                    Swap(ref _neighbours[si], ref _neighbours[ei]);
                    ei--;
                }
            }

            return c;
        }

        private Vector3 ToPosition(in Vector2Int p)
        {
            return new Vector3((p.x - _width / 2) * _meter, _chosenAltitude, (p.y - _height / 2) * _meter);
        }

        private bool AStarSearch(out Vector2Int current)
        {
            _cameFrom.Add(_start, _null);
            _costSoFar.Add(_start, 0);
            _frontier.Enqueue(_start, 0);
            while (!_frontier.IsEmpty())
            {
                current = _frontier.Dequeue();
                if (current == _end) return true;
                var n = Neighbours(in current);
                for (var i = 0; i < n; i++)
                {
                    var next = _neighbours[i].node;
                    var newCost = _costSoFar[current] + Vector2Int.Distance(next, current) * 10;
                    if (_costSoFar.ContainsKey(next) && !(newCost < _costSoFar[next])) continue;
                    
                    if (_costSoFar.ContainsKey(next)) _costSoFar[next] = newCost;
                    else _costSoFar.Add(next, newCost);

                    _frontier.Enqueue(next, Mathf.FloorToInt(newCost + Vector2Int.Distance(next, _end) * 10));

                    if (_cameFrom.ContainsKey(next)) _cameFrom[next] = current;
                    else _cameFrom.Add(next, current);
                }
            }
            current = _null;
            return false;
        }

        private bool Navigate()
        {
            _frontier.Clear();
            _costSoFar.Clear();
            _cameFrom.Clear();
            _reversePath.Clear();

            _origin.y = 0;
            _destination.y = 0;
            _start = ToPixel(_origin);
            _end = ToPixel(_destination);

            if (!AStarSearch(out var current)) return false;
            
            GeneratePath(current);
            return true;
        }

        private void GeneratePath(Vector2Int current)
        {
            _reversePath.Add(ToPosition(current));
            while (_cameFrom.ContainsKey(current))
            {
                current = _cameFrom[current];
                if (current != _null)
                    _reversePath.Add(ToPosition(current));
            }
        }

        private bool IsClear(Vector3 start, Vector3 end)
        {
            var d = end - start;
            _casted.origin = start;
            _casted.direction = d.normalized;
            return !Physics.SphereCast(_casted, 0.5f, d.magnitude, 1 << 12 | 1 << 15| 1 << 11);
        }

        private void ProcessPath()
        {
            int c;
            do
            {
                c = _reversePath.Count;
                for (var i = _reversePath.Count - 1; i > 1; i--)
                {
                    if (IsClear(_reversePath[i], _reversePath[i - 2]))
                    {
                        _reversePath.RemoveAt(i - 1);
                    }
                }
            } while (c != _reversePath.Count);

        }


    }

}