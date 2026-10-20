package com.emberblade.save;

import java.util.Map;

public record SavePayload(
        int currentChapter,
        String[] completedMissions,
        Map<String, Integer> emberUpgrades,
        BarricadeData barricade) {
    public record BarricadeData(int walls, int traps) {
    }
}
