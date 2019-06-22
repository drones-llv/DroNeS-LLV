using System.Collections.Generic;
using Drones;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class Extensions
    {
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }

        public static RectTransform ToRect(this Transform transform)
        {
            return (RectTransform)transform;
        }

        public static float[] ToArray(this Vector3 vector)
        {
            float[] value = { vector.x, vector.y, vector.z };
            return value;
        }

        public static XVector3 ToPoint(this Vector3 source)
        {
            return new XVector3(source.x, source.y, source.z);
        }

        public static Vector2 ToCoordinates(this Vector3 v)
        {
            var c = Singletons.Manhattan.WorldToGeoPosition(v);

            return new Vector2((float)c.x, (float)c.y);
        }

        public static Vector3 ToUnity(this Vector2d v)
        {
            return Singletons.Manhattan.GeoToWorldPosition(v);
        }

        public static Vector3 ToUnity(this Vector2 v)
        {
            return Singletons.Manhattan.GeoToWorldPosition(new Vector2d(v.x, v.y));
        }

        public static Transform FindChildWithTag(this Transform t, string tag)
        {
            foreach (Transform child in t)
            {
                if (child.CompareTag(tag)) 
                { 
                    return child; 
                }

                if (child.childCount <= 0) continue;
                var grandchild = FindChildWithTag(child, tag);
                if (grandchild != null)
                {
                    return grandchild;
                }
            }
            return null;
        }

        public static Transform FindDescendant(this Transform t, string n, int depth = 0)
        {
            Transform output = null;
            foreach (Transform child in t)
            {
                if (child.name == n) return child;
                if (depth > 0)
                {
                    output = child.FindDescendant(n, depth - 1);
                }
                if (output != null) return output;
            }

            return null;
        }

        public static string ToStringXZ(this Vector3 v)
        {
            return v.x.ToString("0.0") + ", " + v.z.ToString("0.0");
        }

        public static string ToStringXYZ(this Vector3 v)
        {
            return v.x.ToString("0.0") + ", " + v.y.ToString("0.0") + ", " + v.z.ToString("0.0");
        }

        public static Vector4 ToDir(this Vector4 v)
        {
            v.w = 0;
            return v;
        }

        public static Vector4 ToPos(this Vector4 v)
        {
            v.w = 1;
            return v;
        }

        public static Vector2 SwapAxes(this Vector2 v)
        {
            var tmp = v.x;
            v.x = v.y;
            v.y = tmp;

            return v;
        }

        public static bool IsEmpty<T>(this Queue<T> q) => q.Count == 0;


    }
}

