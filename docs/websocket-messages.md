# WebSocket messages

The server accepts WebSocket connections at `/ws?token=<JWT>`. Every text frame
is a JSON envelope:

```json
{ "type": "STORM_STATE", "seq": 12, "payload": { "intensity": 0.62, "thunderIn": 0.38 } }
```

`SocketEnvelope` is the server representation:
[`SocketEnvelope.java`](../server/src/main/java/com/thelighthouse/server/ws/SocketEnvelope.java).
The three fields are always `type` (string), `seq` (number), and `payload` (JSON
value). The server requires a non-null type and payload for inbound processing.

## Client to server

The Unity bridge is
[`ServerEventBridge.cs`](../Assets/_Project/Scripts/Network/ServerEventBridge.cs).
It sends `seq: 0`; the server does not use the client sequence.

| Type | Payload | Constraints | Example |
|---|---|---|---|
| `NOISE_EMITTED` | `loudness` number | `0 <= loudness <= 100`; Unity batches local noise but sends this type | `{ "type":"NOISE_EMITTED", "seq":0, "payload":{"loudness":4.5} }` |
| `PLAYER_HID` | `hiding` boolean | Required boolean | `{ "type":"PLAYER_HID", "seq":0, "payload":{"hiding":true} }` |
| `LAMP_STATE` | `state` string, `charge` number | Non-blank state, max 32 chars; `0 <= charge <= 1` | `{ "type":"LAMP_STATE", "seq":0, "payload":{"state":"Failing","charge":0.2} }` |
| `BEAT_REACHED` | `id` string | Non-blank, max 64 chars; an ID beginning with `A` changes the server act | `{ "type":"BEAT_REACHED", "seq":0, "payload":{"id":"A2-B04"} }` |
| `CHECKPOINT` | `act` string, `checkpoint` string | Both non-blank, max 128 chars | `{ "type":"CHECKPOINT", "seq":0, "payload":{"act":"Act2","checkpoint":"Cellar"} }` |
| `DEATH` | object | No fields are read; payload must be non-null | `{ "type":"DEATH", "seq":0, "payload":{} }` |
| `PING` | any JSON value | Payload must be non-null; it is echoed by `PONG` | `{ "type":"PING", "seq":0, "payload":{"clientTime":123} }` |

`NOISE_BATCH` is accepted by the telemetry allow-list but is not handled by
[`WorldDirector.java`](../server/src/main/java/com/thelighthouse/server/ws/WorldDirector.java)
and is not sent by the current Unity bridge.

## Server to client

Server sequence numbers are private to each `GameSession`, start at 1, and
increase for every emitted envelope. The outbound queue is bounded by
`app.websocket.outbound-capacity` (default 64); when full, the oldest envelope
is removed. Therefore a client may observe sequence gaps under pressure.

| Type | Payload | Emission rule | Example |
|---|---|---|---|
| `STORM_STATE` | `intensity` number, `thunderIn` number | World tick, at `app.world.send-interval`; both values are clamped to 0..1 | `{ "type":"STORM_STATE", "seq":12, "payload":{"intensity":0.62,"thunderIn":0.38} }` |
| `PRESSURE_DELTA` | `multiplier` number | Emitted with `STORM_STATE`; pressure is clamped by `pressure-min` and `pressure-max` | `{ "type":"PRESSURE_DELTA", "seq":13, "payload":{"multiplier":1.24} }` |
| `RADIO_MESSAGE` | `id` string, `text` string | World tick, at `app.world.radio-interval`; Act 4 uses the fixed foreshadowing line | `{ "type":"RADIO_MESSAGE", "seq":14, "payload":{"id":"marta-act4-foreshadow","text":"Daniel, the light remembers what you did."} }` |
| `STORY_BEAT` | `id` string | Response to valid `BEAT_REACHED` | `{ "type":"STORY_BEAT", "seq":15, "payload":{"id":"A2-B04"} }` |
| `CHECKPOINT_SAVED` | `checkpoint` string | Response to valid `CHECKPOINT`; the act is not returned | `{ "type":"CHECKPOINT_SAVED", "seq":16, "payload":{"checkpoint":"Cellar"} }` |
| `PONG` | original ping payload | Response to `PING` | `{ "type":"PONG", "seq":17, "payload":{"clientTime":123} }` |

`ALERT` appears in older contract text but is not emitted by the current
[`WorldDirector`](../server/src/main/java/com/thelighthouse/server/ws/WorldDirector.java).

## Limits and error handling

- `GameWebSocketHandler` rejects text frames longer than 8192 payload characters.
- `GameSession` also rejects more than 32 inbound messages per wall-clock second.
  The current limit is hard-coded; the configured inbound queue capacity defaults
  to 32 and is controlled by `app.websocket.inbound-capacity`.
- A full inbound queue drops the new message and increments the dropped-message
  metric. A full outbound queue drops the oldest message and increments the same
  metric.
- Malformed JSON is ignored by `GameWebSocketHandler`; invalid fields are ignored
  by `WorldDirector` without closing the socket.
- Unknown message types are ignored.
- Missing, invalid, or expired JWTs close the socket with
  `CloseStatus.NOT_ACCEPTABLE`; there is no JSON WebSocket error envelope.
- The Unity receiver ignores null/unparseable envelopes and applies accepted
  messages on its `MainThreadDispatcher`.
