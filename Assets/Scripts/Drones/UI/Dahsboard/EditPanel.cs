using System.Collections.Generic;
using Drones.Managers;
using Drones.UI.Dahsboard.Folded;
using Drones.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Selectable = Drones.UI.EditMode.Selectable;

namespace Drones.UI.Dahsboard
{
    public class EditPanel : ControlPanel
    {
        #region Fields
        [SerializeField]
        private Button _Play;
        [SerializeField]
        private Button _Navigation;
        [SerializeField]
        private Button _Lists;
        [SerializeField]
        private Button _Histories;
        [SerializeField]
        private Button _Add;
        [SerializeField]
        private Button _Delete;
        [SerializeField]
        private Button _Menu;
        #endregion

        #region Properties
        private Button Play
        {
            get
            {
                if (_Play == null)
                {
                    _Play = transform.Find("Play").GetComponent<Button>();
                }
                return _Play;
            }
        }

        private Button Navigation
        {
            get
            {
                if (_Navigation == null)
                {
                    _Navigation = transform.Find("Navigation").GetComponent<Button>();
                }
                return _Navigation;
            }
        }

        private Button Lists
        {
            get
            {
                if (_Lists == null)
                {
                    _Lists = transform.Find("Lists").GetComponent<Button>();
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
                    _Histories = transform.Find("Histories").GetComponent<Button>();
                }
                return _Histories;
            }
        }

        private Button Add
        {
            get
            {
                if (_Add == null)
                {
                    _Add = transform.Find("Add").GetComponent<Button>();
                }
                return _Add;
            }
        }

        private Button Delete
        {
            get
            {
                if (_Delete == null)
                {
                    _Delete = transform.Find("Delete").GetComponent<Button>();
                }
                return _Delete;
            }
        }

        private Button Menu
        {
            get
            {
                if (_Menu == null)
                {
                    _Menu = transform.Find("Menu").GetComponent<Button>();
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
                        {Play.transform, Play},
                        {Navigation.transform, Navigation},
                        {Lists.transform, Lists},
                        {Histories.transform, Histories},
                        {Add.transform, Add},
                        {Delete.transform, Delete},
                        {Menu.transform, Menu}
                    };
                }
                return _OwnerToButton;
            }
        }
        #endregion

        private void Awake()
        {
            //Instance = this;
            Play.onClick.AddListener(ExitEditMode);
            Navigation.onClick.AddListener(MapFoldable.OpenNavigationWindow);
            Lists.onClick.AddListener(delegate { EnableFoldable(Lists); });
            Histories.onClick.AddListener(delegate { EnableFoldable(Histories); });
            Add.onClick.AddListener(delegate { EnableFoldable(Add); });
            Delete.onClick.AddListener(DeleteSelection);
            Menu.onClick.AddListener(delegate { EnableFoldable(Menu); });
        }


        private void OnEnable()
        {
            AbstractCamera.ActiveCamera?.BreakFollow();

            CameraSwitch.OnEagleEye();
        }

        public static void ExitEditMode()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            CameraSwitch.OnRTS();
            //Instance.gameObject.SetActive(false);
        }

        private void DeleteSelection() => Selectable.DeleteMode = true;

    }
}
