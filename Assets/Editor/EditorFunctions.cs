using static Drones.Utils.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Drones.Utils;
using Drones;
using System.Collections.Generic;
using System.IO;
using System;
using Drones.Managers;
using Drones.Mapbox;
using Drones.Router;
using Mapbox.Unity.Map.TileProviders;
using Utils;

public class EditorFunctions : EditorWindow
{
    private AbstractMap _abstractMap;
    private GameObject _citySimulatorMap;
    public float minHeight;
    public float maxHeight;

    [MenuItem("Window/Editor Functions")]
    private static void Init()
    {
        var sizeWindow = CreateInstance<EditorFunctions>();
        sizeWindow.autoRepaintOnSceneChange = true;
        sizeWindow.Show();
    }

    private void OnGUI()
    {
        if (_citySimulatorMap == null) { _citySimulatorMap = GameObject.Find("Manhattan"); }
        if (_abstractMap == null) { _abstractMap = _citySimulatorMap?.GetComponent<CustomMap>(); }

        minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
        maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);

        if (GUILayout.Button("Edit Mode Build")) 
        {
            //if (abstractMap.MapVisualizer != null) { abstractMap.ResetMap(); }
            if (_abstractMap != null)
            {
                _abstractMap.MapVisualizer = CreateInstance<MapVisualizer>();
                _abstractMap.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
            }
            //abstractMap.Initialize(new Mapbox.Utils.Vector2d(-29.3151, 27.4869), 16);
        }

        if (GUILayout.Button("1. Setup Objects") && _citySimulatorMap != null)
        {
            RenameRoads(_citySimulatorMap.transform);
            OptimizeRenderer(_citySimulatorMap.transform);
            foreach (Transform tile in _citySimulatorMap.transform)
            {
                SortChildrenByName(tile);
            }

            GroupAllByBlocks(_citySimulatorMap.transform);
            SplitAllBlocks(_citySimulatorMap.transform);
            SortHeirarchy(_citySimulatorMap.transform);
            DestroyImmediate(_citySimulatorMap.GetComponent<RangeTileProvider>());
        }

