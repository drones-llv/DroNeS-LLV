using System.Collections;
using Drones.UI.SaveLoad;
using Drones.UI.Utils;
using UnityEngine;

namespace Drones.Utils
{
    public class RTSCameraComponent : AbstractCamera
    {
        [SerializeField]
        private float _FollowDistance = 10f;
        public float FollowDistance
        {
            get
            {
                return _FollowDistance;
            }
            set
            {
                if (_FollowDistance > 0)
                {
                    _FollowDistance = value;
                }
            }
        }
        private static RTSCameraComponent _RTS;
        public static RTSCameraComponent RTS
        {
            get
            {
                if (_RTS == null)
                {
                    _RTS = GameObject.FindWithTag("RTSCamera").GetComponent<RTSCameraComponent>();
                }
                return _RTS;
            }
        }

        private void Awake() { _RTS = this; }

        private void OnEnable()
        {
            EagleEyeCameraComponent.EagleEye.gameObject.SetActive(false);
            transform.position = EagleEyeCameraComponent.EagleEye.transform.position;
            ActiveCamera = this;
        }

        protected override void OnDestroy()
        {
            _RTS = null;
        }

        private void Update()
        {
            if (PriorityFocus.Count > 0) return;
            Controller.MoveLongitudinal(Input.GetAxis("Vertical") * SpeedScale);
            Controller.MoveLateral(Input.GetAxis("Horizontal") * SpeedScale);
            Controller.Rotate(Input.GetAxis("Rotate"));

            if (UIFocus.Hover == 0)
            {
                Controller.Zoom(Input.GetAxis("Mouse ScrollWheel") * SpeedScale);
                //FPS mouse hold click
                if (Input.GetMouseButton(0) && !UIFocus.Controlling)
                {
                    if (!Controlling) 
                    {
                        StartCoroutine(ControlListener()); 
                    }
                    Controller.Pitch(Input.GetAxis("Mouse Y"));
                    Controller.Yaw(Input.GetAxis("Mouse X"));
                    Controller.ClampPitch();
                }
            }

            // Bounds
            Controller.ClampVertical();

            if (!_Following && Followee != null) { StartCoroutine(FollowObject()); }
        }

        public override void BreakFollow()
        {
            _Following = false;
            Followee = null;
        }

        protected override IEnumerator FollowObject()
        {
            var wait = new WaitForEndOfFrame();
            _Following = true;
            while (!Input.GetKeyDown(KeyCode.Space) && Followee != null)
            {
                transform.position = Followee.transform.position - CameraTransform.forward * FollowDistance;
                yield return wait;
                transform.position = Followee.transform.position - CameraTransform.forward * FollowDistance;
                yield return null;
            }
            AbstractCamera.ActiveCamera.BreakFollow();
            yield break;
        }

        #region ICameraMovement Implementation
        public override void Zoom(float input)
        {
            if (_Following || UIFocus.Hover != 0) return;
            Vector3 positiveDirection = CameraTransform.forward;
            // Cannot zoom when facing up
            if (positiveDirection.y < 0)
            {
                transform.position += input * positiveDirection * Time.unscaledDeltaTime;
            }
        }

        public override void Pitch(float input)
        {
            if (UIFocus.Controlling) return;
            transform.Rotate(input, 0, 0);
        }

        public override void Yaw(float input)
        {
            if (UIFocus.Controlling) return;
            transform.Rotate(0, input, 0, Space.World);
        }

        public override void Rotate(float input)
        {
            float scale = (Controller.Floor - transform.position.y) / CameraTransform.forward.y;
            Vector3 point = transform.position + CameraTransform.forward * scale;
            transform.RotateAround(point, Vector3.up, input);
        }

        public override void ClampVertical(float lowerBound, float upperBound)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y, lowerBound, upperBound);
            transform.position = position;
        }

        public override void ClampPitch(float lowerAngle, float upperAngle)
        {
            Vector3 front = Vector3.Cross(CameraTransform.right, Vector3.up).normalized;
            if (CameraTransform.forward.y > 0)
            {
                float up = Vector3.Angle(front, CameraTransform.forward);
                if (up > upperAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(up - upperAngle, Vector3.right);
                }
                if (lowerAngle < 0 && up < -lowerAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(up + lowerAngle, Vector3.right);
                }
            }
            else
            {
                float down = Vector3.Angle(front, CameraTransform.forward);
                if (down > lowerAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(down - lowerAngle, -Vector3.right);
                }
                if (upperAngle < 0 && down < -upperAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(down + upperAngle, -Vector3.right);
                }
            }
        }

        #endregion

    }
}