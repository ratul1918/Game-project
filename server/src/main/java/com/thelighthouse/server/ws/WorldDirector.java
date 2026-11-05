package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import java.util.Locale;

final class WorldDirector {
    private final ObjectMapper json;
    private final double stormRate;
    private final double actRateStep;
    private final double pressureMin;
    private final double pressureMax;
    private final double hideThreshold;
    private final double weakLampThreshold;
    private final double pressureStep;
    private final double deathRelaxation;
    private final double sendInterval;
    private final double radioInterval;
    private String act = "Act1";
    private double hiddenSeconds;
    private double weakLampSeconds;
    private int recentNoiseCount;
    private int deaths;
    private double pressure = 1;
    private double sendTimer;
    private double radioTimer;
    private long noiseWindow;
    private boolean hidden;
    private String lampState = "FuelLit";
    private double lampCharge = 1;
    double pressure(){return pressure;}
    String act(){return act;}

    WorldDirector(ObjectMapper json, double stormRate, double actRateStep, double pressureMin, double pressureMax,
                  double hideThreshold, double weakLampThreshold, double pressureStep, double deathRelaxation,
                  double sendInterval, double radioInterval) {
        this.json=json; this.stormRate=stormRate; this.actRateStep=actRateStep; this.pressureMin=pressureMin;
        this.pressureMax=pressureMax; this.hideThreshold=hideThreshold; this.weakLampThreshold=weakLampThreshold;
        this.pressureStep=pressureStep; this.deathRelaxation=deathRelaxation; this.sendInterval=sendInterval; this.radioInterval=radioInterval;
    }
    void tick(GameSession session, double deltaSeconds) {
        if (hidden) hiddenSeconds += deltaSeconds;
        if ("Failing".equals(lampState) || "Extinguished".equals(lampState)) weakLampSeconds += deltaSeconds;
        sendTimer += deltaSeconds; radioTimer += deltaSeconds;
        if (hiddenSeconds >= hideThreshold || weakLampSeconds >= weakLampThreshold) pressure += pressureStep * deltaSeconds;
        if (deaths > 0) { pressure -= deathRelaxation * deaths; deaths = 0; }
        pressure = clamp(pressure, pressureMin, pressureMax);
        if (sendTimer >= sendInterval) { sendTimer = 0; emitState(session); }
        if (radioTimer >= radioInterval) { radioTimer = 0; emitRadio(session); }
    }
    void handle(GameSession session, SocketEnvelope message) {
        if (message == null || message.type() == null || message.payload() == null) return;
        var p=message.payload();
        switch (message.type()) {
            case "NOISE_EMITTED" -> { if (number(p,"loudness",0,100)) { recentNoiseCount++; noiseWindow++; } }
            case "PLAYER_HID" -> { if (p.has("hiding") && p.get("hiding").isBoolean()) hidden=p.get("hiding").booleanValue(); }
            case "LAMP_STATE" -> { if (text(p,"state",32) && number(p,"charge",0,1)) { lampState=p.get("state").textValue(); lampCharge=p.get("charge").doubleValue(); } }
            case "BEAT_REACHED" -> { if (text(p,"id",64)) { act=actFrom(p.get("id").textValue()); if(session!=null)session.emit("STORY_BEAT", node().put("id", p.get("id").textValue())); } }
            case "CHECKPOINT" -> { if (text(p,"act",128) && text(p,"checkpoint",128) && session!=null) session.emit("CHECKPOINT_SAVED", node().put("checkpoint",p.get("checkpoint").textValue())); }
            case "DEATH" -> deaths++;
            case "PING" -> { if(session!=null)session.emit("PONG", p); }
            default -> { }
        }
    }
    private void emitState(GameSession session) {
        double actIndex=act.charAt(act.length()-1)-'1';
        double intensity=clamp(.5 + .5*Math.sin((session.elapsedSeconds())*stormRate*(1+actIndex*this.actRateStep)),0,1);
        session.emit("STORM_STATE",node().put("intensity",intensity).put("thunderIn",Math.max(0,1.0-intensity)));
        session.emit("PRESSURE_DELTA",node().put("multiplier",pressure));
        recentNoiseCount=0; noiseWindow=0;
    }
    private void emitRadio(GameSession session) {
        if (act.startsWith("Act4")) session.emit("RADIO_MESSAGE",node().put("id","marta-act4-foreshadow").put("text","Daniel, the light remembers what you did."));
        else session.emit("RADIO_MESSAGE",node().put("id","marta-"+act.toLowerCase(Locale.ROOT)).put("text","Daniel, keep the lamp lit."));
    }
    private ObjectNode node(){return json.createObjectNode();}
    private static String actFrom(String beat){return beat.length() >= 2 && beat.charAt(0)=='A' ? "Act"+beat.substring(1,2) : "Act1";}
    private static boolean text(com.fasterxml.jackson.databind.JsonNode n,String key,int max){return n.has(key)&&n.get(key).isTextual()&&!n.get(key).textValue().isBlank()&&n.get(key).textValue().length()<=max;}
    private static boolean number(com.fasterxml.jackson.databind.JsonNode n,String key,double min,double max){return n.has(key)&&n.get(key).isNumber()&&n.get(key).doubleValue()>=min&&n.get(key).doubleValue()<=max;}
    private static double clamp(double value,double min,double max){return Math.max(min,Math.min(max,value));}
}
