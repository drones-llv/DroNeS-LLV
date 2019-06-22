using System;
using System.IO;
using System.Text;
using Drones.UI.Console;
using Drones.UI.SaveLoad;
using UnityEngine;
using Utils;

namespace Drones.Managers
{
    public static class SaveLoadManager
    {
        private static string _savePath;
        public static string SavePath 
        { 
            get
            {
                if (_savePath == null)
                {
                    _savePath = Path.Combine(DronesPath, "Saves");
                }
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }
                return _savePath;
            }
        }

        private static string _exportPath;
        public static string ExportPath
        {
            get
            {
                if (_exportPath == null)
                {
                    _exportPath = Path.Combine(DronesPath, "Exports");
                }
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                }
                return _exportPath;
            }
        }

        private static string _dronesPath;
        public static string DronesPath
        {
            get
            {
                if (_dronesPath != null) return _dronesPath;
                _dronesPath = OSID.Current != Platform.Windows ? 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents", "DroNeS") : 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "DroNeS");
                if (!Directory.Exists(_dronesPath))
                {
                    Directory.CreateDirectory(_dronesPath);
                }
                return _dronesPath;
            }
        }

        private static void Obfuscate(string filepath, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            for (var i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            File.WriteAllText(filepath, Convert.ToBase64String(bytes));
        }

        private static string Deobfuscate(string filepath)
        {
            var bytes = Convert.FromBase64String(File.ReadAllText(filepath));
            for (var i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
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
            var win = UnityEngine.Object.Instantiate(
                Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject, UIManager.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetSaveMode();
        }

        public static void OpenLoadWindow()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            var win = UnityEngine.Object.Instantiate(
                Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject, UIManager.Transform, false);
            win.GetComponent<SaveLoadWindow>().SetLoadMode();
        }

        public static void OpenOverwriteConfirmation(string filepath)
        {
            var win = UnityEngine.Object.Instantiate(
                Resources.Load("Prefabs/UI/Windows/SaveLoad/Overwrite Dialog") as GameObject, UIManager.Transform, false);
            win.GetComponent<OverwriteConfirmation>().SetFilepath(filepath);
        }

    }
}