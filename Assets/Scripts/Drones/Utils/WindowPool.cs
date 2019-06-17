using System;
using System.Collections.Generic;
using Drones.UI.Drone;
using Drones.UI.Hub;
using Drones.UI.Job;
using Drones.UI.NoFlyZone;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Utils
{
    public class WindowPool : AbstractPool
    {
        private static WindowPool _Instance;
        public static WindowPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new WindowPool();
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
                        { typeof(DroneWindow), DroneWindowPath },
                        { typeof(DroneListWindow), DroneListWindowPath },
                        { typeof(RetiredDroneWindow), RetiredDroneWindowPath },
                        { typeof(RetiredDroneListWindow), RetiredDroneListWindowPath },
                        { typeof(HubWindow), HubWindowPath },
                        { typeof(HubListWindow), HubListWindowPath },
                        { typeof(JobWindow), JobWindowPath },
                        { typeof(JobQueueWindow), JobQueueWindowPath },
                        { typeof(JobHistoryWindow), JobHistoryWindowPath },
                        { typeof(NoFlyZoneListWindow), NoFlyZoneListWindowPath}
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
                        { typeof(DroneWindow), 10 },
                        { typeof(DroneListWindow), 5 },
                        { typeof(RetiredDroneWindow), 10 },
                        { typeof(RetiredDroneListWindow), 5 },
                        { typeof(HubWindow), 10 },
                        { typeof(HubListWindow), 5 },
                        { typeof(JobWindow), 10 },
                        { typeof(JobQueueWindow), 5 },
                        { typeof(JobHistoryWindow), 5 },
                        { typeof(NoFlyZoneListWindow), 5 }
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
                        { typeof(DroneWindow), new Queue<IPoolable>() },
                        { typeof(DroneListWindow), new Queue<IPoolable>() },
                        { typeof(RetiredDroneWindow), new Queue<IPoolable>() },
                        { typeof(RetiredDroneListWindow), new Queue<IPoolable>() },
                        { typeof(HubWindow), new Queue<IPoolable>() },
                        { typeof(HubListWindow), new Queue<IPoolable>() },
                        { typeof(JobWindow), new Queue<IPoolable>() },
                        { typeof(JobQueueWindow), new Queue<IPoolable>() },
                        { typeof(JobHistoryWindow), new Queue<IPoolable>() },
                        { typeof(NoFlyZoneListWindow), new Queue<IPoolable>() }
                    };
                }

                return _Pool;
            }
        }

        public override Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        private const string WindowPrefabPath = "Prefabs/UI/Windows";
        /* Windows */
        private const string DroneWindowPath = WindowPrefabPath + "/Drone/Drone Window";
        private const string DroneListWindowPath = WindowPrefabPath + "/Drone/DroneList Window";
        private const string RetiredDroneWindowPath = WindowPrefabPath + "/Drone/RetiredDrone Window";
        private const string RetiredDroneListWindowPath = WindowPrefabPath + "/Drone/RetiredDroneList Window";
        private const string NavigationWindowPath = WindowPrefabPath + "/Navigation/Navigation Window";
        private const string HubWindowPath = WindowPrefabPath + "/Hub/Hub Window";
        private const string HubListWindowPath = WindowPrefabPath + "/Hub/HubList Window";
        private const string JobWindowPath = WindowPrefabPath + "/Job/Job Window";
        private const string JobHistoryWindowPath = WindowPrefabPath + "/Job/JobHistory Window";
        private const string JobQueueWindowPath = WindowPrefabPath + "/Job/JobQueue Window";
        private const string NoFlyZoneListWindowPath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneList Window";
        private const string ConsoleLogPath = WindowPrefabPath + "/Console/Console Log";
    }
}
