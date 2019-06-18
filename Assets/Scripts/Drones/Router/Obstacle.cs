using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Drones.Router
{
    public class Obstacle
    {
        private static Dictionary<Collider, Obstacle> _accessor;
        public static Dictionary<Collider, Obstacle> Accessor => _accessor ?? (_accessor = new Dictionary<Collider, Obstacle>());

        public Obstacle(Transform t, float excludedRadius = 0)
        {
            var c = t.GetComponent<Collider>();
            var position1 = t.position;
            truePosition = position1;
            position = position1;
            size = t.localScale + (Vector3.forward + Vector3.right) * 2 * excludedRadius;
            var h = c.ClosestPointOnBounds(position + Vector3.up * 500).y;

            size.y = h;
            orientation = t.eulerAngles;
            position.y = 0;
            dx = RotationY(orientation.y) * Vector3.right * size.x / 2;
            dz = RotationY(orientation.y) * Vector3.forward * size.z / 2;
            diag = new Vector2(size.x, size.z).magnitude;
            normals = new Vector3[4];
            verts = new Vector3[4];
            vertices = new Vertex[4];
            normals[0] = dz.normalized;
            normals[1] = dx.normalized;
            normals[2] = -normals[0];
            normals[3] = -normals[1];

            verts[0] = position + dz + dx; // ij
            verts[1] = position - dz + dx; // jk
            verts[2] = position - dz - dx; // kl
            verts[3] = position + dz - dx; // li
            
            vertices[0] = new Vertex {index = 0, point = verts[0], owner = c.GetInstanceID()};
            vertices[1] = new Vertex {index = 1, point = verts[1], owner = c.GetInstanceID()};
            vertices[2] = new Vertex {index = 2, point = verts[2], owner = c.GetInstanceID()};
            vertices[3] = new Vertex {index = 3, point = verts[3], owner = c.GetInstanceID()};
            Accessor.Add(c, this);
        }

        public Obstacle(BoxCollider t, float excludedRadius = 0)
        {
            truePosition = t.transform.position;
            position = truePosition;
            size = t.size + (Vector3.forward + Vector3.right) * 2 * excludedRadius;
            var h = t.GetComponent<Collider>().ClosestPointOnBounds(position + Vector3.up * 500).y;
            size.y = h;
            orientation = t.transform.eulerAngles;
            position.y = 0;
            dx = RotationY(orientation.y) * Vector3.right * size.x / 2;
            dz = RotationY(orientation.y) * Vector3.forward * size.z / 2;
            diag = new Vector2(size.x, size.z).magnitude;
            normals = new Vector3[4];
            verts = new Vector3[4];
            normals[0] = dz.normalized;
            normals[1] = dx.normalized;
            normals[2] = -normals[0];
            normals[3] = -normals[1];

            verts[0] = position + dz + dx; // ij
            verts[1] = position - dz + dx; // jk
            verts[2] = position - dz - dx; // kl
            verts[3] = position + dz - dx; // li
            
            vertices[0] = new Vertex {index = 0, point = verts[0], owner = t.GetInstanceID()};
            vertices[1] = new Vertex {index = 1, point = verts[1], owner = t.GetInstanceID()};
            vertices[2] = new Vertex {index = 2, point = verts[2], owner = t.GetInstanceID()};
            vertices[3] = new Vertex {index = 3, point = verts[3], owner = t.GetInstanceID()};
            Accessor.Add(t, this);
        }

        public Vector3 truePosition;
        public Vector3 position;
        public Vector3 size;
        public Vector3 orientation;
        public float diag;
        public Vector3 dz;
        public Vector3 dx;
        public float mu;
        public Vector3[] normals;
        public Vector3[] verts;
        public Vertex[] vertices;

        public bool Contains(Vector3 p)
        {
            for (var i = 0; i < 4; i++)
            {
                if (Vector3.Dot(p - verts[i], normals[i]) > 0) return false;
            }
            return true;
        }

        public static Matrix4x4 RotationY(float theta)
        {
            theta *= Mathf.PI / 180;

            return new Matrix4x4(new Vector4(Mathf.Cos(theta), 0, -Mathf.Sin(theta), 0),
                new Vector4(0, 1, 0, 0), new Vector4(Mathf.Sin(theta), 0, Mathf.Cos(theta), 0), new Vector4(0, 0, 0, 1));
        }
    }

    public struct Vertex
    {
        public Vector3 point;
        public int index;
        public int owner;
        
        public override int GetHashCode()
        {
            return owner ^ index.GetHashCode() << 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vertex)) return false;
            var o = (Vertex) other;
            return o.owner == owner && o.index == index;
        }
        
        public static bool operator==(Vertex lhs, Vertex rhs)
        {
            return lhs.index == rhs.index && lhs.owner == rhs.owner;
        }

        public static bool operator !=(Vertex lhs, Vertex rhs)
        {
            return !(lhs == rhs);
        }
    }
}
