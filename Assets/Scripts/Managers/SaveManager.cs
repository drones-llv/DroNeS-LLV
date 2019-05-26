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
                    if (OSID.Current != Platform.Windows)
                    {
                        _SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents", "DroNeS","Saves");
                    }
                    else
                    {
                        _SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "DroNeS", "Saves");
                    }
                }
                return _SavePath;
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
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(OpenWindows.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetSaveMode();
        }

        public static void OpenLoadWindow()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(OpenWindows.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetLoadMode();
        }

        public static void OpenOverwriteConfirmation(string filepath)
        {
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/Overwrite Dialog") as GameObject);
            win.transform.SetParent(OpenWindows.Transform, false);
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