using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Drones.Data;
using Drones.Managers;
using Drones.Scheduler;
using Drones.Utils;
using Drones.Objects;
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
            _instance.StopAllCoroutines();
            _instance.session = JobScheduler.ALGORITHM + " " + SimManager.Name.Replace("/", "-").Replace(":", "-");
            _instance.LogPath = Path.Combine(SaveLoadManager.ExportPath, _instance.session);
            Log();
            return _instance;
        }

        public string session;
        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 60;
        
        private readonly string[] _simulationData = new string[14];
        private readonly string[] _jobData = new string[12];
        private readonly string[] _hubData = new string[16];
        public string simCache = "";
        public string jobCache = "";
        public string hubCache = "";
        private string LogPath { get; set; }

        private static void Log()
        {
            _instance.StopAllCoroutines();
            //_instance.StartCoroutine(_instance.SimLog());
            _instance.StartCoroutine(_instance.Logging());
        }

        public static void LogHub(Objects.Hub hub)
        {
            _instance.StartCoroutine(_instance.HubLog(hub));
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
                                    "Jobs Delayed in Queue",
                                    "Jobs Completed",
                                    "Jobs Delayed",
                                    "Jobs Failed",
                                    "Revenue",
                                    "Delay (s)",
                                    "Audibility (s)",
                                    "Energy (kWh)" };
                WriteTupleToMemory(ref simCache, headers);
                Flush(filepath, ref simCache);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            while (true)
            {
                SimManager.GetData(this, time);
                WriteTupleToMemory(ref simCache, _simulationData);
                yield return wait;
                time.Now();
            }
        }

        private IEnumerator HubLog(Objects.Hub hub)
        {
            if (!IsLogging) yield break;
            yield return new WaitUntil(() => TimeKeeper.TimeSpeed != TimeSpeed.Pause);
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            var filepath = Path.Combine(LogPath, "Hub Log.csv");
            if (!File.Exists(filepath))
            {
                string[] headers = {"Timestamp", 
                    "time (s)",
                    "Total Drones",
                    "Active Drones",
                    "Crashed Drones",
                    "Total Batteries",
                    "Charging Batteries",
                    "Job Queue Length", 
                    "Jobs Delayed in Queue",
                    "Jobs Completed",
                    "Completed Jobs Delayed",
                    "Jobs Failed",
                    "Revenue",
                    "Delay (s)",
                    "Audibility (s)",
                    "Energy (kWh)" };
                WriteTupleToMemory(ref hubCache, headers);
                Flush(filepath, ref hubCache);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            while (true)
            {
                hub.GetData(this, time);
                WriteTupleToMemory(ref hubCache, _hubData);
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
                                    "Job Euclidean Distance (m)",
                                    "Delivery Altitude (m)",
                                    "Initial Earnings",
                                    "Delivery Earnings",
                                    "Energy Use (kWh)",
                                    "Failed" };
                WriteTupleToMemory(ref _instance.jobCache, headers);
                Flush(filepath, ref _instance.jobCache);
            }
            _instance._jobData[0] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _instance._jobData[1] = data.Created.ToCsvFormat();
            _instance._jobData[2] = data.Assignment.ToCsvFormat();
            _instance._jobData[3] = data.Completed.ToCsvFormat();
            _instance._jobData[4] = data.ExpectedDuration.ToString("0.00");
            _instance._jobData[5] = data.StDevDuration.ToString("0.00");
            _instance._jobData[6] = (data.Pickup - data.Dropoff).magnitude.ToString("0.00");
            _instance._jobData[7] = data.DeliveryAltitude.ToString("0.00");
            _instance._jobData[8] = data.CostFunction.Reward.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _instance._jobData[9] = data.Earnings.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _instance._jobData[10] = UnitConverter.Convert(Energy.kWh, data.EnergyUse);
            _instance._jobData[11] = (data.Status == JobStatus.Failed) ? "YES" : "NO";
            WriteTupleToMemory(ref _instance.jobCache, _instance._jobData);
        }
        
        public void SetData(SimulationData data, TimeKeeper.Chronos time)
        {
            _simulationData[0] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _simulationData[1] = time.ToCsvFormat();
            _simulationData[2] = data.drones.Count.ToString();
            _simulationData[3] = Objects.Drone.ActiveDrones.childCount.ToString();
            _simulationData[4] = data.crashes.ToString();
            _simulationData[5] = data.queuedJobs.ToString();
            _simulationData[6] = data.inQueueDelayed.ToString();
            _simulationData[7] = data.completedCount.ToString();
            _simulationData[8] = data.delayedJobs.ToString();
            _simulationData[9] = data.failedJobs.ToString();
            _simulationData[10] = data.revenue.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _simulationData[11] = (data.totalDelay / data.completedCount).ToString("0.00");
            _simulationData[12] = data.totalAudible.ToString("0.00");
            _simulationData[13] = UnitConverter.Convert(Energy.kWh, data.totalEnergy);
        }
        public void SetData(HubData data, TimeKeeper.Chronos time)
        {
            _hubData[0] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _hubData[1] = time.ToCsvFormat();
            _hubData[2] = data.drones.Count.ToString();
            _hubData[3] = Objects.Drone.ActiveDrones.childCount.ToString();
            _hubData[4] = data.crashes.ToString();
            _hubData[5] = data.batteries.Count.ToString();
            _hubData[6] = data.chargingBatteriesCount.ToString();
            _hubData[7] = data.queuedJobs.ToString();
            _hubData[8] = data.inQueueDelayed.ToString();
            _hubData[9] = data.completedCount.ToString();
            _hubData[10] = data.delayedJobs.ToString();
            _hubData[11] = data.failedJobs.ToString();
            _hubData[12] = data.revenue.ToString("C", CultureInfo.CurrentCulture).Replace(",", "");
            _hubData[13] = (data.delay / data.completedCount).ToString("0.00");
            _hubData[14] = data.audibility.ToString("0.00");
            _hubData[15] = UnitConverter.Convert(Energy.kWh, data.energyConsumption);
        }

        private IEnumerator Logging()
        {
            var wait = new WaitForSecondsRealtime(300);
            if (!IsLogging) yield break;
            while (true)
            {
                yield return wait;
                if (!IsLogging) continue;
                var filepath = Path.Combine(LogPath, "Job Log.csv");
                Flush(filepath, ref jobCache);
                filepath = Path.Combine(LogPath, "Simulation Log.csv");
                Flush(filepath, ref simCache);
                filepath = Path.Combine(LogPath, "Hub Log.csv");
                Flush(filepath, ref hubCache);
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
            filepath = Path.Combine(_instance.LogPath, "Hub Log.csv");
            Flush(filepath, ref _instance.hubCache);
            //filepath = Path.Combine(_instance.LogPath, "Simulation Log.csv");
            //Flush(filepath, ref _instance.simCache);
        }
    }

}