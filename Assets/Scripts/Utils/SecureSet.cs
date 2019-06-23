using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Interfaces;

namespace Utils
{
    public class SecureSet<T> : HashSet<T>, ISecureCollectible<T>
    {
        private event Action<T> CollectionChanged;
        private event Action<T> ItemAddition;
        private event Action<T> ItemRemoval;

        public Predicate<T> MemberCondition { get; set; }

        public event Action<T> SetChanged
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

        public event Action<T> ItemAdded
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

        public event Action<T> ItemRemoved
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

        public new bool Add(T item)
        {

            if (MemberCondition == null || MemberCondition(item))
            {
                if (base.Add(item))
                {
                    CollectionChanged?.Invoke(item);
                    ItemAddition?.Invoke(item);
                    return true;
                }
            }
            return false;
        }

        public new bool Remove(T item)
        {
            if (base.Remove(item))
            {
                CollectionChanged?.Invoke(item);
                ItemAddition?.Invoke(item);
                return true;
            }
            return false;
        }

        SecureSet() : base() { }
        SecureSet(IEnumerable<T> collection) : base(collection) { }
        SecureSet(IEqualityComparer<T> comparer) : base(comparer) { }
        SecureSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }

    }
}
