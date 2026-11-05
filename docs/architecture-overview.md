# Architecture overview

The project has two runtime halves:

- **Unity client:** C# scripts under
  [`Assets/_Project/Scripts`](../Assets/_Project/Scripts) own movement, story,
  lamp, storm, Drowned, saves, and the optional network side channel.
- **Spring server:** Java 21 code under
  [`server/src/main/java`](../server/src/main/java) owns authentication,
  cloud saves, runs, WebSocket world state, metrics, and asynchronous telemetry.

The server starts with Spring Boot and Flyway. PostgreSQL is supplied by
[`docker-compose.yml`](../docker-compose.yml), using port 5433 by default.
Unity starts [`NetworkBootstrap.cs`](../Assets/_Project/Scripts/Network/NetworkBootstrap.cs)
before the first scene when a `NetworkConfig` asset is enabled.

## End-to-end message path

1. `AuthClient` registers/logs in through `/auth/register` or `/auth/login`.
2. `NetworkClient` connects to `/ws?token=<JWT>`.
3. `GameWebSocketHandler` validates the token and registers a `GameSession`.
4. `WorldClock` calls `SessionRegistry.tickAll()` once per second.
5. `GameSession` drains its inbound queue, enqueues accepted telemetry, and lets
   its `WorldDirector` update storm pressure and scheduled messages.
6. The session's virtual sender writes `{type, seq, payload}` envelopes to the
   socket. `NetworkClient` receives them and queues handling on
   `MainThreadDispatcher`.
7. `ServerDirectorApplier` applies server storm/pressure/radio messages to
   `StormController`, `DrownedManager`, `StalkerAI`, and `Radio`.
8. If the socket is unavailable or silent, `NetworkClient` and
   `ServerDirectorApplier` return control to the local Unity systems. Local
   `StormController`, `DrownedManager`, story state, and saves continue.

`ServerEventBridge` sends selected local events to the server. `CloudSaveClient`
handles `/save` and `/runs`; `SaveSystem` writes its local JSON snapshot before
attempting a cloud PUT. There is currently no Unity `TelemetryReporter`; server
telemetry is derived from WebSocket messages.
