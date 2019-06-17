using System.Collections;
using Drones.UI.SaveLoad;
using Drones.UI.Utils;
using UnityEngine;

namespace Drones.Utils
{
    public class EagleEyeCameraComponent : AbstractCamera
    {
        private static float _DefaultSize;
        private readonly float _DefaultHeight = 600;
        private static EagleEyeCameraComponent _EE;
        public static EagleEyeCameraComponent EagleEye
        {
            get
            {
                if (_EE == null)
                {
                    _EE = GameObject.FindWithTag("EagleEye").GetComponent<EagleEyeCameraComponent>();
                }
                return _EE;
            }
        }

        private void Awake()
        {
            _DefaultSize = CameraComponent.orthographicSize;
            _EE = this;
        }

        void OnEnable()
        {
            CameraComponent.orthographicSize = _DefaultSize;
            var v = RTSCameraComponent.RTS.transform.position;
            ActiveCamera = this;
            v.y = _DefaultHeight;
            transform.position = v;
            if (Followee != null)
            {
                StartCoroutine(FollowObject());
            }
        }
        protected override void OnDestroy()
        {
            _EE = null;
        }
        public override void BreakFollow()
        {
            _Following = false;
            Followee = null;
        }
        private void Update()
        {
            if (PriorityFocus.Count > 0) return;
            Controller.MoveLongitudinal(Input.GetAxis("Vertical") * SpeedScale);
            Controller.MoveLateral(Input.GetAxis("Horizontal") * SpeedScale);
            Controller.Rotate(Input.GetAxis("Rotate"));

            if (UIFocus.Hover == 0 && !UIFocus.Controlling)
            {
                Controller.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                if (Input.GetMouseButton(0))
                {
                    if (!Controlling)
                    {
                        StartCoroutine(ControlListener());
                    }
                    Controller.Roll(Input.GetAxis("Mouse X"));
                    Controller.ClampPitch();
                }
            }

            if (!_Following && Followee != null) { StartCoroutine(FollowObject()); }
        }

        protected override IEnumerator FollowObject()
        {
            var wait = new WaitForEndOfFrame();
            _Following = true;
            while (!Input.GetKeyDown(KeyCode.Space) && Followee != null)
            {
                var v = Followee.transform.position;
                v.y = _DefaultHeight;
                transform.position = v;
                yield return wait;
                v = Followee.transform.position;
                v.y = _DefaultHeight;
                transform.position = v;
                yield return null;

            }
            BreakFollow();
            yield break;
        }

        #region ICameraMovement
        public override void Zoom(float input)
        {
            CameraComponent.orthographicSize -= Controller.ZoomSpeed * 10 * input;
            CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, 0, 4000f);
        }

        public override void Roll(float input)
        {
            transform.Rotate(0, input, 0, Space.World);
        }

        public override void Rotate(float input)
        {
            Roll(input);
        }
        #endregion

    }
}
