Shader "TheLightHouse/Harrow Sea"
{
    Properties
    {
        _DeepColor("Deep Sea Color", Color) = (0.03, 0.08, 0.12, 1.0)
        _ShallowColor("Shallow Water Color", Color) = (0.06, 0.17, 0.21, 1.0)
        _FoamColor("Sea Foam Color", Color) = (0.75, 0.82, 0.86, 1.0)
        _WaveHeight("Wave Amplitude", Range(0.0, 0.5)) = 0.15
        _WaveSpeed("Wave Speed", Range(0.1, 2.5)) = 0.9
        _FoamDistance("Shore Foam Range", Range(0.1, 2.0)) = 0.75
        _Smoothness("Surface Smoothness", Range(0.5, 0.98)) = 0.90
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _FoamColor;
                float _WaveHeight;
                float _WaveSpeed;
                float _FoamDistance;
                float _Smoothness;
            CBUFFER_END

            void GerstnerWave(float2 dir, float steepness, float wavelength, float speed, float3 pos, inout float3 offset, inout float3 normal)
            {
                float k = 2.0 * PI / wavelength;
                float c = sqrt(9.8 / k) * speed;
                float2 d = normalize(dir);
                float f = k * (dot(d, pos.xz) - c * _Time.y);
                float a = steepness / k;

                offset.x += d.x * (a * cos(f));
                offset.y += a * sin(f);
                offset.z += d.y * (a * cos(f));

                normal.x -= d.x * (steepness * sin(f));
                normal.y -= steepness * cos(f);
                normal.z -= d.y * (steepness * sin(f));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                float3 waveOffset = float3(0, 0, 0);
                float3 waveNormal = float3(0, 1, 0);

                float amp = _WaveHeight;
                float spd = _WaveSpeed;
                GerstnerWave(float2(0.85, 0.52), amp * 0.8, 16.0, spd * 0.9, worldPos, waveOffset, waveNormal);
                GerstnerWave(float2(-0.45, 0.89), amp * 0.5, 9.5, spd * 1.1, worldPos, waveOffset, waveNormal);
                GerstnerWave(float2(0.25, -0.96), amp * 0.3, 4.5, spd * 1.3, worldPos, waveOffset, waveNormal);

                worldPos += waveOffset;
                output.worldPos = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / max(0.0001, input.screenPos.w);

                // Depth sampling for smooth water-shore contact
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = input.screenPos.w;
                float waterDepth = max(0.0, sceneDepth - surfaceDepth);

                // Animated high-frequency wind ripples
                float t = _Time.y * _WaveSpeed;
                float u1 = input.worldPos.x * 0.35 + input.worldPos.z * 0.22 + t * 0.65;
                float v1 = input.worldPos.z * 0.45 - input.worldPos.x * 0.18 + t * 0.85;
                float u2 = input.worldPos.x * 1.2 - input.worldPos.z * 0.8 - t * 1.1;
                float v2 = input.worldPos.z * 1.4 + input.worldPos.x * 0.6 + t * 0.95;

                float3 n = normalize(float3(
                    -(sin(u1) * 0.08 + sin(u2) * 0.035),
                    1.0,
                    -(cos(v1) * 0.08 + cos(v2) * 0.035)
                ));

                // Natural organic shoreline surf foam
                float foamNoise = sin(input.worldPos.x * 1.8 + t * 1.2) * cos(input.worldPos.z * 1.8 - t * 0.9);
                float shoreDistNorm = waterDepth / max(0.02, _FoamDistance);
                float shoreFoam = saturate((1.0 - shoreDistNorm) * (1.0 + 0.25 * foamNoise));
                shoreFoam = smoothstep(0.15, 0.85, shoreFoam);

                float totalFoam = shoreFoam;

                // Lighting & cold North Atlantic reflection
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.worldPos);
                Light light = GetMainLight();

                float NdotV = saturate(dot(n, viewDir));
                float fresnel = pow(1.0 - NdotV, 4.0);

                float3 halfDir = normalize(viewDir + light.direction);
                float NdotH = saturate(dot(n, halfDir));
                float specular = pow(NdotH, 90.0) * _Smoothness;

                // Optical color depth absorption
                float depthFactor = saturate(waterDepth / 2.5);
                half3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
                half3 surfaceColor = lerp(waterColor, _FoamColor.rgb, totalFoam);

                half3 ambientReflection = half3(0.03, 0.06, 0.08) * fresnel;
                surfaceColor += light.color * (specular * (1.0 - totalFoam * 0.4)) + ambientReflection;

                // Soft alpha blending along shoreline
                float alpha = saturate(waterDepth * 3.0 + totalFoam * 0.85);

                return half4(MixFog(surfaceColor, input.fogFactor), alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
