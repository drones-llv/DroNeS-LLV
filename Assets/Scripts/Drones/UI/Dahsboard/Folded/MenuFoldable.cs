using System;
using System.Collections;
using System.IO;
using Drones.Managers;
using Drones.UI.SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Drones.UI.Dahsboard.Folded
{
    public class MenuFoldable : FoldableMenu
    {

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(QuitToMainMenu);
            Buttons[1].onClick.AddListener(SaveSimulation);
            Buttons[2].onClick.AddListener(LoadSimulation);
            Buttons[3].onClick.AddListener(ExportToCSV);
            Buttons[4].onClick.AddListener(()=>SimManager.SetStatus(SimulationStatus.EditMode));
            base.Start();
        }

        private void ExportToCSV()
        {
            string path;
            if (!Directory.Exists(SaveLoadManager.DronesPath))
            {
                Directory.CreateDirectory(SaveLoadManager.DronesPath);
            }
            if (!Directory.Exists(SaveLoadManager.ExportPath))
            {
                Directory.CreateDirectory(SaveLoadManager.ExportPath);
            }
            string filename = DateTime.Now.ToString() + ".json";
            filename = filename.Replace("/", "-");
            filename = filename.Replace(@"\", "-");
            filename = filename.Replace(@":", "-");
            path = Path.Combine(SaveLoadManager.ExportPath, filename);
            File.WriteAllText(path, JsonUtility.ToJson(SimManager.SerializeSimulation()));
        }

        private void LoadSimulation() => SaveLoadManager.OpenLoadWindow();
        private void SaveSimulation() => SaveLoadManager.OpenSaveWindow();

        private void QuitToMainMenu()
        {
            DataLogger.Dump();
            SimManager.SetStatus(SimulationStatus.Paused);
            SimManager.Quit();
            SimManager.Instance.StartCoroutine(LoadMainMenu());
        }

        IEnumerator LoadMainMenu()
        {
            yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 30f);
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        }

    }
}