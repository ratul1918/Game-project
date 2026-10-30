package com.thelighthouse.server.save;

import com.thelighthouse.server.player.Player;
import jakarta.persistence.*;
import java.time.Instant;

@Entity
@Table(name = "saves")
public class Save {
    @Id @GeneratedValue(strategy = GenerationType.IDENTITY) private Long id;
    @OneToOne(optional = false, fetch = FetchType.LAZY) @JoinColumn(name = "player_id", nullable = false, unique = true)
    private Player player;
    @Column(nullable = false, length = 128) private String act;
    @Column(nullable = false, length = 128) private String checkpoint;
    @Column(name = "items_json", nullable = false, columnDefinition = "TEXT") private String itemsJson;
    @Column(name = "pages_json", nullable = false, columnDefinition = "TEXT") private String pagesJson;
    @Column(name = "flags_json", nullable = false, columnDefinition = "TEXT") private String flagsJson;
    @Column(name = "options_json", nullable = false, columnDefinition = "TEXT") private String optionsJson;
    @Column(nullable = false) private int version;
    @Column(name = "updated_at", nullable = false) private Instant updatedAt;
    protected Save() {}
    public Save(Player player, SaveRequest request) { this.player=player; update(request, 1); }
    public void update(SaveRequest request, int nextVersion) {
        act=request.act(); checkpoint=request.checkpoint(); itemsJson=request.itemsJson(); pagesJson=request.pagesJson();
        flagsJson=request.flagsJson(); optionsJson=request.optionsJson(); version=nextVersion; updatedAt=Instant.now();
    }
    public Long getId(){return id;} public Player getPlayer(){return player;} public String getAct(){return act;}
    public String getCheckpoint(){return checkpoint;} public String getItemsJson(){return itemsJson;} public String getPagesJson(){return pagesJson;}
    public String getFlagsJson(){return flagsJson;} public String getOptionsJson(){return optionsJson;} public int getVersion(){return version;}
    public Instant getUpdatedAt(){return updatedAt;}
}
