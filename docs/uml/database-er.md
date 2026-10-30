# Database ER diagram

The schema is created by `V1__create_players.sql`,
`V2__create_saves_and_runs.sql`, and `V3__create_telemetry_events.sql`.

```mermaid
erDiagram
    PLAYERS {
        BIGSERIAL id PK
        VARCHAR username UK
        VARCHAR password_hash
        TIMESTAMPTZ created_at
    }
    SAVES {
        BIGSERIAL id PK
        BIGINT player_id FK,UK
        VARCHAR act
        VARCHAR checkpoint
        TEXT items_json
        TEXT pages_json
        TEXT flags_json
        TEXT options_json
        INTEGER version
        TIMESTAMPTZ updated_at
    }
    RUNS {
        UUID id PK
        BIGINT player_id FK
        TIMESTAMPTZ started_at
        TIMESTAMPTZ ended_at
        VARCHAR outcome
        INTEGER deaths
        INTEGER duration_seconds
    }
    TELEMETRY_EVENTS {
        BIGSERIAL id PK
        UUID run_id FK
        BIGINT player_id FK
        VARCHAR type
        TEXT payload_json
        TIMESTAMPTZ created_at
    }
    PLAYERS ||--o| SAVES : owns
    PLAYERS ||--o{ RUNS : starts
    PLAYERS ||--o{ TELEMETRY_EVENTS : produces
    RUNS ||--o{ TELEMETRY_EVENTS : groups
```

`TelemetryService` currently creates telemetry rows with `run_id = NULL`; the
foreign key is present but no active-run lookup is implemented.
