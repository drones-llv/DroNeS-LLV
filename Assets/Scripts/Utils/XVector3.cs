using System;

namespace Utils
{
    public struct XVector3
    {
        public const float EPSILON = 1e-6f;

        public float x;
        public float y;
        public float z;

        public XVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public XVector3(float[] array)
        {
            if (array.Length != 3)
            {
                throw new ArgumentException("Array size must be 3!");
            }
            x = array[0];
            y = array[1];
            z = array[2];
        }

        public float[] ToArray()
        {
            float[] val = { x, y, z };
            return val;
        }

        public static XVector3 operator +(XVector3 b, XVector3 c)
        {
            XVector3 a = new XVector3(b.x + c.x, b.y + c.y, b.z + c.z);
  
            return a;
        }

        public static XVector3 operator -(XVector3 b, XVector3 c)
        {
            XVector3 a = new XVector3(b.x - c.x, b.y - c.y, b.z - c.z);

            return a;
        }

        public static XVector3 operator *(XVector3 b, float multiplier) 
        {
            XVector3 a = new XVector3(b.x * multiplier, b.y * multiplier, b.z * multiplier);
            return a;
        }

        public static XVector3 operator *(float multiplier, XVector3 b)
        {
            XVector3 a = new XVector3(b.x * multiplier, b.y * multiplier, b.z * multiplier);
            return a;
        }

        public static XVector3 operator /(XVector3 b, float divisor)
        {
            XVector3 a = new XVector3(b.x / divisor, b.y / divisor, b.z / divisor);
            return a;
        }
        public static XVector3 operator -(XVector3 b)
        {
            XVector3 a = new XVector3(-b.x, -b.y, -b.z);
            return a;
        }

        public static bool operator ==(XVector3 b, XVector3 c)
        {
            return -float.Epsilon < Distance(b, c) && float.Epsilon > Distance(b, c);
        }

        public static bool operator !=(XVector3 b, XVector3 c)
        {
            return !(b == c);
        }

        public static float Dot(XVector3 a, XVector3 b)
        {
            float dot = 0;
            dot += a.x * b.x;
            dot += a.y * b.y;
            dot += a.z * b.z;
            return dot;
        }

        public static XVector3 Normalize(XVector3 a)
        {
            var o = new XVector3();
            return a / Distance(o, a);
        }

        public static float Distance(XVector3 o, XVector3 a)
        {
            XVector3 line = a - o;
            return (float)Math.Sqrt(line.x * line.x + line.y * line.y + line.z * line.z);
        }

        public XVector3 normalized => Normalize(new XVector3(this));

        public float magnitude => Distance(new XVector3(), this);

        public override bool Equals(object obj)
        {
            return obj is XVector3 && (XVector3)obj == this;
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'
        }

        public override string ToString()
        {
            string str = "(" + x.ToString() + ", ";
            str += y.ToString() + ", ";
            str += z.ToString() + ")";

            return str;
        }

        public static implicit operator float[](XVector3 rValue)
        {
            return new float[] { rValue.x, rValue.y, rValue.z };
        }
        public static implicit operator XVector3(float[] rValue)
        {
            if (rValue.Length != 3)
            {
                throw new ArgumentException("Array size must be 3!");
            }
            return new XVector3(rValue[0], rValue[1], rValue[2]);
        }
    }
}

