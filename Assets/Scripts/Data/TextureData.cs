using System;
using System.Linq;
using UnityEngine;

namespace TG
{
    [CreateAssetMenu(fileName = "TextureData", menuName = "Terrain Generator/TextureData", order = 0)]
    public class TextureData : UpdatableData
    {
        private const int TEXTURE_SIZE = 512;
        private const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB565;

        [SerializeField]
        Region[] m_regions;

        [Serializable]
        struct Region
        {
            /// <summary>
            /// Region Texture
            /// </summary>
            [SerializeField]
            public Texture2D texture;

            /// <summary>
            /// Scale of Region Texture
            /// </summary>
            [SerializeField, Min(0)]
            public float textureScale;

            /// <summary>
            /// Base color tint of the region
            /// </summary>
            [SerializeField]
            public Color tint;

            /// <summary>
            /// Base color tint strength
            /// </summary>
            [SerializeField, Range(0, 1)]
            public float tintStrength;

            /// <summary>
            /// Start height
            /// </summary>
            [SerializeField, Range(0, 1)]
            public float height;

            /// <summary>
            /// Blend value
            /// </summary>
            [SerializeField, Range(0, 1)]
            public float blend;
        }

#if UNITY_EDITOR
        float m_savedMinHeight;
        float m_savedMaxHeight;
#endif

        public void ApplyToMaterial(Material a_material)
        {
            if (m_regions.Length > 0)
            {
                a_material.SetInt("regionCount", m_regions.Length);
                Texture2DArray texture2DArray = GenerateTexture2DArray(m_regions.Select(x => x.texture).ToArray());
                a_material.SetTexture("textures", texture2DArray);
                a_material.SetFloatArray("textureScales", m_regions.Select(x => x.textureScale).ToArray());
                a_material.SetColorArray("tints", m_regions.Select(x => x.tint).ToArray());
                a_material.SetFloatArray("tintStrengths", m_regions.Select(x => x.tintStrength).ToArray());
                a_material.SetFloatArray("heights", m_regions.Select(x => x.height).ToArray());
                a_material.SetFloatArray("blends", m_regions.Select(x => x.blend).ToArray());
            }

#if UNITY_EDITOR
            UpdateMeshHeights(a_material, m_savedMinHeight, m_savedMaxHeight);
#endif
        }

        Texture2DArray GenerateTexture2DArray(Texture2D[] textureArray)
        {
            Texture2DArray texture2DArray = new Texture2DArray(TEXTURE_SIZE, TEXTURE_SIZE, textureArray.Length, TEXTURE_FORMAT, true);

            for (int i = 0; i < textureArray.Length; i++)
            {
                texture2DArray.SetPixels(textureArray[i].GetPixels(), i);
            }

            texture2DArray.Apply();

            return texture2DArray;
        }

        public void UpdateMeshHeights(Material a_material, float a_minHeight, float a_maxHeight)
        {
#if UNITY_EDITOR
            m_savedMinHeight = a_minHeight;
            m_savedMaxHeight = a_maxHeight;
#endif

            a_material.SetFloat("minHeight", a_minHeight);
            a_material.SetFloat("maxHeight", a_maxHeight);
        }
    }
}