package com.thelighthouse.server.telemetry;

import com.thelighthouse.server.player.Player;
import com.thelighthouse.server.run.Run;
import jakarta.persistence.*;
import java.time.Instant;

@Entity
@Table(name = "telemetry_events", indexes = {
    @Index(name = "idx_telemetry_events_run_id", columnList = "run_id"),
    @Index(name = "idx_telemetry_events_type", columnList = "type")
})
public class TelemetryEvent {
    @Id @GeneratedValue(strategy = GenerationType.IDENTITY) private Long id;
    @ManyToOne(optional = false, fetch = FetchType.LAZY) @JoinColumn(name = "player_id") private Player player;
    @ManyToOne(fetch = FetchType.LAZY) @JoinColumn(name = "run_id") private Run run;
    @Column(nullable = false, length = 32) private String type;
    @Column(name = "payload_json", nullable = false, columnDefinition = "TEXT") private String payloadJson;
    @Column(nullable = false) private Instant createdAt;

    protected TelemetryEvent() {}
    public TelemetryEvent(Player player, Run run, String type, String payloadJson) {
        this.player = player; this.run = run; this.type = type; this.payloadJson = payloadJson; this.createdAt = Instant.now();
    }
}
