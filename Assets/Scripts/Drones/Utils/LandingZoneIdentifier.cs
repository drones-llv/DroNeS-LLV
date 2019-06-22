using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    public static class LandingZoneIdentifier
    {
        private static Texture2D _bitmap;
        private static Texture2D Bitmap
        {
            get
            {
                if (_bitmap == null)
                {
                    _bitmap = Resources.Load("Textures/bitmap") as Texture2D;
                }
                return _bitmap;
            }
        }

        private static int Width => Bitmap.width;
        private static int Height => Bitmap.height;

        private static bool OutOfRange(Vector3 area)
        {
            return Mathf.Abs(area.x) > Width * 2 || Mathf.Abs(area.y) > Width * 2;
        }

        private static bool OutOfRange(Vector2Int pixel)
        {
            return pixel.x < 0 || pixel.y < 0 || pixel.x > Width || pixel.y > Height;
        }

        private static bool IsClear(Vector3 area)
        {
            var u = ToPixel(area);
            if (OutOfRange(u)) return false;
            var col = Bitmap.GetPixel(u.x, u.y);

            var v = area;
            v.y = 600;

            return col != Color.black && !(Physics.Raycast(new Ray(v, Vector3.down), out RaycastHit info, 600, 1 << 14) 
                && info.transform.CompareTag("NoFlyZone"));
        }

        private static Vector2Int ToPixel(Vector3 area)
        {
            var v = new Vector2Int(((int)area.x + Width * 2) / 4, ((int)area.z + Height * 2) / 4);

            return v;
        }

        private static Vector3 ToWorld(Vector2Int pixel, Vector3 area = default)
        {
            return new Vector3(pixel.x * 4 - Width * 2, area.y, pixel.y * 4 - Height * 2);
        }

        public static Vector3 Reposition(Vector3 position)
        {
            var cache = position;
            var v = Random.insideUnitSphere;
            v.y = 0;
            Vector3.Normalize(v);
            v *= 4;
            var i = 0;
            while (!IsClear(position))
            {
                position += v;
                if (OutOfRange(position))
                {
                    v = Random.insideUnitSphere;
                    v.y = 0;
                    Vector3.Normalize(v);
                    v *= 4;
                    if (i == 0)
                    {
                        position = Random.insideUnitSphere * 2000;
                        position.y = cache.y;
                    }
                    else
                    {
                        position = cache;
                    }
                }
                i++;
            }
            position = ToWorld(ToPixel(position), position);
            v /= 4;
            v *= 0.2f;
            position += v;
            position.y = 0;
            return position;
        }

    }
}
