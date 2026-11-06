Shader "TheLightHouse/Lamp Beam URP"
{
    Properties
    {
        _BeamColor("Beam Color", Color) = (1.0, 0.88, 0.65, 0.18)
        _BeamIntensity("Beam Intensity", Range(0, 3)) = 1.0
        _EdgeSoftness("Edge Softness", Range(0.5, 6)) = 2.5
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent+10" 
            "RenderType" = "Transparent" 
        }

        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BeamColor;
                half _BeamIntensity;
                half _EdgeSoftness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.viewDir = GetWorldSpaceNormalizeViewDir(worldPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Soft length fade: smooth entry at apex, gradual atmospheric fadeout at far end
                float lengthFade = smoothstep(0.01, 0.15, input.uv.y) * smoothstep(1.0, 0.45, input.uv.y);

                // Volumetric core density: thickest along the cone center, smoothly fading to 0 at the outer silhouette
                float NdotV = saturate(abs(dot(normalize(input.worldNormal), normalize(input.viewDir))));
                float coreGlow = pow(NdotV, 1.0 / max(0.2, _EdgeSoftness));

                float alpha = saturate(coreGlow * lengthFade * _BeamColor.a * _BeamIntensity);

                return half4(_BeamColor.rgb * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
