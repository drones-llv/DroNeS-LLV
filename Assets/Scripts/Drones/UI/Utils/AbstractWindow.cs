using System.Collections.Generic;
using Drones.Managers;
using Drones.Utils;
using Drones.Utils.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Utils
{
    public abstract class AbstractWindow : MonoBehaviour, IPoolable
    {
        public static uint OpenWindowCount { get; private set; }
        public static AbstractWindow GetWindow(Transform current)
        {

            if (current != null)
            {
                AbstractWindow window = current.GetComponent<AbstractWindow>();
                if (window == null)
                {
                    return GetWindow(current.parent);
                }
                return window;
            }
            return null;
        }

        #region Fields
        [SerializeField]
        private TextMeshProUGUI _WindowName;
        [SerializeField]
        private GameObject _ContentPanel;
        [SerializeField]
        private List<GameObject> _DisableOnMinimize;
        private Transform _Decoration;
        [SerializeField]
        protected Button _Close;
        [SerializeField]
        protected Button _MinimizeButton;
        [SerializeField]
        protected Button _MaximizeButton;
        #endregion

        #region IPoolable
        public PoolController PC() => PoolController.Get(WindowPool.Instance);
        public bool InPool { get; private set; }
        public virtual void OnGet(Transform parent)
        {
            var p = PC();
            InPool = false;
            OpenWindowCount++;
            IsOpen = true;
            gameObject.SetActive(true);
            transform.ToRect().offsetMax = p.GetTemplate(GetType()).transform.ToRect().offsetMax;
            transform.ToRect().offsetMin = p.GetTemplate(GetType()).transform.ToRect().offsetMin;
            UIManager.AddToList(this);
        }
        public virtual void OnRelease()
        {
            InPool = true;
            OpenWindowCount--;
            IsOpen = false;
            gameObject.SetActive(false);
            Opener = null;
            CreatorEvent = null;
        }

        public virtual void Delete()
        {
            PC().Release(GetType(), this);
        }
        #endregion

        public bool IsOpen { get; protected set; }

        /* The following properties are used to alter the button used to open this window to avoid duplicate windows */
        // Opener is a delegate
        public UnityAction Opener { get; set; } = null;
        // CreatorEvent is the event that opened this window
        public Button.ButtonClickedEvent CreatorEvent { get; set; } = null;

        #region Protected Properties
        protected List<GameObject> DisableOnMinimize
        {
            get => _DisableOnMinimize;
            set => _DisableOnMinimize = value;
        }

        protected abstract Vector2 MaximizedSize { get; }

        protected abstract Vector2 MinimizedSize { get; }

        protected virtual Transform Decoration
        {
            get
            {
                if (_Decoration == null)
                {
                    _Decoration = transform.FindDescendant("Decoration");
                }
                return _Decoration;
            }
        }

        public virtual TextMeshProUGUI WindowName
        {
            get
            {
                if (_WindowName == null)
                {
                    _WindowName = Decoration.Find("Name").GetComponent<TextMeshProUGUI>();
                }
                return _WindowName;
            }
        }

        public virtual Button Close
        {
            get
            {
                if (_Close == null)
                {
                    _Close = Decoration.Find("Close Button").GetComponent<Button>();
                }
                return _Close;
            }
        }

        protected virtual Button MinimizeButton
        {
            get
            {
                if (_MinimizeButton == null)
                {
                    _MinimizeButton = Decoration.Find("Minimize Button").GetComponent<Button>();
                }
                return _MinimizeButton;
            }
        }

        protected virtual Button MaximizeButton
        {
            get
            {
                if (_MaximizeButton == null)
                {
                    _MaximizeButton = Decoration.Find("Maximize Button").GetComponent<Button>();
                }
                return _MaximizeButton;
            }
        }

        protected virtual GameObject ContentPanel
        {
            get
            {
                if (_ContentPanel == null)
                {
                    _ContentPanel = transform.Find("Contents Panel").gameObject;
                }
                return _ContentPanel;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            MinimizeButton.onClick.AddListener(MinimizeWindow);
            MaximizeButton.onClick.AddListener(MaximizeWindow);

            Close.onClick.AddListener(delegate 
            {
                UIManager.Remove(this);
                if (CreatorEvent != null)
                {
                    CreatorEvent.RemoveAllListeners();
                    CreatorEvent.AddListener(Opener);
                }
                Delete();
            });
        }

        protected virtual void MinimizeWindow()
        {
            foreach(GameObject go in DisableOnMinimize)
            {
                go.SetActive(false);
            }
            MaximizeButton.gameObject.SetActive(true);
            MinimizeButton.gameObject.SetActive(false);
            transform.ToRect().sizeDelta = MinimizedSize;
        }

        protected virtual void MaximizeWindow()
        {
            foreach (GameObject go in DisableOnMinimize)
            {
                go.SetActive(true);
            }
            MaximizeButton.gameObject.SetActive(false);
            MinimizeButton.gameObject.SetActive(true);
            transform.ToRect().sizeDelta = MaximizedSize;
        }

        protected void SetName(string name)
        {
            WindowName.SetText(name);
        }

    }
}