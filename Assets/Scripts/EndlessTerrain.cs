using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TG
{
    public class EndlessTerrain : MonoBehaviour
    {
        [SerializeField]
        TerrainGenerator m_terrainGenerator = null;

        float m_maxViewDistance;
        public float MaxViewDistance => m_maxViewDistance;

        [SerializeField]
        GameObject m_viewer;

        [FormerlySerializedAsAttribute("m_LevelOfDetails")]
        [SerializeField]
        LOD[] m_levelOfDetails;

        [SerializeField]
        int m_collisionLODIndex;

        [SerializeField]
        float m_chunkUpdateThresholdDistance;
        float m_sqrChunkUpdateThresholdDistance;

        [SerializeField]
        float m_chunkColliderUpdateThresholdDistance;
        float m_sqrChunkColliderUpdateThresholdDistance;

        Vector2 m_viewerPosition;
        public Vector2 ViewerPosition => m_viewerPosition;
        Vector2 m_lastViewerPosition;

        int m_chunkSize = 0;
        int m_chunksVisible = 0;

        bool m_loaded = false;
        int m_loadingChunks = 0;

        Dictionary<Vector2, TerrainChunk> m_dictTerrainChunks = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> m_lastFrameTerrainChunks = new List<TerrainChunk>();

        private void Start()
        {
            m_sqrChunkUpdateThresholdDistance = m_chunkUpdateThresholdDistance * m_chunkUpdateThresholdDistance;
            m_sqrChunkColliderUpdateThresholdDistance = m_chunkColliderUpdateThresholdDistance * m_chunkColliderUpdateThresholdDistance;
            m_maxViewDistance = m_levelOfDetails[m_levelOfDetails.Length - 1].ThresholdDistance;
            m_chunkSize = m_terrainGenerator.TerrainData.TerrainChunkSize - 1;
            m_chunksVisible = Mathf.RoundToInt(m_maxViewDistance / m_chunkSize);

            UpdateVisibleChunks();
        }

        private void Update()
        {
            if (m_loaded && !m_viewer.activeSelf)
                m_viewer.SetActive(true);

            m_viewerPosition = new Vector2(m_viewer.transform.position.x, m_viewer.transform.position.z) / m_terrainGenerator.TerrainData.ChunkUniformScale;

            if ((m_lastViewerPosition - m_viewerPosition).sqrMagnitude <= m_sqrChunkUpdateThresholdDistance)
                return;

            m_lastViewerPosition = m_viewerPosition;
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            foreach (var l_chunks in m_lastFrameTerrainChunks)
            {
                l_chunks.SetVisible(false);
            }
            m_lastFrameTerrainChunks.Clear();

            int l_currentChunkCoordX = Mathf.RoundToInt(m_viewerPosition.x / m_chunkSize);
            int l_currentChunkCoordY = Mathf.RoundToInt(m_viewerPosition.y / m_chunkSize);

            for (int l_yOffset = -m_chunksVisible; l_yOffset <= m_chunksVisible; l_yOffset++)
            {
                for (int l_xOffset = -m_chunksVisible; l_xOffset <= m_chunksVisible; l_xOffset++)
                {
                    Vector2 l_chunkCoord = new Vector2(l_currentChunkCoordX + l_xOffset, l_currentChunkCoordY + l_yOffset);

                    m_loadingChunks++;

                    if (!m_dictTerrainChunks.ContainsKey(l_chunkCoord))
                    {
                        TerrainChunk l_terrainChunk = m_terrainGenerator.GetTerrainChunk(l_chunkCoord,
                                                                                         m_chunkSize,
                                                                                         m_levelOfDetails,
                                                                                         m_levelOfDetails[m_collisionLODIndex].Level,
                                                                                         m_sqrChunkColliderUpdateThresholdDistance);
                        l_terrainChunk.SetViewerPositionGetter(() => ViewerPosition);
                        l_terrainChunk.OnVisible += AddToLastFrameTerrainChunksList_IfVisible;
                        l_terrainChunk.OnUpdate += UpdateLoadingStatus;
                        m_dictTerrainChunks.Add(l_chunkCoord, l_terrainChunk);
                        continue;
                    }

                    m_dictTerrainChunks[l_chunkCoord].UpdateTerrainChunk();
                }
            }
        }

        private void AddToLastFrameTerrainChunksList_IfVisible(bool a_visible, TerrainChunk a_terrainChunk)
        {
            if (!a_visible)
            {
                m_loadingChunks--;
                return;
            }

            m_lastFrameTerrainChunks.Add(a_terrainChunk);
        }

        private void UpdateLoadingStatus(TerrainChunk a_terrainChunk)
        {
            if (!a_terrainChunk.Loading)
                m_loadingChunks--;
            m_loaded = m_loadingChunks == 0;
        }
    }
}