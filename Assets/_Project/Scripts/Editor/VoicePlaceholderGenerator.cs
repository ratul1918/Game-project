using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using TheLightHouse.Voice;
using Debug = UnityEngine.Debug;

namespace TheLightHouse.Editor
{
    public static class VoicePlaceholderGenerator
    {
        [MenuItem("TheLightHouse/Voice/Generate Placeholder Voices (macOS say)")]
        public static void GeneratePlaceholders()
        {
            if (Application.platform != RuntimePlatform.OSXEditor)
            {
                Debug.LogWarning("[VoicePlaceholderGenerator] Placeholder generation requires macOS 'say' and 'afconvert' utilities.");
                return;
            }

            var db = AssetDatabase.LoadAssetAtPath<VoiceDatabase>(VoiceDatabaseBuilder.DatabasePath);
            if (!db) db = VoiceDatabaseBuilder.BuildDatabase();

            string danielVoice = "Daniel";
            string boatmanVoice = "Ralph";
            string martaVoice = "Samantha";

            int generated = 0;
            int skipped = 0;

            foreach (var line in db.AllLines)
            {
                if (line == null) continue;

                // Skip non-speech audio types (creatures, hums, breaths, reactions)
                if (line.AudioType == VoiceAudioType.Hum ||
                    line.AudioType == VoiceAudioType.Sung ||
                    line.AudioType == VoiceAudioType.Whisper ||
                    line.AudioType == VoiceAudioType.Reaction)
                {
                    skipped++;
                    continue;
                }

                // If line already has non-placeholder audio clip, never overwrite
                if (line.Clip != null && !line.IsPlaceholder)
                {
                    skipped++;
                    continue;
                }

                string speakerVoice = line.Speaker switch
                {
                    SpeakerId.DanielHale => danielVoice,
                    SpeakerId.Boatman => boatmanVoice,
                    SpeakerId.Marta => martaVoice,
                    _ => danielVoice
                };

                string targetDir = $"Assets/_Project/Audio/Voice/{line.Speaker}";
                Directory.CreateDirectory(targetDir);

                string aiffPath = Path.Combine(targetDir, $"{line.LineId}.aiff");
                string wavPath = Path.Combine(targetDir, $"{line.LineId}.wav");

                bool success = GenerateTts(speakerVoice, line.Text, aiffPath, wavPath);
                if (success)
                {
                    line.IsPlaceholder = true;
                    generated++;
                }
            }

            AssetDatabase.Refresh();
            VoiceClipImporter.ImportClips();
            Debug.Log($"[VoicePlaceholderGenerator] Complete: Generated {generated} placeholder voice clips; skipped {skipped} non-speech or final clips.");
        }

        private static bool GenerateTts(string voice, string text, string aiffPath, string wavPath)
        {
            try
            {
                // Clean text of quotes and special characters for shell
                string cleanText = text.Replace("\"", "").Replace("'", "").Replace("\n", " ");

                // 1. Run say to AIFF
                var psiSay = new ProcessStartInfo
                {
                    FileName = "/usr/bin/say",
                    Arguments = $"-v \"{voice}\" -o \"{aiffPath}\" \"{cleanText}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                using (var proc = Process.Start(psiSay))
                {
                    proc.WaitForExit(5000);
                    if (proc.ExitCode != 0)
                    {
                        // Fallback to default voice
                        var fallbackPsi = new ProcessStartInfo
                        {
                            FileName = "/usr/bin/say",
                            Arguments = $"-o \"{aiffPath}\" \"{cleanText}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using var fallbackProc = Process.Start(fallbackPsi);
                        fallbackProc.WaitForExit(5000);
                    }
                }

                if (!File.Exists(aiffPath)) return false;

                // 2. Convert to standard 16-bit PCM WAV
                var psiConvert = new ProcessStartInfo
                {
                    FileName = "/usr/bin/afconvert",
                    Arguments = $"-f WAVE -d LEI16 \"{aiffPath}\" \"{wavPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                using (var procConv = Process.Start(psiConvert))
                {
                    procConv.WaitForExit(5000);
                }

                if (File.Exists(aiffPath))
                {
                    File.Delete(aiffPath);
                }

                return File.Exists(wavPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VoicePlaceholderGenerator] Failed to generate TTS for \"{text}\": {ex.Message}");
                return false;
            }
        }
    }
}
