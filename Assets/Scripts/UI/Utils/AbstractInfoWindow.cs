using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

namespace Drones.UI
{
    using DataStreamer;
    using Utils;
    using Utils.Extensions;

    public abstract class AbstractInfoWindow : AbstractWindow, ISingleDataSourceReceiver
    {
        public int UID => GetInstanceID();

        private DataField[] _Data;

        protected override Vector2 MinimizedSize 
        {
            get
            {
                return Decoration.ToRect().rect.size + Close.transform.ToRect().rect.size.x * 2 * Vector2.right;
            }
        }

        #region IPoolable
        public override void OnRelease()
        {
            if (Source != null)
            {
                Source.InfoWindow = null;
                Source = null;
            }
            base.OnRelease();
            StopAllCoroutines();
        }

        public override void OnGet(Transform parent)
        {
            base.OnGet(parent);
            MaximizeWindow();
            StartCoroutine(WaitForAssignment());
        }
        #endregion

        protected override void Awake()
        {
            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel
            };
            base.Awake();
        }

        #region ISingleDataSourceReceiver
        public Type ReceiverType => GetType();

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

        public IDataSource Source { get; set; }

        public virtual IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            WindowName.SetText(Source.ToString());
            StartCoroutine(StreamData());
            yield break;
        }

        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(1 / 10f);
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
