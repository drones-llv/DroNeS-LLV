using UnityEngine;

namespace Drones.Utils.Interfaces
{
    public interface IPoolable
    {
        void Delete();
        void OnRelease();
        void OnGet(Transform parent);
        bool InPool { get; }
        PoolController PC();
    }
}
