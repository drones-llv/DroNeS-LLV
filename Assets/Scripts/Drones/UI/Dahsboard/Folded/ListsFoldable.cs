using Drones.Managers;
using Drones.UI.Drone;
using Drones.UI.Hub;
using Drones.UI.Job;
using Drones.UI.NoFlyZone;

namespace Drones.UI.Dahsboard.Folded
{
    public class ListsFoldable : FoldableTaskBar
    {
        private JobQueueWindow _JobQueue;
        private HubListWindow _HubList;
        private DroneListWindow _DroneList;
        private NoFlyZoneListWindow _NFZList;

        #region Properties
        public JobQueueWindow JobQueue
        {
            get
            {
                if (_JobQueue != null && !_JobQueue.IsOpen)
                {
                    return null;
                }
                return _JobQueue;
            }
            private set
            {
                _JobQueue = value;
            }
        }

        public HubListWindow HubList
        {
            get
            {
                if (_HubList != null && !_HubList.IsOpen)
                {
                    return null;
                }
                return _HubList;
            }
            private set
            {
                _HubList = value;
            }
        }

        public DroneListWindow DroneList
        {
            get
            {
                if (_DroneList != null && !_DroneList.IsOpen)
                {
                    return null;
                }
                return _DroneList;
            }
            private set
            {
                _DroneList = value;
            }
        }

        public NoFlyZoneListWindow NFZList
        {
            get
            {
                if (_NFZList != null && !_NFZList.IsOpen)
                {
                    return null;
                }
                return _NFZList;
            }
            private set
            {
                _NFZList = value;
            }
        }
        #endregion

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(OpenWholeJobQueue);
            Buttons[1].onClick.AddListener(OpenWholeHubList);
            Buttons[2].onClick.AddListener(OpenWholeDroneList);
            Buttons[3].onClick.AddListener(OpenWholeNFZList);
            base.Start();
        }

        private void OpenWholeJobQueue()
        {
            if (JobQueue == null)
            {
                JobQueue = JobQueueWindow.New();
                JobQueue.Sources = SimManager.AllIncompleteJobs;
            }
            else
            {
                JobQueue.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeHubList()
        {
            if (HubList == null)
            {
                HubList = HubListWindow.New();
                HubList.Sources = SimManager.AllHubs;
            }
            else
            {
                HubList.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeDroneList()
        {
            if (DroneList == null)
            {
                DroneList = DroneListWindow.New();
                DroneList.Sources = SimManager.AllDrones;
            }
            else
            {
                DroneList.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeNFZList()
        {
            if (NFZList == null)
            {
                NFZList = NoFlyZoneListWindow.New();
                NFZList.Sources = SimManager.AllNfz;
            }
            else
            {
                NFZList.transform.SetAsLastSibling();
            }
        }

    }
}
