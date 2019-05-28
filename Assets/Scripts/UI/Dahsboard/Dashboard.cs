using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.Utils.Extensions;
    using Utils;
    public abstract class Dashboard : MonoBehaviour
    {
        protected static Dictionary<DashboardMode, Vector2> PanelSize = new Dictionary<DashboardMode, Vector2>
        {
            {DashboardMode.EditMode, new Vector2(550, 240)},
            {DashboardMode.Simulation, new Vector2(550, 450)}
        };
        [SerializeField]
        private GameObject _SimulationInfo;
        [SerializeField]
        private GameObject _CameraOptions;

        protected GameObject CameraOptions
        {
            get
            {
                if (_CameraOptions == null)
                {
                    _CameraOptions = transform.FindDescendent("Options Display").gameObject;
                }
                return _CameraOptions;
            }
        }
        protected GameObject SimulationInfo
        {
            get
            {
                if (_SimulationInfo)
                {
                    _SimulationInfo = transform.parent.GetChild(0).gameObject;
                }
                return _SimulationInfo;
            }
        }

        protected Dictionary<Transform, Button> _OwnerToButton;

        protected abstract Dictionary<Transform, Button> OwnerToButton { get; }

        protected GameObject _ActiveFoldable;

        protected void EnableFoldable(Button button)
        {
            if (PriorityFocus.Count > 0) return;

            if (_ActiveFoldable != null && _ActiveFoldable.gameObject.activeSelf)
            {
                OwnerToButton[_ActiveFoldable.transform.parent].onClick.Invoke();
            }
            _ActiveFoldable = button.GetComponentInChildren<FoldableMenu>(true).gameObject;
            _ActiveFoldable.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { DisableFoldable(button); });
        }

        protected void DisableFoldable(Button button)
        {
            _ActiveFoldable.SetActive(false);
            _ActiveFoldable = null;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { EnableFoldable(button); });
        }
    }

}