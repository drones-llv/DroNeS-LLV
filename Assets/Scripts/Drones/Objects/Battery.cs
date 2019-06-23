using System;
using Drones.Data;
using Drones.Managers;
using Unity.Collections;

namespace Drones.Objects
{
    [Serializable]
    public class Battery
    {
        public static NativeList<BatteryData> AllData;
        public static void DeleteData(Battery removed)
        {
            BatteryManager.ConsumptionJobHandle.Complete();
            var j = removed._accessIndex;
            AllData.RemoveAtSwapBack(j);
            SimManager.AllBatteries[AllData[j].UID]._accessIndex = j;
        }
        private static uint Count { get; set; }
        private int _accessIndex;
        public static void Reset() => Count = 0;
        public Battery(Drone drone, Hub hub)
        {
            UID = ++Count;
            BatteryManager.ChargeCountJobHandle.Complete();
            _accessIndex = AllData.Length;
            AllData.Add(new BatteryData(this)
            {
                drone = drone.UID,
                hub = hub.UID
            });
        }
        public Battery(Hub hub)
        {
            UID = ++Count; 
            BatteryManager.ChargeCountJobHandle.Complete();
            _accessIndex = AllData.Length;
            AllData.Add(new BatteryData(this)
            {
                drone = 0,
                hub = hub.UID
            });
        }

        #region Properties
        public string Name => $"B{UID:000000}";

        public float Charge
        {
            get
            {
                BatteryManager.ChargeCountJobHandle.Complete();
                return AllData[_accessIndex].charge / AllData[_accessIndex].capacity;
            }
        }

        public float Capacity
        {
            get
            {
                BatteryManager.ChargeCountJobHandle.Complete();
                return AllData[_accessIndex].capacity / BatteryData.DesignCapacity;
            }
        }
        
        #endregion

        public uint UID { get; }
        
        private BatteryData _data;

        public bool GetDrone(out Drone drone)
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            var j = AllData[_accessIndex].drone;
            if (j == 0)
            {
                drone = null;
                return false;
            }
            drone = (Drone)SimManager.AllDrones[j];
            return true;
        }
        public bool HasDrone()
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            return AllData[_accessIndex].drone != 0;   
        }

        public void AssignHub(Hub hub)
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            var tmp = AllData[_accessIndex];
            tmp.hub = hub.UID;
            AllData[_accessIndex] = tmp;
        }
        public void AssignDrone(Drone drone)
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            var tmp = AllData[_accessIndex];
            tmp.drone = drone.UID;
            AllData[_accessIndex] = tmp;
        }
        public void AssignDrone()
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            var tmp = AllData[_accessIndex];
            tmp.drone = 0;
            AllData[_accessIndex] = tmp;
        }

        public void Destroy()
        {
            BatteryManager.ChargeCountJobHandle.Complete();
            var h = AllData[_accessIndex].hub;
            if (h == 0) return;
            ((Hub)SimManager.AllHubs[h]).DestroyBattery(this);  
        } 
        
    }

}