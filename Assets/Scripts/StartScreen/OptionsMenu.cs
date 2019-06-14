using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Drones.StartScreen
{
    using System;
    using Drones.UI;
    using Drones.Utils;
    using Drones.Utils.Extensions;
    using Drones.Utils.Scheduler;

    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        public TextMeshProUGUI RLDisplay;
        public TextMeshProUGUI LPDisplay;
        public TextMeshProUGUI ASDisplay;

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
        Toggle _SaveToggle;
        [SerializeField]
        Slider _SavePeriod;
        [SerializeField]
        Button _Back;
        [SerializeField]
        Button _Reset;
        [SerializeField]
        TMP_Dropdown _Schdeuler;

        public Toggle SaveToggle
        {
            get
            {
                if (_SaveToggle == null)
                {
                    _SaveToggle = GetComponentsInChildren<Toggle>(true)[1];
                }
                return _SaveToggle;
            }
        }

        public Slider SavePeriod
        {
            get
            {
                if (_SavePeriod == null) _SavePeriod = GetComponentsInChildren<Slider>(true)[1];
                return _SavePeriod;
            }
        }


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
                    _RenderToggle = GetComponentsInChildren<Toggle>(true)[2];
                }
                return _RenderToggle;
            }
        }

        public Slider RenderLimit
        {
            get
            {
                if (_RenderLimit == null) _RenderLimit = GetComponentsInChildren<Slider>(true)[2];
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

            SaveToggle.onValueChanged.AddListener((bool value) =>
            {
                DataLogger.IsAutosave = value;
                SavePeriod.enabled = value;
            });

            SavePeriod.onValueChanged.AddListener((float value) =>
            {
                ASDisplay.SetText(value.ToString());
                DataLogger.AutosavePeriod = value;
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
            SaveToggle.onValueChanged.Invoke(DataLogger.IsAutosave);
            SavePeriod.onValueChanged.Invoke(DataLogger.AutosavePeriod);
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

        private void GoBack() => StartScreen.ShowMain();

        private void OnReset()
        {
            RenderToggle.isOn = true;
            RenderLimit.value = 0;
            LogToggle.isOn = true;
            LogPeriod.value = 60;
            SaveToggle.isOn = true;
            SavePeriod.value = 300;
            Scheduler.value = (int)Scheduling.FCFS;
            RenderLimit.onValueChanged.Invoke(RenderLimit.value);
            RenderToggle.onValueChanged.Invoke(RenderToggle.isOn);
            LogToggle.onValueChanged.Invoke(LogToggle.isOn);
            LogPeriod.onValueChanged.Invoke(LogPeriod.value);
            SaveToggle.onValueChanged.Invoke(SaveToggle.isOn);
            SavePeriod.onValueChanged.Invoke(SavePeriod.value);
            Scheduler.onValueChanged.Invoke(Scheduler.value);
        }

    }
}
