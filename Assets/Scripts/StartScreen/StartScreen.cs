using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Drones.StartScreen
{
    using Drones.Managers;
    using Drones.UI;

    public class StartScreen : MonoBehaviour
    {
        public static StartScreen Instance { get; private set; }

        [SerializeField]
        LoadingAnimation _Loading;

        private static OptionsMenu OM => OptionsMenu.Instance;

        private static MainMenu MM => MainMenu.Instance;

        public LoadingAnimation Loading
        {
            get
            {
                if (_Loading == null)
                {
                    _Loading = Instance.transform.parent.GetComponentInChildren<LoadingAnimation>(true);
                }
                return _Loading;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Loading.gameObject.SetActive(false);
            OM.gameObject.SetActive(false);
            StartCoroutine(ClearSimulationScene());
        }

        public static void ShowOptions()
        {
            OM.gameObject.SetActive(true);
            MM.gameObject.SetActive(false);
        }

        public static void ShowMain()
        {
            OM.gameObject.SetActive(false);
            MM.gameObject.SetActive(true);
        }

        public static void OnPlay() => Instance.StartCoroutine(Instance.LoadSimulationScene());

        private IEnumerator LoadSimulationScene()
        {
            yield return new WaitUntil(() => SceneManager.sceneCount == 1);
            Loading.gameObject.SetActive(true);
            MM.gameObject.SetActive(false);
            yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            yield return new WaitUntil(() => SimManager.LoadComplete && Time.unscaledDeltaTime < 1 / 30f);

            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
            yield return SceneManager.UnloadSceneAsync(0);
        }

        private IEnumerator ClearSimulationScene()
        {
            yield return null;
            if (SceneManager.sceneCount > 1)
            {
                yield return SceneManager.UnloadSceneAsync(1);
            }
        }

    }
}
