using UnityEngine;
using System.Collections;
using System.Globalization;

namespace Drones.UI
{
    using Utils;
    using Managers;
    using Data;

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
            _Fields[1].SetField(Drone.ActiveDrones.childCount.ToString());
            _Fields[2].SetField(data.crashes.ToString());
            _Fields[3].SetField(data.queuedJobs.ToString());
            _Fields[4].SetField(data.completedCount.ToString());
            _Fields[5].SetField(data.delayedJobs.ToString());
            _Fields[6].SetField(data.failedJobs.ToString());
            _Fields[7].SetField(data.hubs.Count.ToString());
            _Fields[8].SetField(data.revenue.ToString("C", CultureInfo.CurrentCulture));
            _Fields[9].SetField(UnitConverter.Convert(Chronos.min, data.totalDelay / data.completedCount));
            _Fields[10].SetField(UnitConverter.Convert(Energy.kWh, data.totalEnergy));
            _Fields[11].SetField(UnitConverter.Convert(Chronos.min, data.totalAudible));
        }
    }

}