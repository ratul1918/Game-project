using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLightHouse.EditorTools
{
    public static class AssetImportAndPlacementUtility
    {
        private static readonly string[] RequiredPacks = new string[]
        {
            "Crest Ocean System",
            "Weather Maker",
            "Rocky Coast / Coastal Cliff",
            "Lighthouse & Keeper's Quarters",
            "Modular Wooden Pier",
            "Modular Dungeon Stone Cellar",
            "Old Cellar Props",
            "Modular Castle & Spiral Stairs",
            "Vintage Radio Station",
            "Horror Props Mega Pack",
            "Vintage Lanterns & Oil Lamps"
        };

        [MenuItem("The LightHouse/Assets/Check Asset Store Package Status")]
        public static void CheckPackageStatus()
        {
            Debug.Log("<b>[The LightHouse] Scanning for Target Asset Store Packages...</b>");

            string artRoot = Path.Combine(Application.dataPath, "_Project", "Art");
            string envRoot = Path.Combine(artRoot, "Environment");
            string propsRoot = Path.Combine(artRoot, "Props");

            EnsureDirectory(Path.Combine(envRoot, "Ocean_Weather"));
            EnsureDirectory(Path.Combine(envRoot, "Cliffs_Rocks"));
            EnsureDirectory(Path.Combine(envRoot, "Lighthouse_Exterior"));
            EnsureDirectory(Path.Combine(envRoot, "Pier_Dock"));
            EnsureDirectory(Path.Combine(envRoot, "Cellar_Underground"));
            EnsureDirectory(Path.Combine(propsRoot, "Cellar_Storage"));
            EnsureDirectory(Path.Combine(propsRoot, "Radio_Electronics"));
            EnsureDirectory(Path.Combine(propsRoot, "Horror_Ledger"));
            EnsureDirectory(Path.Combine(propsRoot, "Lanterns_Lighting"));

            int detectedCount = 0;
            string[] allDirectories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

            foreach (string pack in RequiredPacks)
            {
                bool found = false;
                string searchKeyword = pack.Split('/')[0].Trim().ToLowerInvariant();

                foreach (string dir in allDirectories)
                {
                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                    if (dirName.Contains(searchKeyword) || (searchKeyword.Contains(" ") && dirName.Contains(searchKeyword.Replace(" ", ""))))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    Debug.Log($"<color=#55FF55>✔ Found Installed Pack: {pack}</color>");
                    detectedCount++;
                }
                else
                {
                    Debug.LogWarning($"<color=#FFAA33>⏳ Pending Import: {pack}</color> (Import via Window > Package Manager > My Assets)");
                }
            }

            Debug.Log($"<b>[The LightHouse] Scan complete: {detectedCount} of {RequiredPacks.Length} packs present.</b>");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
