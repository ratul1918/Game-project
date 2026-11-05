package com.thelighthouse.server.ws;

import io.micrometer.core.instrument.*;
import java.util.concurrent.atomic.*;
import org.springframework.stereotype.Component;

@Component
public class ServerMetrics {
    private final AtomicLong activeSessions = new AtomicLong();
    private final AtomicLong outboundDepth = new AtomicLong();
    private final Counter messagesIn, messagesOut, droppedMessages, reconnects;
    private final Timer tickDuration;

    public ServerMetrics(MeterRegistry registry) {
        Gauge.builder("lighthouse.sessions.active", activeSessions, AtomicLong::doubleValue).register(registry);
        Gauge.builder("lighthouse.websocket.outbound.queue.depth", outboundDepth, AtomicLong::doubleValue).register(registry);
        messagesIn = Counter.builder("lighthouse.websocket.messages.in").register(registry);
        messagesOut = Counter.builder("lighthouse.websocket.messages.out").register(registry);
        droppedMessages = Counter.builder("lighthouse.websocket.dropped.messages").register(registry);
        reconnects = Counter.builder("lighthouse.websocket.reconnects").register(registry);
        tickDuration = Timer.builder("lighthouse.world.tick.duration").publishPercentiles(.5, .95, .99).register(registry);
    }
    void sessionOpened() { activeSessions.incrementAndGet(); }
    void sessionClosed() { activeSessions.updateAndGet(value -> Math.max(0, value - 1)); }
    void messageIn() { messagesIn.increment(); }
    void messageOut() { messagesOut.increment(); }
    void droppedMessage() { droppedMessages.increment(); }
    void reconnect() { reconnects.increment(); }
    void tick(Runnable action) { tickDuration.record(action); }
    void outboundDepth(long depth) { outboundDepth.set(depth); }
}
