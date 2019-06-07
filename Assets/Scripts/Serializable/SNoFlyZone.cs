using System.Collections;
using System.Collections.Generic;
using System;

namespace Drones.Serializable
{
    using Data;
    [Serializable]
    public class SNoFlyZone
    {
        public uint uid;
        public uint count;
        public uint droneEntry;
        public uint hubEntry;
        public SVector3 position;
        public SVector3 orientation;
        public SVector3 size;

        public SNoFlyZone(NFZData data)
        {
            uid = data.UID;
            count = NFZData.Count;
            droneEntry = data.droneEntryCount;
            hubEntry = data.hubEntryCount;
            position = data.Position;
            orientation = data.Orientation;
            size = data.Size;
        }

    }
}
