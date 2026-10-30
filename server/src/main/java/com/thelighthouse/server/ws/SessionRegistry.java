package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.annotation.PreDestroy;
import java.util.concurrent.*;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.WebSocketSession;
import com.thelighthouse.server.telemetry.TelemetryService;

@Component
public class SessionRegistry {
    private final ConcurrentHashMap<String,GameSession> sessions=new ConcurrentHashMap<>();private final ExecutorService senders=Executors.newVirtualThreadPerTaskExecutor();private final ObjectMapper json;private final TelemetryService telemetry;private final ServerMetrics metrics;private final int inboundCapacity,outboundCapacity;private final double stormRate,actRateStep,pressureMin,pressureMax,hideThreshold,weakLampThreshold,pressureStep,deathRelaxation,sendInterval,radioInterval;
    public SessionRegistry(ObjectMapper json,@Value("${app.websocket.inbound-capacity}") int inboundCapacity,@Value("${app.websocket.outbound-capacity}") int outboundCapacity,
        @Value("${app.world.storm-rate:0.45}") double stormRate,@Value("${app.world.act-rate-step:0.12}") double actRateStep,@Value("${app.world.pressure-min:0.75}") double pressureMin,@Value("${app.world.pressure-max:2.0}") double pressureMax,@Value("${app.world.hide-threshold:8.0}") double hideThreshold,@Value("${app.world.weak-lamp-threshold:6.0}") double weakLampThreshold,@Value("${app.world.pressure-step:0.03}") double pressureStep,@Value("${app.world.death-relaxation:0.08}") double deathRelaxation,@Value("${app.world.send-interval:1.0}") double sendInterval,@Value("${app.world.radio-interval:12.0}") double radioInterval, TelemetryService telemetry, ServerMetrics metrics){
        this.json=json;this.telemetry=telemetry;this.metrics=metrics;this.inboundCapacity=inboundCapacity;this.outboundCapacity=outboundCapacity;this.stormRate=stormRate;this.actRateStep=actRateStep;this.pressureMin=pressureMin;this.pressureMax=pressureMax;this.hideThreshold=hideThreshold;this.weakLampThreshold=weakLampThreshold;this.pressureStep=pressureStep;this.deathRelaxation=deathRelaxation;this.sendInterval=sendInterval;this.radioInterval=radioInterval;}
    public GameSession register(WebSocketSession socket,String username){GameSession game=new GameSession(socket,username,json,inboundCapacity,outboundCapacity,stormRate,actRateStep,pressureMin,pressureMax,hideThreshold,weakLampThreshold,pressureStep,deathRelaxation,sendInterval,radioInterval,telemetry,metrics);if(sessions.putIfAbsent(game.id(),game)==null){metrics.sessionOpened();senders.submit(()->game.senderLoop(()->remove(game.id())));}return game;}
    public GameSession get(String id){return sessions.get(id);}     public void remove(String id){GameSession session=sessions.remove(id);if(session!=null){metrics.sessionClosed();session.close();}}
    public void tickAll(){sessions.values().forEach(GameSession::tick);}
    @PreDestroy void shutdown(){sessions.values().forEach(GameSession::close);senders.close();}
}
