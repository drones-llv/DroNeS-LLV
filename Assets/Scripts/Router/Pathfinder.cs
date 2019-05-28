using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Router
{
    [Serializable]
    public abstract class Pathfinder
    {
        protected List<Obstacle> _Buildings;
        protected List<Obstacle> _NoFlyZones;

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

        public abstract Queue<Vector3> GetRoute(Vector3 start, Vector3 end, bool hubReturn);

        ~Pathfinder()
        {
            Buildings?.Clear();
            _NoFlyZones?.Clear();
        }
    }
}
