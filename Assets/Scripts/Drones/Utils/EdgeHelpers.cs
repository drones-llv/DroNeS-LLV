using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Drones.Utils
{
    // This isn't my code....
    public static class EdgeHelpers
    {

        public struct Edge
        {
            public Vector3 V1;
            public Vector3 V2;
            private int _triangleIndex;
            public Edge(Vector3 aV1, Vector3 aV2, int aIndex)
            {
                V1 = aV1;
                V2 = aV2;
                _triangleIndex = aIndex;
            }
        }

        public static List<Edge> GetEdges(int[] aIndices, Vector3[] vertices)
        {
            var result = new List<Edge>();
            for (var i = 0; i < aIndices.Length; i += 3)
            {
                var v1 = vertices[aIndices[i]];
                var v2 = vertices[aIndices[i + 1]];
                var v3 = vertices[aIndices[i + 2]];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
            }
            return result;
        }

        public static List<Edge> FindBoundary(this List<Edge> aEdges)
        {
            var result = new List<Edge>(aEdges);
            for (var i = result.Count - 1; i > 0; i--)
            {
                for (var n = i - 1; n >= 0; n--)
                {
                    if ((!(Vector3.Magnitude(result[i].V1 - result[n].V2) < 0.1) ||
                         !(Vector3.Magnitude(result[i].V2 - result[n].V1) < 0.1)) &&
                        (!(Vector3.Magnitude(result[i].V1 - result[n].V1) < 0.1) ||
                         !(Vector3.Magnitude(result[i].V2 - result[n].V2) < 0.1))) continue;
                    // shared edge so remove both
                    result.RemoveAt(n);
                    i--;
                    result.RemoveAt(i);
                    break;
                }
            }
            return result;
        }
        public static List<Edge> SortEdges(this List<Edge> aEdges)
        {
            var result = new List<Edge>(aEdges);
            for (var i = 0; i < result.Count - 2; i++)
            {
                var e = result[i];
                for (var n = i + 1; n < result.Count; n++)
                {
                    var a = result[n];
                    if (e.V2 != a.V1) continue;
                    // in this case they are already in order so just continue with the next one
                    if (n == i + 1)
                        break;
                    // if we found a match, swap them with the next one after "i"
                    result[n] = result[i + 1];
                    result[i + 1] = a;
                    break;
                }
            }
            return result;
        }
        public static List<Edge> FindVertices(this List<Edge> aEdges)
        {
            var result = new List<Edge>(aEdges);
            for (var i = result.Count - 1; i > 0; i--)
            {
                for (var n = i - 1; n >= 0; n--)
                {
                    if (result[i].V1 != result[n].V2 || result[i].V2 != result[n].V1) continue;
                    // shared edge so remove both
                    result.RemoveAt(i);
                    result.RemoveAt(n);
                    i--;
                    break;
                }
            }
            return result;
        }
    }
}

