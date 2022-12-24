using UnityEngine;

namespace TG
{
    [CreateAssetMenu(fileName = "TerrainData", menuName = "Terrain Generator/TerrainData", order = 0)]
    public class TerrainData : UpdatableData
    {
        [Header("Noise Properties")]
        [SerializeField]
        NoiseData m_noiseData;
        public NoiseData NoiseData => m_noiseData;

        [Header("Terrain Properties")]
        [SerializeField]
        int m_terrainChunkSize;
        public int TerrainChunkSize => m_terrainChunkSize;

        [SerializeField]
        AnimationCurve m_heightCurve;
        public AnimationCurve HeightCurve => m_heightCurve;

        // [SerializeField]
        // TerrainType[] m_regions;
        // public TerrainType[] Regions => m_regions;

        [SerializeField]
        int m_heightMultiplier;
        public int HeightMultiplier => m_heightMultiplier;

        [SerializeField]
        bool m_fallOff;
        public bool FallOff => m_fallOff;

        [SerializeField]
        AnimationCurve m_fallOffCurve;
        public AnimationCurve FallOffCurve => m_fallOffCurve;

        [SerializeField]
        bool m_connectableChunks;
        public bool ConnectableChunks => m_connectableChunks;

        [SerializeField]
        float m_chunkUniformScale;
        public float ChunkUniformScale => m_chunkUniformScale;

        [SerializeField]
        private float m_minHeight = float.MaxValue;
        public float MinHeight { get => m_minHeight; private set => m_minHeight = value; }

        [SerializeField]
        private float m_maxHeight = float.MinValue;
        public float MaxHeight { get => m_maxHeight; private set => m_maxHeight = value; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (m_noiseData != null)
            {
                m_noiseData.OnUpdateData -= OnValidate;
                m_noiseData.OnUpdateData += OnValidate;
            }

            UpdateMinMaxHeights();

            base.OnValidate();
        }

        private void UpdateMinMaxHeights()
        {
            m_minHeight = ChunkUniformScale * HeightMultiplier * HeightCurve.Evaluate(0);
            m_maxHeight = ChunkUniformScale * HeightMultiplier * HeightCurve.Evaluate(1);
        }
#endif
    }
}