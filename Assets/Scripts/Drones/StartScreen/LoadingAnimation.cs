using UnityEngine;

namespace Drones.StartScreen
{
    using System.Collections;

    public class LoadingAnimation : MonoBehaviour
    {
        [SerializeField]
        RectTransform[] _Rotors;
        public RectTransform[] Rotors
        {
            get
            {
                if (_Rotors == null)
                {
                    _Rotors = GetComponentsInChildren<RectTransform>();
                }
                return _Rotors;
            }
        }

        private void OnEnable() => StartCoroutine(Spin());

        private void OnDisable() => StopCoroutine(Spin());

        IEnumerator Spin()
        {
            foreach (var rotor in Rotors)
            {
                rotor.eulerAngles = Vector3.forward * Random.value * 360;
            }

            while (true)
            {
                foreach (var rotor in Rotors)
                {
                    rotor.eulerAngles += Vector3.forward * 30;
                }

                yield return null;
            }

        }
    }
}
