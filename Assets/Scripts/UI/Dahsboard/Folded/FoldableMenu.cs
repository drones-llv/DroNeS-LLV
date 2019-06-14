using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Drones.UI
{
    public abstract class FoldableMenu : UIFocus
    {
        [SerializeField]
        private Button _Opener;

        public Button Opener
        {
            get
            {
                if (_Opener == null)
                {
                    _Opener = transform.parent.GetComponent<Button>();
                }
                return _Opener;
            }
        }

        private Button[] _Buttons;

        protected Button[] Buttons
        {
            get
            {
                if (_Buttons == null)
                {
                    _Buttons = GetComponentsInChildren<Button>(true);
                }
                return _Buttons;
            }
        }

        protected virtual void Start()
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].onClick.AddListener(() => Opener.onClick.Invoke());
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            StartCoroutine(DisableCountdown());
        }

        protected IEnumerator DisableCountdown()
        {
            yield return new WaitForSecondsRealtime(15);
            if (gameObject.activeSelf)
            {
                Opener.onClick.Invoke();
            }
        }


    }
}
