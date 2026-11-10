using UnityEngine;

namespace TheLightHouse
{
    public static class TowerWindowEnclosure
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            EnsureTowerWindows();
        }

        public static void EnsureTowerWindows()
        {
            var windowsRoot = GameObject.Find("Windows");
            if (!windowsRoot) return;

            Material windowMat = null;
#if UNITY_EDITOR
            windowMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Cold Window.mat");
#endif

            for (int i = 0; i < windowsRoot.transform.childCount; i++)
            {
                var child = windowsRoot.transform.GetChild(i);
                if (child.name.Contains("Window"))
                {
                    child.gameObject.SetActive(true);
                    var mr = child.GetComponent<MeshRenderer>();
                    if (mr)
                    {
                        mr.enabled = true;
                        if (windowMat && (mr.sharedMaterial == null || mr.sharedMaterial.name == "Metal"))
                        {
                            mr.sharedMaterial = windowMat;
                        }
                    }
                }
            }
        }
    }
}
