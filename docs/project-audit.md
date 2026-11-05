# The LightHouse — Repository Audit & Feature Inventory

**Project:** The LightHouse (Harrow Rock)  
**Target Platform:** macOS (MacBook Air M3, 8 GB Unified Memory)  
**Unity Version:** 6000.6.6f1  
**Backend:** Java 21, Spring Boot 3.4.5, PostgreSQL 16 (port 5433), Docker Compose  
**Audit Date:** 2026-11-05  

---

## 1. Executive Summary

This audit evaluates the entire repository without altering existing game code or scene assets. It catalogs the current status of all Unity gameplay, UI/UX, AI, graphics, audio, save systems, and the Java Spring Boot backend. 

All completed, working systems are strictly protected to avoid regressions, while partial and missing areas are isolated for targeted implementation according to the creative and technical guidelines.

---

## 2. Feature Inventory

### 2.1 Engine, Graphics & Environment

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Render Pipeline** | Universal Render Pipeline (URP 6000.6.6f1) with Volumes for Dawn, Dusk, Night, Storm, Finale. | `Assets/_Project/Settings/Volume_*.asset` | **COMPLETE** | Maintain URP configuration; avoid pipeline migration. Ensure Low/Mac preset respect. | URP package | Visual inspection across acts. |
| **Lighthouse Shared Layout** | Procedural layout builder for masonry tower, keeper cottage, spiral staircase, floors, roof, doors. | `Assets/_Project/Scripts/Editor/LighthouseLayoutBuilder.cs` | **COMPLETE** | Retain static batching & collision tags. Do not overwrite without backup. | Unity Editor, NavMesh | `LevelBuildValidation.ValidateBatch()` |
| **Reference Visual Pass** | PBR materials (salt plaster, old timber, oxidized iron, wet shale), cottage kitchen dressing, watchroom, library. | `Assets/_Project/Scripts/Editor/ReferenceVisualPass.cs` | **COMPLETE** | Maintain mesh caching in `Art/ReferenceMeshes`. Keep 1K texture limit. | LayoutBuilder | Editor execution & asset bounds check. |
| **Island Arrival & Shoreline** | Pier, hurricane lantern, rowboat with oars, mud road ruts, fences, gnarled pine. | `Assets/_Project/Scripts/Editor/IslandArrivalPass.cs` | **COMPLETE** | Preserve dock collision & lighting hierarchy. | ReferenceVisualPass | Act 1 Play mode traversal. |
| **3D Cinematic Main Menu Environment** | Flat 2D canvas with solid dark background; no 3D scene geometry rendered. | `Assets/_Project/Scenes/MainMenu.unity`, `BuildFullGameScenes.cs` | **PARTIAL** | Render Harrow Rock 3D island, lighthouse with rotating volumetric light beam, rainfall, ocean fog, and subtle camera drift. | MainMenu scene, URP | Scene visual inspection & Play mode test. |
| **Atmospheric Rain & Ocean Foam** | Particle rainfall in acts, procedural fog and ambient wetness. | `Assets/_Project/Scripts/Environment/StormController.cs`, `ReferenceWeatherAuthoring.cs` | **COMPLETE** | Ensure particle counts adhere to 8 GB M3 budget. | Universal Additional Camera Data | Frame rate profile under Storm state. |

---

### 2.2 Player System & First-Person Experience

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **First-Person Controller** | `CharacterController`-based movement with walk, sprint, crouch, jump, slope limits, lean, and peek limits. | `Assets/_Project/Scripts/Player/FirstPersonController.cs`, `PlayerMovementSettings.cs` | **COMPLETE** | Preserve input handling via Unity New Input System. | Input System actions | MovementTest and in-act traversal. |
| **Head Bob & Camera Effects** | Procedural sine head bobbing, landing compression, sprint FOV kick. | `Assets/_Project/Scripts/Player/HeadBob.cs`, `CameraEffects.cs` | **COMPLETE** | Connect directly to Accessibility option `disableBobAndFov`. | FirstPersonController | Option toggle verification. |
| **Footstep System & Surfaces** | Surface detection via raycasting (`Wood`, `Stone`, `Metal`, `Ground`, `Water`); emits noise events for AI. | `Assets/_Project/Scripts/Player/FootstepSystem.cs`, `FootstepSurface.cs` | **COMPLETE** | Retain surface audio mapping; sync with sneaking/crouching footsteps. | NoiseEmitter, GameEvents | Noise event logs in Play Mode. |
| **Player Stamina** | Stamina drain on sprinting/jumping, regen delay, and exhaustion lockout. | `Assets/_Project/Scripts/Player/PlayerStamina.cs`, `StaminaBar.cs` | **COMPLETE** | Bind attack and block stamina costs to this system. | FirstPersonController | Stamina drain/recovery testing. |
| **Player Health & Injury System** | `PlayerHealthState` tracks hiding, catch, and checkpoint restore, but lacks numerical hit points, bleeding, and recovery. | `Assets/_Project/Scripts/Player/PlayerHealthState.cs` | **PARTIAL** | Add hit points (0–100), injury states (limp/vignette), bleeding over time, and bandage healing. | GameEvents, HudRoot | Combat damage and healing verification. |
| **First-Person Body / Arms / Hands** | Camera only. No rendered arms, hands, sleeves, or held items. | `Assets/_Project/Scenes/Act1.unity` | **MISSING** | Implement first-person arms mesh with practical sailor sleeves, hands, contextual item hold, and weapon rig. | Camera transform, URP materials | Visual rendering in Play Mode without clipping. |

