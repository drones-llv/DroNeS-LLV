using System.Collections;
using Drones.Utils;
using UnityEngine;

namespace Drones.UI.Navigation
{
    public class Compass : MonoBehaviour
    {
        Vector3 startPosition;
        float movementPerDegree;
        RectTransform rect;
        bool IsOn;

        void Start()
        {
            rect = (RectTransform)transform;
            startPosition = transform.localPosition;
            Vector2 tmp = rect.sizeDelta;
            tmp.x = 2048 / 52 * tmp.y;
            rect.sizeDelta = tmp;
            movementPerDegree = rect.sizeDelta.x / 720;
            StartCoroutine(GetDirection());
        }

        private void OnEnable()
        {
            if (!IsOn)
            {
                StartCoroutine(GetDirection());
            }
        }

        private void OnDisable()
        {
            IsOn = false;
            StopCoroutine(GetDirection());
        }

        IEnumerator GetDirection()
        {
            IsOn = true;
            var wait = new WaitForSeconds(1 / 30f);
            yield return new WaitUntil(() => AbstractCamera.ActiveCamera != null);
            while (true)
            {
                Vector3 localForward = Vector3.Cross(AbstractCamera.CameraTransform.right, Vector3.up);
                float dir = -Vector3.Dot(localForward, Vector3.right);
                transform.localPosition = startPosition + Vector3.Angle(localForward, Vector3.forward) * Mathf.Sign(dir) * movementPerDegree * Vector3.right;
                yield return wait;
            }

        }
    }
}
