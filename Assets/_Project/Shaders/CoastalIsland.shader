Shader "TheLightHouse/CoastalIsland"
{
    Properties
    {
        _RockTex("Rock Albedo", 2D) = "white" {}
        _RockNormal("Rock Normal Map", 2D) = "bump" {}
        _MudTex("Wet Mud / Coastal Soil Albedo", 2D) = "white" {}
        _MudNormal("Wet Mud Normal Map", 2D) = "bump" {}
        _MudMask("Wet Mud Mask (R:Met, G:AO, B:Detail, A:Smooth)", 2D) = "white" {}
        _MudColor("Wet Mud Tint", Color) = (0.18, 0.16, 0.14, 1.0)
        _WetSandColor("Wet Shoreline Sand Tint", Color) = (0.07, 0.09, 0.10, 1.0)
        _CliffColor("Cliff Rock Tint", Color) = (0.24, 0.26, 0.28, 1.0)
        _SeaLevel("Sea Level", Float) = -0.55
        _TideRange("Wet Tide Height Range", Range(0.1, 2.5)) = 0.95
        _SlopeContrast("Slope Cliff Sharpness", Range(1.0, 14.0)) = 6.0
        _SlopeThreshold("Slope Cliff Threshold", Range(0.2, 0.9)) = 0.65
        _MudScale("Mud Triplanar UV Scale", Float) = 0.65
        _RockScale("Rock Triplanar UV Scale", Float) = 0.35
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldTangent : TEXCOORD2;
                float3 worldBitangent : TEXCOORD3;
                half fogFactor : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _MudColor;
                half4 _WetSandColor;
                half4 _CliffColor;
                float _SeaLevel;
                float _TideRange;
                float _SlopeContrast;
                float _SlopeThreshold;
                float _MudScale;
                float _RockScale;
            CBUFFER_END

            float _GlobalWetness;
            float _GlobalWetSmoothnessBoost;

            TEXTURE2D(_RockTex); SAMPLER(sampler_RockTex);
            TEXTURE2D(_RockNormal); SAMPLER(sampler_RockNormal);
            TEXTURE2D(_MudTex); SAMPLER(sampler_MudTex);
            TEXTURE2D(_MudNormal); SAMPLER(sampler_MudNormal);
            TEXTURE2D(_MudMask); SAMPLER(sampler_MudMask);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInputs.positionCS;
                output.worldPos = posInputs.positionWS;
                output.worldNormal = normInputs.normalWS;
                output.worldTangent = normInputs.tangentWS;
                output.worldBitangent = normInputs.bitangentWS;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 worldPos = input.worldPos;
                float3 geomNormal = normalize(input.worldNormal);

                // Triplanar slope blend factor: steep cliffs vs terraced flat ground
                float slope = saturate(geomNormal.y);
                float cliffFactor = saturate((_SlopeThreshold - slope) * _SlopeContrast);

                // Altitude tide blend factor: wet shoreline sand vs upper soil
                float tideDist = worldPos.y - _SeaLevel;
                float wetShoreFactor = saturate(1.0 - (tideDist / max(0.01, _TideRange)));
                wetShoreFactor = smoothstep(0.02, 0.98, wetShoreFactor);

                // Cart road / path mask from dock (z = -34) to courtyard (z = -10)
                float pathCenter = -0.8 + 2.2 * sin(saturate((-34.0 - worldPos.z) / -23.5) * 3.14159);
                float distToPath = abs(worldPos.x - pathCenter);
                float inZRange = step(-35.0, worldPos.z) * step(worldPos.z, -10.0);
                float pathMask = saturate(1.0 - distToPath / 1.7) * inZRange;
                pathMask = smoothstep(0.15, 0.85, pathMask);

                // Triplanar mapping weights (eliminates vertical UV stretching on slopes)
                float3 triWeights = abs(geomNormal);
                triWeights = max(float3(0, 0, 0), triWeights - 0.15);
                triWeights /= (triWeights.x + triWeights.y + triWeights.z + 0.0001);

                // --- 1. TRIPLANAR ROCK CLIFF SAMPLES ---
                float2 rockUV_X = worldPos.yz * _RockScale;
                float2 rockUV_Y = worldPos.xz * _RockScale;
                float2 rockUV_Z = worldPos.xy * _RockScale;

                half4 rockDiffX = SAMPLE_TEXTURE2D(_RockTex, sampler_RockTex, rockUV_X);
                half4 rockDiffY = SAMPLE_TEXTURE2D(_RockTex, sampler_RockTex, rockUV_Y);
                half4 rockDiffZ = SAMPLE_TEXTURE2D(_RockTex, sampler_RockTex, rockUV_Z);
                half4 rockAlbedo = rockDiffX * triWeights.x + rockDiffY * triWeights.y + rockDiffZ * triWeights.z;
                rockAlbedo.rgb *= _CliffColor.rgb * 1.35;

                half3 rockNormX = UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal, sampler_RockNormal, rockUV_X));
                half3 rockNormY = UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal, sampler_RockNormal, rockUV_Y));
                half3 rockNormZ = UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal, sampler_RockNormal, rockUV_Z));
                half3 rockNormTS = rockNormX * triWeights.x + rockNormY * triWeights.y + rockNormZ * triWeights.z;

                // --- 2. TRIPLANAR WET MUD / SOIL SAMPLES ---
                float2 mudUV_X = worldPos.yz * _MudScale;
                float2 mudUV_Y = worldPos.xz * _MudScale;
                float2 mudUV_Z = worldPos.xy * _MudScale;

                half4 mudDiffX = SAMPLE_TEXTURE2D(_MudTex, sampler_MudTex, mudUV_X);
                half4 mudDiffY = SAMPLE_TEXTURE2D(_MudTex, sampler_MudTex, mudUV_Y);
                half4 mudDiffZ = SAMPLE_TEXTURE2D(_MudTex, sampler_MudTex, mudUV_Z);
                half4 mudAlbedo = mudDiffX * triWeights.x + mudDiffY * triWeights.y + mudDiffZ * triWeights.z;
                mudAlbedo.rgb *= _MudColor.rgb * 1.55;

                half3 mudNormX = UnpackNormal(SAMPLE_TEXTURE2D(_MudNormal, sampler_MudNormal, mudUV_X));
                half3 mudNormY = UnpackNormal(SAMPLE_TEXTURE2D(_MudNormal, sampler_MudNormal, mudUV_Y));
                half3 mudNormZ = UnpackNormal(SAMPLE_TEXTURE2D(_MudNormal, sampler_MudNormal, mudUV_Z));
                half3 mudNormTS = mudNormX * triWeights.x + mudNormY * triWeights.y + mudNormZ * triWeights.z;

                half4 mudMaskX = SAMPLE_TEXTURE2D(_MudMask, sampler_MudMask, mudUV_X);
                half4 mudMaskY = SAMPLE_TEXTURE2D(_MudMask, sampler_MudMask, mudUV_Y);
                half4 mudMaskZ = SAMPLE_TEXTURE2D(_MudMask, sampler_MudMask, mudUV_Z);
                half4 mudMask = mudMaskX * triWeights.x + mudMaskY * triWeights.y + mudMaskZ * triWeights.z;

                // --- 3. LAYER COMPOSITION & PBR PROPERTIES ---
                // Base surface is the deep dark wet coastal mud / churned earth
                half3 albedo = mudAlbedo.rgb;

                // Darken & saturate cart path ruts with standing wet grit
                half3 pathAlbedo = mudAlbedo.rgb * half3(0.55, 0.50, 0.44);
                albedo = lerp(albedo, pathAlbedo, pathMask * 0.85);

                // Blend steep rock cliff faces
                albedo = lerp(albedo, rockAlbedo.rgb, cliffFactor);

                // Blend waterlogged shoreline sand and tidal silt near sea level
                half3 wetSandAlbedo = _WetSandColor.rgb * (rockAlbedo.rgb * 0.65 + 0.35);
                albedo = lerp(albedo, wetSandAlbedo, wetShoreFactor);

                // Height-based dampness and moss gradient near base and cellar
                float dampFactor = saturate((1.2 - worldPos.y) * 0.5) + step(worldPos.y, 0.0) * 0.4;
                float effectiveWetness = saturate(_GlobalWetness + dampFactor * 0.65);
                albedo = lerp(albedo, albedo * half3(0.65, 0.68, 0.58), dampFactor * 0.45); // Mossy damp tint
                albedo *= (1.0 - effectiveWetness * 0.28); // Darker albedo with wetness

                // Smoothness / Wetness calculation:
                // Base mud smoothness from mask (high 0.85–0.95 in hollows/depressions)
                float smoothness = mudMask.a;
                // Cliff rock wetness
                smoothness = lerp(smoothness, 0.35, cliffFactor);
                // Heavy water pooling in cart ruts
                smoothness = lerp(smoothness, 0.88, pathMask * 0.75);
                // Shoreline tidal saturation: near 0.95 specular reflection
                smoothness = lerp(smoothness, 0.94, wetShoreFactor);
                smoothness = saturate(smoothness + effectiveWetness * _GlobalWetSmoothnessBoost + dampFactor * 0.35);

                // Ambient Occlusion
                float ao = mudMask.g;
                ao = lerp(ao, 1.0, cliffFactor * 0.5);

                // Combined Tangent Normal
                float3 mixedNormalTS = lerp(mudNormTS, rockNormTS, cliffFactor);
                float3 worldNormal = normalize(
                    input.worldTangent * mixedNormalTS.x +
                    input.worldBitangent * mixedNormalTS.y +
                    geomNormal * mixedNormalTS.z
                );

                // --- 4. LIGHTING & SPECULAR REFLECTION ---
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(worldPos));
                float NdotL = saturate(dot(worldNormal, mainLight.direction));
                half3 directLighting = mainLight.color * (NdotL * mainLight.shadowAttenuation);

                // Specular highlight from moonlight / twilight across wet mud and tide pools
                float3 viewDir = GetWorldSpaceNormalizeViewDir(worldPos);
                float3 halfDir = normalize(viewDir + mainLight.direction);
                float NdotH = saturate(dot(worldNormal, halfDir));
                float specPower = lerp(20.0, 140.0, smoothness);
                float specular = pow(NdotH, specPower) * smoothness * 0.85;
                directLighting += mainLight.color * specular * mainLight.shadowAttenuation;

                // Additional lights (dock lantern, porch sconces) with specular reflections
                half3 additionalLighting = half3(0, 0, 0);
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < pixelLightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, worldPos);
                    float addNdotL = saturate(dot(worldNormal, addLight.direction));
                    half3 lightContrib = addLight.color * (addNdotL * addLight.distanceAttenuation * addLight.shadowAttenuation);

                    // Add localized specular from lanterns on wet mud and puddle pools
                    float3 addHalf = normalize(viewDir + addLight.direction);
                    float addNdotH = saturate(dot(worldNormal, addHalf));
                    float addSpec = pow(addNdotH, specPower) * smoothness * 1.2;
                    lightContrib += addLight.color * addSpec * addLight.distanceAttenuation * addLight.shadowAttenuation;

                    additionalLighting += lightContrib;
                }
                #endif

                // Ambient lighting modulated by ambient occlusion
                half3 ambient = SampleSH(worldNormal) * ao;

                half3 finalColor = albedo * (directLighting + additionalLighting + ambient);

                return half4(MixFog(finalColor, input.fogFactor), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
