package com.emberblade.mission;

public record MissionResultRequest(
        String missionId,
        boolean success,
        int barricadeWalls,
        int barricadeTraps,
        float timeSeconds) {
}
