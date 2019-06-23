using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Interfaces;

namespace Utils
{
    /* Must use with objects that have a CONSTANT HASH */
    [Serializable]
    public class SecureSortedSet<T0, T1> : ISecureCollectible<T1>
    {
        private event Action<T1> CollectionChanged;
        private event Action<T1> ItemAddition;
        private event Action<T1> ItemRemoval;

        private readonly Map<T0, T1> _map;
        private readonly MaxHeap<T1> _maxSorter;
        private readonly MinHeap<T1> _minSorter;

        #region Constructors
        public SecureSortedSet(Comparison<T1> comparer)
        {
            _map = new Map<T0, T1>();
            _maxSorter = new MaxHeap<T1>(comparer);
            _minSorter = new MinHeap<T1>(comparer);
        }

        public SecureSortedSet()
        {
            _map = new Map<T0, T1>();
            _maxSorter = new MaxHeap<T1>(null);
            _minSorter = new MinHeap<T1>(null);
        }
        #endregion

        #region Events
        public event Action<T1> SetChanged
        {
            add
            {
                if (CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value))
                {
                    CollectionChanged += value;
                }
            }
            remove
            {
                CollectionChanged -= value;
            }
        }

        public event Action<T1> ItemAdded
        {
            add
            {
                if ((CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value)) &&
                    (ItemAddition == null || !ItemAddition.GetInvocationList().Contains(value)))
                {
                    ItemAddition += value;
                }
            }
            remove
            {
                ItemAddition -= value;
            }
        }

        public event Action<T1> ItemRemoved
        {
            add
            {
                if ((CollectionChanged == null || !CollectionChanged.GetInvocationList().Contains(value)) &&
                    (ItemRemoval == null || !ItemRemoval.GetInvocationList().Contains(value)))
                {
                    ItemRemoval += value;
                }
            }
            remove
            {
                ItemRemoval -= value;
            }
        }
        #endregion

        public Predicate<T1> MemberCondition { get; set; }

        public bool Add(T0 key, T1 value)
        {

            try
            {
                if (MemberCondition != null && !MemberCondition(value)) return false;
                _map.Add(key, value);
                _maxSorter.Add(value);
                _minSorter.Add(value);
                CollectionChanged?.Invoke(value);
                ItemAddition?.Invoke(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Remove(T1 value)
        {
            try
            {
                _map.Remove(value);
                CollectionChanged?.Invoke(value);
                ItemRemoval?.Invoke(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Remove(T0 key)
        {
            try
            {
                var item = _map.Forward[key];
                _map.Remove(key);
                CollectionChanged?.Invoke(item);
                ItemRemoval?.Invoke(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<T0, T1>.ValueCollection Values => _map.Forward.Values;

        public Dictionary<T0, T1>.KeyCollection Keys => _map.Forward.Keys;

        public T1 FindValue(T0 key)
        {
            try
            {
                return _map.Forward[key];
            }
            catch
            {
                return default;
            }
        }

        public int Count => _map.Count();

        public void Clear() => _map.Clear();

        public bool Contains(T0 key) => _map.Contains(key);

        public bool Contains(T1 val) => _map.Contains(val);

        public int CountWhere(Predicate<T1> match)
        {
            var i = 0;
            foreach (var item in _map.Forward.Values)
            {
                if (match(item)) { i++; }
            }
            return i;
        }

        public T1 GetMax(bool sort)
        {
            try
            {
                if (Count <= 0) return default;
                if (sort) _maxSorter.ReSort();
                var output = _maxSorter.Remove();
                while (!Remove(output))
                {
                    output = _maxSorter.Remove();
                    if (Count == 0) return default;
                }

                return output;
            }
            catch
            {
                return default;
            }
        }

        public T1 GetMin(bool sort)
        {
            try
            {
                if (Count <= 0) return default;
                
                if (sort) _minSorter.ReSort();
                var output = _minSorter.Remove();
                while (!Remove(output))
                {
                    output = _minSorter.Remove();
                    if (Count == 0) return default;
                }

                return output;
            }
            catch
            {
                return default;
            }
        }

        public T1 PeekMax(bool sort)
        {
            try
            {
                if (Count <= 0) return default;
                if (sort) _maxSorter.ReSort();
                var output = _maxSorter.Peek();
                while (!_map.Contains(output))
                {
                    _maxSorter.Remove();
                    if (_maxSorter.Count == 0) return default;
                    output = _maxSorter.Peek();
                } 

                return output;
            }
            catch
            {
                return default;
            }
        }

        public T1 PeekMin(bool sort)
        {
            try
            {
                if (Count <= 0) return default;
                if (sort) _minSorter.ReSort();
                var output = _minSorter.Peek();
                while (!_map.Contains(output))
                {
                    _minSorter.Remove();
                    if (_minSorter.Count == 0) return default;
                    output = _minSorter.Peek();
                }
                return output;
            }
            catch
            {
                return default;
            }
        }

        public T1 Find(Predicate<T1> match)
        {
            foreach (var item in _map.Forward.Values)
            {
                if (match(item))
                {
                    return item;
                }
            }
            return default;
        }

        public T1 this[T0 key] 
        {
            get
            {
                try
                {
                    return _map.Forward[key];
                }
                catch
                {
                    return default;
                }
            }

            set
            {
                if (!Contains(value))
                {
                    _map.Forward[key] = value;
                }
            }

        }

        public T0 this[T1 key]
        {
            get
            {
                try
                {
                    return _map.Reverse[key];
                }
                catch
                {
                    return default;
                }
            }

            set
            {
                if (!Contains(value))
                {
                    _map.Reverse[key] = value;
                }
            }

        }

        public bool TryGet(T0 key, out T1 value) => _map.TryGet(key, out value);
        public bool TryGet(T1 key, out T0 value) => _map.TryGet(key, out value);
        public void ReSort()
        {
            _maxSorter.ReSort();
            _minSorter.ReSort();
        }

        public void ReSort(Comparison<T1> comparer)
        {
            _maxSorter.ReSort(comparer);
            _minSorter.ReSort(comparer);
        }
    }
}
