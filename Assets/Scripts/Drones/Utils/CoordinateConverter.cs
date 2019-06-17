using Mapbox.Utils;
using UnityEngine;
using Constants = Utils.Constants;

namespace Drones.Utils
{
    using static Singletons;
    public static class CoordinateConverter
    {
        public static float CoordDistance(Vector2 a, Vector2 b)
        {
            var PIdeg = Mathf.PI / 180;
            var diff = (b - a) * PIdeg;
            var tmp = Mathf.Sin(diff.x / 2) * Mathf.Sin(diff.x / 2) +
            Mathf.Cos(a.x * PIdeg) * Mathf.Cos(b.x * PIdeg) *
            Mathf.Sin(diff.y / 2) * Mathf.Sin(diff.y / 2);

            var c = 2 * Mathf.Atan2(Mathf.Sqrt(tmp), Mathf.Sqrt(1 - tmp));
            var d = Constants.R * c;
            return d * 1000; // meters
        }

        public static string ToString(Vector2 a)
        {
            return "(" + a.x.ToString("0.0000") + ", " + a.y.ToString("0.0000") + ")";
        }

        public static string ToString(Vector3 a)
        {
            return "{" + a.x.ToString("0.0000") + ", " + a.y.ToString("0.0000") + ", " + a.z.ToString("0.0000") + "}";
        }

        public static Vector2 UnityToCoord(Vector3 v)
        {
            Vector2d c = Manhattan.WorldToGeoPosition(v);

            return new Vector2((float)c.x, (float)c.y);
        }

    }
}
