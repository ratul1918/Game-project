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
