package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import org.junit.jupiter.api.Test;
import static org.assertj.core.api.Assertions.assertThat;

class WorldDirectorTest {
    private final ObjectMapper json = new ObjectMapper();
    private WorldDirector director(){return new WorldDirector(json,.45,.12,.75,1.5,2,2,.1,.2,1000,1000);}
    private SocketEnvelope message(String type, String field, Object value){
        ObjectNode payload=json.createObjectNode();
        if(value instanceof Boolean b)payload.put(field,b); else if(value instanceof Number n)payload.put(field,n.doubleValue()); else payload.put(field,value.toString());
        return new SocketEnvelope(type,0,payload);
    }
    @Test void pressureRisesAndClampsForHiddenWeakLamp(){
        WorldDirector d=director();d.handle(null,message("PLAYER_HID","hiding",true));ObjectNode lamp=json.createObjectNode().put("state","Failing").put("charge",.1);d.handle(null,new SocketEnvelope("LAMP_STATE",0,lamp));
        for(int i=0;i<30;i++)d.tick(null,1);
        assertThat(d.pressure()).isEqualTo(1.5);
    }
    @Test void deathsRelaxPressureAndBeatChangesAct(){
        WorldDirector d=director();d.handle(null,message("PLAYER_HID","hiding",true));for(int i=0;i<10;i++)d.tick(null,1);
        double before=d.pressure();d.handle(null,message("DEATH","unused",0));d.tick(null,1);
        assertThat(d.pressure()).isLessThan(before);d.handle(null,message("BEAT_REACHED","id","A4-B05"));assertThat(d.act()).isEqualTo("Act4");
    }
}
