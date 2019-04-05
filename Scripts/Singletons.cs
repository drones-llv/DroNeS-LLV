﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Mapbox.Unity.Map;

namespace Drones
{
    using DataStreamer;
    using EventSystem;
    using Utils;
    using UI;
    using static Utils.Constants;

    public static class Singletons
    {
        private static GameObject _Sun;
        private static PostProcessingProfile _PostProcessing;
        private static AbstractMap _Manhattan;
        private static AbstractMap _Brooklyn;
        private static GameObject _Boundary;
        private static Transform _CameraContainer;
        private static Transform _CamTrans;
        private static RTSCameraComponent _CameraControl;
        private static Camera _MinimapCamera;
        private static GameObject _Console;
        private static Mesh _CubeMesh;
        private static GameObject _PositionHighlightTemplate;
        private static Transform _UICanvas;
        private static EventSystem<EventType, IEvent> _SimulationEvent;
        private static EventSystem<System.Type, IDataSource> _DataStreamer;
        private static ObjectPool _UIPool;
        private static TimeKeeper _Clock;

        public static TimeKeeper Clock
        {
            get
            {
                if (_Clock == null)
                {
                    _Clock = Sun.GetComponent<TimeKeeper>();
                }
                return _Clock;
            }
        }

        public static ObjectPool UIPool
        {
            get
            {
                if (_UIPool == null)
                {
                    _UIPool = GameObject.FindWithTag("UIObjectPool").GetComponent<ObjectPool>();
                }
                return _UIPool;
            }
        }

        public static GameObject CurrentPosition { get; set; } = null;

        public static EventSystem<EventType, IEvent> SimulationEvent
        {
            get
            {
                if (_SimulationEvent == null)
                {
                    _SimulationEvent = new EventSystem<EventType, IEvent>();
                }
                return _SimulationEvent;
            }
        }

        public static EventSystem<System.Type, IDataSource> DataStreamer
        {
            get
            {
                if (_DataStreamer == null)
                {
                    _DataStreamer = new EventSystem<System.Type, IDataSource>();
                }
                return _DataStreamer;
            }
        }

        public static GameObject Sun 
        {
            get
            {
                if (_Sun == null)
                {
                    _Sun = GameObject.Find("Sun");
                }
                return _Sun;
            }
        }

        public static PostProcessingProfile PostProcessing
        {
            get
            {
                if (_PostProcessing == null)
                {
                    _PostProcessing = Resources.Load("PostProcessing/CityLights") as PostProcessingProfile;
                }
                return _PostProcessing;
            }
        }

        public static AbstractMap Manhattan
        {
            get
            {
                if (_Manhattan == null)
                {
                    _Manhattan = GameObject.FindWithTag("Manhattan").GetComponent<AbstractMap>();
                }
                return _Manhattan;
            }
        }

        public static AbstractMap Brooklyn
        {
            get
            {
                if (_Brooklyn == null)
                {
                    _Brooklyn = GameObject.FindWithTag("Brooklyn").GetComponent<AbstractMap>();
                }
                return _Brooklyn;
            }
        }

        public static GameObject Boundary
        {
            get
            {
                if (_Boundary == null)
                {
                    _Boundary = GameObject.FindWithTag("Boundary");
                }
                return _Boundary;
            }
        }

        public static Transform RTSCameraContainer
        {
            get
            {
                if (_CameraContainer == null)
                {
                    _CameraContainer = GameObject.FindWithTag("MainCamera").transform;
                }
                return _CameraContainer;
            }
        }

        public static Transform CamTrans
        {
            get
            {
                if (_CamTrans == null)
                {
                    _CamTrans = RTSCameraContainer.transform.GetChild(0);
                }
                return _CameraContainer;
            }
        }

        public static RTSCameraComponent CameraControl
        {
            get
            {
                if (_CameraControl == null)
                {
                    _CameraControl = RTSCameraContainer.GetComponent<RTSCameraComponent>();
                }
                return _CameraControl;
            }
        }

        public static Camera MinimapCamera
        {
            get
            {
                if (_MinimapCamera == null)
                {
                    _MinimapCamera = GameObject.FindWithTag("Minimap").GetComponent<Camera>();
                }
                return _MinimapCamera;
            }
        }

        public static GameObject Console
        {
            get
            {
                if (_Console == null)
                {
                    _Console = Object.FindObjectOfType<ConsoleLog>().gameObject;
                }
                return _Console;
            }
        }

        public static Mesh CubeMesh
        {
            get
            {
                if (_CubeMesh == null)
                {
                    _CubeMesh = Resources.Load("Meshes/AltCube") as Mesh;
                }
                return _CubeMesh;
            }
        }

        public static Color ListItemOdd { get; } = new Color
        {
            r = 180f / 255f,
            b = 180f / 255f,
            g = 180f / 255f,
            a = 1
        };

        public static Color ListItemEven { get; } = new Color
        {
            r = 223f / 255f,
            b = 223f / 255f,
            g = 223f / 255f,
            a = 1
        };


        public static GameObject PositionHighlightTemplate
        {
            get
            {
                if (_PositionHighlightTemplate == null)
                {
                    _PositionHighlightTemplate = (GameObject) Resources.Load(PositionHighlightPath);
                }
                return _PositionHighlightTemplate;
            }
        }

        public static Transform UICanvas
        {
            get
            {
                if (_UICanvas == null)
                {
                    _UICanvas = GameObject.Find("UICanvas").transform;
                }
                return _UICanvas;
            }
        }

    }
}