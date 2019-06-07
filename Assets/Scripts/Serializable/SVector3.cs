using UnityEngine;
using System;

namespace Drones.Serializable
{
    /// <summary>
    /// Since unity doesn't flag the Vector3 as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Vector3 and SVector2
    /// </summary>
    [Serializable]
    public struct SVector3
    {
        public const float EPSILON = 1e-6f;

        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public SVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SVector3(float[] array)
        {
            if (array.Length != 3)
            {
                throw new ArgumentException("Array size must be 3!");
            }
            x = array[0];
            y = array[1];
            z = array[2];
        }

        public SVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public float[] ToArray() => new float[] { x, y, z };
        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from SVector4 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector4(SVector3 rValue)
        {
            return new Vector4(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SVector3(Vector3 rValue)
        {
            return new SVector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector4 to SVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SVector3(Vector4 rValue)
        {
            return new SVector3(rValue.x, rValue.y, rValue.z);
        }
    }
}