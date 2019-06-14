using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Drones.StartScreen
{
    using System;
    using Drones.Managers;
    using Drones.UI;
    using Drones.Utils;
    using Drones.Utils.Extensions;
    using Drones.Utils.Scheduler;

    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        public TextMeshProUGUI RLDisplay;
        public TextMeshProUGUI LPDisplay;

        private void OnDestroy()
        {
            Instance = null;
        }
        [SerializeField]
        Toggle _RenderToggle;
        [SerializeField]
        Slider _RenderLimit;
        [SerializeField]
        Toggle _LogToggle;
        [SerializeField]
        Slider _LogPeriod;
        [SerializeField]
        Button _Back;
        [SerializeField]
        Button _Reset;
        [SerializeField]
        TMP_Dropdown _Schdeuler;

        public Toggle LogToggle
        {
            get
            {
                if (_LogToggle == null)
                {
                    _LogToggle = GetComponentsInChildren<Toggle>(true)[0];
                }
                return _LogToggle;
            }
        }

        public Slider LogPeriod
        {
            get
            {
                if (_LogPeriod == null) _LogPeriod = GetComponentsInChildren<Slider>(true)[0];
                return _LogPeriod;
            }
        }

        public Toggle RenderToggle
        {
            get
            {
                if (_RenderToggle == null)
                {
                    _RenderToggle = GetComponentsInChildren<Toggle>(true)[1];
                }
                return _RenderToggle;
            }
        }

        public Slider RenderLimit
        {
            get
            {
                if (_RenderLimit == null) _RenderLimit = GetComponentsInChildren<Slider>(true)[1];
                return _RenderLimit;
            }
        }

        public Button Back
        {
            get
            {
                if (_Back == null)
                {
                    _Back = transform.FindDescendent("Back").GetComponent<Button>();
                }
                return _Back;
            }
        }

        public Button Reset
        {
            get
            {
                if (_Reset == null)
                {
                    _Reset = transform.FindDescendent("Reset").GetComponent<Button>();
                }
                return _Reset;
            }
        }

        public TMP_Dropdown Scheduler
        {
            get
            {
                if (_Schdeuler) _Schdeuler = GetComponentInChildren<TMP_Dropdown>();
                return _Schdeuler;
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
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

        private void GoBack() => StartScreen.ShowMain();

        private void OnReset()
        {
            RenderLimit.value = 0;
            RenderToggle.isOn = true;
            LogToggle.isOn = true;
            LogPeriod.value = 300;
            Scheduler.value = (int)Scheduling.FCFS;
            RenderLimit.onValueChanged.Invoke(RenderLimit.value);
            RenderToggle.onValueChanged.Invoke(RenderToggle.isOn);
            LogToggle.onValueChanged.Invoke(LogToggle.isOn);
            LogPeriod.onValueChanged.Invoke(LogPeriod.value);
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

    }
}
