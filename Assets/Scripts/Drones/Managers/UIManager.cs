using System.Collections.Generic;
using Drones.UI.Console;
using Drones.UI.Dahsboard;
using Drones.UI.Navigation;
using Drones.UI.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Drones.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager New()
        {
            var um = Instantiate(Resources.Load(Constants.UIManagerPath) as GameObject).GetComponent<UIManager>();
            _instance = um;
            return _instance;
        }

        private static UIManager _instance;

        public static Transform Transform => _instance?.transform;

        private readonly List<AbstractWindow> _windows = new List<AbstractWindow>();

        [FormerlySerializedAs("_Navigation")] [SerializeField]
        private NavigationWindow navigation;
        [FormerlySerializedAs("_Dashboard")] [SerializeField]
        private Dashboard dashboard;

        public static NavigationWindow Navigation
        {
            get
            {
                if (_instance.navigation == null)
                {
                    _instance.navigation = _instance.GetComponentInChildren<NavigationWindow>();
                }
                return _instance.navigation;
            }
        }
        public static Dashboard Dashboard
        {
            get
            {
                if (_instance.dashboard == null)
                {
                    _instance.dashboard = _instance.GetComponentInChildren<Dashboard>();
                }
                return _instance.dashboard;
            }
        }

        private void OnDestroy()
        {

            while (_windows != null && _windows.Count > 0)
            {
                if (_windows.Count <= 0) break;

                _windows[0].Close.onClick.Invoke();
            }

            _instance = null;
        }

        public static void AddToList(AbstractWindow window)
        {
            var type = window.GetType();
            if (type == typeof(ConsoleLog) || type == typeof(NavigationWindow)) return;
            _instance._windows.Add(window);
            window.transform.SetParent(_instance.transform, false);
            window.transform.ToRect().anchoredPosition = -25 * _instance._windows.Count * Vector2.one;
        }

        private static AbstractWindow GetTop()
        {
            if (_instance._windows.Count == 0) return null;

            _instance._windows.Sort((x, y) => y.transform.GetSiblingIndex() - x.transform.GetSiblingIndex());
            return _instance._windows[0];
        }

        public static void Remove(AbstractWindow window)
        {
            if (_instance._windows.Count == 0) return;
            _instance._windows.Remove(window);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_windows.Count <= 0) return;

            var close = GetTop().Close;
            if (close != null) close.onClick.Invoke();
        }

    }
}

