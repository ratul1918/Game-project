# WebSocket Message Contract

Connect to `/ws?token=<JWT>`. Every frame uses:

```json
{ "type": "STORM_STATE", "seq": 12, "payload": { "intensity": 0.62, "thunderIn": 0.4 } }
```

`seq` is incremented independently for each authenticated session. Unknown or
malformed message types are ignored. Unity keeps its local directors authoritative
when the server is offline or silent.

## Client to server

```json
{ "type": "NOISE_EMITTED", "seq": 0, "payload": { "loudness": 4.5 } }
{ "type": "PLAYER_HID", "seq": 0, "payload": { "hiding": true } }
{ "type": "LAMP_STATE", "seq": 0, "payload": { "state": "Failing", "charge": 0.2 } }
{ "type": "BEAT_REACHED", "seq": 0, "payload": { "id": "A2-B04" } }
{ "type": "CHECKPOINT", "seq": 0, "payload": { "act": "Act2", "checkpoint": "Cellar" } }
{ "type": "DEATH", "seq": 0, "payload": {} }
{ "type": "PING", "seq": 0, "payload": {} }
```

Noise is batched by the Unity bridge. The server bounds payload size and inbound
messages per session; invalid fields do not close the connection.

## Server to client

`STORM_STATE` is sent at the configured world send interval. Its curve changes
with the current act:

```json
{ "type": "STORM_STATE", "seq": 12, "payload": { "intensity": 0.62, "thunderIn": 0.38 } }
```

Adaptive pressure is throttled to the same interval and clamped by server config:

```json
{ "type": "PRESSURE_DELTA", "seq": 13, "payload": { "multiplier": 1.24 } }
```

Scheduled Marta lines and narrative hints:

```json
{ "type": "RADIO_MESSAGE", "seq": 14, "payload": { "id": "marta-act4-foreshadow", "text": "Daniel, the light remembers what you did." } }
{ "type": "ALERT", "seq": 15, "payload": { "kind": "stalker", "text": "Something heard you." } }
{ "type": "STORY_BEAT", "seq": 16, "payload": { "id": "A4-B05" } }
{ "type": "CHECKPOINT_SAVED", "seq": 17, "payload": { "checkpoint": "DawnCountdown" } }
```

Act 4 radio messages intentionally repeat identically as foreshadowing.
`PONG` echoes a client ping payload.

Accepted gameplay messages are also queued as telemetry (`NOISE_BATCH`,
`PLAYER_HID`, `LAMP_STATE`, `BEAT_REACHED`, `DEATH`, and `CHECKPOINT`) and
persisted asynchronously; telemetry persistence never delays a session tick.
