package com.emberblade.ws;

import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import org.springframework.web.socket.WebSocketSession;

public class SessionRegistry {
    private final Set<WebSocketSession> sessions = ConcurrentHashMap.newKeySet();

    public void add(WebSocketSession session) {
        sessions.add(session);
    }

    public void remove(WebSocketSession session) {
        sessions.remove(session);
    }

    public Set<WebSocketSession> snapshot() {
        return Set.copyOf(sessions);
    }
}
