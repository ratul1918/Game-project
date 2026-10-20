package com.emberblade.ws;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.io.IOException;
import java.util.concurrent.atomic.AtomicLong;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.TextMessage;

@Component
public class Broadcaster {
    private static final Logger log = LoggerFactory.getLogger(Broadcaster.class);
    private final SessionRegistry registry = new SessionRegistry();
    private final ObjectMapper objectMapper;
    private final AtomicLong sequence = new AtomicLong();

    public Broadcaster(ObjectMapper objectMapper) {
        this.objectMapper = objectMapper;
    }

    public SessionRegistry registry() {
        return registry;
    }

    public void broadcast(String type, Object payload) {
        final String text;
        try {
            text = objectMapper.writeValueAsString(new MessageEnvelope(type, sequence.incrementAndGet(), payload));
        } catch (JsonProcessingException exception) {
            log.error("Could not serialize WebSocket message {}", type, exception);
            return;
        }
        for (var session : registry.snapshot()) {
            if (!session.isOpen()) {
                registry.remove(session);
                continue;
            }
            try {
                synchronized (session) {
                    session.sendMessage(new TextMessage(text));
                }
            } catch (IOException exception) {
                log.warn("Removing WebSocket session {} after send failure", session.getId(), exception);
                registry.remove(session);
            }
        }
    }

    public void broadcastToUser(String username, String type, Object payload) {
        final String text;
        try {
            text = objectMapper.writeValueAsString(
                    new MessageEnvelope(type, sequence.incrementAndGet(), payload));
        } catch (JsonProcessingException exception) {
            log.error("Could not serialize WebSocket message {}", type, exception);
            return;
        }
        for (var session : registry.snapshot()) {
            if (!username.equals(session.getAttributes().get("username"))) {
                continue;
            }
            if (!session.isOpen()) {
                registry.remove(session);
                continue;
            }
            try {
                synchronized (session) {
                    session.sendMessage(new TextMessage(text));
                }
            } catch (IOException exception) {
                log.warn("Removing WebSocket session {} after send failure", session.getId(), exception);
                registry.remove(session);
            }
        }
    }
}
