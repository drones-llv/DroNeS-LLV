using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones.Managers
{
    using DataStreamer;
    using Utils.Jobs;
    using Utils;
    using static Utils.Constants;
    public class BatteryManager : MonoBehaviour
    {
        public static JobHandle EnergyJobHandle => _Instance._energyJobHandle;
        private static BatteryManager _Instance;
        public static BatteryManager New()
        {
            _Instance = new GameObject("BatteryManager").AddComponent<BatteryManager>();
            return _Instance;
        }

        private JobHandle _energyJobHandle = new JobHandle();
        private readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private SecureSortedSet<uint, Battery> Batteries => SimManager.AllBatteries;
        private NativeArray<EnergyInfo> _EnergyInfoArray;

        private void OnDisable()
        {
            EnergyJobHandle.Complete();
            if (_EnergyInfoArray.IsCreated) _EnergyInfoArray.Dispose();
            _Instance = null;
        }

        private void Initialise()
        {
            _EnergyInfoArray = new NativeArray<EnergyInfo>(Batteries.Count, Allocator.Persistent);
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
            EnergyJob _energyJob = new EnergyJob();
            while (true)
            {
                if (Batteries.Count == 0) yield return null;
                int j = 0;

                foreach (Battery battery in Batteries.Values)
                {
                    var dE = _EnergyInfoArray[j].energy;
                    battery.GetDrone()?.UpdateEnergy(dE);
                    battery.SetEnergyInfo(_EnergyInfoArray[j]);

                    _EnergyInfoArray[j] = battery.GetEnergyInfo(_EnergyInfoArray[j]);
                    j++;
                }

                _energyJob.energies = _EnergyInfoArray;
                _energyJob.deltaTime = _time.Timer();
                _time.Now();

                _energyJobHandle = _energyJob.Schedule(Batteries.Count, 16);

                yield return null;
                EnergyJobHandle.Complete();
            }
        }

        public void OnCountChange()
        {
            _energyJobHandle.Complete();
            _EnergyInfoArray.Dispose();
            Initialise();

            int j = 0;
            foreach (Battery drone in Batteries.Values)
            {
                _EnergyInfoArray[j] = new EnergyInfo();
                _EnergyInfoArray[j] = drone.GetEnergyInfo(_EnergyInfoArray[j]);
                j++;
            }

        }

        public static void ForceCountChange()
        {
            _Instance._energyJobHandle.Complete();
            _Instance._EnergyInfoArray.Dispose();
            _Instance.Initialise();

            int j = 0;
            foreach (Battery drone in _Instance.Batteries.Values)
            {
                _Instance._EnergyInfoArray[j] = new EnergyInfo();
                _Instance._EnergyInfoArray[j] = drone.GetEnergyInfo(_Instance._EnergyInfoArray[j]);
                j++;
            }
        }

    }


}
