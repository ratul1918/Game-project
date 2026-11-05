# Reference visual pass

These are Unity-rendered previews of the authored Act1 scene. They are not the supplied target images.

- `exterior.png`: tapered tower, circular lantern gallery, keeper house and coastal dressing.
- `office.png`: timber interior, shaded desk light, analogue radio, recorder, blinds and chair.
- `stairwell.png`: iron grating spiral, following handrails, stacked barrels and timber floor.

The pass is applied to Acts 1–5 and is included in Build Full Game Scenes. Original procedural maps are at most 512px (grating 128px); generated geometry is grouped by material and area. ReferenceLookSettings controls the broad palette, wear, lighting, fog and dressing density. Geometry remains less detailed than the supplied reference images.

Verification: scripts compiled; the existing Act1–5 gameplay-anchor and canonical story-beat validation passed; Act1 entered Play Mode with a camera, player, weather system and no missing mesh references. Full act traversal and sustained M3 frame-rate measurements are not yet verified. The separately requested opening-island layout and working doorway corrections follow this visual pass.

Mac development build: succeeded with zero errors on 2026-11-04, output `Builds/ReferenceLook/TheLightHouse.app` (approximately 236 MB). The build reported 348 warnings. The player-only PerformanceHud compilation issue was corrected before this successful build.
