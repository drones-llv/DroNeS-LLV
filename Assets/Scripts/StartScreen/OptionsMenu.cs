using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Drones.StartScreen
{
    using Drones.Managers;
    using Drones.Utils;
    using Drones.Utils.Extensions;


    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        public TextMeshProUGUI sliderDisplay;

        private void OnDestroy()
        {
            Instance = null;
        }

        [SerializeField]
        Slider _RenderLimit;
        [SerializeField]
        Toggle _LogToggle;
        [SerializeField]
        Button _Back;
        [SerializeField]
        Button _Reset;

        public Toggle LogToggle
        {
            get
            {
                if (_LogToggle == null)
                {
                    _LogToggle = GetComponentInChildren<Toggle>(true);
                }
                return _LogToggle;
            }
        }

        public Slider RenderLimit
        {
            get
            {
                if (_RenderLimit == null)
                {
                    _RenderLimit = GetComponentInChildren<Slider>(true);
                }
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

        private void Awake()
        {
            Instance = this;

            RenderLimit.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value * 5, 0, 600);
                sliderDisplay.SetText(value.ToString());
                CustomMap.FilterHeight = value;
            });

            LogToggle.onValueChanged.AddListener((bool value) =>
            {
                SimManager.IsLogging = value;
            });

            Back.onClick.AddListener(GoBack);
            Reset.onClick.AddListener(OnReset);
        }

        private void GoBack()
        {
            StartScreen.ShowMain();
        }

        private void OnReset()
        {
        }

    }
}
