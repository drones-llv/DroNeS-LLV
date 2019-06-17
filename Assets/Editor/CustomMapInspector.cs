using Drones.Mapbox;
using UnityEditor;
using Mapbox.Editor;

namespace Drones.Editor
{
    using Drones.Utils;

    [CustomEditor(typeof(CustomMap))]
    [CanEditMultipleObjects]
    public class CustomMapInspector : MapManagerEditor
    {
        /// <summary>
        /// Draw the standard custom inspector
        /// </summary>
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

}