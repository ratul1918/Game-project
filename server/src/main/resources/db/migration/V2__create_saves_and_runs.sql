CREATE TABLE saves (
    id BIGSERIAL PRIMARY KEY,
    player_id BIGINT NOT NULL UNIQUE REFERENCES players(id),
    act VARCHAR(128) NOT NULL,
    checkpoint VARCHAR(128) NOT NULL,
    items_json TEXT NOT NULL,
    pages_json TEXT NOT NULL,
    flags_json TEXT NOT NULL,
    options_json TEXT NOT NULL,
    version INTEGER NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE runs (
    id UUID PRIMARY KEY,
    player_id BIGINT NOT NULL REFERENCES players(id),
    started_at TIMESTAMP WITH TIME ZONE NOT NULL,
    ended_at TIMESTAMP WITH TIME ZONE,
    outcome VARCHAR(32),
    deaths INTEGER NOT NULL DEFAULT 0,
    duration_seconds INTEGER
);
