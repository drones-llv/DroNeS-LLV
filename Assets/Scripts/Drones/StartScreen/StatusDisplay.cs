using Drones.Extensions;
using UnityEngine;

namespace Drones.StartScreen
{
    public class StatusDisplay : MonoBehaviour
    {
        [SerializeField]
        GameObject _Green;
        [SerializeField]
        GameObject _Yellow;
        [SerializeField]
        GameObject _Red;

        GameObject Green
        {
            get
            {
                if (_Green == null)
                {
                    _Green = transform.FindDescendent("Success").gameObject;
                }
                return _Green;
            }
        }

        GameObject Yellow
        {
            get
            {
                if (_Yellow == null)
                {
                    _Yellow = transform.FindDescendent("Untested").gameObject;
                }
                return _Yellow;
            }
        }

        GameObject Red
        {
            get
            {
                if (_Red == null)
                {
                    _Red = transform.FindDescendent("Failed").gameObject;
                }
                return _Red;
            }
        }

        private void Awake() => ClearStatus();

        public bool Status { get; private set; }

        public void SetStatus(bool isOK)
        {
            Status = isOK;

            if (Status)
            {
                _Yellow.SetActive(false);
                _Red.SetActive(false);
                _Green.SetActive(true);
            }
            else
            {
                _Yellow.SetActive(false);
                _Green.SetActive(false);
                _Red.SetActive(true);
            }

        }

        public void ClearStatus()
        {
            Status = false;
            _Yellow.SetActive(true);
            _Green.SetActive(false);
            _Red.SetActive(false);

        }
    }
}


