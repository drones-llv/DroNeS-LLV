using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using Drones.UI.Utils;
using Drones.Utils;
using Drones.Utils.Interfaces;

namespace Drones.Editor
{
    [InitializeOnLoad]
    public static class ListItemColourAdjust
    {
        static ListItemColourAdjust()
        {
            EditorApplication.hierarchyChanged += Update;
            EditorApplication.playModeStateChanged += StartStop;
        }

        static void Update()
        {
            var tuples = Object.FindObjectsOfType<MonoBehaviour>().OfType<IListElement>();
            foreach (var obj in tuples)
            {
               obj.ItemImage.color = EditorSet(((MonoBehaviour)obj).transform);
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

        static Color EditorSet(Transform transform)
        {
            return (transform.GetSiblingIndex() % 2 == 1) ? AbstractListElement.ListItemOdd : AbstractListElement.ListItemEven;
        }
    }
}

