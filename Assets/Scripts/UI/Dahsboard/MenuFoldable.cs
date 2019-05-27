using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using System;
using System.IO;

namespace Drones.UI
{
    using Utils;
    using Managers;
    using Data;
    public class MenuFoldable : FoldableMenu
    {

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(QuitToMainMenu);
            Buttons[1].onClick.AddListener(SaveSimulation);
            Buttons[2].onClick.AddListener(LoadSimulation);
            Buttons[3].onClick.AddListener(ExportToCSV);
            Buttons[4].onClick.AddListener(OpenEditMode);
            base.Start();
        }

        private void OpenEditMode()
        {
            EditPanel.Instance.gameObject.SetActive(true);
        }


        private void ExportToCSV()
        {
            string path;
            if (!Directory.Exists(SaveManager.DronesPath))
            {
                Directory.CreateDirectory(SaveManager.DronesPath);
            }
            if (!Directory.Exists(SaveManager.ExportPath))
            {
                Directory.CreateDirectory(SaveManager.ExportPath);
            }
            string filename = DateTime.Now.ToString() + ".json";
            filename = filename.Replace("/", "-");
            filename = filename.Replace(@"\", "-");
            filename = filename.Replace(@":", "-");
            path = Path.Combine(SaveManager.ExportPath, filename);
            File.WriteAllText(path, JsonUtility.ToJson(SimManager.SerializeSimulation()));
        }

        private void LoadSimulation() => SaveManager.OpenLoadWindow();
        private void SaveSimulation() => SaveManager.OpenSaveWindow();

        private void QuitToMainMenu()
        {
            SimManager.SetStatus(SimulationStatus.Paused);
            SimManager.ClearObjects();
            BatteryData.Reset();
            DroneData.Reset();
            JobData.Reset();
            HubData.Reset();
            NFZData.Reset();
            StartCoroutine(LoadMainMenu());
        }

        IEnumerator LoadMainMenu()
        {
            yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 30f);
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        }

    }
}