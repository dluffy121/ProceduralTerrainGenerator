using UnityEngine;

namespace TG
{
    [CreateAssetMenu(fileName = "NoiseData", menuName = "Terrain Generator/NoiseData", order = 0)]
    public class NoiseData : UpdatableData
    {
        [SerializeField, Min(0)]
        int m_levels;
        public int Levels => m_levels;

        [SerializeField, Range(0, 1)]
        float m_strength;
        public float Strength => m_strength;

        [SerializeField, Min(1)]
        float m_attenuation;
        public float Attenuation => m_attenuation;

        [SerializeField]
        Vector2 m_offset;
        public Vector2 Offset => m_offset;

        [SerializeField]
        float m_scale;
        public float Scale => m_scale;

        [SerializeField]
        int m_seed;
        public int Seed => m_seed;

        [SerializeField]
        HeightNormalizeMode m_heightNormalizeMode;
        public HeightNormalizeMode HeightNormalizeMode => m_heightNormalizeMode;
    }
}