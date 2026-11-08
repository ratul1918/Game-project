using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace TheLightHouse.Editor
{
    public static class BakeActsData
    {
        public static void BakeAllActs()
        {
            for (int act = 1; act <= 5; act++)
            {
                BakeAct(act);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Completed NavMesh and Occlusion baking across Acts 1 to 5.");
        }

        public static void BakeAct(int act)
        {
            string scenePath = $"Assets/_Project/Scenes/Act{act}.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Scene not found at {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"Baking NavMesh and Occlusion for Act {act} ({scenePath})...");

            TagStaticGeometry();
            BakeNavMeshSurfaces(act);
            BakeOcclusion(act);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Finished Act {act} bake successfully.");
        }

        public static void TagStaticGeometry()
        {
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>())
            {
                string name = renderer.gameObject.name.ToLowerInvariant();
                if (name.Contains("player") || name.Contains("door") || name.Contains("stalker") ||
                    name.Contains("drowned") || name.Contains("figure") || name.Contains("crane") ||
                    name.Contains("boat") || name.Contains("lamp") || name.Contains("lantern"))
                    continue;

                if (renderer.GetComponentInParent<Rigidbody>() || renderer.GetComponentInParent<CharacterController>())
                    continue;

                var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                GameObjectUtility.SetStaticEditorFlags(
                    renderer.gameObject,
                    flags |
                    StaticEditorFlags.ContributeGI |
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.ReflectionProbeStatic |
                    StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.NavigationStatic);
            }
        }

        public static void BakeNavMeshSurfaces(int act)
        {
            var surfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (surfaces == null || surfaces.Length == 0)
            {
                Debug.LogWarning($"Act {act}: No NavMeshSurface found in scene.");
                return;
            }

            string dir = $"Assets/_Project/Scenes/Act{act}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            foreach (var surface in surfaces)
            {
                surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
                surface.collectObjects = Unity.AI.Navigation.CollectObjects.All;
                string assetPath = $"{dir}/NavMesh-{surface.gameObject.name}.asset";
                surface.BuildNavMesh();
                if (surface.navMeshData != null)
                {
                    var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(assetPath);
                    if (existing == null)
                    {
                        AssetDatabase.CreateAsset(surface.navMeshData, assetPath);
                    }
                    else
                    {
                        EditorUtility.CopySerialized(surface.navMeshData, existing);
                        surface.navMeshData = existing;
                    }
                    EditorUtility.SetDirty(surface);
                }
            }
        }

        public static void BakeOcclusion(int act)
        {
            StaticOcclusionCulling.smallestOccluder = 2.0f;
            StaticOcclusionCulling.smallestHole = 0.25f;
            StaticOcclusionCulling.backfaceThreshold = 100f;
            StaticOcclusionCulling.Compute();
        }
    }
}
