Shader "TheLightHouse/Harrow Sea"
{
    Properties
    {
        _DeepColor("Deep Sea Color", Color) = (0.02, 0.055, 0.095, 1.0)
        _ShallowColor("Shallow Water Color", Color) = (0.045, 0.115, 0.165, 1.0)
        _FoamColor("Sea Foam Color", Color) = (0.58, 0.68, 0.74, 1.0)
        _WaveHeight("Wave Amplitude", Range(0.0, 1.2)) = 0.22
        _WaveSpeed("Wave Speed", Range(0.1, 3.0)) = 0.95
        _FoamDistance("Shore Foam Range", Range(0.1, 3.0)) = 0.85
        _Smoothness("Surface Smoothness", Range(0.5, 0.99)) = 0.92
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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

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
                // Primary rolling North Atlantic swell
                GerstnerWave(float2(0.82, 0.48), amp * 0.70, 24.0, spd * 0.82, worldPos, waveOffset, waveNormal);
                // Crossing choppy storm swell
                GerstnerWave(float2(-0.52, 0.85), amp * 0.40, 14.0, spd * 1.05, worldPos, waveOffset, waveNormal);
                // High frequency water texture
                GerstnerWave(float2(0.38, -0.92), amp * 0.20, 6.0, spd * 1.35, worldPos, waveOffset, waveNormal);

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

                // Sample scene depth safely
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = input.screenPos.w;
                float waterDepth = sceneDepth - surfaceDepth;

                // Handle skybox or missing depth
                if (sceneDepth > 300.0 || rawDepth <= 0.00001 || waterDepth < 0.0)
                {
                    waterDepth = 20.0;
                }

                // Surface normal from high-frequency wave ripple superposition
                float t = _Time.y * _WaveSpeed;
                float u1 = input.worldPos.x * 0.42 + input.worldPos.z * 0.26 + t * 0.70;
                float v1 = input.worldPos.z * 0.52 - input.worldPos.x * 0.20 + t * 0.88;
                float u2 = input.worldPos.x * 1.35 - input.worldPos.z * 0.85 - t * 1.15;
                float v2 = input.worldPos.z * 1.55 + input.worldPos.x * 0.68 + t * 1.02;

                float3 n = normalize(float3(
                    -(sin(u1) * 0.10 + sin(u2) * 0.04),
                    1.0,
                    -(cos(v1) * 0.10 + cos(v2) * 0.04)
                ));

                // Soft shoreline depth fading: eliminates harsh white lines
                float depthFade = smoothstep(0.04, 0.45, waterDepth);

                // Animated shoreline foam lapping against coastal rocks
                float waveLapping = sin(t * 1.5 + input.worldPos.x * 0.5 + input.worldPos.z * 0.4) * 0.5 + 0.5;
                float foamNoise = sin(input.worldPos.x * 2.5 + t * 1.2) * cos(input.worldPos.z * 2.5 - t * 0.95);
                float shoreDist = waterDepth / max(0.08, _FoamDistance);
                
                // Foam only occurs where water actually contacts coastal rocks at shallow depth
                float shoreFoam = saturate((1.0 - shoreDist) * (0.75 + 0.35 * waveLapping) * (0.85 + 0.3 * foamNoise));
                shoreFoam = smoothstep(0.35, 0.85, shoreFoam) * depthFade;

                // Prevent false foam near camera near-clip plane
                float cameraProximityFade = saturate((surfaceDepth - 1.2) * 0.6);
                shoreFoam *= cameraProximityFade;

                // Lighting & cold North Atlantic Fresnel reflection
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.worldPos);
                Light mainLight = GetMainLight();

                float NdotV = saturate(dot(n, viewDir));
                float fresnel = pow(1.0 - NdotV, 4.0);

                // Moonlight specular reflection
                float3 halfDir = normalize(viewDir + mainLight.direction);
                float NdotH = saturate(dot(n, halfDir));
                float specular = pow(NdotH, 110.0) * _Smoothness;

                // Optical depth absorption
                float depthFactor = saturate(waterDepth / 2.8);
                half3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
                half3 surfaceColor = lerp(waterColor, _FoamColor.rgb, shoreFoam * 0.65);

                half3 ambientReflection = half3(0.035, 0.065, 0.095) * fresnel;
                surfaceColor += mainLight.color * (specular * (1.0 - shoreFoam * 0.4)) + ambientReflection;

                // Additional light illumination (sweeping lighthouse beacon & cottage window glow)
                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < pixelLightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.worldPos);
                    float3 addHalf = normalize(viewDir + addLight.direction);
                    float addNdotH = saturate(dot(n, addHalf));
                    float addSpec = pow(addNdotH, 75.0) * _Smoothness;
                    float addDiffuse = saturate(dot(n, addLight.direction));
                    surfaceColor += addLight.color * (addLight.distanceAttenuation * (addDiffuse * 0.35 + addSpec * 1.9));
                }
                #endif

                // Alpha blending: transparent at rock contact, opaque in deep water
                float alpha = saturate(depthFade * 0.95 + shoreFoam * 0.5);

                return half4(MixFog(surfaceColor, input.fogFactor), alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
