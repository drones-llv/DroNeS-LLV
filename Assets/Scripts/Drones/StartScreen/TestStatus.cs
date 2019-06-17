using Drones.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.StartScreen
{
    public class TestStatus : MonoBehaviour
    {
        [SerializeField]
        Image _Success;
        [SerializeField]
        Image _Failed;
        [SerializeField]
        Image _Untested;

        public Image Success
        {
            get
            {
                if (_Success == null)
                {
                    _Success = transform.FindDescendent("Success").GetComponent<Image>();
                }
                return _Success;
            }
        }

        public Image Failed
        {
            get
            {
                if (_Failed == null)
                {
                    _Failed = transform.FindDescendent("Failed").GetComponent<Image>();
                }
                return _Failed;
            }
        }

        public Image Untested
        {
            get
            {
                if (_Untested == null)
                {
                    _Untested = transform.FindDescendent("Untested").GetComponent<Image>();
                }
                return _Untested;
            }
        }

        public void UpdateStatus(bool success)
        {
            if (success)
            {
                Untested.gameObject.SetActive(false);
                Failed.gameObject.SetActive(false);
                Success.gameObject.SetActive(true);
            }
            else
            {
                Untested.gameObject.SetActive(false);
                Failed.gameObject.SetActive(true);
                Success.gameObject.SetActive(false);
            }
        }

        public void OnNewURL()
        {
            Untested.gameObject.SetActive(true);
            Failed.gameObject.SetActive(false);
            Success.gameObject.SetActive(false);
        }

    }
}
