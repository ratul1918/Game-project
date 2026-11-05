Shader "TheLightHouse/Harrow Sea"
{
    Properties
    {
        _DeepColor("Deep Sea",Color)=(.012,.035,.045,1)
        _CrestColor("Wave Crests",Color)=(.055,.11,.12,1)
        _WaveHeight("Wave Height",Range(0,.4))=.13
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; half fog:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            half4 _DeepColor;half4 _CrestColor;float _WaveHeight;
            CBUFFER_END
            V vert(A i)
            {
                V o;float3 p=TransformObjectToWorld(i.p.xyz);float t=_Time.y;
                p.y+=_WaveHeight*(sin(p.x*.48+p.z*.25+t*.85)*.6+sin(p.z*.8-p.x*.2+t*1.2)*.4);
                o.world=p;o.p=TransformWorldToHClip(p);o.fog=ComputeFogFactor(o.p.z);return o;
            }
            half4 frag(V i):SV_Target
            {
                float t=_Time.y;float a=i.world.x*.48+i.world.z*.25+t*.85,b=i.world.z*.8-i.world.x*.2+t*1.2;
                float3 n=normalize(float3(-_WaveHeight*(.288*cos(a)-.08*cos(b)),1,-_WaveHeight*(.15*cos(a)+.32*cos(b))));
                float ripple=sin(i.world.x*3.1+i.world.z*1.7+t*1.5)*sin(i.world.z*4.5-i.world.x*.9+t*.7);
                n=normalize(n+float3(ripple*.045,0,ripple*.065));
                float3 view=GetWorldSpaceNormalizeViewDir(i.world);Light light=GetMainLight();
                float fresnel=pow(1-saturate(dot(n,view)),4);
                float specular=pow(saturate(dot(n,normalize(view+light.direction))),100)*.3;
                float crest=saturate((sin(a)+sin(b))*.5+.35);
                half3 color=lerp(_DeepColor.rgb,_CrestColor.rgb,crest*.55+fresnel*.3);
                color+=light.color*specular+fresnel*half3(.025,.04,.05);
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
}
