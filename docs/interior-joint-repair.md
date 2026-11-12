# Interior structure repair — 2026-11-10

Applied to Acts 1–5 using `LighthouseInteriorJointRepair` after the existing
architecture authoring pass. The current four-level tower layout is preserved.

- Closed stair risers replace the thin, visually separated tread plates.
- Upper floor boundaries stop 2 cm beyond the final tread (measured at the
  inner stair edge), retaining the incoming flight openings.
- Separate ceiling meshes follow each elevated floor and its opening, with
  2 cm separation below the floor underside.
- The lantern landing and balcony now join without overlapping slab faces.
- Continuous handrails connect the existing stair and wellhole posts.
- Horizontal surfaces use planar UVs to remove radial texture stretching.
- Existing smooth ramp colliders remain; ceilings have matching mesh colliders.
- NavMeshes were rebuilt and scenes saved. Existing props/story anchors remain.

Validation: Unity recompilation completed with zero errors or warnings.
The existing clearance/floor validator passed 493/511/511/511/484 samples
for Acts 1/2/3/4/5 respectively, with zero issues. These are geometry checks,
not a complete gameplay playthrough or a standalone player build.

Structural cutaway preview: `level-previews/interior-structure-repaired.png`.
Exterior and props are omitted from this diagnostic render only.

Reapply through `The LightHouse > Level > Repair Interior Joints`, or via
`TheLightHouse.Editor.LighthouseInteriorJointRepair.ApplyAll()` in Edit Mode.
Joint gap, ceiling thickness, stair soffit depth and rail thickness are tunable
in the existing Architecture Repair Settings asset.

## Revised traversal contract — 2026-11-11

Floor 0/1/2/3 correspond to existing authored elevations 0.15/3.35/6.55/9.75 m
(the historical object names use Floor 1–4). Preserve props, room layout, exterior
finish and terrain. Flight endpoints and landing edges share exactly the same
position and height; no intentional seam gap. The first visible tread is at the
lower walking surface and the final tread is at the upper walking surface.
Only swept stair/head clearance removes elevated slab sectors. Ground floor stays
solid. Closed mesh slabs and ceilings share opening boundaries. Smooth, static,
non-convex ramp colliders alone provide stair collision; visual treads have none.
Complete the floor perimeter to the existing masonry's inner face, and close
masonry above the three existing ground-level architectural door passages.
Retain actual doorways and windows; do not convert gameplay triggers into walls.
Physical structure uses enabled, non-trigger colliders and opaque thick meshes.
A temporary third-person inspection rig exercises the actual scene in Play Mode;
its automatic traversal uses CharacterController.Move, never teleporting between
waypoints or using jumps. Test ascent, descent, pauses, landing transitions and
wall blocking. Keep production first-person gameplay unchanged.
