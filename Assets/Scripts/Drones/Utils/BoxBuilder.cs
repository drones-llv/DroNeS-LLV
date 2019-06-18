using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Interfaces;

namespace Drones.Utils
{
    public class BoxBuilder : IClosestPoint
    {
        private BoxIdentifier boxID;
        private readonly Transform building;
        private Bounds bound;
        private readonly Collider buildingCollider;
        private static Mesh _CubeMesh;
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
        private static Mesh CubeMesh
        {
            get
            {
                if (_CubeMesh == null)
                {
                    _CubeMesh = Resources.Load("Meshes/AltCube") as Mesh;
                }
                return _CubeMesh;
            }
        }

        public static Vector3 PointToVector(XVector3 source)
        {
            return new Vector3(source.x, source.y, source.z);
        }

        public BoxBuilder(Transform building)
        {
            this.building = building;
            bound = building.GetComponent<MeshRenderer>().bounds;
            buildingCollider = building.GetComponent<Collider>();
            boxID = new BoxIdentifier(this);
        }

        public GameObject Build(Material material, Building type)
        {
            if (boxID.TooSmall) { return null; }
            GameObject box;
            Vector3 vertO = PointToVector(boxID.Start);
            Vector3 vertZ = PointToVector(boxID.Corner);
            vertO.y = bound.center.y;
            vertZ.y = bound.center.y;

            box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.localScale = new Vector3(Width(), Height(), Length());

            box.transform.position = vertO + new Vector3(Width() / 2, 0, Length() / 2);
            float angle = Vector3.Angle(Vector3.forward, (vertZ - vertO).normalized);
            angle = Mathf.Sign(Vector3.Dot(Vector3.right, vertZ - vertO)) < 0 ? -angle : angle;
            box.transform.RotateAround(vertO, Vector3.up, angle);
            MeshRenderer meshRenderer = box.GetComponent<MeshRenderer>();
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            box.GetComponent<MeshFilter>().sharedMesh = CubeMesh;
            box.transform.SetParent(Parent.transform);
            meshRenderer.sharedMaterial = material;
            if (type == Building.Tall)
            {
                box.name = building.name.Replace("Tall", "TLOD");
            }
            else
            {
                box.name = building.name.Replace("Short", "SLOD");
            }
            return box;
        }

        private float Height()
        {
            return bound.size.y;
        }

        private float Width()
        {
            return boxID.Width;
        }

        private float Length()
        {
            return boxID.Length;
        }

        #region IClosestPoint Implementations
        public float[] GetCentre()
        {
            float[] value = { bound.center.x, 0, bound.center.z };

            return value;
        }

        private static readonly Dictionary<Directions, Vector3> offsets = new Dictionary<Directions, Vector3>
        {
            {Directions.North, Vector3.forward},
            {Directions.East, Vector3.right},
            {Directions.South, -Vector3.forward},
            {Directions.West, Vector3.left},
            {Directions.Northeast, Vector3.forward + Vector3.right},
            {Directions.Northwest, Vector3.forward + Vector3.left},
            {Directions.Southeast, Vector3.back + Vector3.right},
            {Directions.Southwest, Vector3.back + Vector3.left},
            {Directions.Up, Vector3.up},
            {Directions.Down, Vector3.down}
        };

        public float[] GetClosestPoint(float[] outside, Directions dir)
        {
            Vector3 point = new Vector3(outside[0], outside[1], outside[2]);
            point = buildingCollider.ClosestPoint(point + 1000 * offsets[dir]);

            return point.ToArray();
        }

        public void Message(object msg)
        {
            Debug.Log(msg);
        }
        #endregion


    }

}