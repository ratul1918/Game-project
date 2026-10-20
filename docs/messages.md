# Ember Blade messages

Every server-to-client message uses this envelope:

```json
{
  "type": "MESSAGE_TYPE",
  "seq": 1,
  "payload": {}
}
```

`seq` is a server-wide increasing number. The Phase 1 server currently pushes:

## `WAR_STATE_DELTA`

```json
{
  "type": "WAR_STATE_DELTA",
  "seq": 12,
  "payload": {
    "front": "Emberfield",
    "value": 64
  }
}
```

## `ALERT_RAID`

```json
{
  "type": "ALERT_RAID",
  "seq": 13,
  "payload": {
    "front": "Emberfield",
    "message": "Raiders near Emberfield"
  }
}
```

## `STORY_BEAT`

Story beats are sent to the authenticated user's WebSocket session when a
Chapter 1 mission starts or completes.

```json
{
  "type": "STORY_BEAT",
  "seq": 14,
  "payload": {
    "chapter": 1,
    "mission": "1.1",
    "beat": "started"
  }
}
```
