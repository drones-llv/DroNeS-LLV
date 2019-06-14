using System.Collections.Generic;
using Drones.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    public class SimulationPanel : ControlPanel
    {
        #region Fields
        [SerializeField]
        private Button _Map;
        [SerializeField]
        private Button _Control;
        [SerializeField]
        private Button _Lists;
        [SerializeField]
        private Button _Histories;
        [SerializeField]
        private Button _Menu;
        #endregion

        #region Properties
        private Button Map
        {
            get
            {
                if (_Map == null)
                {
                    _Map = transform.FindDescendent("Map").GetComponent<Button>();
                }
                return _Map;
            }
        }

        private Button Control
        {
            get
            {
                if (_Control == null)
                {
                    _Control = transform.FindDescendent("Control").GetComponent<Button>();
                }
                return _Control;
            }
        }

        private Button Lists
        {
            get
            {
                if (_Lists == null)
                {
                    _Lists = transform.FindDescendent("Lists").GetComponent<Button>();
                }
                return _Lists;
            }
        }

        private Button Histories
        {
            get
            {
                if (_Histories == null)
                {
                    _Histories = transform.FindDescendent("Histories").GetComponent<Button>();
                }
                return _Histories;
            }
        }

        private Button Menu
        {
            get
            {
                if (_Menu == null)
                {
                    _Menu = transform.FindDescendent("Menu").GetComponent<Button>();
                }
                return _Menu;
            }
        }

        protected override Dictionary<Transform, Button> OwnerToButton
        {
            get
            {
                if (_OwnerToButton == null)
                {
                    _OwnerToButton = new Dictionary<Transform, Button>
                    {
                        {Map.transform, Map},
                        {Control.transform, Control},
                        {Lists.transform, Lists},
                        {Histories.transform, Histories},
                        {Menu.transform, Menu},
                    };
                }
                return _OwnerToButton;
            }
        }
        #endregion

        private void Start()
        {
            Histories.onClick.AddListener(delegate { EnableFoldable(Histories); });
            Map.onClick.AddListener(delegate { EnableFoldable(Map); });
            Control.onClick.AddListener(delegate { EnableFoldable(Control); });
            Lists.onClick.AddListener(delegate { EnableFoldable(Lists); });
            Menu.onClick.AddListener(delegate { EnableFoldable(Menu); });
        }

    }
}
