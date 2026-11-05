Shader "TheLightHouse/Harrow Storm Sky"
{
    Properties { _Exposure("Exposure",Range(0,2))=.2 _CloudColor("Cloud",Color)=(.035,.05,.06,1) }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 d:TEXCOORD0; };
            half _Exposure; half4 _CloudColor;
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p) { float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y); }
            V vert(A i) { V o;o.p=TransformObjectToHClip(i.p.xyz);o.d=i.p.xyz;return o; }
            half4 frag(V i):SV_Target
            {
                float3 d=normalize(i.d);float2 p=d.xz/(abs(d.y)+.35)*2.8+_Time.y*.001;
                float n=noise(p)*.55+noise(p*2.3)*.28+noise(p*5)*.17;
                float horizon=pow(1-abs(d.y),4);
                return half4((_CloudColor.rgb*(.35+n*1.5)+horizon*float3(.014,.022,.025))*(.65+_Exposure),1);
            }
            ENDHLSL
        }
    }
}
