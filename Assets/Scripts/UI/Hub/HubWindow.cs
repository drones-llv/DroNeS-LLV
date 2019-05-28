using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

namespace Drones.UI
{
    using Utils;
    using Data;
    using static Utils.UnitConverter;
    using Utils.Extensions;
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

        #region Buttons

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
        #endregion

        protected override Vector2 MaximizedSize { get; } = new Vector2(550, 570);

        protected override void Awake()
        {
            base.Awake();
            GoToLocation.onClick.AddListener(GoToHub);
            ShowDroneList.onClick.AddListener(OpenDroneList);
            AddDrone.onClick.AddListener(GetDrone);
            RemoveDrone.onClick.AddListener(SellDrone);
            AddBattery.onClick.AddListener(BuyBattery);
            RemoveBattery.onClick.AddListener(SellBattery);
        }

        private void OpenDroneList()
        {
            var dronelist = DroneListWindow.New();
            dronelist.WindowName.SetText(((Hub)Source).Name);
            dronelist.Sources = ((Hub)Source).Drones;
            dronelist.Opener = OpenDroneList;
            dronelist.CreatorEvent = ShowDroneList.onClick;
            ShowDroneList.onClick.RemoveAllListeners();
            ShowDroneList.onClick.AddListener(dronelist.transform.SetAsLastSibling);
        }

        private void GoToHub()
        {
            var position = ((Hub)Source).transform.position;
            AbstractCamera.LookHere(position);
        }

        private void GetDrone() => ((Hub)Source).BuyDrone();

        private void SellDrone() => ((Hub)Source).SellDrone();

        private void BuyBattery() => ((Hub)Source).BuyBattery();

        private void SellBattery() => ((Hub)Source).SellBattery();

        public override void SetData(IData data)
        {
            var hub = (HubData)data;

            Data[0].SetField(Source.ToString());
            Data[1].SetField(hub.Position.ToStringXZ());
            Data[2].SetField(hub.drones.Count.ToString());
            Data[3].SetField((hub.drones.Count - hub.freeDrones.Count).ToString());
            Data[4].SetField(hub.crashes.ToString());
            Data[5].SetField(hub.batteries.Count.ToString());
            Data[6].SetField(hub.chargingBatteries.Count.ToString());
            Data[7].SetField(((Hub)Source).Scheduler.JobQueueLength.ToString());
            Data[8].SetField(hub.completedJobs.Count.ToString());
            Data[9].SetField(hub.delayedJobs.ToString());
            Data[10].SetField(hub.failedJobs.ToString());
            Data[11].SetField(hub.revenue.ToString("C", CultureInfo.CurrentCulture));
            Data[12].SetField(Convert(Chronos.min, hub.delay / hub.completedJobs.Count));
            Data[13].SetField(Convert(Energy.kWh, hub.energyConsumption));
            Data[14].SetField(Convert(Chronos.min, hub.audibility));
        }
    }

}
