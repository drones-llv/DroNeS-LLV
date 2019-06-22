using Drones.Data;
using Drones.Event_System;
using Drones.Managers;
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

        public string Name => $"NFZ{UID:000000}";
        public override string ToString() => Name;

        private NfzData _data;
        private void OnTriggerEnter(Collider other)
        {
            var obj = other.GetComponent<IDataSource>();
            if (obj == null) return;
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
            _data = new NfzData(this);
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

    }

}