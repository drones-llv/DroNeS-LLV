using UnityEngine;
using UnityEditor;
using TMPro;

public class SingleObjectManipulator : EditorWindow
{
    GameObject selection;
    GameObject cube;
    float size = 5;
    Vector3 outside;
    Vector3 point;

    [MenuItem("Window/Object Info")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SingleObjectManipulator sizeWindow = CreateInstance<SingleObjectManipulator>();
        sizeWindow.autoRepaintOnSceneChange = true;
        sizeWindow.Show();
    }

    void OnGUI()
    {
        selection = Selection.activeGameObject;
        if (selection == null) { return; }
        GUILayout.Label("Center Position", EditorStyles.boldLabel);
        Collider mc = selection.GetComponent<Collider>();
        MeshRenderer mr = selection.GetComponent<MeshRenderer>();
        MeshFilter mf = selection.GetComponent<MeshFilter>();

        Vector3 center = Vector3.zero;
        if (mr != null)
            center = mr.bounds.center;

        GUILayout.Box(center.ToString(), GUILayout.ExpandWidth(true));

        GUILayout.Label("Closest Point", EditorStyles.boldLabel);
        if (GUILayout.Button("Paste Position"))
        {
            outside = center;
        }
        outside = EditorGUILayout.Vector3Field("Outside Point", outside);

        if (GUILayout.Button("Get Closest Point"))
        {
            point = mc.ClosestPoint(outside);
        }
        GUILayout.Box(point.ToString(), GUILayout.ExpandWidth(true));
        GUILayout.Label("Cube Size: ");
        size = EditorGUILayout.FloatField(size, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Make a Cube Here"))
        {
            if (cube == null) { cube = GameObject.Find("PointCube"); }
            if (cube != null) { DestroyImmediate(cube); }
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PointCube";
            cube.transform.SetSiblingIndex(0);
            cube.transform.position = point;
            cube.transform.localScale = size * Vector3.one;
        }

        if (GUILayout.Button("Save Mesh"))
        {
            mf.sharedMesh.name = "InsideOutCylinder";
            AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/Resources/Meshes/InsideOutCylinder.asset");
        }

        if (GUILayout.Button("Parent"))
        {
            Debug.Log(selection.transform.parent);
        }

    }


}
