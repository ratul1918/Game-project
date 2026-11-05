# Harrow Rock: Comprehensive QA & Testing Report

## Executive Summary
This report documents the verification, validation, and regression test results for **The LightHouse (Harrow Rock)** across Unity 6 (6000.6.6f1 URP), the Java 21 Spring Boot backend, and PostgreSQL data persistence.

---

## 1. Test Suite Results

### 1.1 Backend & Cloud Services
- **Framework:** JUnit 5, Spring Boot Test, Testcontainers / H2 In-Memory DB.
- **Execution Command:** `cd server && mvn test`
- **Result:** **PASSED** (6 tests run, 0 failures, 0 errors, 0 skipped).
  - `BackendThinSliceIntegrationTest`: Verified health probe (`/actuator/health`), user authentication, run recording, and metrics exposure.
  - `CloudSaveIntegrationTest`: Verified cloud save persistence, slot synchronization, conflict resolution, and save payloads.
  - `WorldDirectorTest`: Verified WebSocket telemetry, world storm pressure calculation, and message envelope parsing.

### 1.2 Bot Client & Load Testing
- **Execution Command:** `cd bot-client && mvn test`
- **Result:** **PASSED** (Build success, dependencies verified, clean mock trajectory injection).

### 1.3 C# Compilation & Static Analysis
- **Execution Command:** `dotnet build Assembly-CSharp-Editor.csproj`
- **Result:** **PASSED** (0 Errors, 52 deprecation warnings from legacy API bridges safely encapsulated).
- **Unity Editor Compiler Diagnostics:** `compilationFailed: false`, `consoleErrors: 0`.

---

## 2. Canonical Story Beats Validation (Acts 1–5)
- **Validation Engine:** `TheLightHouse.Editor.LevelBuildValidation.ValidateBatch()`
- **Result:** **PASSED** across all 30 canonical narrative story beats:

| Act | Canonical Range | Anchor Requirements | Validation Status |
| :--- | :--- | :--- | :--- |
| **Act 1** | `A1-B01` – `A1-B06` | Boatman, Dock, Cottage, Stove Meal, Marta Radio, Spiral Stairs, Lamp Room | **PASSED** |
| **Act 2** | `A2-B01` – `A2-B06` | Stalker AI Patrol, Cellar Footprints, Hiding Spots, Sound Detection, Lantern Leash | **PASSED** |
| **Act 3** | `A3-B01` – `A3-B06` | Shallows Drowned (Weeping-Angel), Generator Failure, Hand Crank Audio Beacon | **PASSED** |
| **Act 4** | `A4-B01` – `A4-B06` | Marguerite Records, Secret Cellar Room, Harbor Board Ledger, Father Photo | **PASSED** |
| **Act 5** | `A5-B01` – `A5-B06` | Courtyard Flood, Crane Lullaby Sacrifice, Doorway Block, Dawn Departure | **PASSED** |

---

## 3. Systems Functional Verification

### 3.1 UI & Settings Menu
- **Main Menu:** Left-docked navigation panel with 8 responsive options (New Game, Continue, Chapter Select, Load Game, Settings, Extras, Credits, Quit).
- **Settings Submenu:** Tabbed settings for Display (Resolution, Fullscreen, VSync, Render Scale), Audio (Master, Music, SFX, Ambience, Dialogue), Gameplay (Look sensitivity, Invert Y, Camera Bob, Motion Sickness Reduction), Accessibility (Subtitles, Font Size, Opacity, Reduced Storm Flashes), and Controls Keybindings.
- **Persistence:** Tested PlayerPrefs serialization and fallback defaults.
- **PlayMode Entry:** Tested entering and exiting Play Mode in `MainMenu.unity` with 0 console errors.

### 3.2 Survival Combat & First-Person Immersion
- **First-Person Arms (`FirstPersonArms`):** Procedural mouse look sway, walking/sprinting bob, contextual holding and swing animations.
- **Fire Axe Combat (`AxeCombat`):**
  - Light Attack: 25 damage, 15 stamina drain, fast recovery.
  - Heavy Attack: 55 damage, 35 stamina drain, staggers enemies.
  - Block: 70% damage reduction, block impact audio.
  - Shove: 10 damage, breaks grapples, pushes back Drowned.
- **Weapon Durability (`WeaponDurability`):** 50 hit cycles before dulling (-40% damage penalty until sharpened).
- **Survival Health (`PlayerHealthState`):** 0–100 numerical HP, bleeding status effect (1.2 HP/s), bandage healing, limping penalty (0.65x move speed) when HP < 30%.

### 3.3 Enemy AI Archetypes
- **The Stalker (`StalkerAI`):** Integrated with `IDamageable`. Axe attacks trigger stagger and temporary retreat.
- **The Listener (`ListenerAI`):** Blind sound-hunter tracking `NoiseEmittedEvent`. Fast charge on noise > 0.5f threshold; deals 25 damage and causes bleeding.
- **The Grasped (`GraspedAI`):** Tidal brute with grappling hold. Staggerable via shove or heavy axe strike.

---

## 4. Sign-Off
- **Playable Build Status:** Clean and playable.
- **Regression Status:** Zero regressions to existing save files, contracts, or scene structures.
