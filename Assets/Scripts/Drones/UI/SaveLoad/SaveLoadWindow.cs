using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Drones.Managers;
using Drones.UI.Utils;
using Drones.Utils.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.SaveLoad
{
    public class SaveLoadWindow : AbstractWindow, IListWindow
    {
        public static SaveLoadWindow Instance { get; private set; }
        [SerializeField]
        private TMP_InputField _InputName;
        [SerializeField]
        private ListTupleContainer _TupleContainer;
        [SerializeField]
        private Button _Operate;
        [SerializeField]
        private Button _Cancel;

        private TMP_InputField InputName
        {
            get
            {
                if (_InputName == null)
                {
                    _InputName = ContentPanel.transform.FindDescendant("Input Container").GetComponentInChildren<TMP_InputField>();
                }
                return _InputName;
            }
        }

        public Button OperateButton
        {
            get
            {
                if (_Operate == null)
                {
                    _Operate = InputName.transform.parent.FindDescendant("Save").GetComponent<Button>();
                }
                return _Operate;
            }
        }

        private Button CancelButton
        {
            get
            {
                if (_Cancel == null)
                {
                    _Cancel = InputName.transform.parent.FindDescendant("Cancel").GetComponent<Button>();
                }
                return _Cancel;
            }
        }

        public List<SaveLoadTuple> Tuples { get; } = new List<SaveLoadTuple>();

        private string[] _files;
        public ListTupleContainer TupleContainer
        {
            get
            {
                if (_TupleContainer == null)
                {
                    _TupleContainer = ContentPanel.GetComponentInChildren<ListTupleContainer>();
                }
                return _TupleContainer;
            }
        }

        public ListElement TupleType { get; } = ListElement.SaveLoad;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 405);

        protected override Vector2 MinimizedSize { get; } = new Vector2(1000, 405);

        private event ListChangeHandler ContentChanged;

        public event ListChangeHandler ListChanged
        {
            add
            {
                if (ContentChanged == null || !ContentChanged.GetInvocationList().Contains(value))
                {
                    ContentChanged += value;
                }
            }
            remove
            {
                ContentChanged -= value;
            }
        }

        protected override void Awake()
        {
            Close.onClick.AddListener(delegate
            {
                if (PriorityFocus.Count > 1) return;
                Destroy(gameObject);
            });
            CancelButton.onClick.AddListener(delegate
            {
                if (PriorityFocus.Count > 1) return;
                Destroy(gameObject);
            });
        }

        protected void OnEnable()
        {
            Instance = this;
            _InputName.text = "";
            _files = Directory.GetFiles(SaveLoadManager.SavePath);
            ListChanged += TupleContainer.AdjustDimensions;
            for (int i = 0;  i <_files.Length; i++)
            {
                if (!Path.GetExtension(_files[i]).Equals(".drn", StringComparison.OrdinalIgnoreCase)) continue;

                var tuple = SaveLoadTuple.New(this);
                tuple.Data[2].SetField(File.GetCreationTime(_files[i]).ToString());
                tuple.Data[1].SetField(File.GetLastWriteTime(_files[i]).ToString());
                tuple.Data[0].SetField(SaveLoadManager.FileNameNoExtension(_files[i]));
                ListChanged += tuple.OnListChange;
                Tuples.Add(tuple);
            }
            ContentChanged?.Invoke();
        }

        private void OnDestroy()
        {
            Instance = null;
            while (Tuples.Count > 0)
            {
                Tuples[0].Delete();
                Tuples.RemoveAt(0);
            }
        }

        public void SetSaveName(string text)
        {
            _InputName.text = text;
        }

        private void OnSave()
        {
            if (string.IsNullOrWhiteSpace(_InputName.text)) return;
            var path = SaveLoadManager.FilePath(_InputName.text);

            if (File.Exists(path))
            {
                SaveLoadManager.OpenOverwriteConfirmation(path);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnLoad()
        {
            if (string.IsNullOrWhiteSpace(_InputName.text)) return;
            var path = SaveLoadManager.FilePath(_InputName.text);

            if (File.Exists(path))
            {
                Destroy(gameObject);
            }
        }

        public void SetSaveMode()
        {
            OperateButton.onClick.RemoveAllListeners();
            OperateButton.onClick.AddListener(delegate
            {
                if (PriorityFocus.Count > 1) return;
                OnSave();
            });
            OperateButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Save");
        }

        public void SetLoadMode()
        {
            OperateButton.onClick.RemoveAllListeners();
            OperateButton.onClick.AddListener(delegate
            {
                if (PriorityFocus.Count > 1) return;
                OnLoad();
            });
            OperateButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Load");
        }
    }
}
