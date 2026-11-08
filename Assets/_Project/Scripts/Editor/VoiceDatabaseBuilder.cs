using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using TheLightHouse.Voice;

namespace TheLightHouse.Editor
{
    public static class VoiceDatabaseBuilder
    {
        public const string DatabasePath = "Assets/_Project/Audio/Voice/VoiceDatabase.asset";
        public const string LinesFolder = "Assets/_Project/Audio/Voice/Lines";
        public const string AuthoredDocsPath = "docs/voice/authored-additions.md";

        [MenuItem("TheLightHouse/Voice/Build Voice Database")]
        public static VoiceDatabase BuildDatabase()
        {
            Directory.CreateDirectory(LinesFolder);
            Directory.CreateDirectory("docs/voice");

            var db = AssetDatabase.LoadAssetAtPath<VoiceDatabase>(DatabasePath);
            if (!db)
            {
                db = ScriptableObject.CreateInstance<VoiceDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            var lineList = new List<VoiceLine>();

            // 1. Core Dialogue Lines & Story Beats
            // Act 1
            Add(lineList, "A1_BOAT_001", SpeakerId.Boatman, "Crane's missed three weeks of calls. Could be the set. Could be worse.", "Dry coastal stoicism, gravelly and tired", "Arrival at dock in boat", VoiceAudioType.Spoken, SpatialMode.ThreeDimensional, 50, true);
            Add(lineList, "A1_BOAT_002", SpeakerId.Boatman, "Light the lamp before dark. And if you hear singing—don't answer it.", "Ominous warning, deliberate and low", "Arrival at dock, parting advice", VoiceAudioType.Spoken, SpatialMode.ThreeDimensional, 100, false);
            Add(lineList, "A1_DAN_BOAT_001", SpeakerId.DanielHale, "When do you come back?", "Cautious, seeking anchor in uncertainty", "Dock departure exchange", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_BOAT_003", SpeakerId.Boatman, "Morning. If the light's on.", "Blunt, fatalistic shrug", "Boat pushes off into the swell", VoiceAudioType.Spoken, SpatialMode.ThreeDimensional, 50, true);
            Add(lineList, "A1_DAN_001", SpeakerId.DanielHale, "Crane?", "Tentative call into the empty stone hallway", "Front door opens into vestibule", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_002", SpeakerId.DanielHale, "This was just made.", "Muttered to self, touching hot stew bowl", "Cottage kitchen table inspection", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            
            // Marta Shared Greeting (A1 and A4 reveal)
            var martaGreeting = Add(lineList, "A1_MARTA_001", SpeakerId.Marta, "Daniel... Daniel Hale. You made it before dark. Thomas left the lamp cold. You have to climb the tower and light it now. The fuel is in the cellar, Daniel.", "Uncannily calm, crisp relay operator tone", "Radio transmission / hidden tape reel", VoiceAudioType.Radio, SpatialMode.ThreeDimensional, 100, false);
            
            Add(lineList, "A1_DAN_003", SpeakerId.DanielHale, "Who is this? Where's Thomas Crane?", "Startled, demanding answers into microphone", "Radio desk dialogue", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_MARTA_002", SpeakerId.Marta, "Marta. Harbor relay. Questions after the lamp, Daniel. You have minutes.", "Firm, clinical urgency", "Radio desk dialogue response", VoiceAudioType.Radio, SpatialMode.ThreeDimensional, 100, false);
            Add(lineList, "A1_DAN_004", SpeakerId.DanielHale, "I never gave you my name.", "Whispered realization, hairs rising", "Radio desk realization", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_MARTA_003", SpeakerId.Marta, "There. Now keep it fed.", "Cold, satisfied dismissal", "Lamp ignited at dusk", VoiceAudioType.Radio, SpatialMode.ThreeDimensional, 50, true);
            Add(lineList, "A1_DAN_LANTERN_WARM", SpeakerId.DanielHale, "Warm.", "Under breath, checking copper mantle", "First approach to beacon", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_LANTERN_NOFUEL", SpeakerId.DanielHale, "Not enough fuel.", "Muttered frustration, shaking empty canister", "Ignition attempt with empty reservoir", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_FLASHBACK_01", SpeakerId.DanielHale, "Six men called from the water.", "Haunted memory, hollow inflection", "Guilt flashback near cellar steps", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_FLASHBACK_02", SpeakerId.DanielHale, "I turned the boat away.", "Self-condemning whisper", "Guilt flashback confession", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);

            // Act 2
            Add(lineList, "A2_DAN_DOOR_KNOCK_REACT", SpeakerId.DanielHale, "Don't open it.", "Urgent self-warning as cottage door rattles", "Drowned knocks on door", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A2_DAN_STALKER_SEE", SpeakerId.DanielHale, "Something is in the fog.", "Hushed dread, squinting into courtyard", "Stalker silhouette glimpse", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);

            // Act 3
            Add(lineList, "A3_DAN_GEN_FAIL", SpeakerId.DanielHale, "The generator is struggling.", "Tense observation as engine coughs", "Storm hits generator shed", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A3_DAN_GEN_FLOOD", SpeakerId.DanielHale, "Flooded. It won't start.", "Frantic realization in blackout", "Restart handle failure", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A3_DAN_LANTERN_CRANKONLY", SpeakerId.DanielHale, "Only the crank can keep it lit now.", "Resigned grim determination", "Lantern room blackout switch", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A3_DAN_STORM_END", SpeakerId.DanielHale, "The storm is easing. Crane's logbook might have answers.", "Exhausted relief after storm wave", "Storm end condition met", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);

            // Act 4
            Add(lineList, "A4_DAN_CELLAR_UNLOCKED", SpeakerId.DanielHale, "The cellar lock released.", "Surprised whisper as iron tumbler clunks", "Hidden shelf interaction", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_MARTA_RECORDING_SHORT", SpeakerId.Marta, "Daniel Hale. You made good time.", "Looping tape recording, mechanical hiss", "Reel-to-reel transmitter loop", VoiceAudioType.Tape, SpatialMode.ThreeDimensional, 100, false);
            Add(lineList, "A4_DAN_RECORDING_REACT", SpeakerId.DanielHale, "The exact same line. It was recorded.", "Disillusioned, creeping horror", "Stopping the tape recorder", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_HARBOR_BOARD_LEDGER", SpeakerId.Marta, "Candidate Daniel Hale. Six souls lost on the Marguerite. Guilt confirmed. Assign to Harrow Rock. He will keep the leash.", "Clinical dictation, objective appraisal", "Reel-to-reel tape playback", VoiceAudioType.Tape, SpatialMode.ThreeDimensional, 100, false);
            Add(lineList, "A4_DAN_PHOTO_REACT", SpeakerId.DanielHale, "That child is me.", "Breathing stops, voice trembling", "Examining framed photograph on nightstand", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_DAN_FATHER_REVEAL", SpeakerId.DanielHale, "Crane... the thing in the keeper's coat... is my father.", "Epiphany breaking through defenses", "Connecting photo and Stalker figure", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 100, false);
            Add(lineList, "A4_DAN_HERDING_REVEAL", SpeakerId.DanielHale, "He kept turning me toward the boat shed—away from the water.", "Connecting stalker behavior to protective paternal instinct", "Study desk revelation", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_DAN_SACRIFICE_REVEAL", SpeakerId.DanielHale, "He gave himself to the sea for me—and it would not let him rest.", "Sorrowful understanding, mournful", "Reading Crane's final log entry", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);

            // Act 5
            Add(lineList, "A5_DAN_SHORE_001", SpeakerId.DanielHale, "You kept me from the water.", "Gratitude mixed with grieving finality", "Shoreline confrontation with father", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A5_CRANE_LULLABY", SpeakerId.ThomasCrane, "Time for us to leave her...", "Rasping, wet baritone voice, bittersweet peace", "Crane sinks into the dawn surf", VoiceAudioType.Sung, SpatialMode.ThreeDimensional, 100, false);
            Add(lineList, "A5_BOAT_FINALE", SpeakerId.Boatman, "Light stayed on. That's enough for me.", "Nod of gruff approval, engine chugging", "Morning boat departure", VoiceAudioType.Spoken, SpatialMode.ThreeDimensional, 50, true);

            // 2. Authored Additions: Daniel Spoken Thoughts
            Add(lineList, "A1_DAN_TOWER_SEE", SpeakerId.DanielHale, "There it is. Harrow Rock.", "Weary, guarded, maritime inflection under breath", "Approaching harbor rock at dusk", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_MEAL_FIND", SpeakerId.DanielHale, "Still warm. Someone was just here.", "Perplexed, quiet suspicion", "Entering kitchen and touching meal", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A1_DAN_LAMP_COLD", SpeakerId.DanielHale, "Reservoir's bone dry. He left it to burn out.", "Disbelieving frustration", "First inspection of the dry beacon", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A2_DAN_TRACKS_WET", SpeakerId.DanielHale, "Wet tracks. Coming from the cellar... none going back.", "Chilled whisper, hairs standing", "Discovering footprints on entryway flagstones", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A2_DAN_WINDOW_SEE", SpeakerId.DanielHale, "Out there in the surf... those aren't rocks.", "Dread taking root, hushed murmur", "Looking out watchroom window at drowned figures", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A3_DAN_SINGING_HEAR", SpeakerId.DanielHale, "The singing. The boatman said never answer.", "Tense, remembering boatman's warning", "Ascending spiral stairs as sirens sing", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A3_DAN_LOG_READ", SpeakerId.DanielHale, "A leash. It was never meant to guide ships.", "Stunned shock, reading wrecker accounts", "Reading logbook on Floor 2 study desk", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_DAN_TAPE_REVEAL", SpeakerId.DanielHale, "It's a recording. She was never on the other end.", "Hollow betrayal, cold realization", "Discovering Marta's tape reel in cellar", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_DAN_LEDGER_FIND", SpeakerId.DanielHale, "My name... written down weeks before I signed the charter.", "Shaken, realizing he was chosen for guilt", "Inspecting Harbor Board Ledger", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);
            Add(lineList, "A4_DAN_PHOTO_FIND", SpeakerId.DanielHale, "That's me. In his arms. Thomas... father?", "Trembling realization, grief and love colliding", "Looking at childhood photo in bedroom", VoiceAudioType.Spoken, SpatialMode.TwoDimensional, 50, true);

            // 3. Authored Additions: Daniel Physical Reactions
            Add(lineList, "REACT_DAN_EXHAUST_01", SpeakerId.DanielHale, "(labored breathing)", "Heavy ragged breath cycle", "Stamina < 20% sprint depletion", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 2.2f);
            Add(lineList, "REACT_DAN_EXHAUST_02", SpeakerId.DanielHale, "(shallow ragged breath)", "Short ragged gasps", "Stamina < 20% recovery phase", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 2.0f);
            Add(lineList, "REACT_DAN_EXHAUST_03", SpeakerId.DanielHale, "(wheezing exhale)", "Deep shaky exhalation", "Stamina fully depleted exhaustion", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 2.5f);
            Add(lineList, "REACT_DAN_HOLD_BREATH", SpeakerId.DanielHale, "(shuddering inhale, holding breath)", "Suppressed breath, muffled tension", "Crouched inside HidingSpot near Stalker", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 3.0f);
            Add(lineList, "REACT_DAN_STARTLE", SpeakerId.DanielHale, "(sharp choked gasp)", "Immediate adrenaline jolt", "CatchSequence triggered by enemy contact", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 1.2f);
            Add(lineList, "REACT_DAN_LANDING", SpeakerId.DanielHale, "(stifled grunt on hard drop)", "Blunt diaphragm compression", "High fall landing impact", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 1.0f);
            Add(lineList, "REACT_DAN_CRANK_EFFORT", SpeakerId.DanielHale, "(strained physical exertion grunt)", "Heavy rhythmic grunting against iron gears", "Hand-cranking the beacon generator", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 1.8f);
            Add(lineList, "REACT_DAN_CATCH", SpeakerId.DanielHale, "No—!", "Choked yell of desperation", "Player caught by Stalker", VoiceAudioType.Reaction, SpatialMode.TwoDimensional, 0, true, 1.0f);

            // 4. Thomas Crane Vocalizations & Respirations
            Add(lineList, "CRANE_HUM_01", SpeakerId.ThomasCrane, "(low, wet baritone humming 'Leave Her, Johnny' - verse phrase)", "Mournful, muffled through wet coat", "Cellar and corridor roaming", VoiceAudioType.Hum, SpatialMode.ThreeDimensional, 50, true, 4.5f);
            Add(lineList, "CRANE_HUM_02", SpeakerId.ThomasCrane, "(Crane hums a broken lullaby)", "Wavering pitch, struggling to stay lucid", "Stalker hunting search loop", VoiceAudioType.Hum, SpatialMode.ThreeDimensional, 50, true, 4.0f);
            Add(lineList, "CRANE_HUM_03", SpeakerId.ThomasCrane, "(nearby, Crane hums a lullaby)", "Faint sea shanty vibration through masonry", "Floor 2 and 3 patrol", VoiceAudioType.Hum, SpatialMode.ThreeDimensional, 50, true, 4.2f);
            Add(lineList, "CRANE_HUM_04", SpeakerId.ThomasCrane, "(Crane hums, steady now)", "Clear, steady, protective focus", "Finale courtyard barrier defense", VoiceAudioType.Hum, SpatialMode.ThreeDimensional, 50, true, 4.5f);
            Add(lineList, "CRANE_BREATH_IDLE", SpeakerId.ThomasCrane, "(barnacle-rattled, wet trachea respiration)", "Gurgling, sodden mechanical breathing", "Proximity presence without humming", VoiceAudioType.Reaction, SpatialMode.ThreeDimensional, 0, true, 3.5f);
            Add(lineList, "CRANE_DRAG_STEP", SpeakerId.ThomasCrane, "(heavy sodden oilskin rustle and labored breath)", "Water-logged boots dragging over stone", "Stalker footstep approach", VoiceAudioType.Reaction, SpatialMode.ThreeDimensional, 0, true, 2.5f);
            Add(lineList, "CRANE_LULLABY_FINAL", SpeakerId.ThomasCrane, "Time for us to leave her...", "Rasping, emotional clarity before sinking", "Shoreline final sacrifice", VoiceAudioType.Sung, SpatialMode.ThreeDimensional, 100, false, 5.0f);

            // 5. The Drowned Spectral Vocalizations
            Add(lineList, "DROWNED_WHISPER_01", SpeakerId.TheDrowned, "(cold tidal murmuring in six voices)", "Overlapping wet whispers, choral murmur", "Shallows figures in surf", VoiceAudioType.Whisper, SpatialMode.ThreeDimensional, 50, true, 4.0f);
            Add(lineList, "DROWNED_WHISPER_02", SpeakerId.TheDrowned, "(spectral wet sigh from the surf)", "Shifting breeze across cold spray", "Tidal approach alert", VoiceAudioType.Whisper, SpatialMode.ThreeDimensional, 50, true, 3.5f);
            Add(lineList, "DROWNED_LURE_SUNG", SpeakerId.TheDrowned, "(haunting siren lullaby echoing across the breakers)", "Eerie harmonic singing across water", "Singing lure near shoreline", VoiceAudioType.Sung, SpatialMode.ThreeDimensional, 50, true, 6.0f);

            // Update Database asset
            db.SetLines(lineList);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            WriteAuthoredDocumentation(lineList);
            Debug.Log($"[VoiceDatabaseBuilder] Voice database populated with {lineList.Count} lines at {DatabasePath}.");
            return db;
        }

        private static VoiceLine Add(List<VoiceLine> list, string lineId, SpeakerId speaker, string text, string direction, string context, VoiceAudioType audioType, SpatialMode spatial, int priority, bool interruptible, float overrideDuration = 0f)
        {
            string path = $"{LinesFolder}/{lineId}.asset";
            var line = AssetDatabase.LoadAssetAtPath<VoiceLine>(path);
            if (!line)
            {
                line = ScriptableObject.CreateInstance<VoiceLine>();
                AssetDatabase.CreateAsset(line, path);
            }

            line.LineId = lineId;
            line.Speaker = speaker;
            line.Text = text;
            line.ActingDirection = direction;
            line.Context = context;
            line.AudioType = audioType;
            line.SpatialMode = spatial;
            line.Priority = priority;
            line.Interruptible = interruptible;
            line.SubtitleOverrideDuration = overrideDuration > 0f ? overrideDuration : 0f;
            line.IsPlaceholder = line.Clip == null;

            EditorUtility.SetDirty(line);
            list.Add(line);
            return line;
        }

        private static void WriteAuthoredDocumentation(List<VoiceLine> lines)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Authored Voice Lines & Character Direction");
            sb.AppendLine();
            sb.AppendLine("This document catalogs all narrative dialogue, internal monologues, and gameplay vocal reactions in *The Lighthouse: Harrow Rock*.");
            sb.AppendLine();
            sb.AppendLine("## Overview & Voice Counts");
            sb.AppendLine();

            var counts = new Dictionary<SpeakerId, int>();
            foreach (var l in lines)
            {
                counts[l.Speaker] = counts.GetValueOrDefault(l.Speaker, 0) + 1;
            }

            sb.AppendLine("| Character | Total Lines | Primary Audio Type | Delivery Inflection |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            sb.AppendLine($"| **Daniel Hale** | {counts.GetValueOrDefault(SpeakerId.DanielHale, 0)} | Spoken / Reaction | Weary, guarded, maritime inflection under breath |");
            sb.AppendLine($"| **The Boatman** | {counts.GetValueOrDefault(SpeakerId.Boatman, 0)} | Spoken | Dry coastal stoicism, gravelly, blunt |");
            sb.AppendLine($"| **Marta** | {counts.GetValueOrDefault(SpeakerId.Marta, 0)} | Radio / Tape | Uncannily calm, crisp relay tone; recorded loop |");
            sb.AppendLine($"| **Thomas Crane** | {counts.GetValueOrDefault(SpeakerId.ThomasCrane, 0)} | Hum / Sung / Reaction | Wet barnacle-rattled respirations, sorrowful lullaby hum |");
            sb.AppendLine($"| **The Drowned** | {counts.GetValueOrDefault(SpeakerId.TheDrowned, 0)} | Whisper / Sung | Overlapping wet whispers, cold tidal sirens |");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Complete Line Registry");
            sb.AppendLine();
            sb.AppendLine("| Line ID | Speaker | Type | Spatial | Priority | Subtitle Text | Acting Direction | Context |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

            foreach (var l in lines)
            {
                string safeText = l.Text.Replace("|", "\\|").Replace("\n", " ");
                string safeDir = l.ActingDirection.Replace("|", "\\|").Replace("\n", " ");
                string safeCtx = l.Context.Replace("|", "\\|").Replace("\n", " ");
                sb.AppendLine($"| `{l.LineId}` | {l.Speaker} | {l.AudioType} | {l.SpatialMode} | {l.Priority} | {safeText} | {safeDir} | {safeCtx} |");
            }

            File.WriteAllText(AuthoredDocsPath, sb.ToString());
        }
    }
}
