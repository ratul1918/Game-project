package com.emberblade.save;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "player_saves")
public class PlayerSave {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String username;
    private int currentChapter = 1;
    private String completedMissions = "[]";
    private String emberUpgrades = "{}";
    private int barricadeWalls;
    private int barricadeTraps;

    protected PlayerSave() {
    }

    public PlayerSave(String username) {
        this.username = username;
    }

    public String getUsername() {
        return username;
    }

    public int getCurrentChapter() {
        return currentChapter;
    }

    public void setCurrentChapter(int currentChapter) {
        this.currentChapter = currentChapter;
    }

    public String getCompletedMissions() {
        return completedMissions;
    }

    public void setCompletedMissions(String completedMissions) {
        this.completedMissions = completedMissions;
    }

    public String getEmberUpgrades() {
        return emberUpgrades;
    }

    public void setEmberUpgrades(String emberUpgrades) {
        this.emberUpgrades = emberUpgrades;
    }

    public int getBarricadeWalls() {
        return barricadeWalls;
    }

    public void setBarricadeWalls(int barricadeWalls) {
        this.barricadeWalls = barricadeWalls;
    }

    public int getBarricadeTraps() {
        return barricadeTraps;
    }

    public void setBarricadeTraps(int barricadeTraps) {
        this.barricadeTraps = barricadeTraps;
    }
}
