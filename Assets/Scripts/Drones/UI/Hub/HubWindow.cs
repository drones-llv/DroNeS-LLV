using System.Collections;
using System.Globalization;
using Drones.Data;
using Drones.Managers;
using Drones.UI.Drone;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Hub
{
    public class HubWindow : AbstractInfoWindow
    {
        public static HubWindow New() => PoolController.Get(WindowPool.Instance).Get<HubWindow>(null);

        [SerializeField]
        private Button _GoToLocation;
        [SerializeField]
        private Button _ShowDroneList;
        [SerializeField]
        private Button _AddDrone;
        [SerializeField]
        private Button _RemoveDrone;
        [SerializeField]
        private Button _AddBattery;
        [SerializeField]
        private Button _RemoveBattery;
        [SerializeField]
        private TMP_InputField _GenerationRate;
        [SerializeField]
        private Button _InputConfirm;

        #region Initializers
        private Button GoToLocation
        {
            get
            {
                if (_GoToLocation == null)
                {
                    _GoToLocation = transform.Find("Goto Button").GetComponent<Button>();
                }
                return _GoToLocation;
            }
        }

        private Button ShowDroneList
        {
            get
            {
                if (_ShowDroneList == null)
                {
                    _ShowDroneList = transform.Find("List Button").GetComponent<Button>();
                }
                return _ShowDroneList;
            }
        }

        private Button AddDrone
        {
            get
            {
                if (_AddDrone == null)
                {
                    _AddDrone = transform.Find("Drones").Find("Add Button").GetComponent<Button>();
                }
                return _AddDrone;
            }
        }

        private Button RemoveDrone
        {
            get
            {
                if (_RemoveDrone == null)
                {
                    _RemoveDrone = transform.Find("Drones").Find("Subtract Button").GetComponent<Button>();
                }
                return _RemoveDrone;
            }
        }

        private Button AddBattery
        {
            get
            {
                if (_AddBattery == null)
                {
                    _AddBattery = transform.Find("Batteries").Find("Add Button").GetComponent<Button>();
                }
                return _AddBattery;
            }
        }

        private Button RemoveBattery
        {
            get
            {
                if (_RemoveBattery == null)
                {
                    _RemoveBattery = transform.Find("Batteries").Find("Subtract Button").GetComponent<Button>();
                }
                return _RemoveBattery;
            }
        }

        private TMP_InputField GenerationRate
        {
            get
            {
                if (_GenerationRate == null) _GenerationRate = GetComponentInChildren<TMP_InputField>();
                return _GenerationRate;
            }
        }

        private Button InputConfirm
        {
            get
            {
                if (_InputConfirm) _InputConfirm = GenerationRate.transform.parent.GetComponentInChildren<Button>();
                return _InputConfirm;
            }
        }
        #endregion

        protected override Vector2 MaximizedSize { get; } = new Vector2(550, 600);

        protected override void Awake()
        {
            base.Awake();
            GoToLocation.onClick.AddListener(GoToHub);
            ShowDroneList.onClick.AddListener(OpenDroneList);
            AddDrone.onClick.AddListener(GetDrone);
            RemoveDrone.onClick.AddListener(SellDrone);
            AddBattery.onClick.AddListener(BuyBattery);
            RemoveBattery.onClick.AddListener(SellBattery);
            InputConfirm.onClick.AddListener(ChangeGenerationRate);
        }

        void OnEnable() => StartCoroutine(Clear());

        IEnumerator Clear()
        {
            GenerationRate.text = null;
            yield return new WaitUntil(() => Source != null);
            ((TextMeshProUGUI)GenerationRate.placeholder).SetText(((Objects.Hub)Source).JobGenerationRate.ToString());
        }

        private void OpenDroneList()
        {
            var dronelist = DroneListWindow.New();
            dronelist.WindowName.SetText(((Objects.Hub)Source).Name);
            dronelist.Sources = ((Objects.Hub)Source).Drones;
            dronelist.Opener = OpenDroneList;
            dronelist.CreatorEvent = ShowDroneList.onClick;
            ShowDroneList.onClick.RemoveAllListeners();
            ShowDroneList.onClick.AddListener(dronelist.transform.SetAsLastSibling);
        }

        private void ChangeGenerationRate()
        {
            if (string.IsNullOrWhiteSpace(GenerationRate.text)) return;
            var val = float.Parse(GenerationRate.text);
            if (val <= 0) return;
            GenerationRate.text = null;
            ((Objects.Hub)Source).JobGenerationRate = val;
            ((TextMeshProUGUI)GenerationRate.placeholder).SetText(val.ToString());
        }
       
        private void GoToHub()
        {
            var position = ((Objects.Hub)Source).transform.position;
            AbstractCamera.LookHere(position);
        }

        private void GetDrone()
        {
            for (var i = 0; i < 10; i++)
                ((Objects.Hub)Source).BuyDrone(); 
        }

        private void SellDrone() => ((Objects.Hub)Source).SellDrone();

        private void BuyBattery() => ((Objects.Hub)Source).BuyBattery();

        private void SellBattery() => ((Objects.Hub)Source).SellBattery();

        public override void SetData(IData data)
        {
            var hub = (HubData)data;

            Data[0].SetField(Source.ToString());
            Data[1].SetField(hub.Position.ToStringXZ());
            Data[2].SetField(hub.drones.Count.ToString());
            Data[3].SetField((hub.drones.Count - hub.DronesWithNoJobs.Count).ToString());
            Data[4].SetField(hub.NumberOfDroneCrashes.ToString());
            Data[5].SetField(hub.batteries.Count.ToString());
            Data[6].SetField(((Objects.Hub)Source).GetChargingBatteryCount().ToString());
            Data[7].SetField(((Objects.Hub)Source).Scheduler.JobQueueLength.ToString());
            Data[8].SetField(hub.CompletedJobCount.ToString());
            Data[9].SetField(hub.DelayedCompletedJobs.ToString());
            Data[10].SetField(hub.FailedJobs.ToString());
            Data[11].SetField(UnitConverter.Convert(Currency.USD, hub.Earnings));
            Data[12].SetField(UnitConverter.Convert(Chronos.min, hub.TotalDelayOfCompletedJobs / hub.CompletedJobCount));
            Data[13].SetField(UnitConverter.Convert(Energy.kWh, hub.EnergyConsumption));
            Data[14].SetField(UnitConverter.Convert(Chronos.min, hub.AudibleDuration));
        }

    }

}
