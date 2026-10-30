# Server components

```mermaid
flowchart LR
    Auth[AuthController]
    SaveC[SaveController]
    RunC[RunController]
    Admin[AdminStatsController]
    WS[GameWebSocketHandler]
    Security[JwtAuthenticationFilter / JwtService]
    SaveS[SaveService]
    RunS[RunService]
    Telemetry[TelemetryService]
    Registry[SessionRegistry]
    Clock[WorldClock]
    Session[GameSession]
    Director[WorldDirector]
    Metrics[ServerMetrics]
    PlayerRepo[PlayerRepository]
    SaveRepo[SaveRepository]
    RunRepo[RunRepository]
    TelemetryRepo[TelemetryEventRepository]
    DB[(PostgreSQL / H2)]

    Auth --> PlayerRepo
    Auth --> Security
    SaveC --> SaveS --> SaveRepo
    RunC --> RunS --> RunRepo
    Admin --> RunRepo
    WS --> Security
    WS --> Registry
    Registry --> Session
    Registry --> Metrics
    Clock --> Registry
    Session --> Director
    Session --> Telemetry
    Session --> Metrics
    Telemetry --> PlayerRepo
    Telemetry --> TelemetryRepo
    PlayerRepo --> DB
    SaveRepo --> DB
    RunRepo --> DB
    TelemetryRepo --> DB
```

Classes are in `server/src/main/java/com/thelighthouse/server/`. Spring MVC
controllers receive HTTP requests. `GameWebSocketHandler` authenticates and
routes frames to `SessionRegistry`; `WorldClock` calls each
[`GameSession`](../../server/src/main/java/com/thelighthouse/server/ws/GameSession.java)'s
tick. `WorldDirector` owns per-session gameplay state. `TelemetryService` is
the asynchronous database writer.
