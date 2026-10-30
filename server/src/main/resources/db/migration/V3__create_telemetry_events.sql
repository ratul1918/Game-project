CREATE TABLE telemetry_events (
    id BIGSERIAL PRIMARY KEY,
    run_id UUID REFERENCES runs(id),
    player_id BIGINT NOT NULL REFERENCES players(id),
    type VARCHAR(32) NOT NULL,
    payload_json TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_telemetry_events_run_id ON telemetry_events(run_id);
CREATE INDEX idx_telemetry_events_type ON telemetry_events(type);
