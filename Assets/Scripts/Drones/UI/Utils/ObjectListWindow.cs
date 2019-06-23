using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.Utils
{
    public abstract class ObjectListWindow : AbstractWindow, IMultiDataSourceReceiver, IListWindow
    {
        private ListTupleContainer _tupleContainer;

        private Dictionary<IDataSource, ObjectTuple> _dataReceivers;

        private event ListChangeHandler ContentChanged;

        protected override Vector2 MinimizedSize => Decoration.ToRect().rect.size + Close.transform.ToRect().rect.size.x * 2 * Vector2.right;

        protected override void Awake()
        {
            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel
            };
            base.Awake();
        }

        public override void OnGet(Transform parent)
        {
            base.OnGet(parent);
            MaximizeWindow();
            StartCoroutine(WaitForAssignment());
            ListChanged += TupleContainer.AdjustDimensions;
        }

        public override void OnRelease()
        {
            StopAllCoroutines();
            if (Sources != null)
            {
                Sources.ItemAdded -= OnNewSource;
                Sources.ItemRemoved -= OnLooseSource;
                Sources = null;
                ClearDataReceivers();
            } 
            else
            {
                gameObject.SetActive(false);
            }
            ListChanged -= TupleContainer.AdjustDimensions;
            transform.SetParent(PC().PoolParent, false);
            Opener = null;
            CreatorEvent = null;
            IsOpen = false;
        }

        #region IListWindow
        public ListTupleContainer TupleContainer
        {
            get
            {
                if (_tupleContainer == null)
                {
                    _tupleContainer = ContentPanel.GetComponentInChildren<ListTupleContainer>();
                }
                return _tupleContainer;
            }
        }

        public abstract ListElement TupleType { get; }

        public event ListChangeHandler ListChanged
        {
            add
            {
                if (ContentChanged == null || !ContentChanged.GetInvocationList().Contains(value))
                {
                    ContentChanged += value;
                }
            }
            remove => ContentChanged -= value;
        }
        #endregion

        #region IMultiDataSourceReceiver
        protected void OnContentChange()
        {
            ContentChanged?.Invoke();
        }

        public int UID => GetInstanceID();

        public virtual SecureSortedSet<uint, IDataSource> Sources { get; set; } 

        public bool IsClearing { get; set; }

        public Dictionary<IDataSource, ObjectTuple> DataReceivers => _dataReceivers ?? (_dataReceivers = new Dictionary<IDataSource, ObjectTuple>());

        public IEnumerator WaitForAssignment()
        {
            var first = new WaitUntil(() => Sources != null);
            var second = new WaitUntil(() => !IsClearing);
            yield return first;
            yield return second;
            gameObject.SetActive(true);

            foreach (var source in Sources.Values)
            {
                OnNewSource(source);
            }
            
            Sources.ItemAdded += OnNewSource;
            Sources.ItemRemoved += OnLooseSource;
        }

        public void ClearDataReceivers()
        {
            IsClearing = true;
            if (DataReceivers.Count > 0)
            {
                foreach (var receiver in DataReceivers.Values)
                {
                    receiver?.Delete();
                }
            }
            DataReceivers.Clear();
            IsClearing = false;
            gameObject.SetActive(false);
        }

        public abstract void OnNewSource(IDataSource source);

        public void OnLooseSource(IDataSource source)
        {
            DataReceivers[source].Delete();
            ListChanged -= DataReceivers[source].OnListChange;
            DataReceivers.Remove(source);
            ContentChanged?.Invoke();
        }
        #endregion

    }

}
