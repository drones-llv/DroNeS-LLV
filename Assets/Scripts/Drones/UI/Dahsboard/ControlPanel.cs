using System.Collections.Generic;
using Drones.UI.Dahsboard.Folded;
using Drones.UI.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI.Dahsboard
{
    public abstract class ControlPanel : MonoBehaviour
    {

        protected Dictionary<Transform, Button> _OwnerToButton;

        protected abstract Dictionary<Transform, Button> OwnerToButton { get; }

        protected GameObject _ActiveFoldable;

        protected void EnableFoldable(Button button)
        {
            if (PriorityFocus.Count > 0) return;

            if (_ActiveFoldable != null && _ActiveFoldable.gameObject.activeSelf)
            {
                OwnerToButton[_ActiveFoldable.transform.parent].onClick.Invoke();
            }
            _ActiveFoldable = button.GetComponentInChildren<FoldableTaskBar>(true).gameObject;
            _ActiveFoldable.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { DisableFoldable(button); });
        }

        protected void DisableFoldable(Button button)
        {
            _ActiveFoldable.SetActive(false);
            _ActiveFoldable = null;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { EnableFoldable(button); });
        }

    }

}