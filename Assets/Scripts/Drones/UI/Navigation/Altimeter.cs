using System.Collections;
using Drones.Utils;
using UnityEngine;
using Utils;

namespace Drones.UI.Navigation
{
    public class Altimeter : MonoBehaviour
    {
        private Vector2 _StartPosition;
        private RectTransform _Rect;
        private RectTransform _ParentRect;
        private float _RealToScale;
        private float _ScaleHeight;

        void Awake()
        {
            _Rect = transform.ToRect();
            _ParentRect = transform.parent.ToRect();
            // 2232/2480/600 is based on image size
            // 2232 is scale height, 2480 is image height. 600 is the scale range (600 m)
            _ScaleHeight = 2 * 2232f / 2480f * _ParentRect.rect.height;
            _RealToScale = _ScaleHeight / 600f;
        }

        private void OnEnable()
        {
            StartCoroutine(Operate());
        }

        private void OnDisable()
        {
            StopCoroutine(Operate());
        }

        IEnumerator Operate()
        {
            _StartPosition = _Rect.offsetMin;
            _StartPosition.y = -2232f / 2480f * _ParentRect.rect.height;
            _Rect.offsetMin = _StartPosition;
            var s = new WaitForSeconds(1 / 30f);

            while (true)
            {
                float currentHeight = RTSCameraComponent.RTS.transform.position.y;
                currentHeight = Mathf.Clamp(currentHeight, 0, 600f);

                _Rect.offsetMin = _StartPosition + currentHeight * _RealToScale * Vector2.up;
                yield return s;
            }
        }

    }
}
