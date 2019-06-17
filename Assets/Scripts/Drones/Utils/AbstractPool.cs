using System;
using System.Collections.Generic;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Utils
{
    public abstract class AbstractPool: IPool
    {
        protected Dictionary<Type, string> _Paths;

        protected Dictionary<Type, uint> _StartSize;

        protected Dictionary<Type, GameObject> _Templates;

        protected Dictionary<Type, Queue<IPoolable>> _Pool;

        protected Dictionary<Type, bool> _IsBuilding;

        public abstract Dictionary<Type, string> Paths { get; }

        public abstract Dictionary<Type, uint> StartSize { get; }

        public abstract Dictionary<Type, GameObject> Templates { get; }

        public abstract Dictionary<Type, Queue<IPoolable>> Pool { get; }

        public abstract Dictionary<Type, bool> IsBuilding { get; }

    }
}