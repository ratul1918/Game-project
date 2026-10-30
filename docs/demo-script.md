# Ten-minute demo script

## Pre-demo checklist

- Java 21, Maven, Docker, and Unity 6 are installed.
- Start PostgreSQL with `docker compose up -d --wait postgres`.
- Start the server with `cd server && mvn spring-boot:run`.
- Confirm `GET http://localhost:8080/actuator/health`.
- Open the Unity project and confirm a `NetworkConfig` resource is enabled.
- Have the configured `ADMIN_DEV_TOKEN` value available.
- Keep `/admin/dashboard.html`, a terminal, and the Unity Game view ready.

## Timeline

| Time | Show and say |
|---|---|
| 0:00–1:00 | Show the repository layout and explain that Unity owns gameplay while Spring owns the optional online side channel. |
| 1:00–2:00 | Start the server. In Unity, show `AuthClient` auto-registering/logging in the configured dev user and the Debug HUD changing from `Logging in`/`Offline` to `Online`. |
| 2:00–4:00 | Play Act 1: move with the new Input System, interact with the radio/lamp, and show local story beats. |
| 4:00–5:00 | Show the Debug HUD's server status, storm intensity, influence state, sequence, pressure, and latency fields. These are rendered by `DebugHud` from `NetworkRuntimeState`. |
| 5:00–6:00 | Stop the server. Show the HUD becoming `Offline`/`Fallback`; continue moving and using the local lamp/storm/story systems. |
| 6:00–7:00 | Restart the server. Show reconnect attempts and the HUD returning to `Online` when the WebSocket succeeds. |
| 7:00–8:00 | Open `/admin/dashboard.html`. For the stats request, use a browser/API client and supply the configured admin bearer credential; the static page itself does not add that header. Show active sessions, queue depth, tick p95, dropped counts, and run averages. |
| 8:00–9:00 | Query `telemetry_events` in PostgreSQL and show rows written by the asynchronous writer. Explain that current rows have `run_id` null. |
| 9:00–10:00 | Run `DURATION_SECONDS=3 RAMP_SECONDS=1 scripts/load-test.sh`. Show the CSVs in `docs/load-test/` and explain the measured 10/50/100/200/500 bot latency and sequence-gap results. |

## If something fails

| Symptom | Action |
|---|---|
| PostgreSQL connection refused | Run `docker compose up -d --wait postgres`; check port 5433. |
| Server health is unavailable | Inspect the server terminal; verify Java 21 and `DATABASE_URL`. |
| Unity remains Offline | Check the `NetworkConfig` asset, server URL, and `/auth/login`; local gameplay should continue. |
| Dashboard shows 403 | Supply the configured admin bearer credential when requesting `/admin/stats`; the static page does not send it. |
| No telemetry rows yet | Generate WebSocket input, then wait for the configured batch interval; telemetry is asynchronous. |
| Load test cannot start a bot | Confirm the server is healthy and run `cd bot-client && mvn package`. |
