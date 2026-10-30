package com.thelighthouse.server;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.WebSocket;
import java.time.Duration;
import java.util.Map;
import java.util.concurrent.*;
import java.lang.StringBuilder;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.test.annotation.DirtiesContext;
import org.springframework.http.*;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest(webEnvironment=SpringBootTest.WebEnvironment.RANDOM_PORT)
@DirtiesContext(classMode = DirtiesContext.ClassMode.AFTER_CLASS)
class BackendThinSliceIntegrationTest {
    @LocalServerPort int port;
    @Autowired TestRestTemplate rest;
    @Autowired ObjectMapper json;

    @Test void registerLoginConnectAndReceiveStormStates() throws Exception {
        String username="keeper-"+System.nanoTime();Map<String,String> credentials=Map.of("username",username,"password","storm-test-password");
        ResponseEntity<Map> registered=rest.postForEntity("/auth/register",credentials,Map.class);assertThat(registered.getStatusCode()).isEqualTo(HttpStatus.CREATED);
        ResponseEntity<Map> loggedIn=rest.postForEntity("/auth/login",credentials,Map.class);assertThat(loggedIn.getStatusCode()).isEqualTo(HttpStatus.OK);String token=(String)loggedIn.getBody().get("token");assertThat(token).isNotBlank();
        BlockingQueue<String> messages=new LinkedBlockingQueue<>();WebSocket socket=HttpClient.newHttpClient().newWebSocketBuilder().connectTimeout(Duration.ofSeconds(5)).buildAsync(URI.create("ws://localhost:"+port+"/ws?token="+token),new WebSocket.Listener(){StringBuilder current=new StringBuilder();public void onOpen(WebSocket webSocket){webSocket.request(1);}public CompletionStage<?> onText(WebSocket webSocket,CharSequence data,boolean last){current.append(data);if(last){messages.offer(current.toString());current.setLength(0);}webSocket.request(1);return null;}}).get(5,TimeUnit.SECONDS);
        int stormStates=0;long deadline=System.nanoTime()+TimeUnit.SECONDS.toNanos(5);while(stormStates<2&&System.nanoTime()<deadline){String message=messages.poll(2,TimeUnit.SECONDS);if(message==null)continue;JsonNode envelope=json.readTree(message);if("STORM_STATE".equals(envelope.path("type").asText())){stormStates++;assertThat(envelope.path("seq").asLong()).isPositive();assertThat(envelope.path("payload").path("intensity").asDouble()).isBetween(0d,1d);}}
        socket.sendClose(WebSocket.NORMAL_CLOSURE,"done").get(2,TimeUnit.SECONDS);assertThat(stormStates).isGreaterThanOrEqualTo(2);
    }
}
