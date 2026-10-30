package com.thelighthouse.server.run;

import com.thelighthouse.server.player.Player;
import jakarta.persistence.*;
import java.time.Instant;
import java.util.UUID;

@Entity @Table(name="runs")
public class Run {
    @Id private UUID id;
    @ManyToOne(optional=false,fetch=FetchType.LAZY) @JoinColumn(name="player_id") private Player player;
    @Column(name="started_at",nullable=false) private Instant startedAt;
    @Column(name="ended_at") private Instant endedAt;
    @Column(length=32) private String outcome;
    @Column(nullable=false) private int deaths;
    @Column(name="duration_seconds") private Integer durationSeconds;
    protected Run(){}
    public Run(Player player){this.id=UUID.randomUUID();this.player=player;this.startedAt=Instant.now();}
    public UUID getId(){return id;} public Player getPlayer(){return player;} public Instant getStartedAt(){return startedAt;}
    public Instant getEndedAt(){return endedAt;} public String getOutcome(){return outcome;} public int getDeaths(){return deaths;}
    public Integer getDurationSeconds(){return durationSeconds;}
    public void end(String outcome,int deaths,int duration){this.endedAt=Instant.now();this.outcome=outcome;this.deaths=deaths;this.durationSeconds=duration;}
}
