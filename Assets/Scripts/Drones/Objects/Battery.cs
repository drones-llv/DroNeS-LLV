using System;
using Drones.Data;
using Drones.JobSystem;
using Drones.Managers;
using Unity.Collections;
using UnityEngine;
using Utils;
using BatteryStatus = Utils.BatteryStatus;

namespace Drones.Objects
{
    [Serializable]
    public class Battery
    {
        public static void DeleteData(Battery removed)
        {
            BatteryManager.EnergyJobHandle.Complete();
            var j = removed._accessIndex;
            BatteryManager.BatteryInfo.RemoveAtSwapBack(j);
            SimManager.AllBatteries[BatteryManager.BatteryInfo[j].UID]._accessIndex = j;
        }
        private static uint Count { get; set; }
        private int _accessIndex;
        public static void Reset() => Count = 0;
        public Battery(Drone drone, Hub hub)
        {
            UID = ++Count;
            BatteryManager.EnergyJobHandle.Complete();
            _accessIndex = BatteryManager.BatteryInfo.Length;
            BatteryManager.BatteryInfo.Add(new BatteryData(this)
            {
                drone = drone.UID,
                hub = hub.UID
            });
        }
        public Battery(Hub hub)
        {
            UID = ++Count;
            BatteryManager.EnergyJobHandle.Complete();
            _accessIndex = BatteryManager.BatteryInfo.Length;
            BatteryManager.BatteryInfo.Add(new BatteryData(this)
            {
                drone = 0,
                hub = hub.UID
            });
        }

        #region Properties
        public string Name => $"B{UID:000000}";

        public BatteryStatus Status 
        {
            get
            {
                BatteryManager.EnergyJobHandle.Complete();
                return BatteryManager.BatteryInfo[_accessIndex].status;
            }
        }

        public float Charge
        {
            get
            {
                BatteryManager.EnergyJobHandle.Complete();
                return BatteryManager.BatteryInfo[_accessIndex].charge / BatteryManager.BatteryInfo[_accessIndex].capacity;
            }
        }

        public float Capacity
        {
            get
            {
                BatteryManager.EnergyJobHandle.Complete();
                return BatteryManager.BatteryInfo[_accessIndex].capacity / BatteryData.DesignCapacity;
            }
        }
        
        #endregion

        public uint UID { get; }
        
        private BatteryData _data;

        public bool GetDrone(out Drone drone)
        {
            BatteryManager.EnergyJobHandle.Complete();
            var j = BatteryManager.BatteryInfo[_accessIndex].drone;
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
            BatteryManager.EnergyJobHandle.Complete();
            return BatteryManager.BatteryInfo[_accessIndex].drone != 0;   
        }

        public void AssignHub(Hub hub)
        {
            BatteryManager.EnergyJobHandle.Complete();
            var tmp = BatteryManager.BatteryInfo[_accessIndex];
            tmp.hub = hub.UID;
            BatteryManager.BatteryInfo[_accessIndex] = tmp;
        }
        public void AssignDrone(Drone drone)
        {
            BatteryManager.EnergyJobHandle.Complete();
            var tmp = BatteryManager.BatteryInfo[_accessIndex];
            tmp.drone = drone.UID;
            BatteryManager.BatteryInfo[_accessIndex] = tmp;
        }
        public void AssignDrone()
        {
            BatteryManager.EnergyJobHandle.Complete();
            var tmp = BatteryManager.BatteryInfo[_accessIndex];
            tmp.drone = 0;
            BatteryManager.BatteryInfo[_accessIndex] = tmp;
        }

        public void Destroy()
        {
            BatteryManager.EnergyJobHandle.Complete();
            var h = BatteryManager.BatteryInfo[_accessIndex].hub;
            if (h == 0) return;
            ((Hub)SimManager.AllHubs[h]).DestroyBattery(this);  
        } 
        
    }

}