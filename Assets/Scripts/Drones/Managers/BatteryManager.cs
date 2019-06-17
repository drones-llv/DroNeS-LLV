using System.Collections;
using Drones.JobSystem;
using Drones.Objects;
using Drones.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utils;

namespace Drones.Managers
{
    public class BatteryManager : MonoBehaviour
    {
        private static JobHandle EnergyJobHandle => _instance._energyJobHandle;
        private static BatteryManager _instance;
        public static BatteryManager New()
        {
            _instance = new GameObject("BatteryManager").AddComponent<BatteryManager>();
            return _instance;
        }

        private JobHandle _energyJobHandle = new JobHandle();
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, Battery> Batteries => SimManager.AllBatteries;
        private NativeArray<EnergyInfo> _energyInfoArray;

        private void OnDisable()
        {
            EnergyJobHandle.Complete();
            if (_energyInfoArray.IsCreated) _energyInfoArray.Dispose();
            _instance = null;
        }

        private void Initialise()
        {
            _energyInfoArray = new NativeArray<EnergyInfo>(Batteries.Count, Allocator.Persistent);
            _time.Now();
        }

        private void Start()
        {
            Batteries.SetChanged += (obj) => OnCountChange();
            Initialise();
            StartCoroutine(Operate());
        }

        private IEnumerator Operate()
        {
            var _energyJob = new EnergyJob();
            while (true)
            {
                if (Batteries.Count == 0) yield return null;
                var j = 0;

                foreach (var battery in Batteries.Values)
                {
                    var dE = _energyInfoArray[j].energy;
                    battery.GetDrone()?.UpdateEnergy(dE);
                    battery.SetEnergyInfo(_energyInfoArray[j]);

                    _energyInfoArray[j] = battery.GetEnergyInfo(_energyInfoArray[j]);
                    j++;
                }

                _energyJob.Energies = _energyInfoArray;
                _energyJob.DeltaTime = _time.Timer();
                _time.Now();

                _energyJobHandle = _energyJob.Schedule(Batteries.Count, 16);

                yield return null;
                EnergyJobHandle.Complete();
            }
        }

        private void OnCountChange()
        {
            _energyJobHandle.Complete();
            _energyInfoArray.Dispose();
            Initialise();

            var j = 0;
            foreach (var drone in Batteries.Values)
            {
                _energyInfoArray[j] = new EnergyInfo();
                _energyInfoArray[j] = drone.GetEnergyInfo(_energyInfoArray[j]);
                j++;
            }

        }

        public static void ForceCountChange()
        {
            _instance._energyJobHandle.Complete();
            _instance._energyInfoArray.Dispose();
            _instance.Initialise();

            var j = 0;
            foreach (var drone in Batteries.Values)
            {
                _instance._energyInfoArray[j] = new EnergyInfo();
                _instance._energyInfoArray[j] = drone.GetEnergyInfo(_instance._energyInfoArray[j]);
                j++;
            }
        }

    }


}
