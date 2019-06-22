﻿using Drones.Managers;
using Drones.UI.Navigation;
using Drones.Utils;
using UnityEngine;

namespace Drones.UI.Dahsboard.Folded
{
    public class MapFoldable : FoldableTaskBar
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
            if (UIManager.Navigation != null && UIManager.Navigation.gameObject.activeSelf)
            {
                UIManager.Navigation.transform.SetAsLastSibling();
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
