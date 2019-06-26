using System.Collections;
using System.Globalization;
using Drones.Managers;
using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using UnityEngine;
using Utils;

namespace Drones.UI.Dahsboard
{
    public class SimulationInfo : MonoBehaviour
    {
        private DataField[] _Fields;

        private void Awake()
        {
            _Fields = GetComponentsInChildren<DataField>();
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(StreamData());
        }

        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(1 / 10f);
            while (true)
            {
                SimManager.GetData(this);
                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice) yield return null;

                yield return wait;
            }
        }

        public void SetData(SimulationData data)
        {
            _Fields[0].SetField(data.drones.Count.ToString());
            _Fields[1].SetField(Objects.Drone.ActiveDrones.childCount.ToString());
            _Fields[2].SetField(data.crashes.ToString());
            _Fields[3].SetField(data.queuedJobs.ToString());
            _Fields[4].SetField(data.completedCount.ToString());
            _Fields[5].SetField(data.delayedJobs.ToString());
            _Fields[6].SetField(data.failedJobs.ToString());
            _Fields[7].SetField(data.hubs.Count.ToString());
            _Fields[8].SetField(UnitConverter.Convert(Currency.USD, data.revenue));
            _Fields[9].SetField(UnitConverter.Convert(Chronos.min, data.totalDelay / data.completedCount));
            _Fields[10].SetField(UnitConverter.Convert(Energy.kWh, data.totalEnergy));
            _Fields[11].SetField(UnitConverter.Convert(Chronos.min, data.totalAudible));
        }
    }

}