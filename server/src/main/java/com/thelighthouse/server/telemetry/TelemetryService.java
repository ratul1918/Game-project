package com.thelighthouse.server.telemetry;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.thelighthouse.server.player.PlayerRepository;
import io.micrometer.core.instrument.*;
import jakarta.annotation.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;
import java.util.concurrent.*;
import java.util.concurrent.atomic.AtomicLong;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.scheduling.annotation.Scheduled;

@Service
public class TelemetryService {
    public record Pending(String username, String type, JsonNode payload) {}
    private final BlockingQueue<Pending> queue;
    private final TelemetryEventRepository events;
    private final PlayerRepository players;
    private final ObjectMapper json;
    private final int batchSize;
    private final long intervalMillis;
    private final long retentionDays;
    private final ExecutorService executor = Executors.newSingleThreadExecutor(Thread.ofPlatform().daemon(true).name("telemetry-writer").factory());
    private final AtomicLong dropped = new AtomicLong();
    private final Counter droppedCounter;
    private final io.micrometer.core.instrument.Timer writeTimer;
    private volatile boolean running = true;

    public TelemetryService(TelemetryEventRepository events, PlayerRepository players, ObjectMapper json, MeterRegistry meters,
            @Value("${app.telemetry.queue-capacity:4096}") int capacity,
            @Value("${app.telemetry.batch-size:128}") int batchSize,
            @Value("${app.telemetry.batch-interval-ms:500}") long intervalMillis,
            @Value("${app.telemetry.retention-days:30}") long retentionDays) {
        this.queue = new ArrayBlockingQueue<>(capacity);
        this.events = events; this.players = players; this.json = json;
        this.batchSize = batchSize; this.intervalMillis = intervalMillis; this.retentionDays = retentionDays;
        this.droppedCounter = Counter.builder("lighthouse.telemetry.dropped.events").register(meters);
        this.writeTimer = Timer.builder("lighthouse.telemetry.db.batch.write").register(meters);
        Gauge.builder("lighthouse.telemetry.queue.depth", queue, BlockingQueue::size).register(meters);
        executor.submit(this::writeLoop);
    }

    public void enqueue(String username, String type, JsonNode payload) {
        if (!Set.of("NOISE_BATCH", "NOISE_EMITTED", "PLAYER_HID", "LAMP_STATE", "BEAT_REACHED", "DEATH", "CHECKPOINT").contains(type)) return;
        if (!queue.offer(new Pending(username, type, payload))) {
            queue.poll();
            dropped.incrementAndGet();
            droppedCounter.increment();
            queue.offer(new Pending(username, type, payload));
        }
    }

    private void writeLoop() {
        while (running) {
            try {
                Pending first = queue.poll(intervalMillis, TimeUnit.MILLISECONDS);
                if (first == null) continue;
                List<Pending> batch = new ArrayList<>(batchSize);
                batch.add(first); queue.drainTo(batch, batchSize - 1);
                writeTimer.record(() -> writeBatch(batch));
            } catch (InterruptedException interrupted) {
                Thread.currentThread().interrupt(); return;
            } catch (RuntimeException ignored) {
                // A failed batch remains isolated from the world clock; the next batch can retry.
            }
        }
    }

    @Transactional
    protected void writeBatch(List<Pending> batch) {
        List<TelemetryEvent> rows = new ArrayList<>(batch.size());
        for (Pending item : batch) players.findByUsername(item.username()).ifPresent(player -> {
            try { rows.add(new TelemetryEvent(player, null, item.type(), json.writeValueAsString(item.payload()))); }
            catch (Exception ignored) {}
        });
        if (!rows.isEmpty()) events.saveAll(rows);
    }

    @PreDestroy
    void stop() { running = false; executor.shutdownNow(); }

    @Scheduled(fixedDelayString = "${app.telemetry.retention-cleanup-ms:3600000}")
    @Transactional
    void purgeExpired() {
        events.deleteByCreatedAtBefore(java.time.Instant.now().minus(java.time.Duration.ofDays(retentionDays)));
    }
}
