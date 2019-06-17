using System.Collections;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Utils
{
    public class Explosion : MonoBehaviour, IPoolable
    {
        [SerializeField]
        ParticleSystem _Component;

        ParticleSystem Component
        {
            get
            {
                if (_Component == null)
                {
                    _Component = GetComponent<ParticleSystem>();
                }
                return _Component;
            }
        }

        private IEnumerator WaitForEnd()
        {
            yield return new WaitUntil(() => !Component.isPlaying);
            Delete();
        }

        public bool InPool { get; private set; }

        public static Explosion New(Vector3 pos)
        {
            var item = PoolController.Get(ObjectPool.Instance).Get<Explosion>(null);
            item.transform.position = pos;
            return item;
        }

        public void Delete() => PC().Release(GetType(), this);

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            transform.SetParent(parent);
            gameObject.SetActive(true);
            Component.Play();
            StartCoroutine(WaitForEnd());
        }

        public void OnRelease()
        {
            InPool = true;
            transform.SetParent(PC().PoolParent);
            gameObject.SetActive(false);
        }

        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
    }
}
