using UnityEngine;

namespace Drones.Utils.Router
{
    public class Obstacle
    {
        public Obstacle(Transform t, float excludedRadius = 0)
        {
            truePosition = t.position;
            position = t.position;
            size = t.localScale + (Vector3.forward + Vector3.right) * 2 * excludedRadius;
            orientation = t.eulerAngles;
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
        }

        public Obstacle(BoxCollider t, float excludedRadius = 0)
        {
            truePosition = t.transform.position;
            position = truePosition;
            size = t.size + (Vector3.forward + Vector3.right) * 2 * excludedRadius;
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

        public bool Contains(Vector3 p)
        {
            for (int i = 0; i < 4; i++)
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
}
