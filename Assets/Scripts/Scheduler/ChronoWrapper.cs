using UnityEngine;
namespace Drones.Utils.Scheduler
{
    public struct ChronoWrapper
    {
        const float EPSILON = 0.01f;
        int day;
        int hr;
        int min;
        float sec;

        public ChronoWrapper(int d, int h, int m, float s)
        {
            day = d;
            hr = h;
            min = m;
            sec = s;
        }

        public static bool operator <(ChronoWrapper t1, ChronoWrapper t2)
        {
            if (t1.day < t2.day)
            {
                return true;
            }

            if (t1.day == t2.day)
            {
                if (t1.hr < t2.hr)
                {
                    return true;
                }
                if (t1.hr == t2.hr)
                {
                    if (t1.min < t2.min)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator >(ChronoWrapper t1, ChronoWrapper t2)
        {
            if (t1.day > t2.day)
            {
                return true;
            }

            if (t1.day == t2.day)
            {
                if (t1.hr > t2.hr)
                {
                    return true;
                }
                if (t1.hr == t2.hr)
                {
                    if (t1.min > t2.min)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool operator ==(ChronoWrapper t1, ChronoWrapper t2)
        {
            return t1.day == t2.day && t1.hr == t2.hr && t1.min == t2.min && Mathf.Abs(t1.sec - t2.sec) < EPSILON;
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
            return new ChronoWrapper(t1.day, t1.hr, t1.min, t1.sec + s);
        }

        public static ChronoWrapper operator -(ChronoWrapper t1, float s)
        {
            return new ChronoWrapper(t1.day, t1.hr, t1.min, t1.sec - s);
        }

        public static float operator -(ChronoWrapper t1, ChronoWrapper t2)
        {
            return (t1.day - t2.day) * 24f * 3600f + (t1.hr - t2.hr) * 3600 + (t1.min - t2.min) * 60 + (t1.sec - t1.sec);
        }

        public override bool Equals(object obj) => obj is ChronoWrapper && this == ((ChronoWrapper)obj);

        public override int GetHashCode() => base.GetHashCode();
    }
}

