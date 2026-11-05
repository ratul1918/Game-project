package com.thelighthouse.server.save;

import java.time.Instant;

public record SaveResponse(String act, String checkpoint, String itemsJson, String pagesJson, String flagsJson,
                           String optionsJson, int version, Instant updatedAt) {
    public static SaveResponse from(Save save) {
        return new SaveResponse(save.getAct(), save.getCheckpoint(), save.getItemsJson(), save.getPagesJson(),
            save.getFlagsJson(), save.getOptionsJson(), save.getVersion(), save.getUpdatedAt());
    }
}
