using System.Globalization;
using Drones.Data;
using Drones.Objects;
using Drones.UI.Job;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Drone
{
    public class RetiredDroneWindow : AbstractInfoWindow
    {
        public static RetiredDroneWindow New() => PoolController.Get(WindowPool.Instance).Get<RetiredDroneWindow>(null);

        [SerializeField]
        private Button _CollidedWith;
        [SerializeField]
        private Button _JobHistory;
        [SerializeField]
        private Button _JobInfo;
        [SerializeField]
        private Button _GoToOrigin;
        [SerializeField]
        private Button _GoToDestination;

        #region Properties
        private Button CollidedWith
        {
            get
            {
                if (_CollidedWith == null)
                {
                    _CollidedWith = ContentPanel.transform.Find("Name").GetComponentInChildren<Button>();
                }
                return _CollidedWith;
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

        protected override Vector2 MaximizedSize { get; } = new Vector2(425, 595);

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((RetiredDrone)Source).GetJob().Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((RetiredDrone)Source).GetJob().DropOff;
                AbstractCamera.LookHere(position);
            });
            CollidedWith.onClick.AddListener(delegate
            {
                ((RetiredDrone)Source).OtherDrone?.OpenInfoWindow();
            });

            JobInfo.onClick.AddListener(OpenJobWindow);

            JobHistory.onClick.AddListener(OpenJobHistoryWindow);
        }

        void OpenJobWindow()
        {
            ((RetiredDrone)Source).GetJob().OpenInfoWindow();
        }

        void OpenJobHistoryWindow()
        {
            var jhw = JobHistoryWindow.New();
            jhw.Sources = ((RetiredDrone)Source).JobHistory;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }

        public override void SetData(IData data)
        {
            var job = ((RetiredDrone)Source).GetJob();
            var rd = (RetiredDroneData)data;
            Data[0].SetField(((RetiredDrone)Source).Name);
            Data[1].SetField(rd.hub);
            Data[2].SetField(rd.waypoint.ToStringXZ());
            Data[3].SetField(rd.destroyedTime);
            Data[4].SetField(rd.collisionLocation.ToStringXYZ());
            Data[5].SetField(rd.packageWorth.ToString("C", CultureInfo.CurrentCulture));
            Data[6].SetField(rd.otherDrone);
            Data[7].SetField(rd.batteryCharge);
            Data[8].SetField(job);
            Data[9].SetField(job?.Pickup.ToStringXZ());
            Data[10].SetField(job?.DropOff.ToStringXZ());
            Data[11].SetField(job?.Deadline.ToString());
        }
    }

}
