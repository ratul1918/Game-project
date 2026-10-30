# REST endpoints

The default base URL is `http://localhost:8080`. JSON responses use Jackson's
normal field names. Protected endpoints use an HTTP `Authorization` bearer JWT.

## Authentication

### `POST /auth/register`

No authentication. Request:

```json
{ "username": "daniel", "password": "lighthouse-dev" }
```

Success (`201 Created`):

```json
{ "token": "<JWT>" }
```

Validation requires a non-blank username of at most 64 characters and a password
of 8–128 characters. Duplicate usernames return `409`:

```json
{ "error": "409 CONFLICT", "message": "Username already exists" }
```

### `POST /auth/login`

No authentication. Request is the same as registration. Success (`200 OK`) is
`{ "token": "<JWT>" }`. Unknown users or incorrect passwords return `401`:

```json
{ "error": "401 UNAUTHORIZED", "message": "Invalid credentials" }
```

JWT signing uses `app.jwt.secret` and expires after
`app.jwt.expiration-seconds`. The development defaults are in
[`application.yml`](../server/src/main/resources/application.yml).

## Saves

### `GET /save`

Requires a JWT. Success (`200 OK`) returns the current save:

```json
{
  "act": "Act1",
  "checkpoint": "dock",
  "itemsJson": "{\"values\":[]}",
  "pagesJson": "{\"values\":[]}",
  "flagsJson": "{\"values\":[]}",
  "optionsJson": "{}",
  "version": 1,
  "updatedAt": "2026-10-30T09:00:00Z"
}
```

No save returns `404` with the standard status error. An absent/invalid JWT
returns `401` (the security entry point emits an empty error response).

### `PUT /save`

Requires a JWT. Request:

```json
{
  "act": "Act1",
  "checkpoint": "dock",
  "itemsJson": "{\"values\":[]}",
  "pagesJson": "{\"values\":[]}",
  "flagsJson": "{\"values\":[]}",
  "optionsJson": "{}",
  "version": 0
}
```

`act` and `checkpoint` are non-blank and max 128 characters. Each JSON string is
non-blank and max 131072 characters. `version` must be non-negative. The first
write must use version 0; success is `200 OK` with the save response and version
1. A stale version returns `409`:

```json
{
  "error": "SAVE_CONFLICT",
  "serverCopy": { "act":"Act1", "checkpoint":"dock", "version":2 }
}
```

The actual `serverCopy` contains the complete `SaveResponse`.

## Runs

### `POST /runs/start`

Requires a JWT and has no request body. Success (`200 OK`):

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "startedAt": "2026-10-30T09:00:00Z",
  "endedAt": null,
  "outcome": null,
  "deaths": 0,
  "durationSeconds": null
}
```

### `POST /runs/{id}/end`

Requires the owner JWT. Request:

```json
{ "outcome": "completed", "deaths": 0, "durationSeconds": 120 }
```

`outcome` is non-blank and max 32 characters; `deaths` is non-negative;
`durationSeconds` is 0–864000. Success (`200 OK`) returns the complete
`RunResponse`. Unknown runs and runs owned by another player return `404`; an
already-ended run returns `409`.

## Administration and Actuator

### `GET /admin/stats`

The route is permitted through Spring Security, but
[`AdminStatsController`](../server/src/main/java/com/thelighthouse/server/admin/AdminStatsController.java)
requires either a JWT whose username is exactly `admin` (assigned
`ROLE_ADMIN` by `JwtAuthenticationFilter`) or the configured development bearer
credential from `app.admin.dev-token`.

Success (`200 OK`) returns a snapshot containing:
`activeSessions`, `messagesInPerSecond`, `messagesOutPerSecond`,
`tickP95Seconds`, `outboundQueueDepth`, `droppedMessages`,
`telemetryQueueDepth`, `droppedEvents`, `dbBatchWriteP95Seconds`, `reconnects`,
and `averagesPerAct`. The current implementation returns one `all` average
bucket; `secondsHidden` is always zero because no hidden-time run data is stored.

An invalid credential returns `403`:

```json
{ "error": "403 FORBIDDEN", "message": "Admin access required" }
```

### `GET /admin/dashboard.html`

No authentication is required to fetch the static page. It polls
`/admin/stats` every two seconds, but does not send the dev token, so the page
shows `HTTP 403` unless the browser has a separately configured authenticated
request path.

### Actuator

`GET /actuator/health` is unauthenticated and returns Spring Boot health JSON.
`GET /actuator/metrics` and `GET /actuator/prometheus` are exposed by
configuration but require a JWT because only health is permitted anonymously.
Other Actuator routes are not exposed by `application.yml`.

## Common errors

Validation failures return `400`:

```json
{ "error": "INVALID_REQUEST", "message": "Request validation failed" }
```

`ResponseStatusException` responses use `{ "error": "<status> <reason>",
"message": "<reason>" }`; malformed or absent authentication rejected by Spring
Security may be an empty `401` response instead. The implementation is in
[`ApiExceptionHandler.java`](../server/src/main/java/com/thelighthouse/server/web/ApiExceptionHandler.java).
