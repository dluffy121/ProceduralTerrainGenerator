using TG.Previewer;
using UnityEditor;

namespace TG.Editor
{
    [CustomEditor(typeof(TerrainPreviewData))]
    public class TerrainPreviewDataEditor : UpdatableDataEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TerrainPreviewData terrainPreviewData = (TerrainPreviewData)target;
            if (terrainPreviewData.PreviewColor)
            {
                SerializedObject serializedObject = new SerializedObject(terrainPreviewData);
                serializedObject.FindProperty("m_previewHeightMap").boolValue = true;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(terrainPreviewData);
            }
        }
    }
}