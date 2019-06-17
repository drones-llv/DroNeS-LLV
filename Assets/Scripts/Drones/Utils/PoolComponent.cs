using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    public class PoolComponent : MonoBehaviour
    {
        public AbstractPool pool;
        public string poolType;

        private void OnEnable()
        {
            StartCoroutine(Debugging());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator Debugging()
        {
            yield return new WaitUntil(() => pool != null);
            poolType = pool.GetType().ToString();
        }
    }
}
