Shader "TheLightHouse/Harrow Storm Sky"
{
    Properties
    {
        _SkyZenithColor("Zenith Dark Slate", Color) = (0.04, 0.065, 0.095, 1.0)
        _SkyHorizonColor("Horizon Fog Charcoal", Color) = (0.05, 0.075, 0.098, 1.0)
        _CloudDarkColor("Cloud Dark Shadow", Color) = (0.02, 0.03, 0.045, 1.0)
        _CloudHighlightColor("Cloud Highlight Slate", Color) = (0.09, 0.13, 0.17, 1.0)
        _MoonGlowColor("Lunar Glow Color", Color) = (0.28, 0.36, 0.45, 1.0)
        _MoonDirection("Moon Direction", Vector) = (-0.55, 0.45, 0.70, 0.0)
        _WindSpeed("Wind Speed", Range(0.01, 0.5)) = 0.06
        _Exposure("Exposure Boost", Range(0.1, 3.0)) = 1.0
        _LightningIntensity("Lightning Flash Boost", Range(0.0, 5.0)) = 0.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Background" 
            "RenderType" = "Background" 
            "PreviewType" = "Skybox" 
        }
        Cull Off 
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _SkyZenithColor;
                half4 _SkyHorizonColor;
                half4 _CloudDarkColor;
                half4 _CloudHighlightColor;
                half4 _MoonGlowColor;
                float4 _MoonDirection;
                float _WindSpeed;
                float _Exposure;
                float _LightningIntensity;
            CBUFFER_END

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float total = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                for (int i = 0; i < 4; i++)
                {
                    total += valueNoise(p * freq) * amp;
                    freq *= 2.05;
                    amp *= 0.5;
                }
                return total;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDir);
                float upFactor = saturate(dir.y);

                // Sky atmospheric gradient
                half3 skyBase = lerp(_SkyHorizonColor.rgb, _SkyZenithColor.rgb, pow(upFactor, 0.65));

                // Volumetric Overcast Storm Cloud projection
                float2 windDir = float2(0.85, 0.52);
                float timeOffset = _Time.y * _WindSpeed;
                float2 uv = (dir.xz / max(0.12, dir.y + 0.28)) * 1.8;

                float n1 = fbm(uv + windDir * timeOffset);
                float n2 = fbm(uv * 1.8 - windDir * (timeOffset * 1.35) + float2(1.4, 2.7));
                float stormClouds = saturate((n1 * 0.65 + n2 * 0.35) * 1.45 - 0.2);

                // Lunar glow through thin cloud overcast
                float3 moonDir = normalize(_MoonDirection.xyz);
                float moonDot = saturate(dot(dir, moonDir));
                float moonHalo = pow(moonDot, 18.0) * 0.8 + pow(moonDot, 4.0) * 0.35;
                half3 lunarGlow = _MoonGlowColor.rgb * moonHalo * (1.0 - stormClouds * 0.7);

                // Cloud shading: dark heavy storm bellies with slate-blue edge rim
                half3 cloudColor = lerp(_CloudDarkColor.rgb, _CloudHighlightColor.rgb, pow(stormClouds, 1.5));
                cloudColor += lunarGlow * (stormClouds * 0.5 + 0.5);

                // Blend sky and clouds
                half3 finalColor = lerp(skyBase, cloudColor, saturate(stormClouds * 1.25) * saturate(upFactor * 3.5 + 0.1));

                // Horizon fog blend: smooth transition into sea-level dense fog
                float horizonFog = pow(1.0 - abs(dir.y), 3.5);
                finalColor = lerp(finalColor, _SkyHorizonColor.rgb, horizonFog * 0.95);

                // Lightning flash burst
                half3 lightningColor = half3(0.65, 0.78, 0.95) * _LightningIntensity;
                finalColor += lightningColor * (0.4 + stormClouds * 0.6);

                finalColor *= _Exposure;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
