using System;
using System.Collections;
using Drones.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace Drones.UI.Utils
{
    public class DataField : TextMeshProUGUI, IPointerEnterHandler
    {
        private string Value => text;

        [SerializeField]
        private readonly string _ToolTip = "";
        private WaitForSecondsRealtime wait = new WaitForSecondsRealtime(2);
        private bool _IsShowing;
        private static ToolTip _Tip;
        public static ToolTip Tip
        {
            get
            {
                if (_Tip == null)
                {
                    _Tip = Instantiate((GameObject)Resources.Load(Constants.ToolTipPath)).GetComponent<ToolTip>();
                }
                _Tip.gameObject.SetActive(false);
                return _Tip;
            }
        }

        public virtual void SetField(object v)
        {
            try
            {
                SetText(v.ToString());
                ForceMeshUpdate();
            }
            catch (NullReferenceException)
            {
                SetText((string)null);
                ForceMeshUpdate();
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_IsShowing) StartCoroutine(ShowToolTip());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(ShowToolTip());
            _IsShowing = false;
        }

        private IEnumerator ShowToolTip()
        {
            _IsShowing = true;
            yield return wait;
            if (_ToolTip != "")
            {
                string output = Value + "\n" + _ToolTip;
                Tip.gameObject.SetActive(true);
                transform.SetParent(UIManager.Transform, false);
                Tip.transform.position = transform.position;
                Tip.Body.SetText(output);
                Tip.Body.ForceMeshUpdate();
                var tmp = Tip.transform.ToRect().sizeDelta;
                tmp.y = Tip.Body.preferredHeight;
                Tip.transform.ToRect().sizeDelta = tmp;
            }
        }
    }

}