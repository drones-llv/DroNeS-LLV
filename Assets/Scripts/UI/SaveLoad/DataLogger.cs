using System.Collections;
using System.IO;
using System.Globalization;
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
            Log();
        }

        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 60;
        private readonly string[] _SimulationData = new string[12];
        private readonly string[] _JobData = new string[9];

        public string LogPath { get; private set; }

        public static void Log()
        {
            _Instance.StopAllCoroutines();
            _Instance.StartCoroutine(_Instance.SimLog());
        }

        public void WriteTupleToCSV(string filepath, params string[] data)
        {
            using (StreamWriter writer = File.AppendText(filepath))
            {
                string output = "";
                for (int i = 0; i < data.Length; i++)
                {
                    output += data[i];
                    if (i < data.Length - 1)
                        output += ",";
                }
                writer.WriteLine(output);
                writer.Close();
            }
        }

        private IEnumerator SimLog()
        {
            if (!IsLogging) yield break;
            yield return new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            string filepath = Path.Combine(LogPath, "Simulation Log.csv");
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
                                    "Energy (kWh)" };
                WriteTupleToCSV(filepath, headers);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            string[] data = new string[12];
            while (true)
            {
                SimManager.GetData(this, time);
                WriteTupleToCSV(filepath, _SimulationData);
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
                string[] headers = { "Generated Time (s)",
                                    "Assignment Time (s)",
                                    "Completed Time (s)",
                                    "Expected Duration (s)",
                                    "Standard Deviation (s)",
                                    "Job Distance (m)",
                                    "Initial Price",
                                    "Final Earnings",
                                    "Failed" };
                _Instance.WriteTupleToCSV(filepath, headers);
            }
            _Instance._JobData[0] = data.created.ToCSVFormat();
            _Instance._JobData[1] = data.assignment.ToCSVFormat();
            _Instance._JobData[2] = data.completed.ToCSVFormat();
            _Instance._JobData[3] = data.expectedDuration.ToString("0.00");
            _Instance._JobData[4] = data.stDevDuration.ToString("0.00");
            _Instance._JobData[5] = (data.pickup - data.dropoff).magnitude.ToString("0.00");
            _Instance._JobData[6] = data.costFunction.Reward.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _Instance._JobData[7] = data.earnings.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _Instance._JobData[8] = (data.status == JobStatus.Failed) ? "YES" : "NO";
            _Instance.WriteTupleToCSV(filepath, _Instance._JobData);

        }

        public void SetData(SimulationData data, TimeKeeper.Chronos time)
        {
            _SimulationData[0] = time.ToCSVFormat();
            _SimulationData[1] = data.drones.Count.ToString();
            _SimulationData[2] = Drone.ActiveDrones.childCount.ToString();
            _SimulationData[3] = data.crashes.ToString();
            _SimulationData[4] = data.queuedJobs.ToString();
            _SimulationData[5] = data.completedCount.ToString();
            _SimulationData[6] = data.delayedJobs.ToString();
            _SimulationData[7] = data.failedJobs.ToString();
            _SimulationData[8] = data.revenue.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _SimulationData[9] = (data.totalDelay / data.completedCount).ToString("0.00");
            _SimulationData[10] = data.totalAudible.ToString("0.00");
            _SimulationData[11] = UnitConverter.Convert(Energy.kWh, data.totalEnergy);
        }
    }

}