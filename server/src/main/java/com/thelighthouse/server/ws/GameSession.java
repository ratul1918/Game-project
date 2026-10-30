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
import com.thelighthouse.server.telemetry.TelemetryService;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;

public final class GameSession {
    private final WebSocketSession socket; private final String username; private final ObjectMapper json;
    private final BlockingQueue<SocketEnvelope> inbound; private final BlockingQueue<SocketEnvelope> outbound;
    private final AtomicLong sequence=new AtomicLong(); private final AtomicBoolean running=new AtomicBoolean(true); private final Instant started=Instant.now();
    private final WorldDirector director;
    private final TelemetryService telemetry;
    private final ServerMetrics metrics;
    private final AtomicLong inboundWindow= new AtomicLong();
    private final AtomicLong inboundCount= new AtomicLong();
    public GameSession(WebSocketSession socket,String username,ObjectMapper json,int inboundCapacity,int outboundCapacity,
                       double stormRate,double actRateStep,double pressureMin,double pressureMax,double hideThreshold,
                       double weakLampThreshold,double pressureStep,double deathRelaxation,double sendInterval,double radioInterval,
                       TelemetryService telemetry, ServerMetrics metrics){
        this.socket=socket;this.username=username;this.json=json;this.inbound=new ArrayBlockingQueue<>(inboundCapacity);this.outbound=new ArrayBlockingQueue<>(outboundCapacity);
        this.director=new WorldDirector(json,stormRate,actRateStep,pressureMin,pressureMax,hideThreshold,weakLampThreshold,pressureStep,deathRelaxation,sendInterval,radioInterval);
        this.telemetry=telemetry; this.metrics=metrics;
    }
    public String id(){return socket.getId();} public String username(){return username;}
    public void accept(SocketEnvelope message){
        long now=System.currentTimeMillis()/1000;
        if(inboundWindow.get()!=now){inboundWindow.set(now);inboundCount.set(0);}
        if(inboundCount.incrementAndGet()>32||message==null||message.type()==null||message.payload()==null||message.payload().toString().length()>8192)return;
        if (!inbound.offer(message)) metrics.droppedMessage(); else metrics.messageIn();
    }
    public void tick(){if(!running.get())return;metrics.tick(() -> { SocketEnvelope message; while((message=inbound.poll())!=null){ telemetry.enqueue(username, message.type(), message.payload()); director.handle(this,message); } director.tick(this,1d); });}
    double elapsedSeconds(){return Duration.between(started,Instant.now()).toMillis()/1000d;}
    void emit(String type,JsonNode payload){enqueue(type,payload);}
    private void enqueue(String type,JsonNode payload){SocketEnvelope next=new SocketEnvelope(type,sequence.incrementAndGet(),payload);if(!outbound.offer(next)){outbound.poll();metrics.droppedMessage();outbound.offer(next);}metrics.outboundDepth(outbound.size());}
    public void senderLoop(Runnable disconnected){try{while(running.get()&&socket.isOpen()){SocketEnvelope message=outbound.poll(250,TimeUnit.MILLISECONDS);if(message==null)continue;synchronized(socket){socket.sendMessage(new TextMessage(json.writeValueAsString(message)));}}}catch(InterruptedException ignored){Thread.currentThread().interrupt();}catch(IOException ignored){}finally{running.set(false);disconnected.run();}}
    public void close(){running.set(false);try{if(socket.isOpen())socket.close();}catch(IOException ignored){}}
}
