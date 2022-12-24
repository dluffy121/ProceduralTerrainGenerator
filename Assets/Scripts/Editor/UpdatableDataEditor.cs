using UnityEngine;
using UnityEditor;

namespace TG.Editor
{
    [CustomEditor(typeof(UpdatableData), true)]
    public class UpdatableDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdatableData updatableData = (UpdatableData)target;

            if (GUILayout.Button("Update"))
            {
                updatableData.AppriseOnValuesUpdated();
                EditorUtility.SetDirty(updatableData);
            }
        }
    }
}
