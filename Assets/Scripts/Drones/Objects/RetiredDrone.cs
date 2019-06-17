using Drones.Data;
using Drones.Event_System;
using Drones.Managers;
using Drones.Serializable;
using Drones.UI.Console;
using Drones.UI.Drone;
using Drones.UI.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.Objects
{
    public class RetiredDrone : IDataSource
    {
        public RetiredDrone(Drone drone, Collider other)
        {
            _Data = new RetiredDroneData(drone, other);
            ConsoleLog.WriteToConsole(new DroneCollision(this));
        }

        public RetiredDrone(Drone drone)
        {
            _Data = new RetiredDroneData(drone);
            ConsoleLog.WriteToConsole(new DroneRetired(this));

        }

        public RetiredDrone(SRetiredDrone data)
        {
            _Data = new RetiredDroneData(data);
            SimManager.AllRetiredDrones.Add(UID, this);
        }

        public string Name => "D" + _Data.UID.ToString("000000");

        public Job GetJob() => (Job)SimManager.AllIncompleteJobs[_Data.job];
        private readonly RetiredDroneData _Data;

        #region IDataSource
        public uint UID => _Data.UID;

        public bool IsDataStatic => _Data.IsDataStatic;

        public AbstractInfoWindow InfoWindow { get; set; }

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_Data);

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = RetiredDroneWindow.New();
                InfoWindow.Source = this;
                InfoWindow.WindowName.SetText(Name);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }
        #endregion

        public SecureSortedSet<uint, IDataSource> JobHistory => _Data.completedJobs;

        public string OtherDroneName => _Data.otherDrone;

        public Vector3 Location => _Data.collisionLocation;

        public RetiredDrone OtherDrone
        {
            get
            {
                if (_Data.isDroneCollision)
                {
                    return (RetiredDrone)SimManager.AllRetiredDrones[_Data.otherUID];
                }
                return null;
            }
        }

        public SRetiredDrone Serialize() => new SRetiredDrone(_Data);


    }
}
