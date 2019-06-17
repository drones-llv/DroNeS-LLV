using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.UI.SaveLoad
{
    public class SaveLoadTuple : AbstractListElement
    {
        private DataField[] _Data;
        private const float _ClickDelta = 0.35f;
        private bool _FirstClick;
        private float _ClickTime;

        public static SaveLoadTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<SaveLoadTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }

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

        private void Awake()
        {
            Link.onClick.AddListener(delegate
            {
                SaveLoadWindow.Instance.SetSaveName(Data[0].text);
                if (_FirstClick && Time.unscaledTime - _ClickTime > _ClickDelta)
                {
                    _FirstClick = false;
                }

                if (_FirstClick && Time.unscaledTime - _ClickTime <= _ClickDelta)
                {
                    SaveLoadWindow.Instance.OperateButton.onClick.Invoke();
                    _FirstClick = false;
                }
                else
                {
                    _FirstClick = true;
                    _ClickTime = Time.unscaledTime;
                }
            });
        }

    }
}
