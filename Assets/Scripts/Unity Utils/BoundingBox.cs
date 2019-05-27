using System.Collections.Generic;
using System.Linq;
using Drones.Utils.Router;
using UnityEngine;

namespace Drones.Utils
{
    public class BoundingBox
    {
        Vector3 _centre;
        Vector3[] _hull;
        private readonly Transform building;
        private Bounds bound;
        private readonly Collider buildingCollider;

        private static GameObject _parent;
        private static GameObject Parent
        {
            get
            {
                if (_parent == null)
                {
                    _parent = new GameObject();
                }
                return _parent;
            }
        }
        public BoundingBox(Transform building)
        {
            this.building = building;
            bound = building.GetComponent<MeshRenderer>().bounds;
            buildingCollider = building.GetComponent<Collider>();
            _centre = bound.center;
            _centre.y = 0;
            _hull = new Vector3[32];
            for (int i = 0; i < _hull.Length; i++)
            {
                Vector3 dir = Obstacle.RotationY(i * 360f / _hull.Length) * Vector3.forward;
                buildingCollider.Raycast(new Ray(_centre + 1000 * dir, -dir), out RaycastHit info, 1000);
                _hull[i] = info.point;

            }
        }

        public GameObject Build(Material material, Building type)
        {
            //return new GameObject();
            Rectangle minBox = null;
            var minAngle = 0f;
            for (int i = 0; i < _hull.Length; i++)
            {
                var cur = _hull[i];
                var nxt = _hull[(i + 1) % _hull.Length];

                var top = float.MinValue;
                var btm = float.MaxValue;
                var lef = float.MaxValue;
                var rit = float.MinValue;

                var angle = AngleToX(cur, nxt);
                foreach (var p in _hull)
                {
                    Vector3 rotated = Obstacle.RotationY(angle) * p;

                    top = Mathf.Max(top, rotated.z);
                    btm = Mathf.Min(btm, rotated.z);
                    lef = Mathf.Min(lef, rotated.x);
                    rit = Mathf.Max(rit, rotated.x);
                }

                var box = new Rectangle(new Vector3(lef, 0, btm), new Vector3(rit, 0, top));

                if (minBox == null || minBox.Area > box.Area)
                {
                    minBox = box;
                    minAngle = angle;
                }
            }

            var xyz = minBox.Points.Select(p => Obstacle.RotationY(-minAngle) * p).ToArray();

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 v;
            v.x = Vector3.Distance(xyz[1], xyz[2]);
            v.y = bound.size.y;
            v.z = Vector3.Distance(xyz[1], xyz[0]);
            cube.transform.localScale = v;

            Vector3 vertO = xyz[0];
            Vector3 vertx = xyz[1];
            cube.transform.position = FindCentre(xyz);

            cube.transform.position += v.y / 2 * Vector3.up;
            float ang = Vector3.Angle(Vector3.right, (vertx - vertO).normalized) - 90;
            ang = Mathf.Sign(Vector3.Dot(Vector3.forward, vertx - vertO)) < 0 ? ang : -ang;
            cube.transform.RotateAround(cube.transform.position, Vector3.up, ang);
            MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.receiveShadows = false;

            cube.transform.SetParent(Parent.transform);
            meshRenderer.sharedMaterial = material;
            if (type == Building.Tall)
            {
                cube.name = building.name.Replace("Tall", "TLOD");
            }
            else
            {
                cube.name = building.name.Replace("Short", "SLOD");
            }

            return cube;
        }

        float AngleToX(Vector3 a, Vector3 b)
        {
            var d = a - b;
            return -Mathf.Atan2(d.z, d.x) * Mathf.Rad2Deg;
        }

        private Vector3 FindCentre(Vector4[] Verts)
        {
            Vector3 o = Verts[0];
            Vector3 d = Vector3.Normalize((Vector3)Verts[2] - o);
            Vector3 p = Verts[1];
            Vector3 e = Vector3.Normalize((Vector3)Verts[3] - p);

            float a = o.z - p.z;
            float b = d.z / d.x * (p.x - o.x);
            float c = d.z / d.x * e.x;

            float nu = (a + b) / (e.z - c);

            return p + nu * e;

        }
    }

    public class Rectangle
    {
        public Vector3 Location { get; set; }
        public Vector3 Size { get; set; }
        public Rectangle(Vector3 a, Vector3 b)
        {
            Location = a;
            Size = b - a;
        }

        public float Area => Size.x * Size.z;

        public Vector3[] Points => new Vector3[]
        {
            Location,
            Location + Size.x * Vector3.right,
            Location + Size.x * Vector3.right + Size.z * Vector3.forward,
            Location + Size.z * Vector3.forward,
        };
    }


}
