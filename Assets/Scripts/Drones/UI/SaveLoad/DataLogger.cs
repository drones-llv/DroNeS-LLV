using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Drones.Data;
using Drones.Managers;
using Drones.Serializable;
using Drones.Utils;
using UnityEngine;
using Utils;

namespace Drones.UI.SaveLoad
{
    public class DataLogger : MonoBehaviour
    {
        private static DataLogger _instance;
        public static DataLogger New()
        {
            _instance = new GameObject("DataLogger").AddComponent<DataLogger>();
            Load();
            return _instance;
        }
        public static void Load()
        {
            _instance.StopAllCoroutines();
            _instance.LogPath = Path.Combine(SaveLoadManager.ExportPath, SimManager.Name.Replace("/", "-").Replace(":", "-"));
            _instance.session = SimManager.Name.Replace("/", "-").Replace(":", "-");
            Log();
        }
        public string session;
        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 60;
        public static bool IsAutosave { get; set; } = true;
        public static float AutosavePeriod { get; set; } = 300;
        private readonly string[] _simulationData = new string[13];
        private readonly string[] _jobData = new string[10];
        public string simCache = "";
        public string jobCache = "";

        private string LogPath { get; set; }

        private static void Log()
        {
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.SimLog());
            _instance.StartCoroutine(_instance.Autosave());
        }

        private static void WriteTupleToMemory(ref string memory, params string[] data)
        {
            if (!string.IsNullOrWhiteSpace(memory)) memory += "\n";
            for (var i = 0; i < data.Length; i++)
            {
                memory += data[i];
                if (i < data.Length - 1)
                    memory += ",";
            }
        }

        private IEnumerator SimLog()
        {
            if (!IsLogging) yield break;
            yield return new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            var filepath = Path.Combine(LogPath, "Simulation Log.csv");
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
                WriteTupleToMemory(ref simCache, headers);
                Flush(filepath, ref simCache);
                //WriteTupleToFile(filepath, headers);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            while (true)
            {
                SimManager.GetData(this, time);
                WriteTupleToMemory(ref simCache, _simulationData);
                //WriteTupleToFile(filepath, _Instance._SimulationData);
                yield return wait;
                time.Now();
            }
        }

        public static void LogJob(JobData data)
        {
            if (!IsLogging) return;
            if (!Directory.Exists(_instance.LogPath)) Directory.CreateDirectory(_instance.LogPath);
            var filepath = Path.Combine(_instance.LogPath, "Job Log.csv");
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
                WriteTupleToMemory(ref _instance.jobCache, headers);
                Flush(filepath, ref _instance.jobCache);
                //_Instance.WriteTupleToFile(filepath, headers);
            }
            _instance._jobData[0] = DateTime.Now.ToString();
            _instance._jobData[1] = data.Created.ToCsvFormat();
            _instance._jobData[2] = data.Assignment.ToCsvFormat();
            _instance._jobData[3] = data.Completed.ToCsvFormat();
            _instance._jobData[4] = data.ExpectedDuration.ToString("0.00");
            _instance._jobData[5] = data.StDevDuration.ToString("0.00");
            _instance._jobData[6] = (data.Pickup - data.Dropoff).magnitude.ToString("0.00");
            _instance._jobData[7] = data.CostFunction.Reward.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _instance._jobData[8] = data.Earnings.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _instance._jobData[9] = (data.Status == JobStatus.Failed) ? "YES" : "NO";
            WriteTupleToMemory(ref _instance.jobCache, _instance._jobData);
            //_Instance.WriteTupleToFile(filepath, _Instance._JobData);

        }

        public void SetData(SimulationData data, TimeKeeper.Chronos time)
        {
            _simulationData[0] = DateTime.Now.ToString();
            _simulationData[1] = time.ToCsvFormat();
            _simulationData[2] = data.drones.Count.ToString();
            _simulationData[3] = Objects.Drone.ActiveDrones.childCount.ToString();
            _simulationData[4] = data.crashes.ToString();
            _simulationData[5] = data.queuedJobs.ToString();
            _simulationData[6] = data.completedCount.ToString();
            _simulationData[7] = data.delayedJobs.ToString();
            _simulationData[8] = data.failedJobs.ToString();
            _simulationData[9] = data.revenue.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _simulationData[10] = (data.totalDelay / data.completedCount).ToString("0.00");
            _simulationData[11] = data.totalAudible.ToString("0.00");
            _simulationData[12] = UnitConverter.Convert(Energy.kWh, data.totalEnergy);
        }

        IEnumerator Autosave()
        {
            var wait = new WaitForSecondsRealtime(AutosavePeriod);
            if (!IsLogging && !IsAutosave) yield break;
            while (true)
            {
                yield return wait;
                if (!IsLogging) continue;
                var filepath = Path.Combine(_instance.LogPath, "Job Log.csv");
                Flush(filepath, ref jobCache);
                filepath = Path.Combine(LogPath, "Simulation Log.csv");
                Flush(filepath, ref simCache);
                //if (IsAutosave) SaveManager.Save(SaveManager.FilePath(Session));
            }
        }
        public void WriteTupleToFile(string filepath, params string[] data)
        {
            using (var writer = File.AppendText(filepath))
            {
                var output = "";
                for (var i = 0; i < data.Length; i++)
                {
                    output += data[i];
                    if (i < data.Length - 1)
                        output += ",";
                }
                writer.WriteLine(output);
                writer.Close();
            }
        }

        private static void Flush(string filepath, ref string data)
        {
            using (var writer = File.AppendText(filepath))
            {
                writer.WriteLine(data);
                writer.Close();
            }
            data = "";
        }

        public static void Dump()
        {
            if (!IsLogging) return;
            var filepath = Path.Combine(_instance.LogPath, "Job Log.csv");
            Flush(filepath, ref _instance.jobCache);
            filepath = Path.Combine(_instance.LogPath, "Simulation Log.csv");
            Flush(filepath, ref _instance.simCache);
            //if (IsAutosave) SaveManager.Save(SaveManager.FilePath(_Instance.Session));
        }
    }

}