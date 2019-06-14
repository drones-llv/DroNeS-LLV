using System.Collections;
using System.Collections.Generic;
using Drones.Utils.Extensions;
using UnityEngine;

namespace Drones.UI
{
    using Utils;
    public class UIManager : MonoBehaviour
    {
        public static UIManager New()
        {
            var um = Instantiate(Resources.Load(Constants.UIManagerPath) as GameObject).GetComponent<UIManager>();
            _Instance = um;
            return _Instance;
        }

        private static UIManager _Instance;

        public static Transform Transform => _Instance?.transform;

        private readonly List<AbstractWindow> _Windows = new List<AbstractWindow>();

        [SerializeField]
        private NavigationWindow _Navigation;
        [SerializeField]
        private Dashboard _Dashboard;

        public static NavigationWindow Navigation
        {
            get
            {
                if (_Instance._Navigation == null)
                {
                    _Instance._Navigation = _Instance.GetComponentInChildren<NavigationWindow>();
                }
                return _Instance._Navigation;
            }
        }
        public static Dashboard Dashboard
        {
            get
            {
                if (_Instance._Dashboard == null)
                {
                    _Instance._Dashboard = _Instance.GetComponentInChildren<Dashboard>();
                }
                return _Instance._Dashboard;
            }
        }

        private void OnDestroy()
        {

            while (_Windows != null && _Windows.Count > 0)
            {
                if (_Windows.Count <= 0) break;

                _Windows[0].Close.onClick.Invoke();
            }

            _Instance = null;
        }

        public static void AddToList(AbstractWindow window)
        {
            var type = window.GetType();
            if (type != typeof(ConsoleLog) && type != typeof(NavigationWindow))
            {
                _Instance._Windows.Add(window);
                window.transform.SetParent(_Instance.transform, false);
                window.transform.ToRect().anchoredPosition = Vector2.one * -25 * _Instance._Windows.Count;
            }
        }

        public static AbstractWindow GetTop()
        {
            if (_Instance._Windows.Count == 0) return null;

            _Instance._Windows.Sort((x, y) => y.transform.GetSiblingIndex() - x.transform.GetSiblingIndex());
            return _Instance._Windows[0];
        }

        public static void Remove(AbstractWindow window)
        {
            if (_Instance._Windows.Count == 0) return;
            _Instance._Windows.Remove(window);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GetTop()?.Close?.onClick.Invoke();
            }
        }

    }
}

