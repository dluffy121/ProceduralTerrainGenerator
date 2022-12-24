using UnityEngine;

namespace TG.Previewer
{
    [RequireComponent(typeof(Renderer), typeof(MeshFilter))]
    public class TerrainChunkPreview : MonoBehaviour
    {
        [SerializeField]
        TerrainGenerator m_terrainGenerator = null;

        [SerializeField]
        Renderer m_renderer;

        [SerializeField]
        MeshFilter m_meshFilter;

        [SerializeField]
        TerrainPreviewData m_terrainPreviewData = null;

        [SerializeField]
        TextureData m_previewTextureData = null;

        [SerializeField]
        Material m_material = null;

        private void OnValidate()
        {
            m_renderer ??= GetComponent<Renderer>();
            m_meshFilter ??= GetComponent<MeshFilter>();

            if (m_terrainPreviewData != null)
            {
                m_terrainPreviewData.OnUpdateData -= UpdatePreview;
                m_terrainPreviewData.OnUpdateData += UpdatePreview;
            }

            if (m_previewTextureData != null)
            {
                m_previewTextureData.OnUpdateData -= OnTextureDataUpdated;
                m_previewTextureData.OnUpdateData += OnTextureDataUpdated;
            }
        }

        private void UpdatePreview()
        {
            m_previewTextureData.UpdateMeshHeights(m_material, m_terrainPreviewData.MinHeight, m_terrainPreviewData.MaxHeight);

            TerrainMapData l_terrainData = m_terrainGenerator.GenerateTerrainMapData(a_terrainData: m_terrainPreviewData,
                                                                                     a_textureData: m_previewTextureData);

            m_meshFilter.mesh = MeshGenerator.GenerateMesh(l_terrainData.NoiseMap,
                                                           m_terrainPreviewData.HeightCurve,
                                                           m_terrainPreviewData.PreviewLOD * 2,
                                                           m_terrainPreviewData.PreviewMesh ? m_terrainPreviewData.HeightMultiplier : 0)
                                                .CreateMesh();

            Texture2D l_terrainTexture = m_terrainPreviewData.PreviewHeightMap ? TextureGenerator.CreateHeightMap(l_terrainData.NoiseMap) : null;

            if (m_terrainPreviewData.PreviewColor && l_terrainTexture)
                TextureGenerator.ApplyColor(ref l_terrainTexture, l_terrainData.ColorMap);

            m_renderer.sharedMaterial.mainTexture = l_terrainTexture;
        }

        private void OnTextureDataUpdated()
        {
            m_previewTextureData.ApplyToMaterial(m_material);
            m_renderer.sharedMaterial = m_material;
        }

        private void Awake()
        {
            m_renderer.enabled = false;

            Destroy(m_meshFilter.mesh);
            m_meshFilter.mesh = null;

            Destroy(m_renderer.sharedMaterial.mainTexture);
            m_renderer.sharedMaterial.mainTexture = null;
        }
    }
}