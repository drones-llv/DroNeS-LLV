using System;

namespace Utils.Interfaces
{
    public interface ISecureCollectible<T>
    {
        event Action<T> SetChanged;

        event Action<T> ItemAdded;

        event Action<T> ItemRemoved;

        Predicate<T> MemberCondition { get; set; }
    }
}
