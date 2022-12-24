using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TG
{
    public class TerrainGenerator : MonoBehaviour
    {
        [SerializeField]
        TerrainData m_terrainData = null;
        public TerrainData TerrainData => m_terrainData;

        [SerializeField]
        TextureData m_textureData = null;
        public TextureData TextureData => m_textureData;

        [SerializeField]
        Material m_material = null;

        private void OnValidate()
        {
            if (TextureData != null)
            {
                TextureData.OnUpdateData -= OnTextureValuesUpdated;
                TextureData.OnUpdateData += OnTextureValuesUpdated;
            }
        }

        private void OnTextureValuesUpdated()
        {
            TextureData.ApplyToMaterial(m_material);
        }

        private void Awake()
        {
            TextureData.ApplyToMaterial(m_material);
            TextureData.UpdateMeshHeights(m_material, TerrainData.MinHeight, TerrainData.MaxHeight);
        }

        private void Update()
        {
            while (m_terrainDataThreadInfoQueue.Count > 0)
            {
                TGThreadInfo<TerrainMapData> l_TGThreadInfo = m_terrainDataThreadInfoQueue.Dequeue();
                l_TGThreadInfo.Callback.Invoke(l_TGThreadInfo.Data);
            }

            while (m_meshDataThreadInfoQueue.Count > 0)
            {
                TGThreadInfo<MeshData> l_TGThreadInfo = m_meshDataThreadInfoQueue.Dequeue();
                l_TGThreadInfo.Callback.Invoke(l_TGThreadInfo.Data);
            }
        }

        public TerrainMapData GenerateTerrainMapData(TerrainData a_terrainData, Vector2 a_position = default, TextureData a_textureData = null)
        {
            float[,] l_noiseMap = GenerateTerrainHeightMap(a_terrainData.TerrainChunkSize + 2, a_terrainData.TerrainChunkSize + 2, a_terrainData.NoiseData, a_position);

            if (a_terrainData.FallOff)
                FallOffGenerator.ApplyFallOff(ref l_noiseMap, a_terrainData.FallOffCurve);

            // Color[] l_colorMap = GenerateTerrainColors(l_noiseMap, a_terrainData.Regions);

            // NOTE: 
            // This only works in editor mode, but will fail in play mode since this method is called inside thread
            // to resolve this we can use a boolean and trigger this method 
            // but since we already have the passed data from start we can invoke this method then and will also be optimized since its only called once
            // a_textureData.UpdateMeshHeights(m_material, a_terrainData.MinHeight, a_terrainData.MaxHeight);

            // return new TerrainMapData(l_noiseMap, l_colorMap);
            return new TerrainMapData(l_noiseMap);
        }

        private float[,] GenerateTerrainHeightMap(int a_mapWidth, int a_mapHeight, NoiseData a_noiseData, Vector2 a_position)
        {
            return NoiseMap.Generate(a_mapWidth,
                                     a_mapWidth,
                                     a_noiseData.Levels,
                                     a_noiseData.Strength,
                                     a_noiseData.Attenuation,
                                     a_position + a_noiseData.Offset,
                                     a_noiseData.Scale,
                                     a_noiseData.Seed,
                                     a_noiseData.HeightNormalizeMode);
        }

        private Color[] GenerateTerrainColors(float[,] a_map, TerrainType[] a_terrainTypes)
        {
            if (a_terrainTypes == null)
                return null;

            int width = a_map.GetLength(0);
            int height = a_map.GetLength(1);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int r = 0; r < a_terrainTypes.Length; r++)
                    {
                        if (a_map[x, y] < a_terrainTypes[r].Height)
                            break;

                        colors[y * height + x] = a_terrainTypes[r].Color;
                    }
                }
            }

            return colors;
        }

        [SerializeField]
        TerrainChunk m_terrainChunkPrefab = null;

        public TerrainChunk GetTerrainChunk(Vector2 a_coord, float a_size, LOD[] a_LODs, int a_colliderLODIndex, float a_colliderUpdateThresholdDst)
        {
            TerrainChunk l_terrainChunk = Instantiate<TerrainChunk>(m_terrainChunkPrefab, transform);
            l_terrainChunk.Init(a_coord,
                                a_size,
                                a_LODs,
                                a_LODs[a_colliderLODIndex].Level,
                                a_colliderUpdateThresholdDst,
                                TerrainData.ChunkUniformScale,
                                m_material);
            return l_terrainChunk;
        }

        #region Threading

        // NOTE :
        // Since the terrain generation process takes time and we don't want to wait till it is generated and cause unnecessary hiccups
        // We use this data requesting architecture, which starts a new thread for generating data 
        // This requesting thread once complete, the data and its callback is queued for invocation in Unity's Update thread

        Queue<TGThreadInfo<TerrainMapData>> m_terrainDataThreadInfoQueue = new Queue<TGThreadInfo<TerrainMapData>>();
        Queue<TGThreadInfo<MeshData>> m_meshDataThreadInfoQueue = new Queue<TGThreadInfo<MeshData>>();

        internal void RequestTerrainData(Action<TerrainMapData> a_callback, Vector2 a_position = default)
        {
            ThreadStart l_threadStart = delegate { TerrainDataThread(a_callback, a_position); };

            new Thread(l_threadStart).Start();
        }

        void TerrainDataThread(Action<TerrainMapData> a_callback, Vector2 a_position = default)
        {
            lock (m_terrainDataThreadInfoQueue)
            {
                TerrainMapData a_data = GenerateTerrainMapData(TerrainData, a_position, TextureData);
                m_terrainDataThreadInfoQueue.Enqueue(new TGThreadInfo<TerrainMapData>(a_callback, a_data));
            }
        }

        internal void RequestMeshData(TerrainMapData a_terrainData, int a_lod, Action<MeshData> a_callback)
        {
            ThreadStart l_threadStart = delegate { MeshDataThread(a_terrainData, a_lod, a_callback); };

            new Thread(l_threadStart).Start();
        }

        void MeshDataThread(TerrainMapData a_terrainData, int a_lod, Action<MeshData> a_callback)
        {
            MeshData l_meshData = MeshGenerator.GenerateMesh(a_terrainData.NoiseMap,
                                                             TerrainData.HeightCurve,
                                                             a_lod,
                                                             TerrainData.HeightMultiplier,
                                                             TerrainData.ConnectableChunks);
            lock (m_terrainDataThreadInfoQueue)
            {
                m_meshDataThreadInfoQueue.Enqueue(new TGThreadInfo<MeshData>(a_callback, l_meshData));
            }
        }

        struct TGThreadInfo<T>
        {
            public readonly Action<T> Callback;
            public readonly T Data;

            public TGThreadInfo(Action<T> a_callback, T a_data)
            {
                Callback = a_callback;
                Data = a_data;
            }
        }
        #endregion
    }

    public struct TerrainMapData
    {
        private float[,] m_noiseMap;
        public float[,] NoiseMap => m_noiseMap;

        private Color[] m_colorMap;
        public Color[] ColorMap => m_colorMap;

        public TerrainMapData(float[,] noiseMap) : this()
        {
            m_noiseMap = noiseMap;
        }

        public TerrainMapData(float[,] a_noiseMap, Color[] a_colorMap)
        {
            m_noiseMap = a_noiseMap;
            m_colorMap = a_colorMap;
        }
    }

    [Serializable]
    public struct TerrainType
    {
        [SerializeField]
        string m_name;
        public string Name => m_name;

        [SerializeField, Range(0, 1)]
        float m_height;
        public float Height => m_height;

        [SerializeField]
        Color m_color;
        public Color Color => m_color;
    }
}