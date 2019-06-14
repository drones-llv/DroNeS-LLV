namespace Drones.UI
{
    using Utils;
    using Managers;
    public class HistoriesFoldable : FoldableMenu
    {
        private JobHistoryWindow _JobHistory;
        private RetiredDroneListWindow _RetiredDrones;

        #region Properties
        public JobHistoryWindow JobHistory
        {
            get
            {
                if (_JobHistory == null || !_JobHistory.gameObject.activeSelf)
                {
                    return null;
                }
                return _JobHistory;
            }
            private set
            {
                _JobHistory = value;
            }
        }

        public RetiredDroneListWindow RetiredDrones
        {
            get
            {
                if (_RetiredDrones == null || !_RetiredDrones.gameObject.activeSelf)
                {
                    return null;
                }
                return _RetiredDrones;
            }
            private set
            {
                _RetiredDrones = value;
            }
        }
        #endregion

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(OpenWholeJobHistory);
            Buttons[1].onClick.AddListener(OpenWholeRetiredDroneList);
            base.Start();
        }

        private void OpenWholeRetiredDroneList()
        {
            if (RetiredDrones == null)
            {
                RetiredDrones = RetiredDroneListWindow.New();
                RetiredDrones.Sources = SimManager.AllRetiredDrones;
            }
            else
            {
                RetiredDrones.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeJobHistory()
        {
            if (JobHistory == null)
            {
                JobHistory = JobHistoryWindow.New();
                JobHistory.Sources = SimManager.AllCompleteJobs;
            }
            else
            {
                JobHistory.transform.SetAsLastSibling();
            }
        }
    }
}
