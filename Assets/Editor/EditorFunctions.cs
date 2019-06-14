using static Drones.LoadingTools.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Drones.Utils;
using Drones;
using System.Collections.Generic;
using System.IO;
using System;
using Drones.Serializable;
using Drones.Managers;
using Mapbox.Unity.Map.TileProviders;
using Drones.Utils.Router;
public class EditorFunctions : EditorWindow
{
    AbstractMap abstractMap;
    GameObject citySimulatorMap;
    public float minHeight;
    public float maxHeight;

    [MenuItem("Window/Editor Functions")]
    static void Init()
    {
        EditorFunctions sizeWindow = CreateInstance<EditorFunctions>();
        sizeWindow.autoRepaintOnSceneChange = true;
        sizeWindow.Show();
    }

    void OnGUI()
    {
        if (citySimulatorMap == null) { citySimulatorMap = GameObject.Find("Manhattan"); }
        if (abstractMap == null) { abstractMap = citySimulatorMap?.GetComponent<CustomMap>(); }

        minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
        maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);

        if (GUILayout.Button("Edit Mode Build")) 
        {
            //if (abstractMap.MapVisualizer != null) { abstractMap.ResetMap(); }
            abstractMap.MapVisualizer = CreateInstance<MapVisualizer>();
            abstractMap.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
            //abstractMap.Initialize(new Mapbox.Utils.Vector2d(-29.3151, 27.4869), 16);
        }

        if (GUILayout.Button("1. Setup Objects"))
        {
            RenameRoads(citySimulatorMap.transform);
            OptimizeRenderer(citySimulatorMap.transform);
            foreach(Transform tile in citySimulatorMap.transform)
            {
                SortChildrenByName(tile);
            }
            GroupAllByBlocks(citySimulatorMap.transform);
            SplitAllBlocks(citySimulatorMap.transform);
            SortHeirarchy(citySimulatorMap.transform);
            DestroyImmediate(citySimulatorMap.GetComponent<RangeTileProvider>());
        }

