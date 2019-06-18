using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.StartScreen
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; private set; }
        [SerializeField]
        Button _Play;
        [SerializeField]
        Button _Options;
        [SerializeField]
        Button _Quit;

        private void OnDestroy()
        {
            Instance = null;
        }

        public Button Play
        {
            get
            {
                if (_Play == null)
                {
                    _Play = transform.FindDescendant("Play").GetComponent<Button>();
                }
                return _Play;
            }
        }

        public Button Options
        {
            get
            {
                if (_Options == null)
                {
                    _Options = transform.FindDescendant("Options").GetComponent<Button>();
                }
                return _Options;
            }
        }

        public Button Quit
        {
            get
            {
                if (_Quit == null)
                {
                    _Quit = transform.FindDescendant("Quit").GetComponent<Button>();

                }
                return _Quit;
            }
        }

        private void Awake()
        {
            Instance = this;
            Quit.onClick.AddListener(Application.Quit);
            Play.onClick.AddListener(StartScreen.OnPlay);
            Options.onClick.AddListener(StartScreen.ShowOptions);
        }

    }
}

