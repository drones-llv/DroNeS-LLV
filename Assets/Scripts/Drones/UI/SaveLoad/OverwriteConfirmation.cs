using Drones.Managers;
using Drones.UI.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.SaveLoad
{
    public class OverwriteConfirmation : AbstractWindow
    {
        [SerializeField]
        Button _Cancel;
        [SerializeField]
        Button _Confirm;

        private string _filepath;

        protected override Vector2 MaximizedSize { get; } = new Vector2(300, 170);

        protected override Vector2 MinimizedSize { get; } = new Vector2(300, 170);

        Button Confirm
        {
            get
            {
                if (_Confirm == null)
                {
                    _Confirm = ContentPanel.transform.FindDescendant("Confirm", 1).GetComponent<Button>();
                }
                return _Confirm;
            }
        }

        Button Cancel
        {
            get
            {
                if (_Cancel == null)
                {
                    _Cancel = ContentPanel.transform.FindDescendant("Cancel", 1).GetComponent<Button>();
                }
                return _Cancel;
            }
        }

        protected override void Awake()
        {
            Confirm.onClick.AddListener(delegate
            {
                if (string.IsNullOrEmpty(_filepath)) return;
//                SaveLoadManager.Save(_filepath);
                Destroy(gameObject);
                Destroy(SaveLoadWindow.Instance.gameObject);
            });

            Cancel.onClick.AddListener(delegate
            {
                Destroy(gameObject);
            });
        }

        public OverwriteConfirmation SetFilepath(string filepath)
        {
            _filepath = filepath;
            return this;
        }
    }
}
