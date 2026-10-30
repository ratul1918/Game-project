package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import java.io.IOException;
import java.time.Duration;
import java.time.Instant;
import java.util.concurrent.*;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicLong;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;

public final class GameSession {
    private final WebSocketSession socket; private final String username; private final ObjectMapper json;
    private final BlockingQueue<SocketEnvelope> inbound; private final BlockingQueue<SocketEnvelope> outbound;
    private final AtomicLong sequence=new AtomicLong(); private final AtomicBoolean running=new AtomicBoolean(true); private final Instant started=Instant.now();
    private double stormIntensity;
    public GameSession(WebSocketSession socket,String username,ObjectMapper json,int inboundCapacity,int outboundCapacity){this.socket=socket;this.username=username;this.json=json;this.inbound=new ArrayBlockingQueue<>(inboundCapacity);this.outbound=new ArrayBlockingQueue<>(outboundCapacity);}
    public String id(){return socket.getId();} public String username(){return username;}
    public void accept(SocketEnvelope message){inbound.offer(message);}
    public void tick(){if(!running.get())return;SocketEnvelope message;while((message=inbound.poll())!=null)handle(message);double seconds=Duration.between(started,Instant.now()).toMillis()/1000d;stormIntensity=.5d+.5d*Math.sin(seconds*.45d);ObjectNode payload=json.createObjectNode().put("intensity",stormIntensity).put("serverTime",Instant.now().toString());enqueue("STORM_STATE",payload);}
    private void handle(SocketEnvelope message){if("PING".equals(message.type()))enqueue("PONG",message.payload());}
    private void enqueue(String type,JsonNode payload){SocketEnvelope next=new SocketEnvelope(type,sequence.incrementAndGet(),payload);if(!outbound.offer(next)){outbound.poll();outbound.offer(next);}}
    public void senderLoop(Runnable disconnected){try{while(running.get()&&socket.isOpen()){SocketEnvelope message=outbound.poll(250,TimeUnit.MILLISECONDS);if(message==null)continue;synchronized(socket){socket.sendMessage(new TextMessage(json.writeValueAsString(message)));}}}catch(InterruptedException ignored){Thread.currentThread().interrupt();}catch(IOException ignored){}finally{running.set(false);disconnected.run();}}
    public void close(){running.set(false);try{if(socket.isOpen())socket.close();}catch(IOException ignored){}}
}
