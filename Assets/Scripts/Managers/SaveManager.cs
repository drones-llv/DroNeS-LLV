using UnityEngine;
using System.IO;
using System.Text;
using System;
using Drones.UI;

namespace Drones.Managers
{
    using Drones.Utils;
    using Serializable;
    public static class SaveManager
    {
        private static string _SavePath;
        public static string SavePath 
        { 
            get
            {
                if (_SavePath == null)
                {
                    _SavePath = Path.Combine(DronesPath, "Saves");
                }
                if (!Directory.Exists(_SavePath))
                {
                    Directory.CreateDirectory(_SavePath);
                }
                return _SavePath;
            }
        }

        private static string _ExportPath;
        public static string ExportPath
        {
            get
            {
                if (_ExportPath == null)
                {
                    _ExportPath = Path.Combine(DronesPath, "Exports");
                }
                if (!Directory.Exists(_ExportPath))
                {
                    Directory.CreateDirectory(_ExportPath);
                }
                return _ExportPath;
            }
        }

        private static string _DronesPath;
        public static string DronesPath
        {
            get
            {
                if (_DronesPath == null)
                {
                    if (OSID.Current != Platform.Windows)
                    {
                        _DronesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents", "DroNeS");
                    }
                    else
                    {
                        _DronesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "DroNeS");
                    }
                    if (!Directory.Exists(_DronesPath))
                    {
                        Directory.CreateDirectory(_DronesPath);
                    }
                }
                return _DronesPath;
            }
        }

        private static void Obfuscate(string filepath, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            File.WriteAllText(filepath, Convert.ToBase64String(bytes));
        }

        private static string Deobfuscate(string filepath)
        {
            var bytes = Convert.FromBase64String(File.ReadAllText(filepath));
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            return Encoding.UTF8.GetString(bytes);
        }

        public static string FilePath(string filename)
        {
            return Path.Combine(SavePath, filename + ".drn");
        }

        public static string FileName(string filepath)
        {
            return Path.GetFileName(filepath);
        }

        public static string FileNameNoExtension(string filepath)
        {
            return Path.GetFileNameWithoutExtension(filepath);
        }

        public static void OpenSaveWindow()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(UIManager.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetSaveMode();
        }

        public static void OpenLoadWindow()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(UIManager.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetLoadMode();
        }

        public static void OpenOverwriteConfirmation(string filepath)
        {
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/Overwrite Dialog") as GameObject);
            win.transform.SetParent(UIManager.Transform, false);
            win.GetComponent<OverwriteConfirmation>().SetFilepath(filepath);
        }

        public static void Save(string filepath)
        {
            Obfuscate(filepath, JsonUtility.ToJson(SimManager.SerializeSimulation()));
        }

        public static void Load(string filepath)
        {
            string data = Deobfuscate(filepath);
            SimManager.LoadSimulation(JsonUtility.FromJson<SSimulation>(data));
            ConsoleLog.Clear();
        }

    }
}