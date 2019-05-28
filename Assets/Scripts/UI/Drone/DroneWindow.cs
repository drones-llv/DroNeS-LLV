using UnityEngine.UI;
using UnityEngine;
using System.Globalization;
namespace Drones.UI
{
    using Utils;
    using Data;
    using static Utils.UnitConverter;
    using Utils.Extensions;

    public class DroneWindow : AbstractInfoWindow
    {
        public static DroneWindow New() => PoolController.Get(WindowPool.Instance).Get<DroneWindow>(null);

        [SerializeField]
        private Button _FollowDrone;
        [SerializeField]
        private Button _GoToHub;
        [SerializeField]
        private Button _JobHistory;
        [SerializeField]
        private Button _JobInfo;
        [SerializeField]
        private Button _GoToOrigin;
        [SerializeField]
        private Button _GoToDestination;

        #region Properties
        private Button FollowDrone
        {
            get
            {
                if (_FollowDrone == null)
                {
                    _FollowDrone = ContentPanel.transform.Find("Name").GetComponentInChildren<Button>();
                }
                return _FollowDrone;
            }
        }
        private Button GoToHub
        {
            get
            {
                if (_GoToHub == null)
                {
                    _GoToHub = ContentPanel.transform.Find("Hub").GetComponentInChildren<Button>();
                }
                return _GoToHub;
            }
        }
        private Button JobHistory
        {
            get
            {
                if (_JobHistory == null)
                {
                    _JobHistory = ContentPanel.transform.Find("History Button").GetComponent<Button>();
                }
                return _JobHistory;
            }
        }
        private Button JobInfo
        {
            get
            {
                if (_JobInfo == null)
                {
                    _JobInfo = ContentPanel.transform.Find("MoreInfo Button").GetComponent<Button>();
                }
                return _JobInfo;
            }
        }
        private Button GoToOrigin
        {
            get
            {
                if (_GoToOrigin == null)
                {
                    _GoToOrigin = ContentPanel.transform.Find("Origin").GetComponentInChildren<Button>();
                }
                return _GoToOrigin;
            }
        }
        private Button GoToDestination
        {
            get
            {
                if (_GoToDestination == null)
                {
                    _GoToDestination = ContentPanel.transform.Find("Dest.").GetComponentInChildren<Button>();
                }
                return _GoToDestination;
            }
        }
        #endregion

        protected override Vector2 MaximizedSize { get; } = new Vector2(450, 650);

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).GetJob().Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).GetJob().DropOff;
                AbstractCamera.LookHere(position);
            });
            GoToHub.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).GetHub().transform.position;
                AbstractCamera.LookHere(position);
            });

            FollowDrone.onClick.AddListener(delegate
            {
                AbstractCamera.Followee = ((Drone)Source).gameObject;
            });

            JobInfo.onClick.AddListener(OpenJobWindow);
            JobHistory.onClick.AddListener(OpenJobHistoryWindow);
        }

        void OpenJobWindow() => ((Drone)Source).GetJob()?.OpenInfoWindow();

        void OpenJobHistoryWindow()
        {
            var jhw = JobHistoryWindow.New();
            jhw.WindowName.SetText(((Drone)Source).Name);
            jhw.Sources = ((Drone)Source).JobHistory;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }

        public override void SetData(IData data)
        {
            var drone = (DroneData)data;
            var source = (Drone)Source;

            Data[0].SetField(source.Name);
            Data[1].SetField(source.GetHub().Name);
            Data[2].SetField(drone.currentWaypoint.ToStringXZ());
            Data[3].SetField(Convert(Length.m, source.transform.position.y));

            Battery battery = source.GetBattery();
            Data[4].SetField(battery?.Charge.ToString("0.000"));
            Data[5].SetField(battery?.Capacity.ToString("0.000"));

            Job job = source.GetJob();
            Data[6].SetField(job?.Name);
            Data[7].SetField(job?.Pickup.ToStringXZ());
            Data[8].SetField(job?.DropOff.ToStringXZ());
            Data[9].SetField(job?.Deadline.ToString());
            Data[10].SetField(Convert(Mass.g, job?.PackageWeight));
            Data[11].SetField(job?.Earnings.ToString("C", CultureInfo.CurrentCulture));
            Data[12].SetField(drone?.JobProgress.ToString("0.000"));

            Data[13].SetField(drone.DeliveryCount);
            Data[14].SetField(Convert(Mass.kg, drone.packageWeight));
            Data[15].SetField(Convert(Length.km, drone.distanceTravelled));

            float tmp = ConvertValue(Mass.kg, drone.packageWeight);
            tmp /= ConvertValue(Length.km, drone.distanceTravelled);

            Data[16].SetField(tmp.ToString("0.000") + " " + Mass.kg + "/" + Length.km);
            Data[17].SetField(Convert(Energy.kWh, drone.totalEnergy));
            Data[18].SetField(drone.batterySwaps);
            Data[19].SetField(drone.hubsAssigned);
            Data[20].SetField(Convert(Chronos.min, drone.audibleDuration));

            //Averages
            Data[21].SetField(Convert(Mass.kg, drone.packageWeight / drone.DeliveryCount));
            Data[22].SetField(Convert(Length.km, drone.distanceTravelled / drone.DeliveryCount));
            Data[23].SetField(Convert(Chronos.min, drone.totalDelay / drone.DeliveryCount));
            Data[24].SetField(Convert(Energy.kWh, drone.totalEnergy / drone.DeliveryCount));
            tmp = drone.batterySwaps;
            tmp /= drone.DeliveryCount;
            Data[25].SetField(tmp);

            tmp = drone.hubsAssigned;
            tmp /= drone.DeliveryCount;
            Data[26].SetField(tmp);
            Data[27].SetField(Convert(Chronos.min, drone.audibleDuration));
        }
    }

}
