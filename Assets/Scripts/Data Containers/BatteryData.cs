namespace Drones.Data
{
    using Utils;
    using Serializable;

    public class BatteryData : IData
    {
        public static uint Count { get; private set; }
        public static void Reset() => Count = 0;
        public static int designCycles = 500;
        public static float designCapacity = 576000f; // 576,000 Coulombs = 160,000 mAh
        public static float chargeTarget = 1;

        public float chargeRate = 0.5f * designCapacity;
        public float dischargeVoltage = 23f;
        public float chargeVoltage = 3.7f;

        public BatteryData() 
        {
            UID = ++Count;
            status = BatteryStatus.Idle;
            charge = designCapacity;
            capacity = designCapacity;
        }

        public BatteryData(SBattery data)
        {
            Count = data.count;
            UID = data.uid;
            drone = data.drone;
            hub = data.hub;
            charge = data.charge;
            capacity = data.capacity;

        }

        public uint UID { get; }
        public bool IsDataStatic { get; set; } = false;
        public uint drone;
        public uint hub;

        public BatteryStatus status;

        public float totalDischarge;

        public float totalCharge;

        public float charge;

        public float capacity;

        public int cycles;
    }

}