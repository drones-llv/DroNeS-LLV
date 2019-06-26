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
            _instance._logPath = Path.Combine(SaveLoadManager.ExportPath, _instance.session);
            Log();
            return _instance;
        }

        private void OnDisable() => Dump();

        public string session;
        public static bool IsLogging { get; set; } = true;
        public static float LoggingPeriod { get; set; } = 60;
        
        private readonly string[] _simulationData = new string[14];
        private readonly string[] _jobData = new string[13];
        private readonly string[] _hubData = new string[16];
        public string simCache = "";
        public string jobCache = "";
        public string hubCache = "";
        private string _logPath;
        public static string LogPath => _instance._logPath;

        private static void Log()
        {
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.SimLog());
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
            if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
            var filepath = Path.Combine(_logPath, "Simulation Log.csv");
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
                                    "Revenue ($)",
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
            if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
            if (!File.Exists(hub.logPath))
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
                    "Income ($)",
                    "Delay (s)",
                    "Audibility (s)",
                    "Energy (kWh)" };
                WriteTupleToMemory(ref hub.dataCache, headers);
                Flush(hub.logPath, ref hub.dataCache);
            }
            var time = TimeKeeper.Chronos.Get();
            var wait = new WaitUntil(() => time.Timer() > LoggingPeriod);
            while (true)
            {
                hub.GetData(this, time);
                WriteTupleToMemory(ref hub.dataCache, _hubData);
                yield return wait;
                time.Now();
            }
        }

        public static void LogJob(JobData data)
        {
            if (!IsLogging) return;
            if (!Directory.Exists(_instance._logPath)) Directory.CreateDirectory(_instance._logPath);
            var filepath = Path.Combine(_instance._logPath, "Job Log.csv");
            if (!File.Exists(filepath))
            {
                string[] headers = {"Timestamp",
                                    "Generated Time (s)",
                                    "Deadline Time (s)",
                                    "Assignment Time (s)",
                                    "Completed Time (s)",
                                    "Expected Duration (s)",
                                    "Standard Deviation (s)",
                                    "Job Euclidean Distance (m)",
                                    "Delivery Altitude (m)",
                                    "Initial Earnings ($)",
                                    "Delivery Earnings ($)",
                                    "Energy Use (kWh)",
                                    "Failed" };
                WriteTupleToMemory(ref _instance.jobCache, headers);
                Flush(filepath, ref _instance.jobCache);
            }
            _instance._jobData[0] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _instance._jobData[1] = data.Cost.Start.ToCsvFormat();
            _instance._jobData[2] = data.Deadline.ToCsvFormat();
            _instance._jobData[3] = data.Assignment.ToCsvFormat();
            _instance._jobData[4] = data.Completed.ToCsvFormat();
            _instance._jobData[5] = data.ExpectedDuration.ToString("0.00");
            _instance._jobData[6] = data.StDevDuration.ToString("0.00");
            _instance._jobData[7] = (data.Pickup - data.Dropoff).magnitude.ToString("0.00");
            _instance._jobData[8] = data.DeliveryAltitude.ToString("0.00");
            _instance._jobData[9] = data.Cost.Reward.ToString("0.00");
            _instance._jobData[10] = data.Earnings.ToString("0.00");
            _instance._jobData[11] = UnitConverter.ConvertValue(Energy.kWh, data.EnergyUse).ToString("0.00");
            _instance._jobData[12] = (data.Status == JobStatus.Failed) ? "YES" : "NO";
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
            _simulationData[10] = data.revenue.ToString("0.00");
            _simulationData[11] = (data.totalDelay / data.completedCount).ToString("0.00");
            _simulationData[12] = data.totalAudible.ToString("0.00");
            _simulationData[13] = UnitConverter.Convert(Energy.kWh, data.totalEnergy);
        }
        public void SetData(HubData data, TimeKeeper.Chronos time)
        {
            var hub = (Objects.Hub)SimManager.AllHubs[data.UID];
            _hubData[0] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _hubData[1] = time.ToCsvFormat();
            _hubData[2] = data.drones.Count.ToString();
            _hubData[3] = data.ActiveDroneCount.ToString();
            _hubData[4] = data.NumberOfDroneCrashes.ToString();
            _hubData[5] = data.batteries.Count.ToString();
            _hubData[6] = hub.GetChargingBatteryCount().ToString();
            _hubData[7] = data.NumberOfJobsInQueue.ToString();
            _hubData[8] = data.NumberOfJobsDelayedInQueue.ToString();
            _hubData[9] = data.CompletedJobCount.ToString();
            _hubData[10] = data.DelayedCompletedJobs.ToString();
            _hubData[11] = data.FailedJobs.ToString();
            _hubData[12] = data.Earnings.ToString("0.00");
            _hubData[13] = (data.TotalDelayOfCompletedJobs / data.CompletedJobCount).ToString("0.00");
            _hubData[14] = data.AudibleDuration.ToString("0.00");
            _hubData[15] = UnitConverter.Convert(Energy.kWh, data.EnergyConsumption);
        }

        private IEnumerator Logging()
        {
            var wait = new WaitForSecondsRealtime(300);
            if (!IsLogging) yield break;
            while (true)
            {
                yield return wait;
                if (!IsLogging) continue;
                var filepath = Path.Combine(_logPath, "Job Log.csv");
                Flush(filepath, ref jobCache);
                filepath = Path.Combine(_logPath, "Simulation Log.csv");
                Flush(filepath, ref simCache);
                foreach (var dataSource in SimManager.AllHubs.Values)
                {
                    var h = (Objects.Hub)dataSource;
                    Flush(h.logPath, ref h.dataCache);
                }
            }
        }

        public static void Flush(string filepath, ref string data)
        {
            using (var writer = File.AppendText(filepath))
            {
                writer.WriteLine(data);
                writer.Close();
            }
            data = "";
        }

        private static void Dump()
        {
            if (!IsLogging) return;
            var filepath = Path.Combine(LogPath, "Job Log.csv");
            Flush(filepath, ref _instance.jobCache);
            foreach (var dataSource in SimManager.AllHubs.Values)
            {
                var h = (Objects.Hub) dataSource;
                Flush(h.logPath, ref h.dataCache);
            }
            
        }
    }

}