using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace Drones.UI
{
    public class UIFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        public static int Hover { get; protected set; }
        public static void Reset() => Hover = 0;

        private Transform _Window;
        public static bool Controlling { get; protected set; }

        protected Transform Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = AbstractWindow.GetWindow(transform).transform;
                }
                return _Window;
            }
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            Controlling = false;
            if (Hover > 0) { Hover--; }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!AbstractCamera.Controlling)
            {
                Hover++;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (Hover > 0) { Hover--; }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!Controlling) { StartCoroutine(ControlListener()); }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                transform.SetAsLastSibling();
            }
        }

        public static IEnumerator ControlListener()
        {
            var wait = new WaitUntil(() => Input.GetMouseButtonUp(0));
            Controlling = true;
            yield return wait;
            Controlling = false;
            yield break;
        }

    }

}
