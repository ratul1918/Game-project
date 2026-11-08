using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using TheLightHouse.Voice;

namespace TheLightHouse.Editor
{
    public static class VoiceScriptExporter
    {
        public const string ScriptMdPath = "docs/voice/recording-script.md";
        public const string ScriptCsvPath = "docs/voice/recording-script.csv";
        public const string CastingDocPath = "docs/voice/casting-and-direction.md";

        [MenuItem("TheLightHouse/Voice/Export Recording Script")]
        public static void ExportScript()
        {
            Directory.CreateDirectory("docs/voice");
            var db = AssetDatabase.LoadAssetAtPath<VoiceDatabase>(VoiceDatabaseBuilder.DatabasePath);
            if (!db)
            {
                db = VoiceDatabaseBuilder.BuildDatabase();
            }

            var md = new StringBuilder();
            var csv = new StringBuilder();

            md.AppendLine("# The Lighthouse: Harrow Rock — Voice Recording Script");
            md.AppendLine();
            md.AppendLine("Official studio recording script for voice cast, sound engineers, and Foley artists.");
            md.AppendLine();
            md.AppendLine("| LineID | Speaker | Act | Subtitle Text | Acting Direction | Context | File Target |");
            md.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

            csv.AppendLine("LineID,Speaker,Act,Subtitle Text,Acting Direction,Context,File Target");

            foreach (var line in db.AllLines)
            {
                if (line == null) continue;
                string act = ExtractAct(line.LineId);
                string fileTarget = $"Assets/_Project/Audio/Voice/{line.Speaker}/{line.LineId}.wav";

                string safeMdText = line.Text.Replace("|", "\\|").Replace("\n", " ");
                string safeMdDir = line.ActingDirection.Replace("|", "\\|").Replace("\n", " ");
                string safeMdCtx = line.Context.Replace("|", "\\|").Replace("\n", " ");

                md.AppendLine($"| `{line.LineId}` | {line.Speaker} | {act} | \"{safeMdText}\" | {safeMdDir} | {safeMdCtx} | `{fileTarget}` |");

                string csvText = $"\"{line.Text.Replace("\"", "\"\"")}\"";
                string csvDir = $"\"{line.ActingDirection.Replace("\"", "\"\"")}\"";
                string csvCtx = $"\"{line.Context.Replace("\"", "\"\"")}\"";
                csv.AppendLine($"{line.LineId},{line.Speaker},{act},{csvText},{csvDir},{csvCtx},{fileTarget}");
            }

            File.WriteAllText(ScriptMdPath, md.ToString());
            File.WriteAllText(ScriptCsvPath, csv.ToString());

            WriteCastingDocumentation();
            Debug.Log($"[VoiceScriptExporter] Exported recording script to {ScriptMdPath} and {ScriptCsvPath}.");
        }

        private static string ExtractAct(string lineId)
        {
            if (string.IsNullOrEmpty(lineId)) return "General";
            if (lineId.StartsWith("A1_")) return "Act 1";
            if (lineId.StartsWith("A2_")) return "Act 2";
            if (lineId.StartsWith("A3_")) return "Act 3";
            if (lineId.StartsWith("A4_")) return "Act 4";
            if (lineId.StartsWith("A5_")) return "Act 5";
            if (lineId.StartsWith("REACT_")) return "Gameplay Reactions";
            if (lineId.StartsWith("CRANE_")) return "Creature Foley & Song";
            if (lineId.StartsWith("DROWNED_")) return "Spectral Ambience";
            return "General";
        }

        private static void WriteCastingDocumentation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Voice Casting & Audio Direction Specification");
            sb.AppendLine();
            sb.AppendLine("## Target Soundscape");
            sb.AppendLine("Set off the North Atlantic coast, 1947. Harrow Rock is a windswept granite spire isolated by rising tides.");
            sb.AppendLine("Voice acting must balance intimate first-person vulnerability with period-accurate restraint.");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Character Casting Profiles");
            sb.AppendLine();
            sb.AppendLine("### 1. Daniel Hale (Player Protagonist)");
            sb.AppendLine("- **Profile**: Male, mid-30s. Former merchant seaman haunted by the sinking of the *Marguerite*.");
            sb.AppendLine("- **Vocal Quality**: Weary, guarded, intimate, quiet maritime inflection. Low-pitched, strained under breath.");
            sb.AppendLine("- **Microphone Technique**: Ultra close-mic (Large diaphragm condenser, 3–4 inches from mouth with pop filter). Minimal room reverb, high dry-ratio.");
            sb.AppendLine("- **Stamina / Foley**: Heavy breathing cycles, breath holds, sharp startle gasps, and physical strain while cranking.");
            sb.AppendLine();
            sb.AppendLine("### 2. The Boatman (Charon Archetype)");
            sb.AppendLine("- **Profile**: Male, late 60s. Weather-beaten ferryman who has ferried many keepers to Harrow Rock.");
            sb.AppendLine("- **Vocal Quality**: Gruff, gravelly, dry coastal stoicism. Speaks with the weight of someone who knows what happens after dark.");
            sb.AppendLine("- **Delivery**: Measured, blunt, zero sentimentality. Parting words are factual, not emotional.");
            sb.AppendLine();
            sb.AppendLine("### 3. Marta (Harbor Relay Operator / Secret Tape)");
            sb.AppendLine("- **Profile**: Female, 40s. Harbor board civil servant whose voice guides Daniel via radio.");
            sb.AppendLine("- **Vocal Quality**: Clinical, crisp, flat, unnervingly calm. Transmit tone has a subtle bureaucratic detachment.");
            sb.AppendLine("- **Acoustic Treatment**: Radio group bandpass filter (400 Hz HPF, 3500 Hz LPF), faint 60 Hz hum, mild analog tube saturation.");
            sb.AppendLine("- **Identity Constraint**: Act 1 live radio transmission and Act 4 hidden cellar reel-to-reel tape are the identical recording.");
            sb.AppendLine();
            sb.AppendLine("### 4. Thomas Crane (The Stalker / Daniel's Father)");
            sb.AppendLine("- **Profile**: Male, late 50s. The former keeper consumed by the sea, now a drowned thing in a heavy keeper's oilskin.");
            sb.AppendLine("- **Vocal Quality**: Non-verbal respirations, raspy wet trachea breathing, baritone humming of 'Leave Her, Johnny'.");
            sb.AppendLine("- **Emotional Arc**: Begins as an acoustic terror stalker in the dark cellar; resolves into a grieving father sacrificing himself at dawn.");
            sb.AppendLine();
            sb.AppendLine("### 5. The Drowned (Chorus of the Sea)");
            sb.AppendLine("- **Profile**: Spectral chorus of drowned sailors lurking in the shallows.");
            sb.AppendLine("- **Vocal Quality**: Cold, layered whispering in multiple voices; siren-like lure harmonies carrying across the breaking waves.");
            sb.AppendLine("- **Acoustic Treatment**: Long cavernous reverb tail, high wet-mix, slight stereo phase modulation.");

            File.WriteAllText(CastingDocPath, sb.ToString());
        }
    }
}
