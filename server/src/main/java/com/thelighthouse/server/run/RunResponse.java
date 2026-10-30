package com.thelighthouse.server.run;
import java.time.Instant; import java.util.UUID;
public record RunResponse(UUID id,Instant startedAt,Instant endedAt,String outcome,int deaths,Integer durationSeconds) {
    public static RunResponse from(Run r){return new RunResponse(r.getId(),r.getStartedAt(),r.getEndedAt(),r.getOutcome(),r.getDeaths(),r.getDurationSeconds());}
}
