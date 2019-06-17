namespace Drones.Utils
{
    public static class Constants
    {
        public const float EPSILON = 1e-5f;
        private const float latitude = 40.764170691358686f;
        private const float longitude = -73.97670925665614f;
        public static float[] OriginCoordinates { get; } = { latitude, longitude };
        public const int mapboxZoom = 16;
        public const int LODLayer = 12;
        public const float R = 6378.137f; // Radius of earth in KM
        public const float CoroutineTimeSlice = 10; // ms

        public const float droneAcceleration = 2f;
        public const float droneHorizontalSpeed = 12f;
        public const float droneVerticalSpeed = 4f;
        public const float droneSeekForce = 10.0f;
        public const float droneApproachRadius = 20f;

        public const float droneLeftSensorRange = 10.0f;
        public const float droneRightSensorRange = 10.0f;
        public const float droneFrontSensorRange = 6.0f;
        public const float droneAvoidanceStrenth = 4.0f;


        public const float cruisingAltitude = 150f;
        public const float droneDescendLevel = 10f;
        public const float returnToHubAltitude = 510f;

        public const string mapStyle = "mapbox://styles/jw5514/cjr5l685g4u4z2sjxfdupnl8b";
        public const string buildingMaterialPath = "Materials/WhiteBuilding";
        public const string SimulationManagerPath = "Prefabs/Managers/Simulation Manager";
        public const string UIManagerPath = "Prefabs/Managers/UIManager";
        public const string PositionHighlightPath = "Prefabs/PositionHighlight";
        public const string HubHighlightPath = "Prefabs/HubHighlight";
        public const string ToolTipPath = "Prefabs/Windows/ToolTip";
        public const string WaypointPath = "Prefabs/Waypoint";
    }
}