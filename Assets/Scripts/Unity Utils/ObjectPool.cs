using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    using Interface;

    public class ObjectPool : AbstractPool
    {
        private static ObjectPool _Instance;
        public static ObjectPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ObjectPool();
                }
                return _Instance;
            }
        }

        public static void Reset()
        {
            _Instance = null;
        }

        public override Dictionary<Type, string> Paths
        {
            get
            {
                if (_Paths == null)
                {
                    _Paths = new Dictionary<Type, string>
                    {
                        { typeof(Drone), DroneObjectPath },
                        { typeof(Hub), HubObjectPath },
                        { typeof(NoFlyZone), NFZObjectPath },
                        { typeof(Explosion), ExplosionPath }
                    };
                }

                return _Paths;
            }
        }

        public override Dictionary<Type, uint> StartSize
        {
            get
            {
                if (_StartSize == null)
                {
                    _StartSize = new Dictionary<Type, uint>
                    {
                        { typeof(Drone), 400 },
                        { typeof(Hub), 10 },
                        { typeof(NoFlyZone), 10 },
                        { typeof(Explosion), 200 }
                    };
                }

                return _StartSize;
            }
        }

        public override Dictionary<Type, GameObject> Templates { get; } = new Dictionary<Type, GameObject>();

        public override Dictionary<Type, Queue<IPoolable>> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new Dictionary<Type, Queue<IPoolable>>
                    {
                        { typeof(Drone), new Queue<IPoolable>() },
                        { typeof(Hub), new Queue<IPoolable>() },
                        { typeof(NoFlyZone), new Queue<IPoolable>() },
                        { typeof(Explosion), new Queue<IPoolable>() }
                    };
                }

                return _Pool;
            }
        }

        public override Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        #region Paths
        private const string DroneObjectPath = "Prefabs/Objects/Drone";
        private const string HubObjectPath = "Prefabs/Objects/Hub";
        private const string NFZObjectPath = "Prefabs/Objects/NoFlyZone";
        private const string ExplosionPath = "Prefabs/Objects/Explosion";
        #endregion
    }
}
