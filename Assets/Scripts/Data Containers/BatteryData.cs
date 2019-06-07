namespace Drones.Data
{
    using Utils;
    using Serializable;

    public class BatteryData
    {
        public static uint Count { get; private set; }
        public static void Reset() => Count = 0;
        public static int designCycles = 500;
        public static float designCapacity = 576000f; // 576,000 Coulombs = 160,000 mAh
        public static float chargeTarget = 0.99f;

        public BatteryData()
        {
            UID = ++Count;
            drone = 0;
            hub = 0;

            status = BatteryStatus.Idle;
            totalDischarge = 0;
            totalCharge = 0;
            cycles = 0;
            charge = designCapacity;
            capacity = designCapacity;
            chargeRate = 0.5f * designCapacity;
            dischargeVoltage = 23f;
            chargeVoltage = 3.7f;
        }

        public BatteryData(SBattery data)
        {
            Count = data.count;
            UID = data.uid;
            drone = data.drone;
            hub = data.hub;
            charge = data.charge;
            capacity = data.capacity;
            totalCharge = data.totalCharge;
            totalDischarge = data.totalDischarge;
            cycles = data.cycles;
            status = data.status;
            chargeRate = 0.5f * designCapacity;
            dischargeVoltage = 23f;
            chargeVoltage = 3.7f;
        }

        public uint UID { get; set; }
        public uint drone;
        public uint hub;

        public BatteryStatus status;
        public float totalDischarge;
        public float totalCharge;
        public float charge;
        public float capacity;
        public int cycles;
        public float chargeRate;
        public float dischargeVoltage;
        public float chargeVoltage;
    }

}