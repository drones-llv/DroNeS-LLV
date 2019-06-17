using System;
using System.Collections.Generic;
using Drones.Managers;
using Drones.Objects;
using UnityEngine;

namespace Drones.Router
{
    public class SmartRaypath : Pathfinder
    {
        private const int Ra = 200; // Corridor width
        private const float Epsilon = 0.01f;
        private static float DroneCount => SimManager.AllDrones.Count;
        private float[] _altitudes;
        private int[] _assigned;
        private Vector3 _origin;
        private Vector3 _destination;
        private float _chosenAltitude;
        private List<Vector3> _output;
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
        private int[] Assigned
        {
            get
            {
                if (_assigned != null) return _assigned;
                const int size = (int)((MaxAlt - MinAlt) / AltDivision) + 1;
                _assigned = new int[size];
                for (var i = 0; i < size; i++)
                    _assigned[i] = 0;
                return _assigned;
            }
        }
        
        // The public interface to get the list of waypoints
        public override Queue<Vector3> GetRoute(Drone drone)
        {
            throw new NotImplementedException();
        }

        // To test: -7.4, 500, 7.0 to -2640.1, 0.0, -5468.1
        // To test: -7.4, 500, 7.0 to -1111.9, 0.0, -2228.0
        public Queue<Vector3> GetRouteTest(Vector3 origin, Vector3 dest)
        {
            var tmp = GameObject.FindGameObjectsWithTag("NoFlyZone");
            Nfz = new Dictionary<uint, Obstacle>();
            foreach (var i in tmp)
            {
                Nfz.Add(1, new Obstacle(i.transform, Rd));
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

                for (var i = 0; i < waypoints.Count; i++)
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

        private int CountAt(int i)
        {
            var count = 0;
            foreach (Transform drone in Drone.ActiveDrones)
            {
                if (Altitudes[i] - AltDivision / 2 < drone.position.y &&
                    Altitudes[i] + AltDivision / 2 > drone.position.y)
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

        private int ChooseAltitude(Vector3 origin, Vector3 dest)
        {
            float max = 0;
            var start = ((dest - origin).z > 0) ? 0 : 1; // North bound => even; South bound => odd

            var maxIndex = Assigned.Length - 1;
            for (var i = start; i < Assigned.Length; i+=2)
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

        private void Navigate()
        {

        }
    }
}