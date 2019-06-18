using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using EventType = Utils.EventType;

namespace Drones.UI.Console
{
    public class ConsoleLog : AbstractWindow, IListWindow
    {
        public int consoleSize = 500;

        private GameObject _ScrollBar;
        private GameObject _Viewport;
        private ListTupleContainer _TupleContainer;
        private ScrollRect _ScrollRect;
        private HashSet<EventType> _Ignored;
        private event ListChangeHandler ContentChanged;

        public static ConsoleLog Instance { get; private set; }

        protected GameObject ElementTemplate => PoolController.Get(ListElementPool.Instance)?.GetTemplate(typeof(ConsoleElement));

        protected GameObject ScrollBar
        {
            get
            {
                if (_ScrollBar == null)
                {
                    _ScrollBar = ContentPanel.transform.Find("Scroll Bar").gameObject;
                }
                return _ScrollBar;
            }
        }

        protected GameObject Viewport
        {
            get
            {
                if (_Viewport == null)
                {
                    _Viewport = TupleContainer.transform.parent.gameObject;
                }
                return _Viewport;
            }
        }

        protected ScrollRect ScrollRect
        {
            get
            {
                if (_ScrollRect == null)
                {
                    _ScrollRect = ContentPanel.GetComponent<ScrollRect>();
                }
                return _ScrollRect;
            }
        }

        protected HashSet<EventType> Ignored
        {
            get
            {
                if (_Ignored == null)
                {
                    _Ignored = new HashSet<EventType>();
                }
                return _Ignored;
            }
        }

        protected override Vector2 MaximizedSize { get; } = new Vector2(900, 300);

        protected override Vector2 MinimizedSize
        {
            get
            {
                var top = -Viewport.transform.ToRect().offsetMax.y;
                var bot = Viewport.transform.ToRect().offsetMin.y;
                var size = transform.ToRect().sizeDelta;
                size.y = ElementTemplate.transform.ToRect().sizeDelta.y + top + bot;
                return size;
            }
        }

        protected override Button MaximizeButton
        {
            get
            {
                if (_MaximizeButton == null)
                {
                    _MaximizeButton = transform.Find("Minimize Button").GetComponent<Button>();
                }
                return _MaximizeButton;
            }
        }

        protected override Button MinimizeButton
        {
            get
            {
                if (_MinimizeButton == null)
                {
                    _MinimizeButton = transform.Find("Minimize Button").GetComponent<Button>();
                }
                return _MinimizeButton;
            }
        }

        public event ListChangeHandler ListChanged
        {
            add
            {
                if (ContentChanged == null || !ContentChanged.GetInvocationList().Contains(value))
                {
                    ContentChanged += value;
                }
            }
            remove
            {
                ContentChanged -= value;
            }
        }

        public ListTupleContainer TupleContainer
        {
            get
            {
                if (_TupleContainer == null)
                {
                    _TupleContainer = ContentPanel.GetComponentInChildren<ListTupleContainer>();
                }
                return _TupleContainer;
            }
        }

        public ListElement TupleType { get; } = ListElement.Console;

        protected override void Awake()
        {
            Instance = this;
            MinimizeButton.onClick.AddListener(MinimizeWindow);
            MaximizeButton.onClick.AddListener(MaximizeWindow);
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => PoolController.Get(ListElementPool.Instance).Initialized);
            MinimizeWindow();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void OnEnable() => StartCoroutine(Start());

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public static void Clear()
        {
            Button button;
            ConsoleElement element;
            while (Instance.TupleContainer.transform.childCount > 0)
            {
                button = Instance.TupleContainer.transform.GetChild(0).GetComponent<Button>();
                element = Instance.TupleContainer.transform.GetChild(0).GetComponent<ConsoleElement>();
                button.onClick.RemoveAllListeners();
                element.Delete();
            }
        }

        public static void WriteToConsole(IEvent iEvent)
        {
            Button button;
            ConsoleElement element = null;
            if (Instance.TupleContainer.transform.childCount >= Instance.consoleSize)
            {
                button = Instance.TupleContainer.transform.GetChild(0).GetComponent<Button>();
                element = Instance.TupleContainer.transform.GetChild(0).GetComponent<ConsoleElement>();
                button.onClick.RemoveAllListeners();
            } 
            else
            {
                element = ConsoleElement.New(Instance);
                Instance.TupleContainer.AdjustDimensions();
                button = element.Link;
                Instance.ListChanged += element.OnListChange;
            }

            button.onClick.AddListener(delegate 
            {
                Instance.transform.SetAsLastSibling();
                Instance.ExecuteButton(iEvent);
            });

            button.name = iEvent.ID;
            button.transform.SetParent(Instance.TupleContainer.transform);
            element.Message.SetText(iEvent.Message);
            Instance.ContentChanged.Invoke();
        }

        private void ExecuteButton(IEvent iEvent)
        {
            if (iEvent.Target != null)
            {
                AbstractCamera.ActiveCamera.BreakFollow();
                AbstractCamera.LookHere(new Vector3(iEvent.Target[0], iEvent.Target[1], iEvent.Target[2]));
            }
            iEvent.OpenWindow();
        }

        protected override void MinimizeWindow()
        {
            ScrollRect.ScrollToBottom();
            ScrollRect.vertical = false;
            ScrollBar.SetActive(false);
            transform.ToRect().sizeDelta = MinimizedSize;
            MaximizeButton.gameObject.SetActive(true);
            MinimizeButton.gameObject.SetActive(false);
            Viewport.transform.ToRect().offsetMax += Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

        protected override void MaximizeWindow()
        {
            ScrollRect.vertical = true;
            ScrollBar.SetActive(true);
            transform.ToRect().sizeDelta = MaximizedSize;
            MaximizeButton.gameObject.SetActive(false);
            MinimizeButton.gameObject.SetActive(true);
            Viewport.transform.ToRect().offsetMax -= Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

    }

}