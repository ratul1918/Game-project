# Ember Blade Phase 1-2 review report

## Problems found and fixes

| File | Issue | Fix | Severity |
|---|---|---|---|
| `client/Assets/Scripts/Missions/Chapter1SceneBootstrap.cs` | `CombatHudController` had no `EmberBlade.UI` import and could not compile. | Added the missing namespace import. | High |
| `client/Assets/Editor/Chapter1Builder.cs` | NPC builder results were `GameObject` values where `DialogueInteractable` values were required. | Read the component from each generated NPC before adding it to the target list. | High |
| `client/Assets/Scripts/Network/NetworkManager.cs` | WebSocket callbacks were not detached, and anonymous delegates could not be removed reliably. | Stored the NativeWebSocket delegate types and detached all handlers before close/destroy. | High |
| `client/Assets/Scripts/Network/NetworkManager.cs` | `Awake` was marked async without awaiting anything. | Made `Awake` synchronous and cleared the singleton when its object is destroyed. | Low |
| `client/Assets/Scripts/Missions/MissionManager.cs` | Dialogue completion could be counted more than once by repeating a conversation. | Track completed dialogue IDs in a set. | Medium |
| `client/Assets/Scripts/Missions/MissionManager.cs` | Mission 1.2 restart did not reset barricades and their counts. | Added `BarricadeManager.ResetBuildPhase()` and invoke it when Mission 1.2 starts. | High |
| `client/Assets/Scripts/Missions/MissionManager.cs` | Mission restart delay was hardcoded and mission event subscriptions were not removed. | Moved the delay to `GameConfig` and unsubscribed in `OnDisable`. | Medium |
| `client/Assets/Scripts/Player/PlayerController.cs` and `client/Assets/Scripts/Combat/PlayerCombat.cs` | Player movement and combat remained active while dialogue was open. | Added a shared dialogue-open state and suppress both systems during dialogue. | High |
| `client/Assets/Scripts/Combat/BarricadeManager.cs` | Placement limit logic was embedded in the input path and was not independently testable. | Added `CanPlaceWithinLimit` and used it for placement decisions. | Low |
| `client/Assets/Scripts/AI/EnemyController.cs` | Missing runtime references could cause failures during enemy initialization. | Added explicit validation for config, player, combat, agent, and health. | High |
| `client/Assets/Scripts/Combat/BarricadeManager.cs` | Missing setup references could cause placement or raid failures. | Added explicit initialization validation with a named error. | High |
| `client/Assets/Scripts/Missions/Chapter1SceneBootstrap.cs` | Generated Chapter 1 references were not validated before `GetComponent` calls. | Added complete bootstrap validation for scene fields and player components. | High |
| `client/Assets/Scripts/Config/GameConfig.cs` | Dialogue range and mission restart delay were not centralized with the other tuning values. | Added both values to `GameConfig`. | Medium |
| `client/Assets/Tests/EditMode` | Tests had no dedicated test assembly, so the initial test command produced no results. | Added an Editor-only test assembly and a runtime `EmberBlade` assembly reference. | Medium |

## Verification performed

- Unity 6000.6.6f1 compiled the project with zero compiler errors and zero warnings after fixes.
- `Phase1SceneBuilder.BuildFromCommandLine` ran twice successfully.
- `CombatTestBuilder.BuildFromCommandLine` ran twice successfully.
- `BarricadeTestBuilder.BuildFromCommandLine` ran twice successfully.
- `Chapter1Builder.BuildFromCommandLine` ran twice successfully.
- `Phase1`, `CombatTest`, `BarricadeTest`, and `Chapter1` are all present in
  `client/ProjectSettings/EditorBuildSettings.asset`.
- PostgreSQL was healthy through Docker Compose.
- `cd server && mvn test` completed successfully. The server has no test source
  files, so this was a clean build with no automated server test cases.
- Live server verification passed:
  - registration returned HTTP 201;
  - login returned a token;
  - a valid WebSocket received sequential `WAR_STATE_DELTA` messages;
  - `ALERT_RAID` was received with the expected message;
  - an invalid token was rejected by a real WebSocket handshake.

## Not verified

- Unity EditMode tests were added, but `-runTests -testPlatform EditMode
  -testResults ...` exited without creating a results XML file or emitting test
  execution markers. The tests are therefore **not reported as passing**.
- Play Mode behavior was not verified: movement, combat hit detection, parry,
  Ember gain, NavMesh path blocking, trap damage, dialogue interaction, mission
  restart timing, and UI presentation still require hands-on testing.
- The server’s second startup attempt was not needed because the already-running
  verified server occupied port 8080; its failure was the expected port conflict.
