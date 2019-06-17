using UnityEngine;

namespace Drones.Scheduler
{
    public struct ChronoWrapper
    {
        const float Epsilon = 0.01f;
        int _day;
        int _hr;
        int _min;
        float _sec;

        public ChronoWrapper(int d, int h, int m, float s)
        {
            _day = d;
            _hr = h;
            _min = m;
            _sec = s;
        }

        public static bool operator <(ChronoWrapper t1, ChronoWrapper t2)
        {
            if (t1._day < t2._day)
            {
                return true;
            }

            if (t1._day != t2._day) return false;
            if (t1._hr < t2._hr)
            {
                return true;
            }

            if (t1._hr != t2._hr) return false;
            return t1._min < t2._min;
        }

        public static bool operator >(ChronoWrapper t1, ChronoWrapper t2)
        {
            if (t1._day > t2._day)
            {
                return true;
            }

            if (t1._day != t2._day) return false;
            if (t1._hr > t2._hr)
            {
                return true;
            }

            if (t1._hr != t2._hr) return false;
            
            return t1._min > t2._min;
        }

        public static bool operator ==(ChronoWrapper t1, ChronoWrapper t2)
        {
            return t1._day == t2._day && t1._hr == t2._hr && t1._min == t2._min && Mathf.Abs(t1._sec - t2._sec) < Epsilon;
        }

        public static bool operator >=(ChronoWrapper t1, ChronoWrapper t2)
        {
            return t1 > t2 || t1 == t2;
        }

        public static bool operator <=(ChronoWrapper t1, ChronoWrapper t2)
        {
            return t1 < t2 || t1 == t2;
        }

        public static bool operator !=(ChronoWrapper t1, ChronoWrapper t2)
        {
            return !(t1 == t2);
        }

        public static ChronoWrapper operator +(ChronoWrapper t1, float s)
        {
            return new ChronoWrapper(t1._day, t1._hr, t1._min, t1._sec + s);
        }

        public static ChronoWrapper operator -(ChronoWrapper t1, float s)
        {
            return new ChronoWrapper(t1._day, t1._hr, t1._min, t1._sec - s);
        }

        public static float operator -(ChronoWrapper t1, ChronoWrapper t2)
        {
            return (t1._day - t2._day) * 24f * 3600f + (t1._hr - t2._hr) * 3600 + (t1._min - t2._min) * 60 + (t1._sec - t1._sec);
        }

        public override bool Equals(object obj) => obj is ChronoWrapper && this == ((ChronoWrapper)obj);

        public override int GetHashCode() => base.GetHashCode();
    }
}

