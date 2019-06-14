namespace Drones.UI
{
    using Drones.Utils;
    using UnityEngine;
    using static UIManager;
    public class MapFoldable : FoldableMenu
    {
        protected override void Start()
        {
            Buttons[0].onClick.AddListener(CameraSwitch.OnEagleEye);
            Buttons[1].onClick.AddListener(CameraSwitch.OnRTS);
            Buttons[2].onClick.AddListener(OpenNavigationWindow);
            base.Start();
        }

        public static void OpenNavigationWindow()
        {
            if (Navigation != null && Navigation.gameObject.activeSelf)
            {
                Navigation.transform.SetAsLastSibling();
            } 
            else
            {
                const string naviPath = "Prefabs/UI/Windows/Navigation/Navigation Window";
                var n = Instantiate(Resources.Load(naviPath) as GameObject).GetComponent<NavigationWindow>();
                n.transform.SetParent(UIManager.Transform, false);
            }
        }
    }

    public static class CameraSwitch
    {
        public static void OnRTS()
        {
            RTSCameraComponent.RTS.gameObject.SetActive(true);
            EagleEyeCameraComponent.EagleEye.gameObject.SetActive(false);
        }

        public static void OnEagleEye()
        {
            RTSCameraComponent.RTS.gameObject.SetActive(false);
            EagleEyeCameraComponent.EagleEye.gameObject.SetActive(true);
        }
    }


}
