using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Interfaces
{
    public interface IPool
    {
        Dictionary<Type, string> Paths { get; }
        Dictionary<Type, uint> StartSize { get; }
        Dictionary<Type, GameObject> Templates { get; }
        Dictionary<Type, Queue<IPoolable>> Pool { get; }
        Dictionary<Type, bool> IsBuilding { get; }
    }
}
