using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI.Console
{
    public class ConsoleElement : AbstractListElement
    {
        [SerializeField]
        private DataField _Message;

        public override Button Link
        {
            get
            {
                if (_Link == null)
                {
                    _Link = transform.GetChild(0).GetComponent<Button>();
                }
                return _Link;
            }
        }

        public DataField Message
        {
            get
            {
                if (_Message == null)
                {
                    _Message = gameObject.GetComponentInChildren<DataField>();
                }
                return _Message;
            }

        }

        public static ConsoleElement New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<ConsoleElement>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}
