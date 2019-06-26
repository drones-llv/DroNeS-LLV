namespace Utils
{

    public enum Directions { North, East, West, South, Northeast, Northwest, Southeast, Southwest, Up, Down }
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
    public enum Scheduling { FCFS, LLV, EP }

    public enum SearchType { Face, Vertex }

    public enum WindowType
    {
        Navigation,
        Console,
        Drone,
        RetiredDrone,
        Hub,
        HubList,
        DroneList,
        RetiredDroneList,
        Job,
        JobHistory,
        JobQueue,
        NFZList,
        Dashboard,
        SaveLoad,
        Overwrite,
        Null
    }

    public enum ListElement
    {
        Console,
        DroneList,
        RetiredDroneList,
        HubList,
        JobHistory,
        JobQueue,
        NFZList,
        SaveLoad
    }

    public enum EventType
    {
        EnteredNoFlyZone,
        Collision,
        POIMarked,
        DroneRetired,
        OutofBounds,
        BatteryLost,
        DroneContactLoss,
        DebugLog,
        CustomJob
    }

    public enum ComparisonType { Max, Min };

    public enum PositionHighlight { Normal, Waypoint };

    public enum JobStatus { Assigning, Pickup, Delivering, Complete, Failed };

    public enum SortOrder { Ascending, Descending };

    public enum TimeSpeed { Normal, Fast, Slow, Ultra, WTF, Pause };

    public enum FractionFormat { Decimal, Percentage };

    public enum DroneMovement { Hover, Ascend, Descend, Horizontal, Idle, Drop };

    public enum FlightStatus { Idle, PreparingHeight, AwaitingWaypoint, Cruising };

    public enum BatteryStatus { Idle, Discharge, Charge, Dead };

    public enum SimulationStatus { EditMode, Running, Paused, Stopped };

    public enum DashboardMode { Simulation, EditMode };

    public enum FunctionType { Heaviside, Polynomial, Tanh, Exp }

    public enum Length { m, km, mi, yd, ft, inch };

    public enum Mass { kg, g, mt, lt, sht, lb, oz };

    public enum Energy { J, kWh, Wh, BTU };

    public enum Power { W, kW, hpe };

    public enum Area { sqm, sqmi, sqyd, sqft, sqin };

    public enum Chronos { s, min, h, day };

    public enum Force { N, kgf, lbf };

    public enum Current { A, mA };

    public enum Charge { C, mC, Ah, mAh };

    public enum Voltage { V, mV };

    public enum Platform
    {
        Windows,
        Linux,
        Mac
    }

    public enum Pricing
    {
        US,
        UK
    }

    public enum Currency
    {
        USD,
        GBP
    }

    public enum SimulationMode
    {
        Emergency,
        Delivery
    }
}
