using Drones.UI.Utils;
using UnityEditor;
using TMPro.EditorUtilities;

namespace Drones.Editor
{
    [CustomEditor(typeof(DataField))]
    [CanEditMultipleObjects]
    public class DataFieldInspector : TMP_UiEditorPanel
    {
        /// <summary>
        /// Draw the standard custom inspector
        /// </summary>
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(VariableBarField))]
    public class VariableBarFieldInspector : DataFieldInspector
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