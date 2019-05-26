using UnityEngine;

namespace Drones.Managers
{
    using Utils;
    using UI;
    using static Utils.Constants;
    public class ShortcutManager : MonoBehaviour
    {
        private static ShortcutManager _Instance;
        public static ShortcutManager Instance
        {
            get
            {
                if (Instance == null)
                {
                    _Instance = ((GameObject)Instantiate(Resources.Load(ShortcutManagerPath))).GetComponent<ShortcutManager>();
                }
                return _Instance;
            }
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private void Awake()
        {
            _Instance = this;
        }

        private void Update()
        {
            if (PriorityFocus.Count > 0) return;

            if (SimManager.Status == SimulationStatus.EditMode)
                EditMode();
            else
                PlayMode();

        }

        private void PlayMode()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                SimManager.SetStatus(SimulationStatus.Paused);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SimManager.SetStatus(SimulationStatus.Running);
                TimeKeeper.TimeSpeed = TimeSpeed.Slow;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SimManager.SetStatus(SimulationStatus.Running);
                TimeKeeper.TimeSpeed = TimeSpeed.Normal;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SimManager.SetStatus(SimulationStatus.Running);
                TimeKeeper.TimeSpeed = TimeSpeed.Fast;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SimManager.SetStatus(SimulationStatus.Running);
                TimeKeeper.TimeSpeed = TimeSpeed.Ultra;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                SimManager.SetStatus(SimulationStatus.Running);
                return;
            }
        }

        private void EditMode()
        {

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Selectable.ShortcutDelete();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                EditPanel.ExitEditMode();
                return;
            }

        }

    }
}
