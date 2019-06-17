using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Drones.UI.Utils
{
    public class ListHeaders : MonoBehaviour
    {
        private ObjectListWindow _Window;
        public ObjectListWindow Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = (ObjectListWindow)AbstractWindow.GetWindow(transform);
                }
                return _Window;
            }
        }

        public ListTupleContainer TupleContainer
        {
            get
            {
                return Window.TupleContainer;
            }
        }

        private Button[] _Headers;
        public Button[] Headers
        {
            get
            {
                if (_Headers == null)
                {
                    _Headers = GetComponentsInChildren<Button>();
                }
                return _Headers;
            }
        }
        private SortOrder[] _Order;
        public SortOrder[] Order
        {
            get
            {
                if (_Order == null)
                {
                    _Order = new SortOrder[Headers.Length];
                    for (int i = 0; i < _Order.Length; i++)
                    {
                        _Order[i] = SortOrder.Ascending;
                    }
                }
                return _Order;
            }
        }

        private void Start()
        {
            foreach (Button header in Headers)
            {
                header.onClick.AddListener(delegate
                {
                    Sort(header.transform.GetSiblingIndex());
                });
            }
        }

        // The indexing will be consistent due to horizontal layout
        // index : data index in the tuple
        private void Sort(int index)
        {
            var tuples = TupleContainer.GetComponentsInChildren<ObjectTuple>().ToList();
            int Comparer(ObjectTuple a, ObjectTuple b)
            {
                return string.Compare(a.Data[index].text, b.Data[index].text);
            }

            tuples.Sort(Comparer);

            for (int i =0; i < tuples.Count; i++)
            {
                tuples[i].transform.SetSiblingIndex(i);
            }

        }
    }

}

