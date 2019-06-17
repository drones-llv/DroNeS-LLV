using System;
using System.Collections.Generic;
using Drones.UI.Console;
using Drones.UI.Drone;
using Drones.UI.Hub;
using Drones.UI.Job;
using Drones.UI.NoFlyZone;
using Drones.UI.SaveLoad;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Utils
{
    public class ListElementPool : AbstractPool
    {
        private static ListElementPool _Instance;
        public static ListElementPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ListElementPool();
                }
                return _Instance;
            }
        }
        public static void Reset() 
        {
            _Instance = null;
        }

        public override Dictionary<Type, string> Paths
        {
            get
            {
                if (_Paths == null)
                {
                    _Paths = new Dictionary<Type, string>
                    {
                        { typeof(ConsoleElement), ConsoleElementPath },
                        { typeof(DroneTuple), DroneListTuplePath },
                        { typeof(RetiredDroneTuple), RetiredDroneListTuplePath },
                        { typeof(HubTuple), HubListTuplePath },
                        { typeof(JobQueueTuple), JobQueueTuplePath },
                        { typeof(JobHistoryTuple), JobHistoryTuplePath },
                        { typeof(NFZTuple), NoFlyZoneTuplePath },
                        { typeof(SaveLoadTuple), SaveLoadTuplePath }
                    };
                }

                return _Paths;
            }
        }

        public override Dictionary<Type, uint> StartSize
        {
            get
            {
                if (_StartSize == null)
                {
                    _StartSize = new Dictionary<Type, uint>
                    {
                        { typeof(NFZTuple), 50 },
                        { typeof(SaveLoadTuple), 200 },
                        { typeof(DroneTuple), 200 },
                        { typeof(HubTuple), 50 },
                        { typeof(JobQueueTuple), 200 },
                        { typeof(RetiredDroneTuple), 200 },
                        { typeof(JobHistoryTuple), 200 },
                        { typeof(ConsoleElement), 500 }

                    };
                }
                return _StartSize;
            }
        }

        public override Dictionary<Type, GameObject> Templates { get; } = new Dictionary<Type, GameObject>();

        public override Dictionary<Type, Queue<IPoolable>> Pool 
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new Dictionary<Type, Queue<IPoolable>>
                    {
                        { typeof(ConsoleElement), new Queue<IPoolable>() },
                        { typeof(DroneTuple), new Queue<IPoolable>() },
                        { typeof(RetiredDroneTuple), new Queue<IPoolable>() },
                        { typeof(HubTuple), new Queue<IPoolable>() },
                        { typeof(JobQueueTuple), new Queue<IPoolable>() },
                        { typeof(JobHistoryTuple), new Queue<IPoolable>() },
                        { typeof(NFZTuple), new Queue<IPoolable>() },
                        { typeof(SaveLoadTuple), new Queue<IPoolable>() }
                    };
                }

                return _Pool;
            }
        }

        public override Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        private const string WindowPrefabPath = "Prefabs/UI/Windows";
        /* List Elements */
        public const string DroneListTuplePath = WindowPrefabPath + "/Drone/DroneListTuple";
        public const string RetiredDroneListTuplePath = WindowPrefabPath + "/Drone/RetiredDroneListTuple";
        public const string HubListTuplePath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryTuplePath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueTuplePath = WindowPrefabPath + "/Job/JobQueueTuple";
        public const string NoFlyZoneTuplePath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneListTuple";
        public const string ConsoleElementPath = WindowPrefabPath + "/Console/ConsoleElement";
        public const string SaveLoadTuplePath = WindowPrefabPath + "/SaveLoad/SaveLoadTuple";
    }
}
