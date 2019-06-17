using Drones.UI.Utils;
using UnityEngine;
using UnityEditor;

namespace Drones.Editor
{
    [InitializeOnLoad]
    public static class ListContentHeightAdjustEditor
    {
        static ListContentHeightAdjustEditor()
        {
            EditorApplication.hierarchyChanged += Update;
            EditorApplication.playModeStateChanged += StartStop;
        }

        static void Update()
        {
            ListTupleContainer[] listlinks = Object.FindObjectsOfType<ListTupleContainer>();
            foreach (ListTupleContainer lcha in listlinks)
            {
                lcha.AdjustDimensions();
            }
        }

        static void StartStop(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.update -= Update;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.update += Update;
                    break;
            }
        }
    }

}
