# Act 5 greybox playtest

Build with `TheLightHouse/Build Act 5 Scene`, bake NavMeshSurface, save, and Play. The scene begins in Finale
with charge at 0.18, fourteen Drowned figures, Crane non-hostile, and the crank as the only power source.

## Beat sequence

1. A5-B01 courtyard floods after the short opening delay.
2. A5-B02 completes after the authored Hold duration while lamp pressure rises.
3. A5-B03 starts Crane's slams, lure route and wavering lullaby, then the Dawn countdown.
4. A5-B04 commits at first light. StoryDirector publishes DawnReached; figures return to water, Crane blocks
   the doorway, catch pressure stops, and the objective becomes **Get to the dock**.
5. A5-B05 triggers at the shoreline after the final lullaby line and Crane's calm, non-gory sinking.
6. A5-B06 triggers when the morning-boat ledger shows Daniel crossed out and the next keeper entered.

Tune `Assets/_Project/Settings/FinaleSettings.asset`: lower chargeDecayFactor/maximumPressureMultiplier or raise
holdDuration only when more endurance is wanted; lower holdDuration, pressure or figuresAtDoorForFailure to make
the slice easier. Slam interval/lure radius tune Crane's help. Dawn duration tunes the release.

## Full flow

`TheLightHouse/Build Full Game Scenes` creates MainMenu and rebuilds Acts 1–5, then writes their order to Build
Settings. MainMenu's ActFlowController changes scene at each terminal act beat and its persistent SaveSystem
restores IDs/checkpoints/options. F1 opens the persistent debug menu in Editor/development builds.

## Verification boundaries

Strict Unity compilation returned zero errors and warnings. Editor APIs generated and baked Act5 with fourteen
figures. Runtime checks confirmed Finale/charge start, 0.18 initial charge, a complete Crane path into the
courtyard, ordered A5-B01–A5-B06 publication, Finale→Dawn→Ending state transitions, 28 m slam noise, disabled
hostile StalkerAI and JSON checkpoint creation. Phase durations and return/sink coroutines were accelerated or
signalled during checks; a complete player-controlled run, visual fades, menu controls, audio and a standalone
player build remain manual verification work.
