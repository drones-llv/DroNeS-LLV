using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Router
{
    public abstract class Pathfinder
    {
        protected List<Obstacle> _Buildings;
        protected List<Obstacle> _NoFlyZones;
        protected const int _Rd = 2; // drone Radius

        protected Pathfinder()
        {
            _Buildings = new List<Obstacle>();
            var container = GameObject.FindWithTag("Building").transform;
            foreach (Transform b in container)
            {
                _Buildings.Add(new Obstacle(b, _Rd));
            }
            _NoFlyZones = new List<Obstacle>();
        }

        public abstract Queue<Vector3> GetRoute(Vector3 start, Vector3 end, bool hubReturn);

        ~Pathfinder()
        {
            _Buildings.Clear();
            _NoFlyZones.Clear();
        }
    }
}
