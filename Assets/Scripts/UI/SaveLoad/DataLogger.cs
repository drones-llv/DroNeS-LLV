using System.Collections;
using System.IO;
using System.Globalization;
using System;
using UnityEngine;

namespace Drones.UI
{
    using Managers;
    using Utils;
    using Data;

    public class DataLogger : MonoBehaviour
    {
        private static DataLogger _Instance;
        public static DataLogger New()
        {
            _Instance = new GameObject("DataLogger").AddComponent<DataLogger>();
            Load();
            return _Instance;
        }
        public static void Load()
        {
            _Instance.StopAllCoroutines();
            _Instance.LogPath = Path.Combine(SaveManager.ExportPath, SimManager.Name.Replace("/", "-").Replace(":", "|"));
            _Instance.Session = SimManager.Name.Replace("/", "-").Replace(":", "|");
            Log();
        }
        public string Session;
        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 60;
        public static bool IsAutosave { get; set; } = true;
        public static float AutosavePeriod { get; set; } = 300;
        private readonly string[] _SimulationData = new string[13];
        private readonly string[] _JobData = new string[10];
        public string SimMemory = "";
        public string JobMemory = "";

        public string LogPath { get; private set; }

        public static void Log()
        {
            _Instance.StopAllCoroutines();
            _Instance.StartCoroutine(_Instance.SimLog());
            _Instance.StartCoroutine(_Instance.Autosave());
        }

        public void WriteTupleToMemory(ref string memory, params string[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                memory += data[i];
                if (i < data.Length - 1)
                    memory += ",";
            }
            memory += "\n";
        }

        private IEnumerator SimLog()
        {
            if (!IsLogging) yield break;
            yield return new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            string filepath = Path.Combine(LogPath, "Simulation Log.csv");
            if (!File.Exists(filepath))
            {
                string[] headers = {"Timestamp", 
                                    "time (s)",
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
                                    "Energy (kWh)" };
                WriteTupleToMemory(ref SimMemory, headers);
                Flush(filepath, ref SimMemory);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            string[] data = new string[12];
            while (true)
            {
                SimManager.GetData(this, time);
                WriteTupleToMemory(ref SimMemory, _SimulationData);
                yield return wait;
                time.Now();
            }
        }

        public static void LogJob(JobData data)
        {
            if (!IsLogging) return;
            if (!Directory.Exists(_Instance.LogPath)) Directory.CreateDirectory(_Instance.LogPath);
            string filepath = Path.Combine(_Instance.LogPath, "Job Log.csv");
            if (!File.Exists(filepath))
            {
                string[] headers = {"Timestamp",
                                    "Generated Time (s)",
                                    "Assignment Time (s)",
                                    "Completed Time (s)",
                                    "Expected Duration (s)",
                                    "Standard Deviation (s)",
                                    "Job Distance (m)",
                                    "Initial Price",
                                    "Final Earnings",
                                    "Failed" };
                _Instance.WriteTupleToMemory(ref _Instance.JobMemory, headers);
                _Instance.Flush(filepath, ref _Instance.JobMemory);
            }
            _Instance._JobData[0] = DateTime.Now.ToString();
            _Instance._JobData[1] = data.created.ToCSVFormat();
            _Instance._JobData[2] = data.assignment.ToCSVFormat();
            _Instance._JobData[3] = data.completed.ToCSVFormat();
            _Instance._JobData[4] = data.expectedDuration.ToString("0.00");
            _Instance._JobData[5] = data.stDevDuration.ToString("0.00");
            _Instance._JobData[6] = (data.pickup - data.dropoff).magnitude.ToString("0.00");
            _Instance._JobData[7] = data.costFunction.Reward.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _Instance._JobData[8] = data.earnings.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _Instance._JobData[9] = (data.status == JobStatus.Failed) ? "YES" : "NO";
            _Instance.WriteTupleToMemory(ref _Instance.JobMemory, _Instance._JobData);

        }

        public void SetData(SimulationData data, TimeKeeper.Chronos time)
        {
            _SimulationData[0] = DateTime.Now.ToString();
            _SimulationData[1] = time.ToCSVFormat();
            _SimulationData[2] = data.drones.Count.ToString();
            _SimulationData[3] = Drone.ActiveDrones.childCount.ToString();
            _SimulationData[4] = data.crashes.ToString();
            _SimulationData[5] = data.queuedJobs.ToString();
            _SimulationData[6] = data.completedCount.ToString();
            _SimulationData[7] = data.delayedJobs.ToString();
            _SimulationData[8] = data.failedJobs.ToString();
            _SimulationData[9] = data.revenue.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _SimulationData[10] = (data.totalDelay / data.completedCount).ToString("0.00");
            _SimulationData[11] = data.totalAudible.ToString("0.00");
            _SimulationData[12] = UnitConverter.Convert(Energy.kWh, data.totalEnergy);
        }

        IEnumerator Autosave()
        {
            var wait = new WaitForSecondsRealtime(AutosavePeriod);
            if (!IsLogging && !IsAutosave) yield break;
            while (true)
            {
                yield return wait;
                if (IsLogging)
                {
                    string filepath = Path.Combine(_Instance.LogPath, "Job Log.csv");
                    Flush(filepath, ref JobMemory);
                    filepath = Path.Combine(LogPath, "Simulation Log.csv");
                    Flush(filepath, ref SimMemory);
                }
                //if (IsAutosave) SaveManager.Save(SaveManager.FilePath(Session));
            }
        }

        void Flush(string filepath, ref string data)
        {
            using (StreamWriter writer = File.AppendText(filepath))
            {
                writer.WriteLine(data);
                writer.Close();
            }
            data = "";
        }

        public static void Dump()
        {
            if (IsLogging)
            {
                string filepath = Path.Combine(_Instance.LogPath, "Job Log.csv");
                _Instance.Flush(filepath, ref _Instance.JobMemory);
                filepath = Path.Combine(_Instance.LogPath, "Simulation Log.csv");
                _Instance.Flush(filepath, ref _Instance.SimMemory);
            }
            //if (IsAutosave) SaveManager.Save(SaveManager.FilePath(_Instance.Session));
        }
    }

}