using System.Globalization;
using Drones.Data;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using Drones.Objects;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Job
{
    public class JobWindow : AbstractInfoWindow
    {
        public static JobWindow New() => PoolController.Get(WindowPool.Instance).Get<JobWindow>(null);
        [SerializeField]
        private Button goToOrigin;
        [SerializeField]
        private Button goToDestination;

        #region Buttons
        private Button GoToOrigin
        {
            get
            {
                if (goToOrigin == null)
                {
                    goToOrigin = transform.Find("Origin").GetComponentInChildren<Button>();
                }
                return goToOrigin;
            }
        }
        private Button GoToDestination
        {
            get
            {
                if (goToDestination == null)
                {
                    goToDestination = transform.Find("Dest.").GetComponentInChildren<Button>();
                }
                return goToDestination;
            }

        }
        #endregion

        protected override Vector2 MaximizedSize { get; } = new Vector2(450, 500);

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Objects.Job)Source).Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Objects.Job)Source).DropOff;
                AbstractCamera.LookHere(position);
            });
        }

        public override void SetData(IData data)
        {
            var job = (JobData)data;
            Data[0].SetField(Source);
            Data[1].SetField(job.Pickup.ToStringXZ());
            Data[2].SetField(job.Dropoff.ToStringXZ());
            Data[3].SetField(job.Cost.Start.ToString());
            Data[4].SetField(job.Assignment.ToString());
            Data[5].SetField(job.Deadline.ToString());
            Data[6].SetField(job.Completed.ToString());
            Data[7].SetField(UnitConverter.Convert(Mass.g, job.PackageWeight));
            Data[8].SetField(job.Earnings.ToString("C", CultureInfo.CurrentCulture));
            Data[9].SetField((job.Deadline.IsNull()) ? "" : UnitConverter.Convert(Chronos.min, job.Deadline.Timer()));
            Data[10].SetField("D" + job.Drone.ToString("000000"));
            Data[11].SetField(((Objects.Job)Source).Progress());
        }
    }

}
