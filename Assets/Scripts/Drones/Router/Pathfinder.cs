using System;
using System.Collections.Generic;
using Drones.Objects;
using UnityEngine;

namespace Drones.Router
{
    [Serializable]
    public abstract class Pathfinder
    {
        protected List<Obstacle> _Buildings;
        protected static Dictionary<uint, Obstacle> _NoFlyZones;
        protected static Dictionary<uint, Obstacle> _Hubs;
        public static Dictionary<uint, Obstacle> Hubs
        {
            get
            {
                if (_Hubs == null)
                {
                    _Hubs = new Dictionary<uint, Obstacle>();
                }
                return _Hubs;
            }
        }
        public static Dictionary<uint, Obstacle> NoFlyZones
        {
            get
            {
                if (_NoFlyZones == null)
                {
                    _NoFlyZones = new Dictionary<uint, Obstacle>();
                }
                return _NoFlyZones;
            }
        }

        protected List<Obstacle> Buildings
        {
            get
            {
                if (_Buildings == null)
                {
                    _Buildings = new List<Obstacle>();
                    var container = GameObject.FindWithTag("Building").transform;
                    foreach (Transform b in container)
                    {
                        _Buildings.Add(new Obstacle(b, _Rd));
                    }
                }
                return _Buildings;
            }
        }

        protected const int _Rd = 2; // drone Radius

        protected Pathfinder() { }

        public abstract Queue<Vector3> GetRoute(Drone drone);

        ~Pathfinder()
        {
            Buildings?.Clear();
            NoFlyZones?.Clear();
        }
    }
}
