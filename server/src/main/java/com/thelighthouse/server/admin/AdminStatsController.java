package com.thelighthouse.server.admin;

import com.thelighthouse.server.run.RunRepository;
import io.micrometer.core.instrument.*;
import io.micrometer.core.instrument.distribution.ValueAtPercentile;
import java.util.*;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;

@RestController
public class AdminStatsController {
    private final MeterRegistry meters;
    private final RunRepository runs;
    private final String devToken;

    public AdminStatsController(MeterRegistry meters, RunRepository runs, @Value("${app.admin.dev-token:dev-admin-token}") String devToken) {
        this.meters = meters; this.runs = runs; this.devToken = devToken;
    }

    @GetMapping("/admin/stats")
    public Map<String, Object> stats(@RequestHeader(value = "Authorization", required = false) String authorization) {
        boolean admin = SecurityContextHolder.getContext().getAuthentication().getAuthorities().stream()
                .anyMatch(a -> a.getAuthority().equals("ROLE_ADMIN"));
        if (!admin && (authorization == null || !authorization.equals("Bearer " + devToken)))
            throw new ResponseStatusException(HttpStatus.FORBIDDEN, "Admin access required");
        Map<String, Object> result = new LinkedHashMap<>();
        result.put("activeSessions", gauge("lighthouse.sessions.active"));
        result.put("messagesInPerSecond", rate("lighthouse.websocket.messages.in"));
        result.put("messagesOutPerSecond", rate("lighthouse.websocket.messages.out"));
        result.put("tickP95Seconds", percentile("lighthouse.world.tick.duration", 0.95));
        result.put("outboundQueueDepth", gauge("lighthouse.websocket.outbound.queue.depth"));
        result.put("droppedMessages", count("lighthouse.websocket.dropped.messages"));
        result.put("telemetryQueueDepth", gauge("lighthouse.telemetry.queue.depth"));
        result.put("droppedEvents", count("lighthouse.telemetry.dropped.events"));
        result.put("dbBatchWriteP95Seconds", percentile("lighthouse.telemetry.db.batch.write", 0.95));
        result.put("reconnects", count("lighthouse.websocket.reconnects"));
        result.put("averagesPerAct", Map.of("all", runAverages()));
        return result;
    }

    private Map<String, Object> runAverages() {
        var ended = runs.findAll().stream().filter(run -> run.getEndedAt() != null).toList();
        double hidden = 0, deaths = 0, duration = 0;
        for (var run : ended) { deaths += run.getDeaths(); duration += Optional.ofNullable(run.getDurationSeconds()).orElse(0); }
        return Map.of("secondsHidden", hidden, "deaths", ended.isEmpty() ? 0 : deaths / ended.size(),
                "timeToFinishSeconds", ended.isEmpty() ? 0 : duration / ended.size());
    }
    private double gauge(String name) { Gauge gauge = meters.find(name).gauge(); return gauge == null ? 0d : gauge.value(); }
    private double count(String name) { Counter counter = meters.find(name).counter(); return counter == null ? 0d : counter.count(); }
    private double rate(String name) { return count(name); }
    private double percentile(String name, double percentile) {
        io.micrometer.core.instrument.Timer timer = meters.find(name).timer();
        if (timer == null) return 0d;
        return Arrays.stream(timer.takeSnapshot().percentileValues())
                .filter(p -> p.percentile() == percentile).mapToDouble(ValueAtPercentile::value).findFirst().orElse(0d) / 1_000_000_000d;
    }
}
