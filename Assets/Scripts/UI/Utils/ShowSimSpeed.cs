using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace Drones.UI
{
    using UnityEngine;
    using Drones.Utils;
    public class ShowSimSpeed : MonoBehaviour
    {
        private static ShowSimSpeed Instance { get; set; }
        [SerializeField]
        Image _Slow;
        [SerializeField]
        Image _Play;
        [SerializeField]
        Image _Fast;
        [SerializeField]
        Image _Ultra;
        [SerializeField]
        Image _Pause;

        static void Init()
        {
            if (Instance == null)
            {
                Instance = UIManager.Transform.GetComponentInChildren<ShowSimSpeed>(true);
                Instance.gameObject.SetActive(true);
            }
            Image[] all = Instance.GetComponentsInChildren<Image>(true);
            foreach (var i in all)
            {
                switch (i.name)
                {
                    case "Pause":
                        Instance._Pause = i;
                        break;
                    case "Slow":
                        Instance._Slow = i;
                        break;
                    case "Play":
                        Instance._Play = i;
                        break;
                    case "Fast":
                        Instance._Fast = i;
                        break;
                    default:
                        Instance._Ultra = i;
                        break;
                }
            }
        }
        static Image Pause
        {
            get
            {
                if (Instance?._Pause == null)
                {
                    Init();
                }
                return Instance._Pause;
            }
        }
        static Image Slow
        {
            get
            {
                if (Instance?._Slow == null)
                {
                    Init();
                }
                return Instance._Slow;
            }
        }
        static Image Play
        {
            get
            {
                if (Instance?._Play == null)
                {
                    Init();
                }
                return Instance._Play;
            }
        }
        static Image Fast
        {
            get
            {
                if (Instance?._Fast == null)
                {
                    Init();
                }
                return Instance._Fast;
            }
        }
        static Image Ultra
        {
            get
            {
                if (Instance?._Ultra == null)
                {
                    Init();
                }
                return Instance._Ultra;
            }
        }

        public void Awake()
        {
            Instance = this;
        }

        public void OnDestroy()
        {
            Instance = null;
            Active = null;
        }

        public static Image Active { get; private set; }
        private Dictionary<TimeSpeed, Image> _SpeedToImage;
        private static Dictionary<TimeSpeed, Image> SpeedToImage
        {
            get
            {
                if (Instance._SpeedToImage == null)
                {
                    Instance._SpeedToImage = new Dictionary<TimeSpeed, Image>
                    {
                        {TimeSpeed.Pause, Pause},
                        {TimeSpeed.Slow, Slow},
                        {TimeSpeed.Normal, Play},
                        {TimeSpeed.Fast, Fast},
                        {TimeSpeed.Ultra, Ultra},
                        {TimeSpeed.WTF, Ultra}
                    };
                }
                return Instance._SpeedToImage;
            }
        }

        public static void OnSpeedChange()
        {
            if (Instance == null)
            {
                Instance = UIManager.Transform.GetComponentInChildren<ShowSimSpeed>(true);
                Instance.gameObject.SetActive(true);
            }
            Instance.StopAllCoroutines();

            Active?.gameObject.SetActive(false);
            Active = SpeedToImage[TimeKeeper.TimeSpeed];
            Active.gameObject.SetActive(true);
            Instance.transform.SetAsLastSibling();
            if (TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                Instance.StartCoroutine(Instance.Fade());
        }

        IEnumerator Fade()
        {
            var clock = Stopwatch.StartNew();
            var col = Active.color;
            col.a = 1;
            Active.color = col;
            while (clock.ElapsedMilliseconds / 1000 < 5)
            {
                col.a = 1 - clock.ElapsedMilliseconds / 5000f;
                Active.color = col;
                yield return null;
            }

        }
    }

}
