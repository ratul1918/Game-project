using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using TheLightHouse.Voice;

namespace TheLightHouse.Editor
{
    public static class GameAudioMixerBuilder
    {
        public const string MixerPath = "Assets/_Project/Audio/Mixers/GameAudioMixer.mixer";
        public const string SpeakersFolder = "Assets/_Project/Audio/Voice/Speakers";

        [MenuItem("TheLightHouse/Audio/Build Game Audio Mixer")]
        public static AudioMixer BuildMixer()
        {
            Directory.CreateDirectory("Assets/_Project/Audio/Mixers");
            Directory.CreateDirectory(SpeakersFolder);

            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            var asm = typeof(EditorWindow).Assembly;
            var controllerType = asm.GetType("UnityEditor.Audio.AudioMixerController");
            var groupType = asm.GetType("UnityEditor.Audio.AudioMixerGroupController");
            var paramType = asm.GetType("UnityEditor.Audio.ExposedAudioParameter");

            if (mixer == null)
            {
                var createMethod = controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Public | BindingFlags.Static);
                mixer = (AudioMixer)createMethod.Invoke(null, new object[] { MixerPath });
                AssetDatabase.ImportAsset(MixerPath);
            }

            // Inspect or create child groups
            var masterProp = controllerType.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var masterGroup = masterProp.GetValue(mixer);

            var createGroupMethod = controllerType.GetMethod("CreateNewGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(bool) }, null);
            var addChildMethod = controllerType.GetMethod("AddChildToParent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { groupType, groupType }, null);
            var getAllGroupsMethod = controllerType.GetMethod("GetAllAudioGroupsSlow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var allGroups = (System.Collections.IList)getAllGroupsMethod.Invoke(mixer, null);

            string[] requiredGroups = { "Voice", "Radio", "SFX", "Ambience", "Music", "Thomas Crane", "The Drowned" };
            foreach (var gName in requiredGroups)
            {
                bool exists = false;
                foreach (var g in allGroups)
                {
                    var nameProp = groupType.GetProperty("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if ((string)nameProp.GetValue(g) == gName) { exists = true; break; }
                }

                if (!exists)
                {
                    var newGroup = createGroupMethod.Invoke(mixer, new object[] { gName, false });
                    addChildMethod.Invoke(mixer, new object[] { newGroup, masterGroup });
                }
            }

            // Expose volume parameters
            var exposedProp = controllerType.GetProperty("exposedParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var currentExposed = (Array)exposedProp.GetValue(mixer);
            var exposedList = currentExposed != null ? currentExposed.Cast<object>().ToList() : new System.Collections.Generic.List<object>();

            allGroups = (System.Collections.IList)getAllGroupsMethod.Invoke(mixer, null);
            var getGuidMethod = groupType.GetMethod("GetGUIDForVolume", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var paramMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Master", "MasterVolume" },
                { "Voice", "VoiceVolume" },
                { "Radio", "RadioVolume" },
                { "SFX", "SfxVolume" },
                { "Ambience", "AmbienceVolume" },
                { "Music", "MusicVolume" }
            };

            foreach (var kvp in paramMap)
            {
                object targetGroup = null;
                foreach (var g in allGroups)
                {
                    var nameProp = groupType.GetProperty("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if ((string)nameProp.GetValue(g) == kvp.Key) { targetGroup = g; break; }
                }

                if (targetGroup != null)
                {
                    var guid = (GUID)getGuidMethod.Invoke(targetGroup, null);
                    bool alreadyExposed = false;
                    foreach (var exp in exposedList)
                    {
                        var expNameField = paramType.GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if ((string)expNameField.GetValue(exp) == kvp.Value) { alreadyExposed = true; break; }
                    }

                    if (!alreadyExposed)
                    {
                        var newParam = Activator.CreateInstance(paramType);
                        paramType.GetField("guid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newParam, guid);
                        paramType.GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newParam, kvp.Value);
                        exposedList.Add(newParam);
                    }
                }
            }

            var newArray = Array.CreateInstance(paramType, exposedList.Count);
            for (int i = 0; i < exposedList.Count; i++) newArray.SetValue(exposedList[i], i);
            exposedProp.SetValue(mixer, newArray);

            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();

            BuildSpeakers(mixer);
            Debug.Log($"[GameAudioMixerBuilder] Audio mixer successfully configured at {MixerPath}.");
            return mixer;
        }

        private static void BuildSpeakers(AudioMixer mixer)
        {
            var groups = mixer.FindMatchingGroups("");
            AudioMixerGroup FindGroup(string name) => groups.FirstOrDefault(g => g.name == name) ?? groups.FirstOrDefault(g => g.name == "Master");

            CreateOrUpdateSpeaker(SpeakerId.DanielHale, "Daniel Hale", FindGroup("Voice"), 0f, new Vector2(0.99f, 1.01f));
            CreateOrUpdateSpeaker(SpeakerId.Boatman, "The Boatman", FindGroup("Voice"), 1.0f, new Vector2(0.97f, 1.01f));
            CreateOrUpdateSpeaker(SpeakerId.Marta, "Marta", FindGroup("Radio"), 0.5f, new Vector2(1.0f, 1.0f));
            CreateOrUpdateSpeaker(SpeakerId.ThomasCrane, "Thomas Crane", FindGroup("Thomas Crane"), 1.5f, new Vector2(0.95f, 1.03f));
            CreateOrUpdateSpeaker(SpeakerId.TheDrowned, "The Drowned", FindGroup("The Drowned"), -1.0f, new Vector2(0.92f, 1.04f));

            AssetDatabase.SaveAssets();
        }

        private static void CreateOrUpdateSpeaker(SpeakerId id, string displayName, AudioMixerGroup group, float trimDb, Vector2 pitchRange)
        {
            string path = $"{SpeakersFolder}/Speaker_{id}.asset";
            var speaker = AssetDatabase.LoadAssetAtPath<VoiceSpeaker>(path);
            if (!speaker)
            {
                speaker = ScriptableObject.CreateInstance<VoiceSpeaker>();
                AssetDatabase.CreateAsset(speaker, path);
            }

            speaker.SpeakerId = id;
            speaker.DisplayName = displayName;
            speaker.OutputGroup = group;
            speaker.VolumeTrimDb = trimDb;
            speaker.PitchRandomRange = pitchRange;
            EditorUtility.SetDirty(speaker);
        }
    }
}
