using System;
using UnityEngine;

namespace Drones.Serializable
{
    [Serializable]
    public class StaticObstacle
    {
        public StaticObstacle(Transform t)
        {
            position = t.position;
            size = t.localScale;
            orientation = t.eulerAngles;
            position.y = 0;
            dx = RotationY(orientation.y) * Vector3.right * size.x / 2;
            dz = RotationY(orientation.y) * Vector3.forward * size.z / 2;
            diag = new Vector2(size.x, size.z).magnitude;
            normals = new SVector3[4];
            verts = new SVector3[4];
            normals[0] = ((Vector3)dz).normalized;
            normals[1] = ((Vector3)dx).normalized;
            normals[2] = -(Vector3)normals[0];
            normals[3] = -(Vector3)normals[1];

            verts[0] = (Vector3)position + dz + dx; // ij
            verts[1] = (Vector3)position - dz + dx; // jk
            verts[2] = (Vector3)position - dz - dx; // kl
            verts[3] = (Vector3)position + dz - dx; // li

        }

        public SVector3 position;
        public SVector3 size;
        public SVector3 orientation;
        public float diag;
        public SVector3 dz;
        public SVector3 dx;
        public float mu;
        public SVector3[] normals;
        public SVector3[] verts;

        public static Matrix4x4 RotationY(float theta)
        {
            theta *= Mathf.PI / 180;

            return new Matrix4x4(new Vector4(Mathf.Cos(theta), 0, -Mathf.Sin(theta), 0),
                new Vector4(0, 1, 0, 0), new Vector4(Mathf.Sin(theta), 0, Mathf.Cos(theta), 0), new Vector4(0, 0, 0, 1));
        }
    }
}

