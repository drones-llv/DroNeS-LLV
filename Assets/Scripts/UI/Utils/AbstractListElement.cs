using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Drones.UI
{
    using Utils;
    using Interface;

    public abstract class AbstractListElement : MonoBehaviour, IListElement, IPoolable
    {
        #region Statics
        public static Color ListItemOdd { get; } = new Color
        {
            r = 180f / 255f,
            b = 180f / 255f,
            g = 180f / 255f,
            a = 1
        };

        public static Color ListItemEven { get; } = new Color
        {
            r = 223f / 255f,
            b = 223f / 255f,
            g = 223f / 255f,
            a = 1
        };
        #endregion

        #region Fields
        [SerializeField]
        protected Button _Link;
        protected AbstractWindow _Window;
        private Image _ItemImage;
        #endregion

        #region IListElement
        public Color Odd { get; } = ListItemOdd;

        public Color Even { get; } = ListItemEven;

        public Image ItemImage
        {
            get
            {
                if (_ItemImage == null)
                {
                    _ItemImage = GetComponent<Image>();
                }
                return _ItemImage;
            }

        }

        public void OnListChange()
        {
            ItemImage.color = (transform.GetSiblingIndex() % 2 == 1) ? Odd : Even;
        }
        #endregion

        #region Properties
        public virtual Button Link
        {
            get
            {
                if (_Link == null)
                {
                    _Link = GetComponent<Button>();
                }
                return _Link;
            }
        }
        public AbstractWindow Window => _Window;
        #endregion

        #region IPoolable
        public bool InPool { get; private set; }

        public virtual void OnGet(Transform parent)
        {
            InPool = false;
            gameObject.SetActive(true);
            transform.SetParent(parent, false);
        }

        public virtual void OnRelease()
        {
            InPool = true;
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent, false);
        }

        public virtual void Delete()
        {
            PC().Release(GetType(), this);
        }
        public PoolController PC() => PoolController.Get(ListElementPool.Instance);
        #endregion

    }
}
