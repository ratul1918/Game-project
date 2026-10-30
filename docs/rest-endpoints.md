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
