# REST Endpoints

Development base URL: `http://localhost:8080`.

## POST /auth/register

Request:

```json
{ "username": "daniel", "password": "lighthouse-dev" }
```

Creates a BCrypt password hash and returns HTTP 201:

```json
{ "token": "<JWT>" }
```

Returns 409 when the username already exists.

## POST /auth/login

Uses the same request body. Valid credentials return HTTP 200 and `{ "token": "<JWT>" }`; invalid credentials return 401. The JWT signing secret comes from `JWT_SECRET`, with a development-only default for local use.

## GET /actuator/health

Unauthenticated local health check. All other endpoints are denied unless explicitly configured.

## Cloud saves and runs

Authenticated requests use `Authorization: Bearer <JWT>`. `GET /save` returns 404 when
the player has no cloud save. `PUT /save` accepts `act`, `checkpoint`, JSON strings
`itemsJson`, `pagesJson`, `flagsJson`, `optionsJson`, and the client version. The first
write uses version 0; later writes must send the returned version. The server increments
the version and returns 409 with `error: SAVE_CONFLICT` and `serverCopy` when it is stale.
Field lengths and JSON payload strings are bounded.

`POST /runs/start` creates an owned run and returns its UUID. The owner ends it with
`POST /runs/{id}/end` and `{ "outcome": "completed", "deaths": 0, "durationSeconds": 120 }`.
Run and save ownership is derived from the JWT principal; unknown owners cannot access
another player's records.

## Operations

`GET /actuator/metrics` and `GET /actuator/prometheus` expose server metrics.
`GET /admin/stats` requires an authenticated admin principal or
`Authorization: Bearer <ADMIN_DEV_TOKEN>`. `/admin/dashboard.html` polls the
snapshot every two seconds.
