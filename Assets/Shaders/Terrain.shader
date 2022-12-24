Shader "Custom/Terrain"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "UnityCG.cginc"

        const static int MaxRegionCount = 10;
        const static float Epsilon = 1E-4;

        int regionCount;
        UNITY_DECLARE_TEX2DARRAY(textures);
        float textureScales[MaxRegionCount];
        float3 tints[MaxRegionCount];
        float tintStrengths[MaxRegionCount];
        float heights[MaxRegionCount];
        float blends[MaxRegionCount];

        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float inverseLerp(float min, float max, float current)
        {
            // NOTE : saturate to clamp between 0 and 1
            return saturate((current - min)/(max - min));
        }

        // NOTE : 
        // we use this method to ensure that the texture does not stretch at any point
        // by is mapping its coordinates as per the normals
        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            float3 scaledWorldPos = worldPos / scale;

            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(textures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;

            return xProjection + yProjection + zProjection;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);

            // NOTE : To generate basic grayscale noise map
            // o.Albedo = heightPercent;

            float3 blendAxes = abs(IN.worldNormal);
            // NOTE : we divide the normal by sum of its coordinates to ensure that its sum is 1
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            for(int i = 0; i < regionCount; i++)
            {
                // NOTE : 
                // Use this to produce flat colors for regions
                // sign to provide height values between of -1 if -ve, 0 if 0 and 1 if +ve
                // saturate to clamp it back between 0 and 1
                // so final values will either be 0 or 1
                // float strength = saturate(sign(heightPercent - startHeights[i]));

                // NOTE : Use this to blend between regions
                float strength = inverseLerp(-blends[i]/2 - Epsilon, blends[i]/2,  heightPercent - heights[i]);

                float3 tintColor = tints[i] * tintStrengths[i];
                float3 textureColor = triplanar(IN.worldPos, textureScales[i], blendAxes, i) * (1 -tintStrengths[i]);

                // NOTE : o.Albedo * (1 - strength) will be the original albedo since strength is either 0 or 1
                o.Albedo = o.Albedo * (1 - strength) + (tintColor + textureColor) * strength;
            }


        }
        ENDCG
    }
    FallBack "Diffuse"
}
