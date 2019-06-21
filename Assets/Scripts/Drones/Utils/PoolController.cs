using System.Collections;
using System.Collections.Generic;
using System;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.Utils
{
    using Object = UnityEngine.Object;

    public class PoolController
    {
        private static Dictionary<Type, PoolController> _existingPools; 
        public static PoolController Get(AbstractPool pool)
        {
            var type = pool.GetType();
            if (_existingPools == null) _existingPools = new Dictionary<Type, PoolController>();

            if (_existingPools.TryGetValue(type, out PoolController value))
            {
                if (value != null) return value;
            }

            _existingPools.Add(type, new PoolController(pool));

            return _existingPools[type];
        }

        public static void Reset()
        {
            _existingPools = null;
            ObjectPool.Reset();
            ListElementPool.Reset();
            WindowPool.Reset();
        }

        private readonly AbstractPool _pool;
        private PoolController(AbstractPool pool)
        {
            _pool = pool;
            PoolParent.position = Vector3.zero;
        }

        public bool Initialized { get; private set; } = false;

        private PoolComponent _container;

        private PoolComponent Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new GameObject(_pool.GetType().ToString()).AddComponent<PoolComponent>();
                    _container.pool = _pool;
                    _container.StartCoroutine(Initialize());
                }
                return _container;
            }
        }

        public Transform PoolParent => Container.transform;

        public void Release(Type type, IPoolable item)
        {
            item.OnRelease();
            _pool.Pool[type].Enqueue(item);
        }

        public T Get<T>(Transform parent, bool noOnGet = false) => (T)Get(typeof(T), parent, noOnGet);

        private IPoolable Get(Type type, Transform parent = null, bool noOnGet = false)
        {
            IPoolable item = null;
            if (!_pool.Pool.TryGetValue(type, out Queue<IPoolable> pool)) return item;
            if (pool.Count < _pool.StartSize[type] / 4 && !_pool.IsBuilding[type])
                Container.StartCoroutine(Build(type));

            item = pool.Count == 0 ? ManualBuild(type) : pool.Dequeue();

            if (!noOnGet)
                item.OnGet(parent);
            return item;
        }

        private IPoolable ManualBuild(Type type)
        {
            var go = Object.Instantiate(_pool.Templates[type], PoolParent);
            return go.GetComponent<IPoolable>();
        }

        public GameObject GetTemplate(Type type) => _pool.Templates[type];

        private IEnumerator Build(Type type)
        {
            _pool.IsBuilding[type] = true;
            for (var i = 0; i < _pool.StartSize[type]; i++)
            {
                var go = Object.Instantiate(_pool.Templates[type], PoolParent);

                Release(type, go.GetComponent<IPoolable>());

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    yield return null;
            }
            _pool.IsBuilding[type] = false;
        }

        private IEnumerator Initialize()
        {
            if (Initialized) yield break;
            foreach (var type in _pool.Paths.Keys)
            {
                try
                {
                    _pool.Templates.Add(type, Resources.Load(_pool.Paths[type]) as GameObject);
                    _pool.IsBuilding.Add(type, false);
                }
                catch (ArgumentException)
                {

                }

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                }
            }
            foreach (var type in _pool.Paths.Keys)
            {
                Container.StartCoroutine(Build(type));
            }
            Initialized = true;
            yield break;
        }
    }

}