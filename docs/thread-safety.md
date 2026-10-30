# Server Thread-Safety Contract

The session registry is a `ConcurrentHashMap` keyed by WebSocket session ID. A single scheduled world-clock thread calls each `GameSession.tick()` once per second. Only that tick mutates session gameplay state, including storm intensity and future pressure/alert state.

WebSocket receiver threads deserialize frames and offer them to the session's bounded inbound queue; they never mutate session state. The tick drains that queue and handles PING or future client commands in order.

Outgoing envelopes enter a bounded queue. A dedicated virtual sender task drains each session queue and is the only task that calls `sendMessage` for that socket. When the queue is full, the oldest state update is discarded so a slow client cannot block or grow memory without limit.

Disconnect, transport error and sender failure remove the session idempotently from the registry and close the socket. Sequence numbers are atomic and private to one session.

Unity socket tasks only enqueue actions into `MainThreadDispatcher`. JSON-to-event translation and every `GameEvents` publication occur on Unity's main thread. Networking is optional: failure publishes Offline and never stops local gameplay.

Cloud saves are written on request threads and repositories are transaction-scoped.
They contain no shared mutable state. Unity always writes its local JSON save first;
cloud PUTs and retries run asynchronously and never block gameplay. A failed request
leaves the local save authoritative until a later Continue merge.

The world director is owned by one `GameSession` and runs only from that session's
scheduled tick. WebSocket receiver threads validate size/rate limits and enqueue
inbound envelopes; they do not mutate director state. The tick drains the bounded
queue, updates act/hidden/lamp/noise/death state, and enqueues bounded outbound
envelopes. The sender task remains the only task that calls `sendMessage`, so a
slow client cannot delay another session's tick. Unity receives frames off-thread
but enqueues all parsing and gameplay/event application on `MainThreadDispatcher`.

Telemetry events use a separate bounded queue sized by `app.telemetry.queue-capacity`.
The tick only offers an event to that queue; a dedicated telemetry executor drains
events in batches using `app.telemetry.batch-size` and
`app.telemetry.batch-interval-ms`. If full, it drops the oldest event and increments
the dropped-events counter. Database writes and retention cleanup therefore never
block the world-clock thread.
