using System.Collections;
using Drones.Managers;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.EditMode
{
    public class Selectable: MonoBehaviour
    {
        private static readonly WaitUntil _Wait = new WaitUntil(() => Input.GetMouseButton(0));
        public static void Reset()
        {
            _Cam = null;
            Selected = null;
            _DeleteMode = false;
            _IsMoving = false;
        }
        private static Camera _Cam;
        private const float _ClickDelta = 0.35f;
        public static Camera Cam
        {
            get
            {
                if (_Cam == null)
                {
                    _Cam = EagleEyeCameraComponent.EagleEye.GetComponentInChildren<Camera>();
                }
                return _Cam;
            }
        }
        public static Selectable Selected { get; private set; }
        private static bool _DeleteMode;
        private static bool _IsMoving;
        public static bool DeleteMode
        {
            get
            {
                return _DeleteMode;
            }
            set
            {
                _DeleteMode = value;
                SimManager.Instance.StartCoroutine(CancelDelete());
            }
        }

        #region Fields
        private float _ClickTime;
        private bool _FirstClick;
        private bool _Rotating;
        private Vector3 _Origin;
        private Vector3 _CurrPos;
        private Vector2 _ScreenPos;
        [SerializeField]
        private bool _Editable;
        [SerializeField]
        private IDataSource _ThisSource;
        #endregion

        private IDataSource ThisSource
        {
            get
            {
                if (_ThisSource == null)
                {
                    _ThisSource = GetComponent<IDataSource>();
                }
                return _ThisSource;
            }
        }

        private bool Editable
        {
            get
            {
                return SimManager.Status == SimulationStatus.EditMode && _Editable;
            }
            set
            {
                _Editable = value;
            }
        }

        #region Mouse Interactions
        private void OnMouseEnter()
        {
            if (Editable && Selected == this)
            {
                StopCoroutine(WaitForDeselect());
            }
        }

        private void OnMouseUpAsButton()
        {
            if (Editable)
            {
                if (Selected != null && Selected != this)
                {
                    Deselect();
                    Select();
                }
                else if (Selected == null)
                {
                    Select();
                }
                else if (DeleteMode && Selected == this)
                {
                    ((IPoolable)ThisSource).Delete();
                    DeleteMode = false;
                }
                _IsMoving = false;

            }

            if (!DeleteMode)
            {
                if (_FirstClick && Time.unscaledTime - _ClickTime > _ClickDelta)
                {
                    _FirstClick = false;
                }

                if (_FirstClick && Time.unscaledTime - _ClickTime <= _ClickDelta)
                {
                    ThisSource.OpenInfoWindow();
                }
                else
                {
                    _FirstClick = true;
                    _ClickTime = Time.unscaledTime;
                }
            }

        }

        public static void ShortcutDelete()
        {
            if (Selected == null) return;

            Selected.GetComponent<IPoolable>().Delete();
            DeleteMode = false;
        }

        private void OnMouseDown()
        {
            if (Editable && Selected == this)
            {
                StartCoroutine(UIFocus.ControlListener());
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
            }
        }

        private void OnMouseExit()
        {
            if (Editable && Selected == this)
            {
                StartCoroutine(WaitForDeselect());
            }
        }

        private void OnMouseDrag()
        {
            if (Editable && Selected == this)
            {
                _IsMoving = true;
                _ScreenPos = Input.mousePosition;
                _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _CurrPos.y = 0;
                transform.position += _CurrPos - _Origin;
                _Origin = _CurrPos;
            }
        }

        private void OnMouseOver()
        {
            if (!_Rotating && Editable && Input.GetMouseButtonDown(1))
            {
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
                StartCoroutine(Rotation());
            }
        }
        #endregion

        private void OnDisable()
        {
            if (Selected == this)
            {
                Deselect();
            }
            StopAllCoroutines();
        }

        private IEnumerator Rotation()
        {
            _Rotating = true;
            while (!Input.GetMouseButtonUp(1))
            {
                _ScreenPos = Input.mousePosition;
                _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _CurrPos.y = 0;
                Vector3 t = _CurrPos - transform.position;
                Vector3 c = _Origin - transform.position;

                float angle = Vector3.SignedAngle(c, t, Vector3.up);
                transform.RotateAround(transform.position, Vector3.up, angle);
                _Origin = _CurrPos;
                yield return null;
            }
            _Rotating = false;
            yield break;
        }

        private void Select()
        {
            Selected = this;
            var pivots = GetComponentsInChildren<Pivot>(true);
            for (int i = 0; i < pivots.Length; i++)
            {
                pivots[i].gameObject.SetActive(true);
            }
            if (pivots.Length == 0)
            {
                HighlightHub(this);
            }
        }

        private static GameObject _HubHighlight;
        private static void HighlightHub(Selectable obj)
        {
            if (_HubHighlight == null)
            {
                _HubHighlight = Instantiate((GameObject)Resources.Load(Constants.HubHighlightPath));
                _HubHighlight.name = "Hub Highlight";
            }
            _HubHighlight.SetActive(true);
            _HubHighlight.transform.SetParent(obj.transform, true);
            _HubHighlight.transform.localPosition = Vector3.zero;
        }

        public static void DehighlightHub() => _HubHighlight?.SetActive(false);

        public static void Deselect()
        {
            if (Pivot.Operating || Selected == null) { return; } // If in the middle of resizing don't deselect
            if (Selected.transform.childCount > 3)
            {
                var pivots = Selected.transform.GetComponentsInChildren<Pivot>(true);
                for (int i = 0; Selected != null && i < pivots.Length; i++)
                {
                    pivots[i].gameObject.SetActive(false);
                }
            }
            DehighlightHub();
            Selected = null;
        }

        public IEnumerator WaitForDeselect()
        {
            if (Selected == null) { yield break; }

            yield return _Wait;
            _ScreenPos = Input.mousePosition;
            Vector3 origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
            origin.y = 800;

            if ((!Physics.Raycast(new Ray(origin, Vector3.down), out RaycastHit info, 800, 1 << 14)
            || (info.transform.parent != Selected && info.transform != Selected)) && !_IsMoving)
            {
                Deselect();
            }

            yield break;
        }

        private static IEnumerator CancelDelete()
        {
            if (DeleteMode)
            {
                yield return new WaitUntil(() => Input.GetMouseButtonUp(1));
                DeleteMode = false;
            }
        }

    }
}
