using System.Collections;using System.Collections.Generic;using UnityEngine;namespace Drones.Utils
{    // This isn't my code....
    public static class EdgeHelpers
    {

        public struct Edge
        {
            public Vector3 v1;
            public Vector3 v2;
            public int triangleIndex;
            public Edge(Vector3 aV1, Vector3 aV2, int aIndex)
            {
                v1 = aV1;
                v2 = aV2;
                triangleIndex = aIndex;
            }
        }

        public static List<Edge> GetEdges(int[] aIndices, Vector3[] vertices)
        {
            List<Edge> result = new List<Edge>();
            for (int i = 0; i < aIndices.Length; i += 3)
            {
                Vector3 v1 = vertices[aIndices[i]];
                Vector3 v2 = vertices[aIndices[i + 1]];
                Vector3 v3 = vertices[aIndices[i + 2]];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
            }
            return result;
        }

        public static List<Edge> FindBoundary(this List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = result.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    if ((Vector3.Magnitude(result[i].v1 - result[n].v2) < 0.1
                    && Vector3.Magnitude(result[i].v2 - result[n].v1) < 0.1) ||
                    (Vector3.Magnitude(result[i].v1 - result[n].v1) < 0.1
                    && Vector3.Magnitude(result[i].v2 - result[n].v2) < 0.1))
                    {
                        // shared edge so remove both
                        result.RemoveAt(n);
                        i--;
                        result.RemoveAt(i);
                        break;
                    }
                }
            }
            return result;
        }
        public static List<Edge> SortEdges(this List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = 0; i < result.Count - 2; i++)
            {
                Edge E = result[i];
                for (int n = i + 1; n < result.Count; n++)
                {
                    Edge a = result[n];
                    if (E.v2 == a.v1)
                    {
                        // in this case they are already in order so just continoue with the next one
                        if (n == i + 1)
                            break;
                        // if we found a match, swap them with the next one after "i"
                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }
            return result;
        }
        public static List<Edge> FindVertices(this List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = result.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                    {
                        // shared edge so remove both
                        result.RemoveAt(i);
                        result.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }
            return result;
        }
    }}