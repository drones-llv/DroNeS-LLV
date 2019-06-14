using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Drones.Interface;
using System.Collections;
using Drones.Utils.Extensions;

namespace Drones.UI
{
    public class ToolTip : UIFocus, IOnScreen
    {
        [SerializeField]
        TextMeshProUGUI _Body;
        public TextMeshProUGUI Body
        {
            get
            {
                if (_Body == null)
                {
                    _Body = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                }
                return _Body;
            }
        }

        Rect _ScreenRect;

        private readonly static WaitForSecondsRealtime WaitingToDisappear = new WaitForSecondsRealtime(1);
        private void OnEnable()
        {
            _ScreenRect = new Rect(0, 0, Screen.width, Screen.height);
            StartCoroutine(WaitForAssignment());
            SetUnderflow();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Body.text = "";
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            StopCoroutine(Disappear());
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            StartCoroutine(Disappear());
        }

        IEnumerator Disappear()
        {
            yield return WaitingToDisappear;
            gameObject.SetActive(false);
            yield break;
        }

        public void SetUnderflow()
        {
            Vector3[] corners = new Vector3[4];
            transform.ToRect().GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                if (!_ScreenRect.Contains(corners[i]))
                {
                    if (corners[i].x > Screen.width)
                    {
                        transform.position += Vector3.left * (corners[i].x - Screen.width);
                    }
                    else if (corners[i].x < 0)
                    {
                        transform.position += Vector3.left * corners[i].x;
                    }
                    else if (corners[i].y < 0)
                    {
                        transform.position += Vector3.down * corners[i].y;
                    }
                    else if (corners[i].y > Screen.height)
                    {
                        transform.position += Vector3.down * (corners[i].y - Screen.height);
                    }
                    transform.ToRect().GetWorldCorners(corners);
                }
            }

        }

        IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Body.text != "");
            Body.ForceMeshUpdate();
            yield break;

        }
    }
}
