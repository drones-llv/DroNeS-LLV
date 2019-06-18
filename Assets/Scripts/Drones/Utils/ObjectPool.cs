using System;
using System.Collections;
using System.Collections.Generic;
using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Utils
{
    public class ObjectPool : AbstractPool
    {
        private static ObjectPool _instance;
        public static ObjectPool Instance => _instance ?? (_instance = new ObjectPool());

        public static void Reset()
        {
            _instance = null;
        }

        public override Dictionary<Type, string> Paths =>
            _Paths ?? (_Paths = new Dictionary<Type, string>
            {
                {typeof(Drone), DroneObjectPath},
                {typeof(Hub), HubObjectPath},
                {typeof(NoFlyZone), NFZObjectPath},
                {typeof(Explosion), ExplosionPath}
            });

        public override Dictionary<Type, uint> StartSize =>
            _StartSize ?? (_StartSize = new Dictionary<Type, uint>
            {
                {typeof(Drone), 800},
                {typeof(Hub), 10},
                {typeof(NoFlyZone), 10},
                {typeof(Explosion), 200}
            });

        public override Dictionary<Type, GameObject> Templates { get; } = new Dictionary<Type, GameObject>();

        public override Dictionary<Type, Queue<IPoolable>> Pool =>
            _Pool ?? (_Pool = new Dictionary<Type, Queue<IPoolable>>
            {
                {typeof(Drone), new Queue<IPoolable>()},
                {typeof(Hub), new Queue<IPoolable>()},
                {typeof(NoFlyZone), new Queue<IPoolable>()},
                {typeof(Explosion), new Queue<IPoolable>()}
            });

        public override Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        #region Paths
        private const string DroneObjectPath = "Prefabs/Objects/Drone";
        private const string HubObjectPath = "Prefabs/Objects/Hub";
        private const string NFZObjectPath = "Prefabs/Objects/NoFlyZone";
        private const string ExplosionPath = "Prefabs/Objects/Explosion";
        #endregion
    }
}
