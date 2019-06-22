using Utils;

namespace Drones.Data
{
    using Utils;

    public struct BatteryData
    {
        public const int DesignCycles = 500;
        public const float DesignCapacity = 576000f; // 576,000 Coulombs = 160,000 mAh
        public const float ChargeTarget = 0.98f;

        public BatteryData(Objects.Battery battery)
        {
            UID = battery.UID;
            drone = 0;
            hub = 0;
            status = BatteryStatus.Idle;
            totalDischarge = 0;
            totalCharge = 0;
            cycles = 0;
            charge = DesignCapacity;
            capacity = DesignCapacity;
            DeltaEnergy = 0;
        }

        public uint UID { get; set; }
        public uint drone;
        public uint hub;
        
        public float charge;
        public float capacity;
        public float totalCharge;
        public float totalDischarge;
        public int cycles;
        public BatteryStatus status;
        public float DeltaEnergy;
    }

}