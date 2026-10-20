package com.emberblade.mission;

import com.emberblade.save.SavePayload;
import com.emberblade.save.SaveService;
import com.emberblade.ws.Broadcaster;
import java.util.Arrays;
import java.util.LinkedHashSet;
import java.util.Set;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class MissionService {
    private final SaveService saveService;
    private final Broadcaster broadcaster;

    public MissionService(SaveService saveService, Broadcaster broadcaster) {
        this.saveService = saveService;
        this.broadcaster = broadcaster;
    }

    @Transactional
    public SavePayload start(String username, String missionId) {
        validateMissionId(missionId);
        SavePayload save = saveService.load(username);
        broadcaster.broadcastToUser(username, "STORY_BEAT", new StoryBeatPayload(1, missionId, "started"));
        return save;
    }

    @Transactional
    public SavePayload result(String username, MissionResultRequest request) {
        validateMissionId(request.missionId());
        SavePayload save = saveService.load(username);
        Set<String> completed = new LinkedHashSet<>(Arrays.asList(save.completedMissions()));
        if (request.success()) {
            completed.add(request.missionId());
        }
        SavePayload updated = new SavePayload(
                save.currentChapter(),
                completed.toArray(String[]::new),
                save.emberUpgrades(),
                new SavePayload.BarricadeData(request.barricadeWalls(), request.barricadeTraps()));
        SavePayload stored = saveService.save(username, updated);
        broadcaster.broadcastToUser(
                username,
                "STORY_BEAT",
                new StoryBeatPayload(1, request.missionId(), request.success() ? "completed" : "failed"));
        return stored;
    }

    private void validateMissionId(String missionId) {
        if (!"1.1".equals(missionId) && !"1.2".equals(missionId) && !"1.3".equals(missionId)) {
            throw new IllegalArgumentException("Unknown Chapter 1 mission: " + missionId);
        }
    }

    public record StoryBeatPayload(int chapter, String mission, String beat) {
    }
}
