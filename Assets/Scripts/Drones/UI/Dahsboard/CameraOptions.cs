using Drones.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Dahsboard
{
    public class CameraOptions : MonoBehaviour
    {
        [SerializeField]
        Slider _MouseSensitivity;
        [SerializeField]
        Slider _MoveSpeed;
        [SerializeField]
        Slider _RotationSpeed;
        [SerializeField]
        Slider _ZoomSpeed;

        public Slider MouseSensitivity
        {
            get
            {
                if (_MouseSensitivity == null)
                {
                    _MouseSensitivity = transform.FindDescendant("Mouse Sensitivity").GetComponent<Slider>();
                }
                return _MouseSensitivity;
            }
        }
        public Slider MoveSpeed
        {
            get
            {
                if (_MoveSpeed == null)
                {
                    _MoveSpeed = transform.FindDescendant("Move Speed").GetComponent<Slider>();
                }
                return _MoveSpeed;
            }
        }
        public Slider RotationSpeed
        {
            get
            {
                if (_RotationSpeed == null)
                {
                    _RotationSpeed = transform.FindDescendant("Rotation Speed").GetComponent<Slider>();
                }
                return _RotationSpeed;
            }
        }
        public Slider ZoomSpeed
        {
            get
            {
                if (_ZoomSpeed == null)
                {
                    _ZoomSpeed = transform.FindDescendant("Zoom Speed").GetComponent<Slider>();
                }
                return _ZoomSpeed;
            }
        }

        private void Awake()
        {
            MouseSensitivity.value = RTSCameraComponent.RTS.Controller.MouseSensitivity / 10 - 1;
            MouseSensitivity.value = EagleEyeCameraComponent.EagleEye.Controller.MouseSensitivity / 10 - 1;
            MouseSensitivity.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value, 0, 100);
                RTSCameraComponent.RTS.Controller.MouseSensitivity = 10 + value * 10;
                EagleEyeCameraComponent.EagleEye.Controller.MouseSensitivity = 10 + value * 10;
            });
            MoveSpeed.value = RTSCameraComponent.RTS.Controller.MoveSpeed;
            MouseSensitivity.value = EagleEyeCameraComponent.EagleEye.Controller.MoveSpeed;
            MoveSpeed.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value, 0, 10);
                RTSCameraComponent.RTS.Controller.MoveSpeed = value + 1;
                EagleEyeCameraComponent.EagleEye.Controller.MoveSpeed = value + 1;
            });
            RotationSpeed.value = RTSCameraComponent.RTS.Controller.RotationSpeed;
            MouseSensitivity.value = EagleEyeCameraComponent.EagleEye.Controller.RotationSpeed;
            RotationSpeed.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value, 0, 10);
                RTSCameraComponent.RTS.Controller.RotationSpeed = value + 1;
                EagleEyeCameraComponent.EagleEye.Controller.RotationSpeed = value + 1;
            });
            ZoomSpeed.value = RTSCameraComponent.RTS.Controller.ZoomSpeed;
            MouseSensitivity.value = EagleEyeCameraComponent.EagleEye.Controller.ZoomSpeed;
            ZoomSpeed.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value, 0, 10);
                RTSCameraComponent.RTS.Controller.ZoomSpeed = value + 1;
                EagleEyeCameraComponent.EagleEye.Controller.ZoomSpeed = value + 1;
            });
        }
    }

}