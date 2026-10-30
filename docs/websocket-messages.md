# WebSocket Message Contract

Connect to `/ws?token=<JWT>`. Every frame uses this envelope:

```json
{ "type": "STORM_STATE", "seq": 12, "payload": { "intensity": 0.62, "serverTime": "2026-10-30T06:00:00Z" } }
```

`seq` is incremented independently for each authenticated session. Unknown message types must be ignored so clients and servers can roll forward independently.

## Implemented

### STORM_STATE (server → client)

Sent once per second. `intensity` is normalized from 0 to 1. It is diagnostic in Backend 1 and does not replace the offline `StormController`.

### PING (client → server)

```json
{ "type": "PING", "seq": 0, "payload": { "clientTime": "2026-10-30T06:00:00Z" } }
```

### PONG (server → client)

```json
{ "type": "PONG", "seq": 13, "payload": { "clientTime": "2026-10-30T06:00:00Z" } }
```

## Reserved placeholders

```json
{ "type": "ALERT", "seq": 20, "payload": { "source": "stalker", "level": 0.7 } }
{ "type": "PRESSURE_DELTA", "seq": 21, "payload": { "delta": 0.1 } }
{ "type": "RADIO_MESSAGE", "seq": 22, "payload": { "id": "marta-01" } }
{ "type": "STORY_BEAT", "seq": 23, "payload": { "id": "A1-B05" } }
{ "type": "CHECKPOINT_SAVED", "seq": 24, "payload": { "id": "DawnCountdown" } }
```

Reserved messages have no gameplay implementation yet.