        if (GUILayout.Button("1.5. Combine") && _citySimulatorMap != null)
        {
            Transform road = null;
            foreach (Transform tile in _citySimulatorMap.transform)
            {
                for (var i = 0; i < tile.childCount; i++)
                {
                    var current = tile.GetChild(i);
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

        if (GUILayout.Button("2. Box buildings") && _citySimulatorMap != null)
        {
            var material = Resources.Load("Materials/WhiteLOD") as Material;

            foreach (Transform tile in _citySimulatorMap.transform)
            {
                foreach (Transform building in tile)
                {
                    if (building.childCount > 0) { continue; }

                    new BoundingBox(building).Build(material, IsTall(building) ? Building.Tall : Building.Short);
                    //DestroyImmediate(building.GetComponent<MeshCollider>());
                }
            }
        }

        if (GUILayout.Button("3. Combine Mesh in Tiles") && _citySimulatorMap != null)
        {
            foreach (Transform tile in _citySimulatorMap.transform)
                GroupByTile(tile);
        }

        if (GUILayout.Button("Test Starpath"))
        {
            Obstacle.Accessor.Clear();
            TestStar();
        }

        if (GUILayout.Button("Test SmartStarpath"))
        {
            Obstacle.Accessor.Clear();
            TestSmartStar();
        }

        if (GUILayout.Button("HEIGHT!"))
        {
            GenerateHeightBitMapTEST(8);
        }

    }

    public static void GenerateHeightBitMap(int x, int y, int metre)
    {
        var data = new Texture2D(x, y);
        try
        {
            const float tallest = 40;
            for (var i = 0; i < x * 2; i += metre)
            {
                for (var j = 0; j < y * 2; j += metre)
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
            var path = Path.Combine(SaveLoadManager.ExportPath, "height_bitmap.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    public static void GenerateHeightBitMapNY(int meter)
    {
        var data = new Texture2D(8640 / meter, 16000 / meter);
        try
        {
            const float tallest = 500;
            for (var i = 0; i < 8640; i += meter)
            {
                for (var j = 0; j < 16000; j += meter)
                {
                    var v = new Vector3(i - 4320, 1000, j - 7500);
                    var c = Color.black;
                    if (Physics.BoxCast(v, Vector3.one * meter / 2f, Vector3.down, out RaycastHit info, Quaternion.identity, 1000, 1 << 12))
                    {
                        c += Color.white * Mathf.Clamp(info.point.y / tallest, 0, 1);
                        c.a = 1;
                    }
                    else if (!Physics.BoxCast(v, Vector3.one * meter / 2f, Vector3.down, Quaternion.identity, 1000, 1 << 13))
                    {
                        c = Color.white;
                    }
                    data.SetPixel(i / meter, j / meter, c);
                }
            }
            var path = Path.Combine(SaveLoadManager.SavePath, $"height_bitmap_{meter}m.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    private static void GenerateHeightBitMapTEST(int meter)
    {
        var data = new Texture2D(1200 / meter, 1400 / meter);
        try
        {
            const float tallest = 500;
            for (var i = 0; i < data.width * meter; i += meter)
            {
                for (var j = 0; j < data.height * meter; j += meter)
                {
                    var v = new Vector3(i - data.width * meter / 2, 1000, j - data.height * meter / 2);
                    var c = Color.black;
                    if (Physics.BoxCast(v, Vector3.one * meter / 2f, Vector3.down, out var info, Quaternion.identity, 1000, 1 << 12))
                    {
                        c += Color.white * Mathf.Clamp(info.point.y / tallest, 0, 1);
                        c.a = 1;
                    }
                    else if (!Physics.BoxCast(v, Vector3.one * meter / 2f, Vector3.down, Quaternion.identity, 1000, 1 << 13))
                    {
                        c = Color.white;
                    }
                    data.SetPixel(i / meter, j / meter, c);
                }
            }
            var path = Path.Combine(SaveLoadManager.SavePath, $"height_bitmap_test_{meter}m.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    public static void GenerateOccupancyBitMap()
    {
        var data = new Texture2D(2160, 3750);
        try
        {
            for (var i = 0; i < 4320 * 2; i += 4)
            {
                for (var j = 0; j < 7500 * 2; j += 4)
                {
                    var v = new Vector3(i - 4320, 300, j - 7500);
                    var cols = new Collider[32];
                    var size = Physics.OverlapBoxNonAlloc(v, new Vector3(2, 300, 2), cols, Quaternion.identity, 1 << 12);
                    data.SetPixel(i / 4, j / 4, (cols.Length == 0) ? Color.white : Color.black);
                }
            }
            var path = Path.Combine(SaveLoadManager.SavePath, "bitmap_mesh.png");
            File.WriteAllBytes(path, data.EncodeToPNG());
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Error");
        }
    }

    private static void TestStar()
    {
        var pathfinder = new Starpath(8, 0);

        var q = pathfinder.GetRouteTest(GameObject.Find("Start").transform.position, GameObject.Find("End").transform.position);
        while (q.Count > 0)
        {
            var qb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            qb.transform.position = q.Dequeue();
            qb.transform.localScale = Vector3.one * 25;
        }
    }

    private static void TestSmartStar()
    {
        var pathfinder = new SmartStarpath(0);


        var q = pathfinder.GetRouteTest(GameObject.Find("Start").transform.position, GameObject.Find("End").transform.position);
        while (q.Count > 0)
        {
            var qb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            qb.transform.position = q.Dequeue();
            qb.transform.localScale = Vector3.one * 30;
        }
    }

    public static int BuildingCounter(Transform city)
    {
        var count = 0;
        foreach (Transform tile in city)
        {
            foreach (Transform building in tile) { count++; }
        }
        return count;
    }


    private static void RenameRoads(Transform city)
    {
        foreach(Transform tile in city)
        {
            foreach (Transform component in tile)
            {
                if (component.name == "road")
                {
                    component.name = $"Road - {tile.name.Replace("/", " ")}";
                }
            }
        }
    }

}

