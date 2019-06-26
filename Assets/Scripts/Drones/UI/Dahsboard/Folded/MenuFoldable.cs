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
    public class MenuFoldable : FoldableTaskBar
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

        private static void ExportToCSV()
        {
        }

        private static void LoadSimulation()
        {
            
        }
        private static void SaveSimulation() 
        {
        
        }

        private void QuitToMainMenu()
        {
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