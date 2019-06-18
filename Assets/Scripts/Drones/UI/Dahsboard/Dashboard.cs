using System.Collections.Generic;
using Drones.UI.Dahsboard.Folded;
using Drones.Utils;
using UnityEngine;
using Utils;
using Selectable = Drones.UI.EditMode.Selectable;

namespace Drones.UI.Dahsboard
{
    public class Dashboard : MonoBehaviour
    {
        protected static Dictionary<DashboardMode, Vector2> DashboardSize = new Dictionary<DashboardMode, Vector2>
        {
            {DashboardMode.EditMode, new Vector2(550, 240)},
            {DashboardMode.Simulation, new Vector2(550, 450)}
        };
        [SerializeField]
        private SimulationInfo _SimulationInfo;
        [SerializeField]
        private CameraOptions _CameraOptions;
        [SerializeField]
        private SimulationPanel _SimPanel;
        [SerializeField]
        private EditPanel _EdPanel;

        public CameraOptions CameraOptions
        {
            get
            {
                if (_CameraOptions == null)
                {
                    _CameraOptions = GetComponentInChildren<CameraOptions>();
                }
                return _CameraOptions;
            }
        }
        public SimulationInfo SimulationInfo
        {
            get
            {
                if (_SimulationInfo == null)
                {
                    _SimulationInfo = GetComponentInChildren<SimulationInfo>();
                }
                return _SimulationInfo;
            }
        }
        public SimulationPanel SimPanel
        {
            get
            {
                if (_SimPanel == null)
                {
                    _SimPanel = GetComponentInChildren<SimulationPanel>();
                }
                return _SimPanel;
            }
        }
        public EditPanel EdPanel
        {
            get
            {
                if (_EdPanel == null)
                {
                    _EdPanel = GetComponentInChildren<EditPanel>();
                }
                return _EdPanel;
            }
        }

        public void OnEdit()
        {
            AbstractCamera.ActiveCamera?.BreakFollow();
            CameraSwitch.OnEagleEye();
            transform.ToRect().sizeDelta = DashboardSize[DashboardMode.EditMode];
            SimulationInfo.gameObject.SetActive(false);
            SimPanel.gameObject.SetActive(false);
            EdPanel.gameObject.SetActive(true);
            CameraOptions.gameObject.SetActive(true);
        }

        public void OnRun()
        {
            Selectable.Deselect();
            Selectable.DeleteMode = false;
            AbstractCamera.ActiveCamera?.BreakFollow();
            CameraSwitch.OnRTS();
            transform.ToRect().sizeDelta = DashboardSize[DashboardMode.Simulation];
            SimulationInfo.gameObject.SetActive(true);
            SimPanel.gameObject.SetActive(true);
            EdPanel.gameObject.SetActive(false);
            CameraOptions.gameObject.SetActive(false);
        }

    }

}