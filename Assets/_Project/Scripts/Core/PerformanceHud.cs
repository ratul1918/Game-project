using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    public sealed class PerformanceHud : MonoBehaviour
    {
        [SerializeField] bool visibleByDefault;
        [SerializeField,Min(.05f)] float refreshInterval=.25f;
        [SerializeField] Rect panelRect=new(16f,16f,310f,150f);
        [SerializeField,Min(8)] int fontSize=14;
        bool visible;float nextRefresh,fps,frameMs;int realtimeLights;long managedBytes,totalBytes,drawCalls;
        ProfilerRecorder totalMemoryRecorder,drawCallRecorder;
        void Awake(){visible=visibleByDefault;totalMemoryRecorder=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory");drawCallRecorder=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");}
        void OnDestroy(){totalMemoryRecorder.Dispose();drawCallRecorder.Dispose();}
        void Update()
        {
            if(Keyboard.current!=null&&Keyboard.current.f4Key.wasPressedThisFrame)visible=!visible;
            if(Time.unscaledTime<nextRefresh)return;nextRefresh=Time.unscaledTime+refreshInterval;frameMs=Time.unscaledDeltaTime*1000f;fps=frameMs>0f?1000f/frameMs:0f;managedBytes=GC.GetTotalMemory(false);totalBytes=totalMemoryRecorder.Valid?totalMemoryRecorder.LastValue:0;drawCalls=drawCallRecorder.Valid?drawCallRecorder.LastValue:0;realtimeLights=0;
            foreach(Light light in FindObjectsByType<Light>())if(light.enabled&&light.gameObject.activeInHierarchy&&light.lightmapBakeType!=LightmapBakeType.Baked)realtimeLights++;
        }
        void OnGUI()
        {
            if(!visible)return;GUIStyle style=new(GUI.skin.box){alignment=TextAnchor.UpperLeft,fontSize=fontSize};string text=$"Performance (F4)\nFPS {fps:0}  Frame {frameMs:0.0} ms\nManaged {ToMb(managedBytes):0.0} MB\nTotal {ToMb(totalBytes):0.0} MB\nRealtime lights {realtimeLights}\nDraw calls {drawCalls}";GUI.Box(panelRect,text,style);
        }
        static float ToMb(long bytes)=>bytes/(1024f*1024f);
    }
}
