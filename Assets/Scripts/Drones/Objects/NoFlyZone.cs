using Drones.Data;
using Drones.Event_System;
using Drones.Managers;
using Drones.Serializable;
using Drones.UI.Console;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Objects
{
    public class NoFlyZone : MonoBehaviour, IPoolable, IDataSource
    {
        public static NoFlyZone New() => PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null);
        public static NoFlyZone Load(SNoFlyZone data)
        {
            var nfz = PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null, true);
            nfz.InPool = false;
            nfz._data = new NFZData(data, nfz);
            SimManager.AllNfz.Add(nfz.UID, nfz);
            return nfz;
        }

        public string Name => $"NFZ{UID:000000}";
        public override string ToString() => Name;

        private NFZData _data;
        private void OnTriggerEnter(Collider other)
        {
            var obj = other.GetComponent<IDataSource>();
            if (obj != null)
            {
                if (obj is Drone)
                {
                    _data.droneEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
                else if (obj is Hub)
                {
                    _data.hubEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
            }
        }
        public Vector3 Position => transform.position;

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public bool InPool { get; private set; }
        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            _data = null;
            SimManager.AllNfz.Remove(this);
            transform.SetParent(PC().PoolParent);
            gameObject.SetActive(false);
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            _data = new NFZData(this);
            gameObject.SetActive(true);
            transform.SetParent(parent);
            SimManager.AllNfz.Add(UID, this);
        }
        #endregion

        #region IDataSource
        public uint UID { get; private set; }

        public bool IsDataStatic { get; } = false;

        public AbstractInfoWindow InfoWindow { get; set; } = null;

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_data);

        public void OpenInfoWindow() { return; }
        #endregion

        public SNoFlyZone Serialize() => new SNoFlyZone(_data);

    }

}