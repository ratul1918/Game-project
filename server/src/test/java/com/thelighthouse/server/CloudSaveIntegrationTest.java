package com.thelighthouse.server;

import java.util.Map;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.http.*;
import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest(webEnvironment=SpringBootTest.WebEnvironment.RANDOM_PORT)
class CloudSaveIntegrationTest {
    @Autowired TestRestTemplate rest;
    private String token(String username) {
        var credentials=Map.of("username",username,"password","cloud-test-password");
        rest.postForEntity("/auth/register",credentials,Map.class);
        return (String)rest.postForEntity("/auth/login",credentials,Map.class).getBody().get("token");
    }
    private HttpEntity<Map<String,Object>> request(String token, Map<String,Object> body) {
        var headers=new HttpHeaders(); headers.setBearerAuth(token); headers.setContentType(MediaType.APPLICATION_JSON);
        return new HttpEntity<>(body,headers);
    }
    private Map<String,Object> save(int version) {
        return Map.of("act","Act1","checkpoint","dock","itemsJson","{\"values\":[]}","pagesJson","{\"values\":[]}",
            "flagsJson","{\"values\":[]}","optionsJson","{}","version",version);
    }
    @Test void saveRoundTripAndConflict() {
        String token=token("save-"+UUID.randomUUID());
        var put=rest.exchange("/save",HttpMethod.PUT,request(token,save(0)),Map.class);
        assertThat(put.getStatusCode()).isEqualTo(HttpStatus.OK); assertThat(put.getBody().get("version")).isEqualTo(1);
        assertThat(rest.exchange("/save",HttpMethod.GET,request(token,null),Map.class).getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(rest.exchange("/save",HttpMethod.PUT,request(token,save(0)),Map.class).getStatusCode()).isEqualTo(HttpStatus.CONFLICT);
    }
    @Test void unauthorizedAccessIsDenied() {
        assertThat(rest.getForEntity("/save",Map.class).getStatusCode()).isEqualTo(HttpStatus.UNAUTHORIZED);
    }
    @Test void runLifecycleBelongsToOwner() {
        String owner=token("run-owner-"+UUID.randomUUID()), other=token("run-other-"+UUID.randomUUID());
        var started=rest.postForEntity("/runs/start",new HttpEntity<>(new HttpHeaders(){ {setBearerAuth(owner);} }),Map.class);
        String id=(String)started.getBody().get("id");
        var body=Map.<String,Object>of("outcome","completed","deaths",1,"durationSeconds",12);
        assertThat(rest.exchange("/runs/"+id+"/end",HttpMethod.POST,request(other,body),Map.class).getStatusCode()).isEqualTo(HttpStatus.NOT_FOUND);
        assertThat(rest.exchange("/runs/"+id+"/end",HttpMethod.POST,request(owner,body),Map.class).getStatusCode()).isEqualTo(HttpStatus.OK);
    }
}
