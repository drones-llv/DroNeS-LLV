using UnityEngine;
using UnityEngine.UI;
using System;

namespace Drones.UI
{

    using Drones.Utils;
    using Utils.Extensions;

    public class ListTupleContainer : MonoBehaviour
    {
        [Serializable]
        public class Dimensions
        {
            public float separation;
            public float height;
        }
        [SerializeField]
        private Dimensions _Dimension;
        [SerializeField]
        private AbstractWindow _Window;

        public AbstractWindow Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = AbstractWindow.GetWindow(transform.parent);
                }
                return _Window;
            }
        }

        public Dimensions Dimension
        {
            get
            {
                if (_Dimension == null)
                {
                    _Dimension = new Dimensions
                    {
                        separation = GetComponent<VerticalLayoutGroup>().spacing,
                        height = PoolController.Get(ListElementPool.Instance).GetTemplate(Window.GetType()).transform.ToRect().sizeDelta.y
                    };
                }
                return _Dimension;
            }
        }

        public void AdjustDimensions()
        {
            int n = transform.childCount;
            Vector2 sizeDelta = transform.ToRect().sizeDelta;
            sizeDelta.y = n * Dimension.height + (n - 1) * Dimension.separation;
            transform.ToRect().sizeDelta = sizeDelta;
        }
    }
}