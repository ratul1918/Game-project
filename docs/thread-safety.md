# Thread-safety and concurrency

## Server

| Structure | Writers/readers | Synchronization | Why it is safe |
|---|---|---|---|
| `SessionRegistry.sessions` (`ConcurrentHashMap`) | WebSocket callbacks register/remove/get; `WorldClock` reads values during `tickAll` | `ConcurrentHashMap` operations; `putIfAbsent`; idempotent remove | Registry operations are thread-safe and a disconnect can race safely with sender cleanup. |
| `SessionRegistry.senders` | `SessionRegistry` submits; virtual sender tasks execute | `ExecutorService` | Each registered session gets one sender task. |
| `GameSession.inbound` (`ArrayBlockingQueue`) | WebSocket receiver offers; world-clock tick polls | Bounded blocking queue; tick uses non-blocking `offer`/`poll` | Receiver threads never mutate `WorldDirector`; the tick drains messages in order. |
| `GameSession.outbound` (`ArrayBlockingQueue`) | World tick offers; sender polls | Bounded queue; oldest item is polled when full | A slow client cannot block the world tick or grow memory without bound. |
| `GameSession.sequence` and `running` | Tick/sender/close paths | `AtomicLong` and `AtomicBoolean` | Sequence ownership is per session; stop is idempotent. |
| `GameSession.inboundWindow`/`inboundCount` | WebSocket receiver threads | `AtomicLong` values | Counters are atomic, but the window reset and increment are separate operations. |
| `WebSocketSession` | Sender task sends; close paths close; sender uses `synchronized(socket)` around `sendMessage` | Socket monitor for sends | Only the sender calls `sendMessage`; close is idempotent. |
| `WorldDirector` fields | Only `WorldClock`'s tick path | Thread confinement to the world-clock thread | Gameplay state is not changed by WebSocket receiver threads. |
| `TelemetryService.queue` (`ArrayBlockingQueue`) | Session ticks enqueue; telemetry writer drains | Bounded queue, non-blocking `offer`/`poll`/`drainTo` | Database work is not on the world-clock path. A full queue drops the oldest event. |
| `TelemetryService.dropped` | Tick-side enqueue path and metrics | `AtomicLong` | Drop count is safe to update from concurrent sessions. |
| Telemetry JPA repositories | One telemetry executor and Spring request/scheduled threads | Transaction-scoped Spring Data calls | Repository calls do not share mutable entity state between requests. |
| `ServerMetrics` counters/gauges | Sessions, ticks, and telemetry writer | Micrometer counters plus `AtomicLong` gauge sources | Micrometer registry instruments concurrent updates. |
| `WorldClock.clock` | Scheduled executor only; lifecycle methods stop it | Single-thread scheduled executor | `tickAll` is serialized once per second. |

`GameSession.tick()` is measured by `ServerMetrics` and calls
`TelemetryService.enqueue`, `WorldDirector.handle`, and `WorldDirector.tick`.
It never waits for a database write. The telemetry writer uses a daemon
platform thread and batches up to `app.telemetry.batch-size` after waiting up to
`app.telemetry.batch-interval-ms`.

## Unity

| Structure | Writers/readers | Synchronization | Why it is safe |
|---|---|---|---|
| `MainThreadDispatcher.pending` (`ConcurrentQueue<Action>`) | `NetworkClient` receive tasks enqueue; Unity `Update` drains | Concurrent queue | Socket callbacks do not directly mutate Unity objects. |
| `ClientWebSocket` | `NetworkClient` connect/receive/ping/send tasks | Async WebSocket API; one active connection loop | Networking is isolated from Unity's frame loop; disposal occurs in `OnDestroy`. |
| `NetworkRuntimeState` | Dispatcher actions and Unity systems | Main-thread event publication by the client | Received messages are dispatched before `GameEvents` or director application. |
| `GameEvents` static events | Unity components subscribe/unsubscribe | Unity main-thread event calls | Producers and consumers run through Unity lifecycle methods. |
| `SaveSystem` local data and hash sets | Unity gameplay callbacks and save/load methods | Unity main thread; local file writes | Cloud requests are coroutines and local JSON remains authoritative until merge. |

## Decisions to defend

1. Never perform JPA work from `WorldClock` or a WebSocket receiver.
2. Bound both inbound and outbound per-session memory.
3. Keep one owner for mutable gameplay state (`WorldDirector` per session).
4. Make disconnect cleanup idempotent because transport close and sender failure
   can happen concurrently.
5. Cross the Unity thread boundary once, at `MainThreadDispatcher`, before
   applying received data to gameplay.

## Known risks

- The inbound rate-window reset and increment are separate atomic operations, so
  simultaneous receiver threads can make the exact 32-message boundary
  nondeterministic.
- `ServerMetrics.messagesOut` and `reconnects` are defined but no current code
  increments them.
- `TelemetryService` catches batch `RuntimeException` without retrying that
  batch, so a database failure can lose dequeued telemetry.
- `TelemetryService` looks up players by username for each row and currently
  writes `run_id = NULL`; this can become a throughput bottleneck.
- `NetworkClient` uses a single reusable receive buffer and does not expose
  server sequence-gap errors to gameplay.
