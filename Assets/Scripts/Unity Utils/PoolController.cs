using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Drones.Utils
{
    using Interface;
    using Object = UnityEngine.Object;

    public class PoolController
    {
        private static Dictionary<Type, PoolController> _ExistingPools; 
        public static PoolController Get(AbstractPool pool)
        {
            var type = pool.GetType();
            if (_ExistingPools == null) _ExistingPools = new Dictionary<Type, PoolController>();

            if (_ExistingPools.TryGetValue(type, out PoolController value))
            {
                if (value != null) return value;
            }

            _ExistingPools.Add(type, new PoolController(pool));

            return _ExistingPools[type];
        }

        public static void Reset()
        {
            _ExistingPools = null;
            ObjectPool.Reset();
            ListElementPool.Reset();
            WindowPool.Reset();
        }

        private readonly AbstractPool _Pool;
        private PoolController(AbstractPool pool)
        {
            _Pool = pool;
            PoolParent.position = Vector3.zero;
        }

        public bool Initialized { get; private set; } = false;

        private PoolComponent _Container;

        private PoolComponent Container
        {
            get
            {
                if (_Container == null)
                {
                    _Container = new GameObject(_Pool.GetType().ToString()).AddComponent<PoolComponent>();
                    _Container.pool = _Pool;
                    _Container.StartCoroutine(Initialize());
                }
                return _Container;
            }
        }

        public Transform PoolParent => Container.transform;

        public void Release(Type type, IPoolable item)
        {
            item.OnRelease();
            _Pool.Pool[type].Enqueue(item);
        }

        public T Get<T>(Transform parent, bool noOnGet = false) => (T)Get(typeof(T), parent, noOnGet);

        public IPoolable Get(Type type, Transform parent = null, bool noOnGet = false)
        {
            IPoolable item = null;
            if (_Pool.Pool.TryGetValue(type, out Queue<IPoolable> pool))
            {
                if (pool.Count < _Pool.StartSize[type] / 4 && !_Pool.IsBuilding[type])
                    Container.StartCoroutine(Build(type));

                if (pool.Count == 0)
                    item = ManualBuild(type);
                else
                    item = pool.Dequeue();

                if (!noOnGet)
                    item.OnGet(parent);
            }
            return item;
        }

        public IPoolable ManualBuild(Type type)
        {
            GameObject go = Object.Instantiate(_Pool.Templates[type], PoolParent);
            return go.GetComponent<IPoolable>();
        }

        public GameObject GetTemplate(Type type) => _Pool.Templates[type];

        private IEnumerator Build(Type type)
        {
            _Pool.IsBuilding[type] = true;
            for (int i = 0; i < _Pool.StartSize[type]; i++)
            {
                GameObject go = Object.Instantiate(_Pool.Templates[type], PoolParent);

                Release(type, go.GetComponent<IPoolable>());

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    yield return null;
            }
            _Pool.IsBuilding[type] = false;
        }

        public IEnumerator Initialize()
        {
            if (!Initialized)
            {
                foreach (var type in _Pool.Paths.Keys)
                {
                    try
                    {
                        _Pool.Templates.Add(type, Resources.Load(_Pool.Paths[type]) as GameObject);
                        _Pool.IsBuilding.Add(type, false);
                    }
                    catch (ArgumentException)
                    {

                    }

                    if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    {
                        yield return null;
                    }
                }
                foreach (var type in _Pool.Paths.Keys)
                {
                    Container.StartCoroutine(Build(type));
                }
                Initialized = true;
            }
            yield break;
        }
    }

}