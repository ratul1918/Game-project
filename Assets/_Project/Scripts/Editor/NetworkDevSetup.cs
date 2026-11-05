using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class NetworkDevSetup
    {
        const string Folder="Assets/_Project/Resources";
        const string Path=Folder+"/NetworkConfig.asset";
        [MenuItem("TheLightHouse/Network/Create Dev Config")]
        public static void Create()
        {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var config=AssetDatabase.LoadAssetAtPath<NetworkConfig>(Path);if(!config){config=ScriptableObject.CreateInstance<NetworkConfig>();AssetDatabase.CreateAsset(config,Path);}EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();Selection.activeObject=config;Debug.Log("Network dev config ready at "+Path+". It uses localhost and the serialized dev account fields.");
        }
    }
}
