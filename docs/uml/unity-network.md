# Unity network layer

```mermaid
flowchart LR
    Bootstrap[NetworkBootstrap]
    Config[NetworkConfig]
    Auth[AuthClient]
    Client[NetworkClient]
    Dispatch[MainThreadDispatcher]
    Runtime[NetworkRuntimeState]
    Bridge[ServerEventBridge]
    Apply[ServerDirectorApplier]
    Cloud[CloudSaveClient]
    Run[RunTracker]
    Save[SaveSystem]
    Storm[StormController]
    Drowned[DrownedManager]
    Radio[Radio]
    Events[GameEvents]
    Telemetry[TelemetryReporter - not present]

    Bootstrap --> Config
    Bootstrap --> Auth
    Bootstrap --> Client
    Bootstrap --> Cloud
    Bootstrap --> Bridge
    Bootstrap --> Apply
    Auth -->|REST login/register| Server[(Lighthouse server)]
    Client -->|WebSocket| Server
    Client --> Dispatch --> Runtime
    Runtime --> Apply
    Apply --> Storm
    Apply --> Drowned
    Apply --> Radio
    Events --> Bridge --> Client
    Save --> Cloud --> Server
    Run --> Cloud
    Telemetry -.-> Server
```

`NetworkBootstrap` creates the persistent runtime objects when a
`NetworkConfig` asset is enabled. `AuthClient` obtains a JWT. `NetworkClient`
connects, sends client envelopes, receives server envelopes, and queues parsing
on `MainThreadDispatcher`. `ServerEventBridge` translates selected
`GameEvents` into outbound messages. `ServerDirectorApplier` applies
`STORM_STATE`, `PRESSURE_DELTA`, `RADIO_MESSAGE`, and selected narrative messages
to local systems. `CloudSaveClient` calls `/save` and `/runs`; `SaveSystem`
continues to write the local JSON save first.

There is no `TelemetryReporter` class or Unity telemetry endpoint in the current
repository. Telemetry is generated on the Java server from accepted WebSocket
messages.
