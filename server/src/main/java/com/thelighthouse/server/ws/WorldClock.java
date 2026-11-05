package com.thelighthouse.server.ws;

import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import java.util.concurrent.*;
import org.springframework.stereotype.Component;

@Component
public class WorldClock {
    private final SessionRegistry sessions;private final ScheduledExecutorService clock=Executors.newSingleThreadScheduledExecutor(Thread.ofPlatform().daemon(true).name("world-clock").factory());
    public WorldClock(SessionRegistry sessions){this.sessions=sessions;}
    @PostConstruct void start(){clock.scheduleAtFixedRate(sessions::tickAll,0,1,TimeUnit.SECONDS);}
    @PreDestroy void stop(){clock.shutdownNow();}
}
