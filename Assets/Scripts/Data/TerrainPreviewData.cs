using UnityEngine;

namespace TG.Previewer
{
    [CreateAssetMenu(fileName = "TerrainPreviewData", menuName = "Terrain Generator/TerrainPreviewData", order = 1)]
    public class TerrainPreviewData : TerrainData
    {
        [SerializeField, Range(0, 6)]
        int m_previewLOD;
        public int PreviewLOD => m_previewLOD;

        [SerializeField]
        bool m_previewHeightMap;
        public bool PreviewHeightMap => m_previewHeightMap;

        [SerializeField]
        bool m_previewColor;
        public bool PreviewColor => m_previewColor;

        [SerializeField]
        bool m_previewMesh;
        public bool PreviewMesh => m_previewMesh;
    }
}