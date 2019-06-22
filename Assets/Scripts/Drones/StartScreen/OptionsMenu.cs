using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using Drones.Mapbox;
using Drones.Objects;
using Drones.Scheduler;
using Drones.UI.SaveLoad;
using TMPro;
using Utils;

namespace Drones.StartScreen
{
    using System;
    using Drones.Utils;

    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        public TextMeshProUGUI RLDisplay;
        public TextMeshProUGUI LPDisplay;
        public TextMeshProUGUI batteryDroneRatioDisplay;

        private void OnDestroy()
        {
            Instance = null;
        }
        [SerializeField] private Toggle renderToggle;
        [SerializeField] private Slider renderLimit;
        [SerializeField] private Toggle logToggle;
        [SerializeField] private Slider logPeriod;
        [SerializeField] private Slider batteryToDroneRatioSlider;
        [SerializeField] private Button back;
        [SerializeField] private Button reset;
        [SerializeField] private TMP_Dropdown scheduler;

        public Slider BatteryToDroneRatioSlider
        {
            get
            {
                if (batteryToDroneRatioSlider == null) batteryToDroneRatioSlider = GetComponentsInChildren<Slider>(true)[1];
                return batteryToDroneRatioSlider;
            }
        }


        public Toggle LogToggle
        {
            get
            {
                if (logToggle == null)
                {
                    logToggle = GetComponentsInChildren<Toggle>(true)[0];
                }
                return logToggle;
            }
        }

        public Slider LogPeriod
        {
            get
            {
                if (logPeriod == null) logPeriod = GetComponentsInChildren<Slider>(true)[0];
                return logPeriod;
            }
        }

        public Toggle RenderToggle
        {
            get
            {
                if (renderToggle == null)
                {
                    renderToggle = GetComponentsInChildren<Toggle>(true)[2];
                }
                return renderToggle;
            }
        }

        public Slider RenderLimit
        {
            get
            {
                if (renderLimit == null) renderLimit = GetComponentsInChildren<Slider>(true)[2];
                return renderLimit;
            }
        }

        public Button Back
        {
            get
            {
                if (back == null)
                {
                    back = transform.FindDescendant("Back").GetComponent<Button>();
                }
                return back;
            }
        }

        public Button Reset
        {
            get
            {
                if (reset == null)
                {
                    reset = transform.FindDescendant("Reset").GetComponent<Button>();
                }
                return reset;
            }
        }

        public TMP_Dropdown Scheduler
        {
            get
            {
                if (scheduler) scheduler = GetComponentInChildren<TMP_Dropdown>();
                return scheduler;
            }
        }

        private void Awake()
        {
            Instance = this;

            RenderLimit.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value * 5, 0, 600);
                RLDisplay.SetText(value.ToString());
                CustomMap.FilterHeight = value;
            });

            LogToggle.onValueChanged.AddListener((bool value) =>
            {
                DataLogger.IsLogging = value;
                LogPeriod.enabled = value;
            });

            LogPeriod.onValueChanged.AddListener((float value) =>
            {
                LPDisplay.SetText(value.ToString());
                DataLogger.LoggingPeriod = value;
            });

            BatteryToDroneRatioSlider.onValueChanged.AddListener((float value) =>
            {
                batteryDroneRatioDisplay.SetText(value.ToString());
                Hub.BatteryPerDrone = Mathf.FloorToInt(value);
            });

            Scheduler.onValueChanged.AddListener((int arg0) =>
            {
                JobScheduler.ALGORITHM = (Scheduling)Enum.Parse(typeof(Scheduling), Scheduler.options[arg0].text);
            });

            Back.onClick.AddListener(GoBack);
            Reset.onClick.AddListener(OnReset);
        }

        private void OnEnable()
        {
            RenderLimit.onValueChanged.Invoke(RenderLimit.value);
            RenderToggle.onValueChanged.Invoke(RenderToggle.isOn);
            LogToggle.onValueChanged.Invoke(DataLogger.IsLogging);
            LogPeriod.onValueChanged.Invoke(DataLogger.LoggingPeriod);
            BatteryToDroneRatioSlider.onValueChanged.Invoke(Hub.BatteryPerDrone);
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

        private static void GoBack() => StartScreen.ShowMain();

        private void OnReset()
        {
            RenderToggle.isOn = true;
            RenderLimit.value = 0;
            LogToggle.isOn = true;
            LogPeriod.value = 60;
            BatteryToDroneRatioSlider.value = 300;
            Scheduler.value = (int)Scheduling.FCFS;
            RenderLimit.onValueChanged.Invoke(RenderLimit.value);
            RenderToggle.onValueChanged.Invoke(RenderToggle.isOn);
            LogToggle.onValueChanged.Invoke(LogToggle.isOn);
            LogPeriod.onValueChanged.Invoke(LogPeriod.value);
            BatteryToDroneRatioSlider.onValueChanged.Invoke(BatteryToDroneRatioSlider.value);
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

    }
}
