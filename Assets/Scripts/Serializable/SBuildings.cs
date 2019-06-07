using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Serializable
{
    [Serializable]
    public class SBuildings
    {
        public List<StaticObstacle> buildings = new List<StaticObstacle>();
        public SBuildings(Transform[] trans)
        {
            foreach (var b in trans)
            {
                buildings.Add(new StaticObstacle(b));
            }
        }

    }

}
