# The LightHouse Asset Credits

| Asset | Source | License | URL | Date added |
|---|---|---|---|---|
| Lamp beam cone, cookie/glow, fog and silhouette materials | Generated in-project by `ApplyGraphics1` | Original project asset | — | 2026-10-30 |
| Game Icons (selected HUD/menu icons) by Kenney Vleugels | Kenney | CC0 1.0 | https://kenney.nl/assets/game-icons | 2026-11-02 |
| UI pack, input prompts, interface sounds, and Kenney Future fonts by Kenney Vleugels | Kenney | CC0 1.0 | https://kenney.nl/assets | 2026-11-02 |

Credit text: UI assets and fonts by Kenney Vleugels, released under CC0 1.0.

Add every third-party art, texture, model, font, audio file, shader, and tool before it enters a playable build. Preserve the creator's requested attribution wording.

Every credited asset must also pass the import and runtime limits in `docs/performance-budget.md`; licensing approval does not override the 8 GB budget.

## Reference visual pass — 2026-11-03

Original procedural salt plaster, timber, iron, slate and coastal scrub textures; original house, radio desk, barrels, spiral treads and lantern gallery geometry: created for The LightHouse in this repository. Source: `ReferenceSurfaceLibrary.cs` and `ReferenceVisualPass.cs` (local source; no external download URL or third-party license). The three user-provided WhatsApp screenshots are visual references only and are not distributed as game assets.

## Interior v2 and High-Quality Look — 2026-11-08

Original modular interior kit prefabs, continuous ramp staircases, subterranean vaulted cellar corridor, secret room complex with functional rotating reel-to-reel transmitter (`ReelRotator`), packed Metallic-Smoothness PBR Lit materials, reflection probe arrays, and validation test suite: created for The LightHouse in this repository. Source: `InteriorKit.cs`, `InteriorKitBuilder.cs`, `InteriorLayoutBuilder.cs`, `InteriorSettings.cs`, `LightingAndAtmospherePass.cs`, `LevelPreviewCapture.cs`, `InteriorValidation.cs`, and `ReelRotator.cs`.

## Voice System & Sound Architecture — 2026-11-09

Original voice data architecture (`VoiceLine`, `VoiceDatabase`, `VoiceSpeaker`), decoupled `VoiceManager`, audio ducking system, studio recording script, casting documentation, and multi-act voice validation suite: created for *The LightHouse* in this repository.
Source: `Assets/_Project/Scripts/Voice/` and `Assets/_Project/Scripts/Editor/`.

Recommended external audio references and verified production asset specifications:
- Dialogue Architecture & Subtitle Management: *Dialogue System for Unity* (Pixel Crushers) & *Master Audio - AAA Sound Solution* (Dark Tonic Inc).
- Character Reactions & Foley Breathing (Daniel Hale): *Male Voice, Breaths, and Grunts FX* & *Horror Foley & Movement*.
- Creature Vocals & Respirations (Thomas Crane & The Drowned): *Horror Voices, Screams, and Death Rattles* & *Ghostly Whispers & Spectral Ambience*.
- Vintage Radio & Tape Noise (Marta & Hidden Recorder): *Vintage Radio & Walkie-Talkie Static SFX* & *Analog Tape & Recorder Foley*.
- Maritime Lullaby & Acoustic Resonance: *Maritime Sea Shanties & Nautical Music Pack*.
- Development placeholders generated locally using macOS built-in speech synthesis system (`say` / `afconvert`).
