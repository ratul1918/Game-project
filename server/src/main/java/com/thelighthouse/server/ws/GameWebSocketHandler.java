package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.thelighthouse.server.security.JwtService;
import java.net.URI;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.*;
import org.springframework.web.socket.handler.TextWebSocketHandler;

@Component
public class GameWebSocketHandler extends TextWebSocketHandler {
    private final JwtService jwt;private final SessionRegistry sessions;private final ObjectMapper json;
    public GameWebSocketHandler(JwtService jwt,SessionRegistry sessions,ObjectMapper json){this.jwt=jwt;this.sessions=sessions;this.json=json;}
    @Override public void afterConnectionEstablished(WebSocketSession socket)throws Exception{try{String token=query(socket.getUri(),"token");String username=jwt.validateAndGetUsername(token);sessions.register(socket,username);}catch(Exception invalid){socket.close(CloseStatus.NOT_ACCEPTABLE.withReason("Invalid token"));}}
    @Override protected void handleTextMessage(WebSocketSession socket,TextMessage message)throws Exception{GameSession session=sessions.get(socket.getId());if(session!=null)session.accept(json.readValue(message.getPayload(),SocketEnvelope.class));}
    @Override public void afterConnectionClosed(WebSocketSession socket,CloseStatus status){sessions.remove(socket.getId());}
    @Override public void handleTransportError(WebSocketSession socket,Throwable exception){sessions.remove(socket.getId());}
    private static String query(URI uri,String name){if(uri==null||uri.getRawQuery()==null)throw new IllegalArgumentException("Missing token");return Arrays.stream(uri.getRawQuery().split("&")).map(value->value.split("=",2)).filter(parts->parts.length==2&&parts[0].equals(name)).map(parts->URLDecoder.decode(parts[1],StandardCharsets.UTF_8)).findFirst().orElseThrow(()->new IllegalArgumentException("Missing token"));}
}
