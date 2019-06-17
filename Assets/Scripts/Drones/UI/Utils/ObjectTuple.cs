using System;
using System.Collections;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.UI.Utils
{
    using Random = UnityEngine.Random;
    public abstract class ObjectTuple : AbstractListElement, ISingleDataSourceReceiver
    {
        private DataField[] _Data;

        void Awake()
        {
            Link.onClick.AddListener(delegate
            {
                Source.OpenInfoWindow();
            });
        }

        public override void OnGet(Transform parent)
        {
            base.OnGet(parent);
            StartCoroutine(WaitForAssignment());
        }

        public override void OnRelease()
        {
            StopAllCoroutines();
            Source = null;
            base.OnRelease();
        }

        #region ISingleDataSourceReceiver
        public DataField[] Data
        {
            get
            {
                if (_Data == null)
                {
                    _Data = GetComponentsInChildren<DataField>();
                }
                return _Data;
            }
        }

        public Type ReceiverType => Window.GetType();

        public IDataSource Source { get; set; }

        public int UID => GetInstanceID();

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            StartCoroutine(StreamData());
            yield break;
        }

        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(Random.Range(1, 2));
            while (Source != null)
            {
                Source.GetData(this);
                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                }
                if (Source.IsDataStatic) { break; }
                yield return wait;
            }
            yield break;
        }

        public abstract void SetData(IData data);
        #endregion

    }
}
