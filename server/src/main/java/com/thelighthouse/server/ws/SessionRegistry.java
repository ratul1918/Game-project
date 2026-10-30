package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.annotation.PreDestroy;
import java.util.concurrent.*;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;

@Component
public class SessionRegistry {
    private final ConcurrentHashMap<String,GameSession> sessions=new ConcurrentHashMap<>();private final ExecutorService senders=Executors.newVirtualThreadPerTaskExecutor();private final ObjectMapper json;private final int inboundCapacity,outboundCapacity;
    public SessionRegistry(ObjectMapper json,@Value("${app.websocket.inbound-capacity}") int inboundCapacity,@Value("${app.websocket.outbound-capacity}") int outboundCapacity){this.json=json;this.inboundCapacity=inboundCapacity;this.outboundCapacity=outboundCapacity;}
    public GameSession register(WebSocketSession socket,String username){GameSession game=new GameSession(socket,username,json,inboundCapacity,outboundCapacity);sessions.put(game.id(),game);senders.submit(()->game.senderLoop(()->remove(game.id())));return game;}
    public GameSession get(String id){return sessions.get(id);} public void remove(String id){GameSession session=sessions.remove(id);if(session!=null)session.close();}
    public void tickAll(){sessions.values().forEach(GameSession::tick);}
    @PreDestroy void shutdown(){sessions.values().forEach(GameSession::close);senders.close();}
}
