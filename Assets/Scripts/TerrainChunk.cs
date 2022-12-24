using System;
using UnityEngine;

namespace TG
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TerrainChunk : MonoBehaviour
    {
        [SerializeField]
        MeshFilter m_meshFilter;

        [SerializeField]
        MeshRenderer m_meshRenderer;

        [SerializeField]
        MeshCollider m_meshCollider;

        Vector2 m_position;
        Bounds m_bounds;

        TerrainMapData m_terrainData;
        bool m_hasTerrainData;

        bool m_loading = true;
        public bool Loading => m_loading;

        LOD[] m_LODs;
        LODMesh[] m_LODMeshes;
        LODMesh m_collisionLODMesh;
        bool m_colliderSet;
        float m_colliderUpdateThresholdDst;

        int m_currentLOD = -1;

        private void OnValidate()
        {
            m_meshFilter ??= GetComponent<MeshFilter>();
            m_meshRenderer ??= GetComponent<MeshRenderer>();
            m_meshCollider ??= GetComponent<MeshCollider>();
        }

        public event Action<TerrainChunk> OnUpdate;
        public event Action<bool, TerrainChunk> OnVisible;
        Func<Vector2> m_viewerPositionGetter;

        private TerrainGenerator m_terrainGeneratorRef = null;
        private TerrainGenerator TerrainGeneratorRef
        {
            get
            {
                m_terrainGeneratorRef ??= GameObject.FindObjectOfType<TerrainGenerator>();
                return m_terrainGeneratorRef;
            }
        }

        public void SetViewerPositionGetter(Func<Vector2> a_getter)
        {
            m_viewerPositionGetter = a_getter;
        }

        public void Init(Vector2 a_coord, float a_size, LOD[] a_LODs, int a_colliderLOD, float a_colliderUpdateThresholdDst, float a_scale, Material a_material, Func<Vector2> a_viewerPositionGetter = null)
        {
            m_LODs = a_LODs;
            m_colliderUpdateThresholdDst = a_colliderUpdateThresholdDst;

            m_meshRenderer.material = a_material;

            if (a_viewerPositionGetter != null)
                SetViewerPositionGetter(a_viewerPositionGetter);

            m_position = a_coord * a_size;
            m_bounds = new Bounds(m_position, Vector2.one * a_size);
            transform.position = new Vector3(m_position.x, 0, m_position.y) * a_scale;
            transform.localScale = Vector3.one * a_scale;
            SetVisible(false);

            m_LODMeshes = new LODMesh[m_LODs.Length];
            for (int i = 0; i < m_LODMeshes.Length; i++)
            {
                m_LODMeshes[i] = new LODMesh(m_LODs[i].Level);
                m_LODMeshes[i].OnMeshUpdate += UpdateTerrainChunk;

                if (m_LODs[i].Level == a_colliderLOD)
                    m_collisionLODMesh = m_LODMeshes[i];
            }

            TerrainGeneratorRef.RequestTerrainData(OnTerrainDataReceived, m_position);
        }

        public void OnTerrainDataReceived(TerrainMapData a_terrainData)
        {
            m_terrainData = a_terrainData;
            m_hasTerrainData = true;

            UpdateTerrainChunk();
        }

        public bool IsVisible => gameObject.activeSelf;

        public void SetVisible(bool a_value)
        {
            gameObject.SetActive(a_value);
            OnVisible?.Invoke(a_value, this);
        }

        public void UpdateTerrainChunk()
        {
            if (m_viewerPositionGetter == null) return;

            float l_viewerDistance = Mathf.Sqrt(m_bounds.SqrDistance(m_viewerPositionGetter()));

            int i_LOD = GetLOD(l_viewerDistance);
            UpdateVisibility(i_LOD);
            UpdateLOD(i_LOD);
            if (l_viewerDistance < m_colliderUpdateThresholdDst)
                UpdateCollisionMesh(i_LOD);

            OnUpdate?.Invoke(this);
        }

        private void UpdateVisibility(int i_LOD)
        {
            SetVisible(i_LOD < m_LODMeshes.Length);
        }

        int GetLOD(float a_viewerDistance)
        {
            int i_LOD = 0;

            for (int i = 0; i < m_LODMeshes.Length; i++)
            {
                if (a_viewerDistance <= m_LODs[i].ThresholdDistance)
                    break;

                i_LOD = i + 1;
            }

            return i_LOD;
        }

        void UpdateLOD(int a_LODIndex)
        {
            if (!IsVisible) return;

            if (a_LODIndex == m_currentLOD) return;

            if (!m_hasTerrainData) return;

            LODMesh l_LODMesh = m_LODMeshes[a_LODIndex];
            if (l_LODMesh.MeshUpdated)
            {
                m_currentLOD = a_LODIndex;
                m_meshFilter.mesh = l_LODMesh.Mesh;
                // m_meshCollider.sharedMesh = l_LODMesh.Mesh;

                m_loading = false;
                return;
            }

            if (!l_LODMesh.MeshDataRequested)
            {
                l_LODMesh.RequestMeshData(m_terrainData);
                m_loading = true;
            }
        }

        void UpdateCollisionMesh(int a_LODIndex)
        {
            if (m_colliderSet) return;

            m_meshCollider.enabled = a_LODIndex == 0;

            if (!m_meshCollider.enabled) return;

            if (m_collisionLODMesh.MeshUpdated)
            {
                m_meshCollider.sharedMesh = m_collisionLODMesh.Mesh;
                m_colliderSet = true;
            }

            if (!m_collisionLODMesh.MeshDataRequested)
                m_collisionLODMesh.RequestMeshData(m_terrainData);
        }
    }
}