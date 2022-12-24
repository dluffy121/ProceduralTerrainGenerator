using System;
using UnityEngine;

namespace TG
{
    internal class LODMesh
    {
        Mesh m_mesh;
        public Mesh Mesh => m_mesh;

        public readonly int LODLevel;

        bool m_meshDataRequested;
        public bool MeshDataRequested => m_meshDataRequested;

        bool m_meshUpdated;
        public bool MeshUpdated => m_meshUpdated;

        public event Action OnMeshUpdate;

        private TerrainGenerator m_terrainGeneratorRef = null;
        private TerrainGenerator TerrainGeneratorRef
        {
            get
            {
                m_terrainGeneratorRef ??= GameObject.FindObjectOfType<TerrainGenerator>();
                return m_terrainGeneratorRef;
            }
        }

        public LODMesh(int a_LODLevel)
        {
            LODLevel = a_LODLevel;
        }

        public void RequestMeshData(TerrainMapData a_terrainData)
        {
            m_meshDataRequested = true;
            TerrainGeneratorRef.RequestMeshData(a_terrainData, LODLevel, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData a_meshData)
        {
            m_meshUpdated = true;
            m_mesh = a_meshData.CreateMesh();
            OnMeshUpdate();
        }
    }

    [Serializable]
    public struct LOD
    {
        [SerializeField]
        int m_level;
        public int Level => m_level;

        [SerializeField]
        float m_thresholdDistance;
        public float ThresholdDistance => m_thresholdDistance;

        public LOD(int a_level, float a_thresholdDistance)
        {
            m_level = a_level;
            m_thresholdDistance = a_thresholdDistance;
        }
    }
}