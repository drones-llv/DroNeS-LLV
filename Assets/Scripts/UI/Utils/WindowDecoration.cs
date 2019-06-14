using UnityEngine.EventSystems;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils.Extensions;
    using Drones.Interface;

    public class WindowDecoration : UIFocus, IDragHandler, IEndDragHandler, IOnScreen
    {
        Vector2 _Origin;

        Rect _ScreenRect;

        private void OnEnable()
        {
            _ScreenRect = new Rect(0, 0, Screen.width, Screen.height);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            transform.parent.SetAsLastSibling();
            _Origin = eventData.position;
            if (!Controlling) { StartCoroutine(ControlListener()); }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            transform.parent.position += (Vector3)(eventData.position - _Origin);
            _Origin = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetUnderflow();
        }

        public void SetUnderflow()
        {
            Vector3[] corners = new Vector3[4];
            transform.parent.ToRect().GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                if (!_ScreenRect.Contains(corners[i]))
                {
                    if (corners[i].x > Screen.width)
                    {
                        transform.parent.position += Vector3.left * (corners[i].x - Screen.width);
                    }
                    else if (corners[i].x < 0)
                    {
                        transform.parent.position += Vector3.left * corners[i].x;
                    }
                    else if (corners[i].y < 0)
                    {
                        transform.parent.position += Vector3.down * corners[i].y;
                    }
                    else if (corners[i].y > Screen.height)
                    {
                        transform.parent.position += Vector3.down * (corners[i].y - Screen.height);
                    }
                    transform.parent.ToRect().GetWorldCorners(corners);
                }
            }

        }

        public bool IsOverflow()
        {
            Vector3[] corners = new Vector3[4];
            transform.parent.ToRect().GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++)
            {
                if (!_ScreenRect.Contains(corners[i]))
                {
                    return true;
                }
            }
            return false;
        }


    }
}
