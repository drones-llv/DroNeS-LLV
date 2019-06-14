using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Drones.Managers
{
    using UI;
    using Utils;
    using DataStreamer;
    using Data;
    using static Singletons;
    using Serializable;

    public class SimManager : MonoBehaviour
    {
        #region Fields
        private SimulationData _Data;
        private bool _Initialized;
        private bool _IsQuitting;
        #endregion

        #region Properties
        public static SimManager Instance { get; private set; }
        public static string Name => Instance._Data.simulation.ToString();
        public static SimulationStatus Status => Instance._Data.status;
        public static SecureSortedSet<uint, IDataSource> AllRetiredDrones => Instance._Data.retiredDrones;
        public static SecureSortedSet<uint, IDataSource> AllDrones => Instance._Data.drones;
        public static SecureSortedSet<uint, IDataSource> AllHubs => Instance._Data.hubs;
        public static SecureSortedSet<uint, IDataSource> AllNFZ => Instance._Data.noFlyZones;
        public static SecureSortedSet<uint, IDataSource> AllIncompleteJobs => Instance._Data.incompleteJobs;
        public static SecureSortedSet<uint, IDataSource> AllCompleteJobs => Instance._Data.completeJobs;
        public static SecureSortedSet<uint, Battery> AllBatteries => Instance._Data.batteries;
        public static SecureSortedSet<uint, Job> AllJobs => Instance._Data.jobs;


        public static bool LoadComplete => Manhattan != null && Brooklyn != null && Manhattan.RedrawComplete && Brooklyn.RedrawComplete;
        public static bool Initialized
        {
            get => Instance._Initialized;

            set
            {
                if (value)
                {
                    if (UIManager.Transform == null) UIManager.New();
                    SetStatus(SimulationStatus.EditMode);
                }
                Instance._Initialized = value;
            }
        }
        #endregion

        #region MonoBehaviours
        private void Awake()
        {
            Instance = this;
            _Data = new SimulationData();
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
            if (!_IsQuitting) return;
            StopAllCoroutines();
            ClearObjects();
            BatteryData.Reset();
            DroneData.Reset();
            JobData.Reset();
            HubData.Reset();
            NFZData.Reset();
            ResetSingletons();
            UIFocus.Reset();
            PriorityFocus.Reset();
            Selectable.Reset();
            Instance = null;
        }
        #endregion
        public static void Quit() => Instance._IsQuitting = true;
        public static void SetStatus(SimulationStatus status)
        {
            Instance._Data.status = status;
            if (status == SimulationStatus.Paused || status == SimulationStatus.EditMode)
                TimeKeeper.TimeSpeed = TimeSpeed.Pause;

            if (status != SimulationStatus.EditMode)
                UIManager.Dashboard.OnRun();
            else
                UIManager.Dashboard.OnEdit();
        }
        public static void UpdateRevenue(float value) => Instance._Data.revenue += value;
        public static void UpdateDelay(float dt)
        {
            Instance._Data.totalDelay += dt;
            if (dt > 0) UpdateDelayCount();
        }
        public static void UpdateCompleteCount() => Instance._Data.completedCount++;
        private static void UpdateDelayCount() => Instance._Data.delayedJobs++;
        public static void UpdateFailedCount() => Instance._Data.failedJobs++;
        public static void UpdateCrashCount() => Instance._Data.crashes++;
        public static void UpdateAudible(float dt) => Instance._Data.totalAudible += dt;
        public static void UpdateEnergy(float dE) => Instance._Data.totalEnergy += dE;
        public static void JobEnqueued() => Instance._Data.queuedJobs++;
        public static void JobDequeued() => Instance._Data.queuedJobs--;
        public static SSimulation SerializeSimulation() => new SSimulation(Instance._Data);
        public static void GetData(SimulationInfo info) => info.SetData(Instance._Data);
        public static void GetData(DataLogger logger, TimeKeeper.Chronos time) => logger.SetData(Instance._Data, time);
        public static void ClearObjects()
        {
            NoFlyZone[] nfzArr = new NoFlyZone[AllNFZ.Count];
            AllNFZ.Values.CopyTo(nfzArr, 0);
            for (int i = 0; i < nfzArr.Length; i++)
            {
                nfzArr[i]?.Delete();
            }
            Hub[] hubArr = new Hub[AllHubs.Count];
            AllHubs.Values.CopyTo(hubArr, 0);
            for (int i = 0; i < hubArr.Length; i++)
            {
                hubArr[i]?.Delete();
            }
            while (Drone.ActiveDrones.childCount > 0)
            {
                Drone.ActiveDrones?.GetChild(0)?.GetComponent<Drone>()?.Delete();
            }
        }
        public static void LoadSimulation(SSimulation data)
        {
            ClearObjects();
            Instance._Data = new SimulationData(data);
            Instance._Data.Load(data);
            DataLogger.Load();
            BatteryManager.ForceCountChange();
            DroneManager.ForceDroneCountChange();
            TimeKeeper.SetTime(data.currentTime);
            SetStatus(SimulationStatus.EditMode);
        }

    }
}