        if (GUILayout.Button("1.5. Combine"))
        {
            Transform road = null;
            foreach (Transform tile in citySimulatorMap.transform)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform current = tile.GetChild(i);
                    if (current.name.Substring(0, 4) == "Road")
                    {
                        road = current;
                        continue;
                    }
                    CombineParentAndChildrenMeshes(current, !EditorApplication.isPlaying);
                }
                if (road != null && EditorApplication.isPlaying) { Destroy(road.gameObject); }
                else if (road != null && !EditorApplication.isPlaying) { DestroyImmediate(road.gameObject); }
            }
        }

        if (GUILayout.Button("2. Box buildings"))
        {
            Material material = Resources.Load("Materials/WhiteLOD") as Material;

            foreach (Transform tile in citySimulatorMap.transform)
            {
                foreach (Transform building in tile)
                {
                    if (building.childCount > 0) { continue; }
                    if (IsTall(building))
                    {
                        new BoundingBox(building).Build(material, Building.Tall);
                    }
                    else
                    {
                        new BoundingBox(building).Build(material, Building.Short);
                    }
                    //DestroyImmediate(building.GetComponent<MeshCollider>());
                }
            }
        }

        if (GUILayout.Button("3. Combine Mesh in Tiles"))
        {
            foreach (Transform tile in citySimulatorMap.transform)
            {
                GroupByTile(tile);
            }
        }

        if (GUILayout.Button("Count Buildings"))
        {
            int c = 0;
            foreach (Transform tile in citySimulatorMap.transform)
            {
                foreach (Transform building in tile)
                {
                    c++;
                }
            }
            Debug.Log(c);
        }

        if (GUILayout.Button("HEIGHT!"))
        {
            GenerateHeightBitMapNY();
        }



    }

    void FindTallest()
    {
        float c = 0;
        foreach (Transform tile in citySimulatorMap.transform)
        {
            foreach (Transform building in tile)
            {
                var d = building.GetComponent<MeshRenderer>().bounds.size.y;
                if (c < d)
                {
                    c = d;
                }

            }
        }
        Debug.Log(c);
    }

    [Serializable]
    public class Buildings
    {
        public static int jb = 0;
        public List<StaticObstacle> buildings;
    }
    [Serializable]
    public class Payload
    {
        public List<StaticObstacle> Buildings;
        public List<StaticObstacle> NFZs;
    }

    public static void GenerateHeightBitMap(int x, int y, int metre)
    {
        Texture2D data = new Texture2D(x, y);
        try
        {
            float tallest = 40;
            for (int i = 0; i < x * 2; i += metre)
            {
                for (int j = 0; j < y * 2; j += metre)
                {
                    var v = new Vector3(i - x, 1000, j - y);
                    var c = Color.black;
                    if (Physics.BoxCast(v, new Vector3(metre/2f, 1, metre/2f), Vector3.down, out RaycastHit info, Quaternion.identity, 1000, 1 << 12))
                    {
                        c += Color.white * info.point.y / tallest;
                        c.a = 1;
                    }
                    else if (!Physics.BoxCast(v, new Vector3(metre / 2f, 1, metre / 2f), Vector3.down, Quaternion.identity, 1001, 1 << 13))
                    {
                        c = Color.white;
                    }
                    data.SetPixel(i / metre, j / metre, c);
                }
            }
            var path = Path.Combine(SaveManager.ExportPath, "height_bitmap.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }


    public static void GenerateHeightBitMapNY()
    {
        Texture2D data = new Texture2D(4*2160,4*3750);
        try
        {
            float tallest = 500;
            for (int i = 0; i < 4320 * 2; i += 4/4)
            {
                for (int j = 0; j < 7500 * 2; j += 4/4)
                {
                    var v = new Vector3(i - 4320, 1000, j - 7500);
                    var c = Color.black;
                    if (Physics.BoxCast(v, new Vector3(2/4f, 1, 2/4f), Vector3.down, out RaycastHit info, Quaternion.identity, 1000, 1 << 12))
                    {
                        c += Color.white * Mathf.Clamp(info.point.y / tallest, 0, 1);
                        c.a = 1;
                    }
                    else if (!Physics.BoxCast(v, new Vector3(2/4f, 1, 2/4f), Vector3.down, Quaternion.identity, 1000, 1 << 13))
                    {
                        c = Color.white;
                    }
                    //data.SetPixel(i / 4, j / 4, c);
                    data.SetPixel(i, j, c);
                }
            }
            var path = Path.Combine(SaveManager.SavePath, "height_bitmap.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    public static void GenerateOccupancyBitMap()
    {
        Collider[] cols;
        Texture2D data = new Texture2D(2160, 3750);
        try
        {
            for (int i = 0; i < 4320 * 2; i += 4)
            {
                for (int j = 0; j < 7500 * 2; j += 4)
                {
                    var v = new Vector3(i - 4320, 300, j - 7500);
                    cols = Physics.OverlapBox(v, new Vector3(2, 300, 2), Quaternion.identity, 1 << 12);
                    data.SetPixel(i / 4, j / 4, (cols.Length == 0) ? Color.white : Color.black);
                }
            }
            var path = Path.Combine(SaveManager.SavePath, "bitmap_mesh.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    public static void TestRoute()
    {
        Raypath pathfinder = new Raypath();

        var q = pathfinder.GetRouteTest(GameObject.Find("Start").transform.position, GameObject.Find("End").transform.position);
        while (q.Count > 0)
        {
            var qb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            qb.transform.position = q.Dequeue();
            qb.transform.localScale = Vector3.one * 25;
        }
    }

    public static void BuildTorus()
    {
        MeshFilter filter = new GameObject().AddComponent<MeshFilter>();
        filter.transform.position = new Vector3(67, 100, 37);
        Mesh mesh = filter.mesh;
        mesh.Clear();

        float radius1 = 1f;
        float radius2 = .05f;
        int nbRadSeg = 24;
        int nbSides = 18;

        #region Vertices        
        Vector3[] vertices = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
        float _2pi = Mathf.PI * 2f;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                int currSide = side == nbSides ? 0 : side;

                Vector3 normale = Vector3.Cross(r1, Vector3.up);
                float t2 = (float)currSide / nbSides * _2pi;
                Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

                vertices[side + seg * (nbSides + 1)] = r1 + r2;
            }
        }
        #endregion

        #region Normales        
        Vector3[] normales = new Vector3[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                normales[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)] - r1).normalized;
            }
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
            for (int side = 0; side <= nbSides; side++)
                uvs[side + seg * (nbSides + 1)] = new Vector2((float)seg / nbRadSeg, (float)side / nbSides);
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        int i = 0;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            for (int side = 0; side <= nbSides - 1; side++)
            {
                int current = side + seg * (nbSides + 1);
                int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

                if (i < triangles.Length - 6)
                {
                    triangles[i++] = current;
                    triangles[i++] = next;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = current + 1;
                }
            }
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }

    public static void CreateCity(AbstractMap m)
    {
        if (m.MapVisualizer != null) { m.ResetMap(); }
        m.MapVisualizer = CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
    }

    public static int BuildingCounter(Transform city)
    {
        int count = 0;
        foreach (Transform tile in city)
        {
            foreach (Transform building in tile) { count++; }
        }
        return count;
    }

    public static void DeleteAllBuildings(Transform city)
    {
        foreach (Transform tile in city)
        {
            while (tile.childCount > 0)
            {
                DestroyImmediate(tile.GetChild(0).gameObject);
            }
        }
    }

    public static void DeleteEmptyTiles(Transform city)
    {
        for (int i = city.childCount - 1; i >= 0; i--)
        {
            Transform tile = city.GetChild(i);
            if (tile.childCount == 0) { DestroyImmediate(tile.gameObject); }
        }
    }

    private static void RenameRoads(Transform city)
    {
        foreach(Transform tile in city)
        {
            foreach (Transform component in tile)
            {
                if (component.name == "road")
                {
                    component.name = "Road - " + tile.name.Replace("/", " ");
                }
            }
        }
    }

}

