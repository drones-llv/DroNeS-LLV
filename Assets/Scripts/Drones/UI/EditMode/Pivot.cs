using Drones.UI.Utils;
using UnityEngine;

namespace Drones.UI.EditMode
{
    public class Pivot : MonoBehaviour
    {
        private Vector3 _Origin;
        private Vector3 _Anchor;
        private Vector3 _CurrPos;
        private Vector2 _ScreenPos;
        private Vector3 _OldScale;
        private Selectable _Owner;
        private Pivot[] _Siblings;
        private Pivot _Opposite;

        private Camera Cam
        {
            get
            {
                return Selectable.Cam;
            }
        }

        private Selectable Owner
        {
            get
            {
                if (_Owner == null)
                {
                    _Owner = Selectable.Selected;
                }
                return _Owner;
            }
        }

        private Pivot[] Siblings
        {
            get
            {
                if (_Siblings == null)
                {
                    _Siblings = transform.parent.GetComponentsInChildren<Pivot>(true);
                }
                return _Siblings;
            }
        }

        private Pivot Opposite
        {
            get
            {
                if (_Opposite == null)
                {
                    Vector3 v;
                    for (int i = 0; i < Siblings.Length; i++)
                    {
                        v = Siblings[i].transform.localPosition + transform.localPosition;
                        v.y = 0;
                        if (v.magnitude < 0.001f)
                        {
                            _Opposite = _Siblings[i];
                            break;
                        }
                    }
                }
                return _Opposite;
            }
        }

        public static bool Operating { get; private set; }

        private void Awake()
        {
            Owner.transform.position += Vector3.zero;
        }

        private void OnMouseDown()
        {
            Operating = true;
            StartCoroutine(UIFocus.ControlListener());
            _ScreenPos = Input.mousePosition;
            _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
            _Origin.y = 0;
            _Anchor = Opposite.transform.position;
        }

        private void OnMouseUp()
        {
            Operating = false;
            StartCoroutine(Owner.WaitForDeselect());
        }

        private void OnMouseDrag()
        {
            _ScreenPos = Input.mousePosition;
            _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
            _CurrPos.y = 0;

            transform.SetParent(null, true);
            _OldScale = transform.localScale;
            transform.SetParent(Owner.transform, true);

            var s = Owner.transform.localScale;
            var ds = DeltaScale();
            s += ds;

            s.x = Mathf.Clamp(s.x, 5f, float.MaxValue);
            s.z = Mathf.Clamp(s.z, 5f, float.MaxValue);

            Owner.transform.localScale = s;

            for (int i = 0; i < Siblings.Length; i++)
            {
                Siblings[i].transform.SetParent(null, true);
                Siblings[i].transform.localScale = _OldScale;
                Siblings[i].transform.SetParent(Owner.transform,true);
            }

            if (!(s.x <= 5.01f) && !(s.z <= 5.01f))
            {
                Owner.transform.position += _Anchor - Opposite.transform.position;
            }

            _Origin = _CurrPos;
        }

        private Vector3 DeltaScale()
        {
            Vector3 flip = transform.localPosition;
            Vector3 move = _CurrPos - _Origin;
            var mag = move.magnitude;
            move = move.normalized;
            Vector4 v = new Vector4(move.x, move.y, move.z, 0);
            move = Owner.transform.worldToLocalMatrix * v;

            flip.x /= Mathf.Abs(flip.x);
            flip.y /= Mathf.Abs(flip.y);
            flip.z /= Mathf.Abs(flip.z);

            move.x *= flip.x;
            move.y *= flip.y;
            move.z *= flip.z;

            return move.normalized * mag;
        }

    }
}