---

### 2.3 Combat & Weapon Mechanics

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Maritime Rescue Axe** | None. Only interactive pickups and fuel cans exist. | `Assets/_Project/Scripts/Interaction/` | **MISSING** | Create `AxeCombat.cs` with light attack, heavy overhead swing, block, shove, stamina usage, and impact sounds. | FirstPersonController, PlayerStamina, New Input System | Swing animations, raycast/spherecast hit detection, stamina consumption. |
| **Weapon Durability & Repair** | None. | `Assets/_Project/Scripts/Interaction/` | **MISSING** | Track axe blade sharpness and handle integrity; add sharpening stone interaction. | Inventory, AxeCombat | Durability decrease per impact, repair at workbench. |
| **Combat Hit Detection & Stagger** | Stalker catches instantly on touch; no melee hit detection or enemy stagger. | `Assets/_Project/Scripts/AI/CatchSequence.cs`, `StalkerAI.cs` | **MISSING** | Implement impact detection on Drowned and Stalker, causing temporary stagger, knockback, and opening escape windows. | AxeCombat, StalkerAI, DrownedFigure | Melee impact tests against AI enemies. |

---

### 2.4 Enemy AI & Supernatural Horror

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Stalker AI (Thomas Crane / The Keeper's Shadow)** | State-driven NavMesh AI (`Idle`, `Investigate`, `Search`, `Patrol`, `Hunt`, `CheckHidingSpot`, `Retreat`, `Climb`). Reacts to sound. | `Assets/_Project/Scripts/AI/StalkerAI.cs`, `CraneHerding.cs`, `CraneFinale.cs` | **COMPLETE** | Preserve sound-driven state machine and post-reveal lullaby hum. Add melee stagger response when struck by axe. | NavMeshAgent, NoiseManager | Act 2 & Act 3 sound lure and patrol tests. |
| **Drowned Figures (The Shallows)** | Discrete unobserved advance toward lighthouse; frozen while watched; pushed back by lamp pulse; sink at dawn. | `Assets/_Project/Scripts/Drowned/DrownedManager.cs`, `DrownedFigure.cs`, `DoorKnocker.cs` | **COMPLETE** | Preserve freeze-while-observed rule. Add distinct archetypes (Drifter, Listener, Grasped). | Camera frustum culling, GameEvents | Act 3 & Act 5 advance tests. |
| **The Listener (Sound-Sensitive Enemy)** | Stalker has hearing curve, but dedicated blinded Listener enemy type does not exist as separate prefab. | `Assets/_Project/Scripts/AI/` | **PARTIAL** | Implement specific Listener archetype with heightened sound reaction and no visual cone. | NoiseManager, StalkerAI base | Auditory navigation test without line-of-sight. |
| **The Grasped (Heavy Enemy)** | None. | `Assets/_Project/Scripts/AI/` | **MISSING** | Implement heavy Drowned variant with grab attack, resistance to basic stagger, and door-battering ability. | FirstPersonController, PlayerHealthState | Grab breakout prompt & damage test. |
| **Thomas Crane Dual Form** | Character narrative authored across Acts 1–5; 3D models are greybox/placeholder primitives. | `Assets/_Project/Scripts/AI/CraneFinale.cs`, `StoryDirector.cs` | **PARTIAL** | Upgrade character visuals: waterlogged oilskin keeper coat, obscured facial silhouette, lullaby audio integration. | Character meshes/materials | Playtest Act 4 reveal & Act 5 finale. |

---

### 2.5 Lighthouse Mechanics & Survival Systems

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Lamp Simulation** | `LampController` tracks fuel, wick, decay, states (`Unlit`, `FuelLit`, `Failing`, `Extinguished`, `CrankPowered`), and pulse. | `Assets/_Project/Scripts/Lamp/LampController.cs`, `LanternInteract.cs` | **COMPLETE** | Retain all states and pulse mechanics. | GameEvents | Lamp drain and fueling test in Act 1 & 2. |
| **Hand Crank Station** | Crank interaction charges lamp in Act 3; emits sound scaled by speed; masks thunder. | `Assets/_Project/Scripts/Lamp/CrankStation.cs`, `NoiseMasking.cs` | **COMPLETE** | Maintain 1 Hz sound pulse and charge contribution curve. | LampController, NoiseManager | Act 3 hand-crank charging test. |
| **Storm & Weather Progression** | `StormController` coordinates lightning, thunder delays, rain audio, and wind pressure. | `Assets/_Project/Scripts/Environment/StormController.cs` | **COMPLETE** | Ensure smooth interpolation between storm phases. | GameEvents | Act 3 storm sequence test. |
| **Cellar Hum & Footprints** | Auditory shanty hum and wet footprints that lead into the cellar but never out. | `Assets/_Project/Scripts/Environment/CellarHum.cs`, `WetFootprints.cs` | **COMPLETE** | Preserve Act 2 story triggers and material footstep decals. | GameEvents, StoryDirector | Act 2 cellar inspection test. |

---

### 2.6 User Interface & User Experience (UI/UX)

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Main Menu** | Basic functional canvas with Start, Continue, Options toggles, Sensitivity, Volume, RenderScale, Quit. | `Assets/_Project/Scripts/UI/MainMenu.cs`, `BuildFullGameScenes.cs` | **PARTIAL** | Redesign with professional typography, real-time 3D background, Chapter Select, Load Game submenu, Extras, Credits, and confirmation dialogs. | UITheme, ActFlowController | Full click-through and navigation test. |
| **Settings Menu** | Single basic options panel inside MainMenu and PauseMenu with a few sliders. | `Assets/_Project/Scripts/UI/HudSettingsPanel.cs`, `MainMenu.cs` | **PARTIAL** | Create full, tabbed settings menu: Display (Resolution, Fullscreen, VSync, Preset, Quality), Audio (Master, Ambience, SFX, Dialogue), Gameplay, Accessibility, Input bindings. Persist via PlayerPrefs / JSON. | UniversalRenderPipelineAsset, AudioMixer | Verify every slider and toggle updates engine and persists across restart. |
| **HUD (Minimalist & Dynamic)** | Dynamic autohiding noise meter, stamina fill, lamp status, objective toast, composure edge pulse, interaction prompt. | `Assets/_Project/Scripts/UI/HudRoot.cs`, `InteractionPrompt.cs`, `ObjectiveUI.cs` | **COMPLETE** | Preserve auto-hide timers and composure meter. Ensure no overlapping labels. | UITheme, HudSettings | Play Mode visual checks across Acts 1–5. |
| **Inventory System UI** | String list in `Inventory.cs` and basic text strip in HUD. | `Assets/_Project/Scripts/Interaction/Inventory.cs`, `HudRoot.cs` | **PARTIAL** | Implement grid/slot-based inventory UI with item icons, descriptions, weapon slot, fuel gauge, inspection modal, and quick-use slots. | Inventory data, New Input System | Inventory open/close, item select, inspect, and use tests. |
| **Journal & Logbook UI** | `JournalUI` with Pages, Items, and Notes tabs; locks player movement while reading with threat speed multiplier. | `Assets/_Project/Scripts/UI/JournalUI.cs`, `LogbookReaderUI.cs` | **COMPLETE** | Enhance parchment visual presentation and typography. Preserve threat decay multiplier. | LogbookJournal, ActFourSettings | Tab switching and page reading test in Act 4. |
| **Pause Menu** | Pause menu toggles `Time.timeScale = 0`, unlocks cursor, supports Resume, Journal, Options, Save, and Quit. | `Assets/_Project/Scripts/UI/PauseMenu.cs` | **COMPLETE** | Preserve pause state and integrate new comprehensive Settings submenu. | Input System (Escape key) | Pause/Resume cycles during gameplay. |
| **Death / Catch Screen** | Basic fade to black with instant respawn at checkpoint. | `Assets/_Project/Scripts/AI/CatchSequence.cs` | **PARTIAL** | Create distinct horror death screen with maritime mist vignette, tragic death quote/sound, and Retry / Load / Main Menu options. | PlayerHealthState, ActFlowController | Intentional player catch test. |
| **Loading & Interstitial Screens** | Immediate asynchronous scene transition via `ActFlowController`. | `Assets/_Project/Scripts/Core/ActFlowController.cs` | **PARTIAL** | Add atmospheric loading screen with artwork, lore fragments, and tip text during act transitions. | ActFlowController | Scene load transition timing test. |

---

### 2.7 Story, Narrative & Progression

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Story Director & Canonical Beats** | 30 canonical beats across Acts 1–5 (`A1-B01` through `A5-B06`), single authority on progression. | `Assets/_Project/Scripts/Story/StoryDirector.cs`, `StoryBeat.cs`, `docs/story-beats.md` | **COMPLETE** | Preserve all 30 beat IDs and contracts. | GameEvents | `LevelBuildValidation.ValidateBatch()` (all 30 beats fire in order). |
| **Radio & Marta Transmissions** | Radio tuning, static, frequency tracker, Marta dialogue lines, and foreshadowing broadcast. | `Assets/_Project/Scripts/Story/Radio.cs`, `RadioSignalTracker.cs` | **COMPLETE** | Retain signal tracker and story unlocks. Ensure voice audio clarity. | DialogueLine, StoryDirector | Act 1 & Act 4 radio interaction tests. |
| **Maritime Lore & Discoveries** | Logbook pages on the *Marguerite*, wreckers, lighthouse leash, harbor board ledger, and Crane's diary. | `Assets/_Project/Scripts/Interaction/LogbookPickup.cs`, `LogbookJournal.cs` | **COMPLETE** | Preserve page placement and story flags. Add optional environmental documents. | JournalUI | Page inspection and flag triggers. |
| **Act Chaining & Endings** | ActFlowController manages transitions from MainMenu through Act 5 and epilogue. | `Assets/_Project/Scripts/Core/ActFlowController.cs`, `EndingUI.cs` | **COMPLETE** | Preserve scene chaining and save state restoration. | SaveSystem | Full campaign transition test. |

---

### 2.8 Save System & Local Fallback

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Local Save System** | JSON snapshot of act, checkpoint ID/position, inventory items, journal pages, story flags, and options. | `Assets/_Project/Scripts/Core/SaveSystem.cs` | **COMPLETE** | Add multi-slot support (`slot_1.json`, `slot_2.json`, `slot_3.json`) and Load Game menu interface. | File IO, JsonUtility | Save/load cycle across restarts. |
| **Cloud Save Synchronization** | Asynchronous PUT to `/save` with conflict resolution (HTTP 409) and background retry. | `Assets/_Project/Scripts/Network/CloudSaveClient.cs`, `SaveSystem.cs` | **COMPLETE** | Preserve backward compatibility and non-blocking coroutines. | Spring Boot backend, JWT | Cloud save put/get with server running and offline. |
| **Offline Fallback Guarantee** | Seamless local gameplay when server is unreachable or offline; zero game-loop stalls. | `Assets/_Project/Scripts/Network/NetworkBootstrap.cs`, `NetworkClient.cs` | **COMPLETE** | Maintain offline independence across all systems. | Local managers | Play game with backend stopped. |

---

### 2.9 Audio & Sound Design

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Footstep & Surface Audio** | Layered footstep audio driven by surface type and movement speed. | `Assets/_Project/Scripts/Player/FootstepSystem.cs` | **COMPLETE** | Ensure footstep loudness corresponds with stealth and AI detection. | NoiseManager | Auditory verification on all surfaces. |
| **Environmental & Weather Audio** | Rain, thunder, wind, cellar hum, ocean waves, dock creaks. | `Assets/_Project/Scripts/Environment/StormController.cs`, `CellarHum.cs` | **COMPLETE** | Maintain loop smoothing and spatial attenuation. | AudioListener | Volume slider tests across all acts. |
| **Audio Mixer Routing** | Direct AudioSource volume controls via scripts; lacks unified Audio Mixer hierarchy. | `Assets/_Project/` | **PARTIAL** | Route all sources into Master, Music, Ambience, SFX, Dialogue, and Footsteps mixer groups. | Unity AudioMixer | Volume slider adjustments in Settings. |

---

### 2.10 Backend (Java Spring Boot & PostgreSQL)

| Feature | Existing Implementation | Relevant Files | Status | Required Improvement | Dependencies | Testing Requirements |
|---|---|---|---|---|---|---|
| **Authentication & JWT** | User registration and login with BCrypt hashing and JWT bearer tokens (`/auth/register`, `/auth/login`). | `server/src/main/java/com/thelighthouse/server/auth/AuthController.java`, `JwtService.java` | **COMPLETE** | Maintain security configuration and token validation. | Spring Security, PostgreSQL | `mvn test` (`BackendThinSliceIntegrationTest`). |
| **Cloud Saves API** | `GET /save` and `PUT /save` with version tracking and conflict detection. | `server/.../save/SaveController.java`, `SaveService.java` | **COMPLETE** | Preserve JSON payload contracts (`itemsJson`, `pagesJson`, `flagsJson`). | Flyway migration V2 | `CloudSaveIntegrationTest` passes with 0 failures. |
| **Run Tracking API** | `POST /runs/start` and `PUT /runs/{id}/complete` for tracking session duration and status. | `server/.../run/RunController.java`, `RunService.java` | **COMPLETE** | Retain run statistics aggregation. | Flyway migration V2 | Run API integration tests. |
| **WebSocket World Pressure** | `/ws?token=<JWT>` bidirectional communication. Emits `STORM_STATE`, `PRESSURE_DELTA`, `RADIO_MESSAGE`, `STORY_BEAT`. | `server/.../ws/GameWebSocketHandler.java`, `WorldDirector.java`, `GameSession.java` | **COMPLETE** | Retain envelope format `{type, seq, payload}` and thread confinement. | SessionRegistry, WorldClock | `WorldDirectorTest` passes. |
| **Asynchronous Telemetry** | Bounded blocking queue and background daemon writer for gameplay events; non-blocking to world clock. | `server/.../telemetry/TelemetryService.java`, `TelemetryEventRepository.java` | **COMPLETE** | Maintain asynchronous worker model; do not perform database writes on tick thread. | HikariCP, PostgreSQL | Verification via admin dashboard. |
| **Bot Client & Load Testing** | Multi-threaded client simulating concurrent game clients with customizable ramp-up and duration. | `bot-client/src/main/java/com/thelighthouse/bot/BotClient.java` | **COMPLETE** | Preserve bot client compatibility. | Maven | `mvn package` and execution of bot client. |

---

## 3. Preserved Systems Verification

The following existing architectures have been audited and verified as rock-solid; **they will NOT be rebuilt, deleted, or replaced**:

1. **Unity New Input System:** `Assets/InputSystem_Actions.inputactions` and all action bindings (`Move`, `Look`, `Sprint`, `Crouch`, `Jump`, `LeanLeft`, `LeanRight`, `Interact`).
2. **Event-Driven Communication Contract:** All events declared in `docs/events.md` and `GameEvents.cs`.
3. **Canonical Narrative Beats:** All 30 beat IDs (`A1-B01` through `A5-B06`) defined in `docs/story-beats.md`.
4. **Local Offline Fallback:** Complete independence of Unity gameplay when the server or database is absent.
5. **Java Spring Boot 3.4.5 Server:** Architecture, controllers, security, and WebSocket handling.
6. **Flyway Migrations & PostgreSQL Schema:** Existing tables (`players`, `saves`, `runs`, `telemetry_events`).

---

## 4. Priority Improvements Roadmap

Based on the audit, the following components require extension and enhancement:

1. **Phase 1: Professional UI Redesign & Settings Implementation**
   - 3D Cinematic Main Menu environment (real-time lighthouse, rotating light beam, stormy sea).
   - Comprehensive Settings Menu (Display, Audio Mixer, Gameplay, Accessibility, Input Remapping).
   - Chapter Select and Multiple Save Slots / Load Game interface.
   - Horror Death Screen and Loading Screen with lore hints.
2. **Phase 2: Character, Weapon & Combat Implementation**
   - Maritime Rescue Axe model, animation rig, swing mechanics, stamina cost, and durability.
   - First-person arm rig with matching clothing.
   - Melee hit detection with Drowned stagger and knockback.
   - Player numerical health, injury visualization, bleeding, and bandage healing.
3. **Phase 3: Enemy Variants & Sound Propagation**
   - Drowned enemy archetypes: The Drifter, The Listener, The Grasped.
   - Visual enhancements for Drowned and Thomas Crane.
4. **Phase 4: Acts 1–5 Environmental Polish & Narrative Deepening**
   - Verification of all interaction anchors, audio ambiance, and lighting cues across all 5 Acts.
   - End-to-end playtest from Main Menu to Act 5 epilogue.
5. **Phase 5: Backend & Integration Quality Assurance**
   - Run full test suite (`mvn test`, Unity `LevelBuildValidation.ValidateBatch()`).
   - Profile performance on target 8 GB M3 memory budget.
