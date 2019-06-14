using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Router
{
    using Utils;
    using Managers;
    public class Raypath : Pathfinder
    {
        private List<List<Obstacle>> _SortedBuildings;
        private List<Obstacle> _Hubs;
        private const float _maxAlt = 200;
        private const float _minAlt = 60;
        private const int _altDiv = 10; // Altitude interval
        private const int _buildingDiv = 30; // Building bucket height interval
        private const int _Ra = 200; // Corridor width
        private const float _epsilon = 0.01f;
        private readonly int[] _hubAlt = { 475, 525 }; // hub altitudes, 450 for northbound drones, 550 for south
        private float DroneCount => SimManager.AllDrones.Count;
        private float[] _altitudes;
        private int[] _assigned;
        private Vector3 _origin;
        private Vector3 _destination;
        private float[] Altitudes
        {
            get
            {
                if (_altitudes == null)
                {
                    int size = (int)((_maxAlt - _minAlt) / _altDiv) + 1;
                    _altitudes = new float[size];
                    for (int i = 0; i < size; i++)
                        _altitudes[i] = _minAlt + i * _altDiv;
                }
                return _altitudes;
            }
        }
        private int[] Assigned
        {
            get
            {
                if (_assigned == null)
                {
                    int size = (int)((_maxAlt - _minAlt) / _altDiv) + 1;
                    _assigned = new int[size];
                    for (int i = 0; i < size; i++)
                        _assigned[i] = 0;
                }
                return _assigned;
            }
        }
        private List<List<Obstacle>> SortedBuildings
        {
            get
            {
                if (_SortedBuildings == null)
                {
                    int added = 0;
                    int i = 0;
                    float lower, upper;
                    _SortedBuildings = new List<List<Obstacle>>();
                    while (added < Buildings.Count)
                    {
                        _SortedBuildings.Add(new List<Obstacle>());
                        foreach (var building in Buildings)
                        {
                            lower = i * _buildingDiv;
                            upper = (i + 1) * _buildingDiv;
                            if (building.size.y < upper && building.size.y >= lower)
                            {
                                _SortedBuildings[i].Add(building);
                                added++;
                            }
                        }
                        i++;
                    }
                }
                return _SortedBuildings;
            }
        }

        private Queue<Vector3> Route(Drone drone, int stack = 0, float alt = -1)
        {
            UpdateGameState();
            var job = drone.GetJob();

            _destination =
                job == null || job.Status == JobStatus.Pickup ? drone.GetHub().Position :
                job.Status == JobStatus.Delivering ? job.DropOff :
                drone.GetHub().Position;
            _origin = drone.transform.position;
            var hubReturn = job == null || job.Status == JobStatus.Pickup;
            if (alt < 0)
            {
                alt = hubReturn ? _hubAlt[(_destination - _origin).z > 0 ? 0 : 1] :
                    Altitudes[ChooseAltitude(_origin, _destination)];
            }

            _origin.y = 0;
            _destination.y = 0;
            try
            {
                var waypoints = Navigate(_origin, _destination, alt, hubReturn);

                for (int i = 0; i < waypoints.Count; i++)
                {
                    var u = waypoints[i];
                    u.y = alt;
                    waypoints[i] = u;
                }

                var v = _destination;
                v.y = hubReturn ? 500 : 5;
                waypoints.Add(v);

                return new Queue<Vector3>(waypoints);
            }
            catch (StackOverflowException)
            {
                stack++;
                var newalt = hubReturn ? _hubAlt[0] : _maxAlt;
                if (stack > 1)
                {
                    _origin.y = newalt;
                    _destination.y = newalt;
                    var v = _destination;
                    v.y = hubReturn ? 500 : 5;

                    return new Queue<Vector3>(new[] { _origin, _destination, v });
                }

                return Route(drone, stack, newalt);
            }
        }

        // The public interface to get the list of waypoints
        public override Queue<Vector3> GetRoute(Drone drone) => Route(drone);

        // To test: -7.4, 500, 7.0 to -2640.1, 0.0, -5468.1
        // To test: -7.4, 500, 7.0 to -1111.9, 0.0, -2228.0
        public Queue<Vector3> GetRouteTest(Vector3 origin, Vector3 dest)
        {
            var tmp = GameObject.FindGameObjectsWithTag("NoFlyZone");
            _NoFlyZones = new List<Obstacle>();
            foreach (var i in tmp)
            {
                _NoFlyZones.Add(new Obstacle(i.transform, _Rd));
            }
            _destination = dest;
            _origin = origin;
            var hubReturn = false;

            float alt = 250;
            _origin.y = 0;
            _destination.y = 0;
            try
            {
                var waypoints = Navigate(_origin, _destination, alt, hubReturn);

                for (int i = 0; i < waypoints.Count; i++)
                {
                    var u = waypoints[i];
                    u.y = alt;
                    waypoints[i] = u;
                }

                var v = _destination;
                v.y = hubReturn ? 500 : 5;
                waypoints.Add(v);

                return new Queue<Vector3>(waypoints);
            }
            catch (StackOverflowException)
            {
                return new Queue<Vector3>();
            }

        }

        public static int HashVector(Vector3 v) => (v.x.ToString("0.000") + "," + v.z.ToString("0.000")).GetHashCode();

        private int CountAt(int i)
        {
            int count = 0;
            foreach (Transform drone in Drone.ActiveDrones)
            {
                if (Altitudes[i] - _altDiv / 2 < drone.position.y &&
                    Altitudes[i] + _altDiv / 2 > drone.position.y)
                    count++;
            }
            return count;
        }

        public void UpdateGameState()
        {
            _NoFlyZones = new List<Obstacle>();
            _Hubs = new List<Obstacle>();
            foreach (NoFlyZone nfz in SimManager.AllNFZ.Values)
            {
                _NoFlyZones.Add(new Obstacle(nfz.transform, _Rd));
            }
            foreach (Hub hub in SimManager.AllHubs.Values)
            {
                _Hubs.Add(new Obstacle((BoxCollider)hub.Collider, _Rd));
            }
            for (int i = 0; i < Altitudes.Length; i++)
            {
                Assigned[i] = CountAt(i);
            }
        }

        public static Matrix4x4 RotationY(float theta) => Obstacle.RotationY(theta);

        private int ChooseAltitude(Vector3 origin, Vector3 dest)
        {
            float max = 0;
            int start = ((dest - origin).z > 0) ? 0 : 1; // North bound => even; South bound => odd

            int maxindex = Assigned.Length - 1;
            for (int i = start; i < Assigned.Length; i+=2)
            {
                // maximise altitude, minimize traffic, + 1 to prevent singularity
                float tmp = Altitudes[i] / _maxAlt / (Assigned[i] / DroneCount + 1);
                if (tmp > max)
                {
                    max = tmp;
                    maxindex = i;
                }
            }

            Assigned[maxindex]++;
            return maxindex;
        }

        // Get a direction vector perpendicular to dir 90 clockwise about y-axis looking from top to bottom
        private static Vector3 GetPerp(Vector3 dir)
        {
            Vector4 tmp1 = dir;
            tmp1.w = 0;
            Vector4 tmp2 = RotationY(90) * tmp1;
            tmp2.w = 0;
            return tmp2;
        }
        // Get a sorted list/heap of buildings in a corridor between start and end
        private MinHeap<Obstacle> Blockers(Vector3 start, Vector3 end, float alt)
        {
            Vector3 direction = end - start;

            // Sorted by normalized projected distance
            MinHeap<Obstacle> obstacles = new MinHeap<Obstacle>((a, b) =>
            {
                if (a.mu <= b.mu) { return -1; }
                return 1;
            });

            // R_a is the corridor half-width
            Vector3 perp = GetPerp(direction).normalized * _Ra;

            int startIndex = (int)(alt / _buildingDiv); // i.e. the building list index where we should start
            for (int i = startIndex; i < SortedBuildings.Count; i++)
            {
                for (int j = 0; j < SortedBuildings[i].Count; j++)
                {
                    var obs = SortedBuildings[i][j];
                    if (obs.size.y > alt - _altDiv / 2)
                    {
                        // normalized projected distance
                        float mu = Vector3.Dot(obs.position - start, direction) / direction.sqrMagnitude;
                        // normalized perpendicular distance
                        float nu = Vector3.Dot(start - obs.position, perp) / perp.sqrMagnitude;
                        if (nu <= 1 && nu >= -1 && mu <= 1 + _Ra / direction.magnitude && mu >= 0)
                        {
                            obs.mu = mu;
                            obstacles.Add(obs);
                        }
                    }

                }
            }

            return obstacles;
        }

        // swap function
        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private int FindIntersect(Obstacle obs, Vector3 start, Vector3 end, out int[] indices)
        {
            var dir = end - start;
            var _dir = dir.normalized;
            Vector3 path(float m) => start + m * dir; // 0 < mu < 1
            int num = 0;
            float[] mu = new float[4];
            indices = new int[] { -1, -1};
            for (int j = 0; j < obs.normals.Length; j++)
            {
                // if heading not parallel to surface
                if (Mathf.Abs(Vector3.Dot(_dir, obs.normals[j])) > _epsilon)
                {
                    // Solve ray-plane intersection 
                    // f(mu).n = P0.n 
                    // where . is dot product, n is plane normal, P0 is a point on the plane, f(mu) is ray equation
                    mu[j] = Vector3.Dot(obs.verts[j] - start, obs.normals[j]) / Vector3.Dot(dir, obs.normals[j]);
                    if (Vector3.Distance(path(mu[j]), obs.position) < obs.diag / 2 && mu[j] > 0 && mu[j] <= 1)
                    {
                        indices[num++] = j;
                    }
                }
            }

            if (num > 1 && mu[indices[1]] < mu[indices[0]])
            {
                Swap(ref indices[0], ref indices[1]); // make sure the first index refers to the closer intersect
            }

            return num;
        }

        private Vector3 FindOtherWaypoint(Obstacle obs, Vector3 start, Vector3 not)
        {
            foreach (var vert in obs.verts)
            {
                var point = vert + 0.25f * Vector3.Normalize(vert - obs.position);
                if ((point - start).magnitude > _epsilon && (point - not).magnitude > _epsilon && (point - not).magnitude < obs.diag)
                {
                    return point;
                }
            }
            return start;
        }

        private static bool IsContained(Obstacle obs, Vector3 p)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Vector3.Dot(p - obs.verts[i], obs.normals[i]) > 0) return false;
            }
            return true;
        }

        private Vector3 FindWaypoint(Obstacle obs, Vector3 start, Vector3 end, int[] indices)
        {
            var _dir = (end - start).normalized;
            Vector3 waypoint;

            if (indices[1] == -1)
            {
                // If only one intersetion detected sets the way point near the vertex clockwise from the 
                // intersection point
                int num = FindIntersect(obs, start, end + obs.diag * _dir, out int[] indi);
                if (num > 0) return FindWaypoint(obs, start, end + obs.diag * _dir, indi);
            }

            Vector3 a;
            Vector3 b;
            if (Mathf.Abs(indices[1] - indices[0]) == 1 || Mathf.Abs(indices[1] - indices[0]) == 3)
            {
                // indices previously swapped to ensure 1 is bigger than 0
                // adjacent faces interseciton
                int j;
                if (Mathf.Abs(indices[1] - indices[0]) == 1) j = indices[1] < indices[0] ? indices[1] : indices[0];
                else j = 3;
                a = obs.verts[j] + 0.25f * Vector3.Normalize(obs.verts[j] - obs.position);
                b = obs.verts[(j + 1) % 4] + 0.25f * Vector3.Normalize(obs.verts[(j + 1) % 4] - obs.position);
                waypoint = ((a - start).magnitude > _epsilon) ? a : b;
            }
            else
            {
                // opposite faces interseciton
                a = obs.verts[indices[0]] + 0.25f * Vector3.Normalize(obs.verts[indices[0]] - obs.position);
                b = obs.verts[(indices[1] + 1) % 4] + 0.25f * Vector3.Normalize(obs.verts[(indices[1] + 1) % 4] - obs.position);
                if ((a - start).magnitude > _epsilon && (b - start).magnitude > _epsilon)
                {
                    // Gets the waypoint with the smallest deviation angle from the path
                    waypoint = Mathf.Abs(Vector3.Dot((a - start).normalized, _dir)) >
                        Mathf.Abs(Vector3.Dot((b - start).normalized, _dir)) ? a : b;
                }
                else
                {
                    waypoint = ((a - start).magnitude > _epsilon) ? a : b;
                }
            }
            foreach (var nf in _NoFlyZones)
            {
                if (nf.Contains(waypoint)) waypoint = FindOtherWaypoint(obs, start, waypoint);
            }
            return waypoint;
        }

        private List<Vector3> Navigate(Vector3 start, Vector3 end, float alt, bool hubReturn = false, int frame = 0)
        {
            frame++;
            if (frame > 1500) throw new StackOverflowException("Failed!");
            List<Vector3> waypoints = new List<Vector3> { start };

            var dir = end - start;
            if (dir.magnitude < _epsilon) { return waypoints; } // If start = end return start
            // Finds all the buildings sorted by distance from the startpoint in a 200m wide corridor
            MinHeap<Obstacle> buildings = Blockers(start, end, alt);
            MinHeap<Vector3> possibilities = new MinHeap<Vector3>((a, b) =>
            {
                // These are normalized projected distance, i.e. how far along the path the waypoint is located
                float mua = Vector3.Dot(a - start, dir) / dir.sqrMagnitude; 
                float mub = Vector3.Dot(b - start, dir) / dir.sqrMagnitude; 
                if (mua <= mub) return -1;
                return 1;
            });

            // waypoints are hashed, see above for function
            HashSet<int> errorPoints = new HashSet<int>(); 

            bool intersected = false;

            foreach (var obs in _NoFlyZones)
            {
                //For each no fly zone find the number intersects and index of vertices/normals
                if (FindIntersect(obs, start, end, out int[] i) > 0)
                {
                    intersected = true;
                    // Find the corresponding waypoint for the given vertices/normal
                    Vector3 v = FindWaypoint(obs, start, end, i);
                    possibilities.Add(v);
                    if (i[1] == -1) errorPoints.Add(HashVector(v));
                }
            }
            if (hubReturn)
            {
                foreach (var obs in _Hubs)
                {
                    if (obs.Contains(_destination)) continue;
                    //For each no fly zone find the number intersects and index of vertices/normals
                    if (FindIntersect(obs, start, end, out int[] i) > 0)
                    {
                        intersected = true;
                        // Find the corresponding waypoint for the given vertices/normal
                        Vector3 v = FindWaypoint(obs, start, end, i);
                        possibilities.Add(v);
                        if (i[1] == -1) errorPoints.Add(HashVector(v));
                    }
                }
            }

            int k = 0;
            while (!buildings.IsEmpty() && k < 5) 
            {
                var obs = buildings.Remove();
                int num = FindIntersect(obs, start, end, out int[] j);
                if (num > 0)
                {
                    k++;
                    intersected = true;
                    Vector3 v = FindWaypoint(obs, start, end, j);
                    possibilities.Add(v);
                    if (j[1] == -1) errorPoints.Add(HashVector(v));
                }
            }

            if (intersected)
            {
                var next = possibilities.Remove();
                possibilities.Clear();
                buildings.Clear();

                var list = Navigate(start, next, alt, hubReturn, frame);

                for (int i = 1; i < list.Count; i++)
                    waypoints.Add(list[i]);

                if (errorPoints.Count > 0 && errorPoints.Contains(HashVector(next)))
                    end = _destination;

                list = Navigate(list[list.Count - 1], end, alt, hubReturn, frame); // pass arguments by value!

                for (int i = 1; i < list.Count; i++)
                    waypoints.Add(list[i]);
            }
            else
            {
                waypoints.Add(end);
            }
            frame--;
            return waypoints;

        }

    }

}