Shader "TheLightHouse/Lamp Beam URP"
{
    Properties { _BeamColor("Beam Color",Color)=(1,.75,.35,.16) _BeamIntensity("Beam Intensity",Range(0,2))=1 _EdgeSoftness("Edge Softness",Range(.25,8))=2 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial) half4 _BeamColor; half _BeamIntensity; half _EdgeSoftness; CBUFFER_END
            Varyings vert(Attributes input){Varyings output;output.positionHCS=TransformObjectToHClip(input.positionOS.xyz);output.uv=input.uv;return output;}
            half4 frag(Varyings input):SV_Target{half edge=pow(saturate(1-input.uv.x),_EdgeSoftness);half lengthFade=saturate(input.uv.y*(1-input.uv.y)*4);return half4(_BeamColor.rgb,_BeamColor.a*edge*lengthFade*_BeamIntensity);}
            ENDHLSL
        }
    }
}
