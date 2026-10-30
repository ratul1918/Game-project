# Server Thread-Safety Contract

The session registry is a `ConcurrentHashMap` keyed by WebSocket session ID. A single scheduled world-clock thread calls each `GameSession.tick()` once per second. Only that tick mutates session gameplay state, including storm intensity and future pressure/alert state.

WebSocket receiver threads deserialize frames and offer them to the session's bounded inbound queue; they never mutate session state. The tick drains that queue and handles PING or future client commands in order.

Outgoing envelopes enter a bounded queue. A dedicated virtual sender task drains each session queue and is the only task that calls `sendMessage` for that socket. When the queue is full, the oldest state update is discarded so a slow client cannot block or grow memory without limit.

Disconnect, transport error and sender failure remove the session idempotently from the registry and close the socket. Sequence numbers are atomic and private to one session.

Unity socket tasks only enqueue actions into `MainThreadDispatcher`. JSON-to-event translation and every `GameEvents` publication occur on Unity's main thread. Networking is optional: failure publishes Offline and never stops local gameplay.
