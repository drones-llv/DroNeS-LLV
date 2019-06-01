namespace Mapbox.Unity.Utilities
{
	using UnityEngine;
	using UnityEngine.UI;
	
	public class Console : MonoBehaviour
	{
		[SerializeField]
        public Button _toggleButton;

		[SerializeField]
        public GameObject _menu;

		[SerializeField]
        public Text _logText;

		[SerializeField]
        public ScrollRect _logScroll;

		static Console _instance;
		public static Console Instance { get { return _instance; } }

		string _log;


		protected virtual void Awake()
		{
			if (_instance != null)
			{
				Debug.LogError("Duplicate singleton!", gameObject);
			}
			_instance = this;
			ClearLog();
		}

		void ClearLog()
		{
			_log = "";
			_logText.text = _log;
			_logScroll.verticalNormalizedPosition = 0f;
		}

		public void Log(string log, string color)
		{
			if (!string.IsNullOrEmpty(_log) && _log.Length > 15000)
			{
				_log = "";
			}
			_log += string.Format("<color={0}>{1}</color>\n", color, log);
			_logText.text = _log;
			_logScroll.verticalNormalizedPosition = 0f;
		}


		public void ToggleMenu()
		{
			_menu.SetActive(!_menu.activeSelf);
		}
	}
}
