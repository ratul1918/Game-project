# The LightHouse

The LightHouse is a first-person lighthouse horror game about Daniel Hale,
guilt, a failing lamp, and the drowned keeper Crane. Unity contains the
playable Acts 1–5 greybox and local fallback gameplay. The Java server provides
optional authentication, cloud saves, run tracking, WebSocket world pressure,
metrics, and asynchronous telemetry.

## Prerequisites

For the target 8 GB MacBook Air M3:

- macOS with Unity 6 / the project version in `ProjectSettings/ProjectVersion.txt`
- Java 21 and Maven
- Docker Desktop with Compose
- 8 GB shared CPU/GPU memory; use the documented Low/Mac quality settings and
  the asset budget in [`docs/performance-budget.md`](docs/performance-budget.md)

## Run the server

```sh
docker compose up -d --wait postgres
cd server
mvn test
mvn spring-boot:run
```

The server listens on `http://localhost:8080` and PostgreSQL is mapped to
`localhost:5433`. Configuration and tunables are in
[`server/src/main/resources/application.yml`](server/src/main/resources/application.yml).

## Run Unity

Open the repository in Unity, ensure the `NetworkConfig` resource is enabled,
and press Play. The default development server is `http://localhost:8080` and
`ws://localhost:8080/ws`. If the server is unavailable, the client uses its
local gameplay systems when `offlineFallback` is enabled.

## Bot client and load test

```sh
cd bot-client
mvn package
java -cp target/lighthouse-bot-client-0.1.0.jar com.thelighthouse.bot.BotClient \
  --url http://localhost:8080 --bots 10 --duration 30 --ramp-up 5
```

The full staged load run is:

```sh
DURATION_SECONDS=3 RAMP_SECONDS=1 scripts/load-test.sh
```

See the measured results in [`docs/load-test.md`](docs/load-test.md).

## Documentation

- [`docs/architecture-overview.md`](docs/architecture-overview.md)
- [`docs/websocket-messages.md`](docs/websocket-messages.md)
- [`docs/rest-endpoints.md`](docs/rest-endpoints.md)
- [`docs/thread-safety.md`](docs/thread-safety.md)
- [`docs/demo-script.md`](docs/demo-script.md)
- [`docs/requirements-traceability.md`](docs/requirements-traceability.md)
- [`docs/uml/`](docs/uml/)
- [`docs/events.md`](docs/events.md)
- [`docs/architecture.md`](docs/architecture.md)
