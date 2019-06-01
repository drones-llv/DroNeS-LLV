using System.Collections;
using System.Globalization;
using System.Collections.Generic;
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
    using Utils.Extensions;
    using Serializable;


    public class SimManager : MonoBehaviour
    {
        #region Fields
        private SimulationData _Data;
        private uint _mapsLoaded;
        private DataField[] _DataFields;
        private bool _Initialized;
        #endregion

        #region Properties
        public static SimManager Instance { get; private set; }
        public static SimulationStatus Status => Instance._Data.status;
        public static SecureSortedSet<uint, IDataSource> AllRetiredDrones => Instance._Data.retiredDrones;
        public static SecureSortedSet<uint, IDataSource> AllDrones => Instance._Data.drones;
        public static SecureSortedSet<uint, IDataSource> AllHubs => Instance._Data.hubs;
        public static SecureSortedSet<uint, IDataSource> AllNFZ => Instance._Data.noFlyZones;
        public static SecureSortedSet<uint, IDataSource> AllIncompleteJobs => Instance._Data.incompleteJobs;
        public static SecureSortedSet<uint, IDataSource> AllCompleteJobs => Instance._Data.completeJobs;
        public static SecureSortedSet<uint, Battery> AllBatteries => Instance._Data.batteries;
        public static SecureSortedSet<uint, Job> AllJobs => Instance._Data.jobs;
        private static DataField[] DataFields
        {
            get
            {
                if (Instance._DataFields == null)
                    Instance._DataFields = OpenWindows.Transform.FindDescendent("Drone Network", 1).GetComponentsInChildren<DataField>();
                return Instance._DataFields;
            }
        }
        public static bool LoadComplete => !(Manhattan == null || Brooklyn == null) && Manhattan.RedrawComplete && Brooklyn.RedrawComplete;
        public static uint MapsLoaded => Instance._mapsLoaded;
        public static bool Initialized
        {
            get => Instance._Initialized;

            set
            {
                if (value)
                {
                    if (!OpenWindows.Transform.gameObject.activeSelf)
                        OpenWindows.Transform.gameObject.SetActive(true);
                    SetStatus(SimulationStatus.EditMode);
                }
                Instance._Initialized = value;
            }
        }
        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 300;
        #endregion

        private void OnDestroy()
        {
            StopAllCoroutines();
            ResetSingletons();
            Instance = null;
        }

        #region Initializers
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
            Instance.StartCoroutine(StreamDataToDashboard());
        }
        #endregion

        public static void OnMapLoaded() => Instance._mapsLoaded++;

        public static void SetStatus(SimulationStatus status)
        {
            Instance._Data.status = status;
            if (status == SimulationStatus.Paused || status == SimulationStatus.EditMode)
                OnPause();
            else
                OnPlay();

            if (status != SimulationStatus.EditMode)
                Selectable.Deselect();
            else
                EditPanel.Instance.gameObject.SetActive(true);
        }

        public static void OnPlay()
        {
            Selectable.Deselect();
            Instance.StartCoroutine(StreamDataToDashboard());
        }

        public static void OnPause()
        {
            Instance.StopCoroutine(StreamDataToDashboard());
            TimeKeeper.TimeSpeed = TimeSpeed.Pause;
        }

        public static void UpdateRevenue(float value) => Instance._Data.revenue += value;
        public static void UpdateDelay(float dt)
        {
            Instance._Data.totalDelay += dt;
            if (dt > 0) UpdateDelayCount();
        }
        private static void UpdateDelayCount() => Instance._Data.delayedJobs++;
        public static void UpdateFailedCount() => Instance._Data.failedJobs++;
        public static void UpdateCrashCount() => Instance._Data.crashes++;
        public static void UpdateAudible(float dt) => Instance._Data.totalAudible += dt;
        public static void UpdateEnergy(float dE) => Instance._Data.totalEnergy += dE;
        public static void JobEnqueued() => Instance._Data.queuedJobs++;
        public static void JobDequeued() => Instance._Data.queuedJobs--;

        private static IEnumerator StreamDataToDashboard()
        {
            Instance.StartCoroutine(DataLogger());
            var wait = new WaitForSeconds(0.75f);
            while (true)
            {
                DataFields[0].SetField(AllDrones.Count.ToString());
                DataFields[1].SetField(Drone.ActiveDrones.childCount.ToString());
                DataFields[2].SetField(Instance._Data.crashes.ToString());
                DataFields[3].SetField(Instance._Data.queuedJobs.ToString());
                DataFields[4].SetField(AllCompleteJobs.Count.ToString());
                DataFields[5].SetField(Instance._Data.delayedJobs.ToString());
                DataFields[6].SetField(Instance._Data.failedJobs.ToString());
                DataFields[7].SetField(AllHubs.Count.ToString());
                DataFields[8].SetField(Instance._Data.revenue.ToString("C", CultureInfo.CurrentCulture));
                DataFields[9].SetField(UnitConverter.Convert(Chronos.min, Instance._Data.totalDelay / AllCompleteJobs.Count));
                DataFields[10].SetField(UnitConverter.Convert(Energy.kWh, Instance._Data.totalEnergy));
                DataFields[11].SetField(UnitConverter.Convert(Chronos.min, Instance._Data.totalAudible));
                yield return wait;
            }
        }

        public static SSimulation SerializeSimulation() => new SSimulation(Instance._Data);

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
            TimeKeeper.SetTime(data.currentTime);
        }

        private static IEnumerator DataLogger()
        {
            if (!IsLogging) yield break;
            string filename = Instance._Data.simulation.ToString().Replace("/","-");
            filename = Path.ChangeExtension(filename, ".csv");
            string filepath = Path.Combine(SaveManager.ExportPath, filename);
            if (!File.Exists(filepath))
            {
                string[] headers = { "time (s)", 
                                    "Total Drones",
                                    "Active Drones",
                                    "Crashed Drones",
                                    "Job Queue Length",
                                    "Jobs Completed",
                                    "Jobs Delayed",
                                    "Jobs Failed",
                                    "Revenue", 
                                    "Delay (s)", 
                                    "Audibility (s)", 
                                    "Energy (J)" };
                SaveManager.WriteTupleToCSV(filepath, headers);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            string[] data = new string[12];
            while (true)
            {
                data[0] = time.ToCSVFormat();
                data[1] = AllDrones.Count.ToString();
                data[2] = Drone.ActiveDrones.childCount.ToString();
                data[3] = Instance._Data.crashes.ToString();
                data[4] = Instance._Data.queuedJobs.ToString();
                data[5] = AllCompleteJobs.Count.ToString();
                data[6] = Instance._Data.delayedJobs.ToString();
                data[7] = Instance._Data.failedJobs.ToString();
                data[8] = Instance._Data.revenue.ToString("C", CultureInfo.CurrentCulture);
                data[9] = (Instance._Data.totalDelay/AllCompleteJobs.Count).ToString("0.00");
                data[10] = Instance._Data.totalAudible.ToString("0.00");
                data[11] = Instance._Data.totalEnergy.ToString("0.00");
                SaveManager.WriteTupleToCSV(filepath, data);
                yield return wait;
                time.Now();
            }
        }

    }
}
