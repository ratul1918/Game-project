package com.emberblade.save;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.core.type.TypeReference;
import java.io.IOException;
import java.util.Map;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class SaveService {
    private final PlayerSaveRepository saves;
    private final ObjectMapper objectMapper;

    public SaveService(PlayerSaveRepository saves, ObjectMapper objectMapper) {
        this.saves = saves;
        this.objectMapper = objectMapper;
    }

    @Transactional
    public SavePayload load(String username) {
        return toPayload(saves.findByUsername(username).orElseGet(() -> saves.save(new PlayerSave(username))));
    }

    @Transactional
    public SavePayload save(String username, SavePayload payload) {
        PlayerSave save = saves.findByUsername(username).orElseGet(() -> new PlayerSave(username));
        save.setCurrentChapter(payload.currentChapter());
        save.setCompletedMissions(writeJson(payload.completedMissions()));
        save.setEmberUpgrades(writeJson(payload.emberUpgrades()));
        save.setBarricadeWalls(payload.barricade().walls());
        save.setBarricadeTraps(payload.barricade().traps());
        return toPayload(saves.save(save));
    }

    private SavePayload toPayload(PlayerSave save) {
        try {
            return new SavePayload(
                    save.getCurrentChapter(),
                    objectMapper.readValue(save.getCompletedMissions(), String[].class),
                    objectMapper.readValue(
                            save.getEmberUpgrades(),
                            new TypeReference<Map<String, Integer>>() {}),
                    new SavePayload.BarricadeData(save.getBarricadeWalls(), save.getBarricadeTraps()));
        } catch (IOException exception) {
            throw new IllegalStateException("Stored save data is invalid for " + save.getUsername(), exception);
        }
    }

    private String writeJson(Object value) {
        try {
            return objectMapper.writeValueAsString(value);
        } catch (JsonProcessingException exception) {
            throw new IllegalArgumentException("Could not serialize save data", exception);
        }
    }
}
