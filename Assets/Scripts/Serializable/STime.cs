using System.Collections.Generic;
using System;
namespace Drones.Serializable
{
    [Serializable]
    public class STime
    {
        public float sec;
        public int min;
        public int hr;
        public int day;
        public bool isReadOnly;
    }

}
