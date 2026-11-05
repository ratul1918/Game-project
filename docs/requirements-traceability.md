# Requirements traceability

The exact rubric will be pasted later. The requirement column is intentionally
the requested placeholder list.

| Requirement | File / class evidence | Test or verification evidence |
|---|---|---|
| Spring Boot REST | `server/src/main/java/com/thelighthouse/server/auth/AuthController.java`, `save/SaveController.java`, `run/RunController.java`, `admin/AdminStatsController.java` | `CloudSaveIntegrationTest`; `BackendThinSliceIntegrationTest` |
| WebSocket | `ws/GameWebSocketHandler.java`, `ws/GameSession.java`, `ws/WorldDirector.java` | `BackendThinSliceIntegrationTest.registerLoginConnectAndReceiveStormStates` |
| threading and concurrency | `ws/WorldClock.java`, `ws/SessionRegistry.java`, `telemetry/TelemetryService.java`, Unity `NetworkClient.cs` and `MainThreadDispatcher.cs` | `WorldDirectorTest`; load-test CSVs; no dedicated slow-bot/DB-stall test |
| database | Flyway `V1__create_players.sql`, `V2__create_saves_and_runs.sql`, `V3__create_telemetry_events.sql`; JPA entities and repositories | Spring context migrations; `CloudSaveIntegrationTest`; no telemetry persistence test |
| authentication | `security/JwtService.java`, `JwtAuthenticationFilter.java`, `SecurityConfig.java`, `AuthController.java` | Auth portions of `BackendThinSliceIntegrationTest` and `CloudSaveIntegrationTest` |
| testing | `server/src/test/java/com/thelighthouse/server` | Maven `mvn test` |
| load test | `bot-client/src/main/java/com/thelighthouse/bot/BotClient.java`, `scripts/load-test.sh` | `docs/load-test/run-*.csv` |
| documentation | `docs/*.md`, `docs/uml/*.md`, `README.md` | Documentation review; no automated documentation test |
| UML | `docs/uml/server-components.md`, `websocket-flow.md`, `unity-network.md`, `database-er.md`, `game-state.md` | Diagrams map to named source classes and migrations |
