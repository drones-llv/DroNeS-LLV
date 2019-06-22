using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Data
{
    public class NfzData :IData
    {
        public static uint Count;
        public static void Reset() => Count = 0;
        public bool IsDataStatic { get; } = false;

        private readonly NoFlyZone _source;
        public uint droneEntryCount;
        public uint hubEntryCount;
        public uint UID { get; private set; }
        public Vector3 Position => _source.transform.position;
        public Vector3 Orientation => _source.transform.eulerAngles;
        public Vector3 Size => _source.transform.localScale;

        public NfzData(NoFlyZone src)
        {
            UID = ++Count;
            _source = src;
        }

    }

}