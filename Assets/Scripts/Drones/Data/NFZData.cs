using Drones.Objects;
using Drones.Serializable;
using Drones.Utils.Interfaces;
using UnityEngine;

namespace Drones.Data
{
    public class NFZData :IData
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

        public NFZData(NoFlyZone src)
        {
            UID = ++Count;
            _source = src;
        }

        public NFZData(SNoFlyZone data, NoFlyZone src)
        {
            _source = src;
            UID = data.uid;
            Count = data.count;
            droneEntryCount = data.droneEntry;
            hubEntryCount = data.hubEntry;
            _source.transform.position = data.position;
            _source.transform.eulerAngles = data.orientation;
            _source.transform.localScale = data.size;
        }

    }

}