using UnityEngine;

namespace TG
{
    public enum HeightNormalizeMode
    {
        None,
        Local,
        Global
    }

    public static class NoiseMap
    {
        /// <summary>
        /// Core function generating noise maps for color and terrain
        /// </summary>
        /// <param name="width">width of noise map</param>
        /// <param name="height">height of noise map</param>
        /// <param name="levels">determines the stages of irregularites with each next level having half the resolution of previous one, for eg: smooth base, boulders, small details of rocks.</param>
        /// <param name="strength">Smoothness between pixels i.e. it deals with amplitude of wave generated at each levels, will half itself for each next level</param>
        /// <param name="attenuation">Increases frequency of each levels i.e. it deals with frequency of wave generated at each levels, will half itself for each next level</param>
        /// <param name="offset"></param>
        /// <param name="scale">Size of noise map</param>
        /// <param name="seed">for generating seeded randoms or recall random generated by associating random with a seed no.</param>
        /// <param name="heightNormalizeMode">data for choosing height normalization method</param>
        /// <returns>2D Array of floats x y position</returns>
        public static float[,] Generate(int width, int height, int levels, float strength, float attenuation, Vector2 offset, float scale, int seed, HeightNormalizeMode heightNormalizeMode)
        {
            float[,] noiseMap = new float[width, height];

            System.Random prng = new System.Random(seed);
            Vector2[] levelOffsets = new Vector2[levels];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < levels; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) - offset.y;
                levelOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= strength;
            }

            if (scale <= 0)
                scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < levels; i++)
                    {
                        // NOTE : Frequency is adjusted with respect to Scale of Map as the waves need to be resized/normalized as per scale
                        float sampleX = (x - halfWidth + levelOffsets[i].x) / scale * frequency;
                        float sampleY = (y - halfHeight + levelOffsets[i].y) / scale * frequency;

                        // NOTE :
                        // A random PerlinNoise Value is generated using sampleX & sampleY, 
                        // as we are generating a land mass which seems random but the values also need to be gradual in nature to properly show mountains, planes, depths.
                        // * 2 - 1 to allow negatives by spreading the value between -1 and 1;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        // NOTE : Just like frequency needs to be rescaled, amplitude needs to be applied to PerlinNoise value to produce final height depending on scale of map
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= strength; // NOTE : strength needs to be applied for smooth changes of amplitude
                        frequency *= attenuation; // NOTE : attenuation needs to be applied for smooth changes of frequency
                    }

                    // NOTE : this is to constrain noiseheight values between min and max values
                    if (noiseHeight > maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight)
                        minNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            if (heightNormalizeMode == HeightNormalizeMode.None)
                return noiseMap;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (heightNormalizeMode == HeightNormalizeMode.Local)
                        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]); // NOTE : InverseLerp to constrain values between 0 and 1
                    else if (heightNormalizeMode == HeightNormalizeMode.Global)
                    {
                        float noiseHeight = (noiseMap[x, y] + 1) / (1.5f * maxPossibleHeight);
                        noiseMap[x, y] = Mathf.Clamp(noiseHeight, 0, float.MaxValue);
                    }
                }
            }

            return noiseMap;
        }
    }
}