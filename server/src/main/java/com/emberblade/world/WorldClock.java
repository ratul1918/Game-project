package com.emberblade.world;

import com.emberblade.ws.Broadcaster;
import java.util.concurrent.ThreadLocalRandom;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;

@Component
public class WorldClock {
    private final Broadcaster broadcaster;
    private int threat;
    private boolean raidAnnounced;
    private int secondsSinceRaid;

    public WorldClock(Broadcaster broadcaster) {
        this.broadcaster = broadcaster;
    }

    @Scheduled(fixedRate = 1000)
    public synchronized void tick() {
        if (raidAnnounced) {
            secondsSinceRaid++;
            if (secondsSinceRaid >= 20) {
                threat = 0;
                raidAnnounced = false;
                secondsSinceRaid = 0;
            }
        } else {
            threat = Math.min(100, threat + ThreadLocalRandom.current().nextInt(1, 7));
            if (threat > 70) {
                raidAnnounced = true;
                secondsSinceRaid = 0;
                broadcaster.broadcast("ALERT_RAID",
                        new RaidPayload("Emberfield", "Raiders near Emberfield"));
            }
        }
        broadcaster.broadcast("WAR_STATE_DELTA", new ThreatPayload("Emberfield", threat));
    }

    public record RaidPayload(String front, String message) {
    }

    public record ThreatPayload(String front, int value) {
    }
}
