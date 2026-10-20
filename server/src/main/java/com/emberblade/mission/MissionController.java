package com.emberblade.mission;

import com.emberblade.save.SavePayload;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/missions")
public class MissionController {
    private final MissionService missionService;

    public MissionController(MissionService missionService) {
        this.missionService = missionService;
    }

    @PostMapping("/start")
    public SavePayload start(Authentication authentication, @RequestBody MissionStartRequest request) {
        return missionService.start(authentication.getName(), request.missionId());
    }

    @PostMapping("/result")
    public SavePayload result(Authentication authentication, @RequestBody MissionResultRequest request) {
        return missionService.result(authentication.getName(), request);
    }
}
