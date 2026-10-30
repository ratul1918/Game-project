package com.thelighthouse.server.telemetry;

import org.springframework.data.jpa.repository.JpaRepository;
import java.time.Instant;

public interface TelemetryEventRepository extends JpaRepository<TelemetryEvent, Long> {
    long deleteByCreatedAtBefore(Instant cutoff);
}
