using System;
using System.Collections.Generic;
using Drones.Objects;
using UnityEngine;

namespace Drones.Router
{
    [Serializable]
    public abstract class Pathfinder
    {
        private List<Obstacle> _buildings;
        protected static Dictionary<uint, Obstacle> Nfz;
        private static Dictionary<uint, Obstacle> _hubs;
        protected Queue<Vector3> Path;
        protected const float MaxAlt = 200;
        protected const float MinAlt = 60;
        protected const int AltDivision = 10; // Altitude interval
        protected const int Rd = 2; // drone Radius
        protected readonly int[] HubAlt = { 480, 490 };
        public static Dictionary<uint, Obstacle> Hubs => _hubs ?? (_hubs = new Dictionary<uint, Obstacle>());

        public static Dictionary<uint, Obstacle> NoFlyZones => Nfz ?? (Nfz = new Dictionary<uint, Obstacle>());

        protected List<Obstacle> Buildings
        {
            get
            {
                if (_buildings != null) return _buildings;
                _buildings = new List<Obstacle>();
                var container = GameObject.FindWithTag("Building").transform;
                foreach (Transform b in container)
                {
                    _buildings.Add(new Obstacle(b, Rd));
                }
                return _buildings;
            }
        }

        public abstract void GetRoute(Drone drone, ref Queue<Vector3> waypoints);

        ~Pathfinder()
        {
            _buildings?.Clear();
            Nfz?.Clear();
            _hubs?.Clear();
        }
    }
}
