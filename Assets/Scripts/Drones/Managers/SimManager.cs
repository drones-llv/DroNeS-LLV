using System.Collections;
using System.Globalization;
using Drones.Data;
using Drones.Objects;
using Drones.UI.Dahsboard;
using Drones.UI.EditMode;
using Drones.UI.SaveLoad;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Drones.Managers
{
    public class SimManager : MonoBehaviour
    {
        public static SimulationMode Mode { get; set; } = SimulationMode.Delivery;

        #region Fields
        private SimulationData _data;
        private bool _initialized;
        private bool _isQuitting;
        #endregion

        #region Properties
        public static SimManager Instance { get; private set; }
        public static string Name => Instance._data.simulation.ToString(CultureInfo.CurrentCulture);
        public static SimulationStatus Status => Instance._data.status;
        public static SecureSortedSet<uint, IDataSource> AllRetiredDrones => Instance._data.retiredDrones;
        public static SecureSortedSet<uint, IDataSource> AllDrones => Instance._data.drones;
        public static SecureSortedSet<uint, IDataSource> AllHubs => Instance._data.hubs;
        public static SecureSortedSet<uint, IDataSource> AllNfz => Instance._data.noFlyZones;
        public static SecureSortedSet<uint, IDataSource> AllIncompleteJobs => Instance._data.incompleteJobs;
        public static SecureSortedSet<uint, IDataSource> AllCompleteJobs => Instance._data.completeJobs;
        public static SecureSortedSet<uint, Battery> AllBatteries => Instance._data.batteries;
        public static SecureSortedSet<uint, Job> AllJobs => Instance._data.jobs;

        public static bool LoadComplete => Singletons.Manhattan != null && Singletons.Brooklyn != null && Singletons.Manhattan.RedrawComplete && Singletons.Brooklyn.RedrawComplete;
        public static bool Initialized
        {
            get => Instance._initialized;

            set
            {
                if (value)
                {
                    if (UIManager.Transform == null) UIManager.New();
                    SetStatus(SimulationStatus.EditMode);
                }
                Instance._initialized = value;
            }
        }
        #endregion

        #region MonoBehaviours
        private void Awake()
        {
            Instance = this;
            _data = new SimulationData();
            if (Drone.ActiveDrones == null) { }
            DontDestroyOnLoad(PoolController.Get(ListElementPool.Instance).PoolParent.gameObject);
            DontDestroyOnLoad(PoolController.Get(ObjectPool.Instance).PoolParent.gameObject);
            DontDestroyOnLoad(PoolController.Get(WindowPool.Instance).PoolParent.gameObject);
        }
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1));
            Initialized = true;
            DroneManager.New();
            BatteryManager.New();
            ShortcutManager.New();
            DataLogger.New();
            
        }

        public void OnDestroy()
        {
            if (!_isQuitting) return;
            StopAllCoroutines();
            ClearObjects();
            Battery.Reset();
            Hub.Reset();
            DroneData.Reset();
            JobData.Reset();
            NfzData.Reset();
            Singletons.ResetSingletons();
            UIFocus.Reset();
            PriorityFocus.Reset();
            Selectable.Reset();
            Instance = null;
        }
        
        #endregion
        public static void Quit() => Instance._isQuitting = true;
        public static void SetStatus(SimulationStatus status)
        {
            Instance._data.status = status;
            if (status == SimulationStatus.Paused || status == SimulationStatus.EditMode)
                TimeKeeper.TimeSpeed = TimeSpeed.Pause;

            if (status != SimulationStatus.EditMode)
                UIManager.Dashboard.OnRun();
            else
                UIManager.Dashboard.OnEdit();
        }
        
        public static void UpdateRevenue(float value) => Instance._data.revenue += value;
        public static void UpdateDelay(float dt)
        {
            Instance._data.totalDelay += dt;
            if (dt > 0) UpdateDelayCount();
        }
        public static void UpdateCompleteCount() => Instance._data.completedCount++;
        private static void UpdateDelayCount() => Instance._data.delayedJobs++;
        public static void UpdateFailedCount() => Instance._data.failedJobs++;
        public static void UpdateCrashCount() => Instance._data.crashes++;
        public static void UpdateAudible(float dt) => Instance._data.totalAudible += dt;
        public static void UpdateEnergy(float dE) => Instance._data.totalEnergy += dE;
        public static void JobEnqueued() => Instance._data.queuedJobs++;
        public static void JobDequeued() => Instance._data.queuedJobs--;
        public static void GetData(SimulationInfo info) => info.SetData(Instance._data);
        public static void GetData(DataLogger logger, TimeKeeper.Chronos time) => logger.SetData(Instance._data, time);

        private static void ClearObjects()
        {
            var nfzArr = new NoFlyZone[AllNfz.Count];
            AllNfz.Values.CopyTo(nfzArr, 0);
            foreach (var t in nfzArr)
            {
                t?.Delete();
            }
            var hubArr = new Hub[AllHubs.Count];
            AllHubs.Values.CopyTo(hubArr, 0);
            foreach (var t in hubArr)
            {
                t?.Delete();
            }
            while (Drone.ActiveDrones.childCount > 0)
            {
                Drone.ActiveDrones?.GetChild(0)?.GetComponent<Drone>()?.Delete();
            }
        }

        public static void InQueueDelayed() => Instance._data.inQueueDelayed++;
        public static void DequeuedDelay() => Instance._data.inQueueDelayed--;
    }
}
