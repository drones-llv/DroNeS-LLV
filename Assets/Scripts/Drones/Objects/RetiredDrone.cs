using Drones.Data;
using Drones.Event_System;
using Drones.Managers;
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
            _data = new RetiredDroneData(drone, other);
            ConsoleLog.WriteToConsole(new DroneCollision(this));
        }

        public RetiredDrone(Drone drone)
        {
            _data = new RetiredDroneData(drone);
            ConsoleLog.WriteToConsole(new DroneRetired(this));

        }

        public string Name => "D" + _data.UID.ToString("000000");

        public Job GetJob() => (Job)SimManager.AllIncompleteJobs[_data.job];
        private readonly RetiredDroneData _data;

        #region IDataSource
        public uint UID => _data.UID;

        public bool IsDataStatic => _data.IsDataStatic;

        public AbstractInfoWindow InfoWindow { get; set; }

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_data);

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

        public SecureSortedSet<uint, IDataSource> JobHistory => _data.completedJobs;

        public string OtherDroneName => _data.otherDrone;

        public Vector3 Location => _data.collisionLocation;

        public RetiredDrone OtherDrone
        {
            get
            {
                if (_data.isDroneCollision)
                {
                    return (RetiredDrone)SimManager.AllRetiredDrones[_data.otherUID];
                }
                return null;
            }
        }


    }
}
