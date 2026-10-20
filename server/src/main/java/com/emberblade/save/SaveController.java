package com.emberblade.save;

import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/save")
public class SaveController {
    private final SaveService saveService;

    public SaveController(SaveService saveService) {
        this.saveService = saveService;
    }

    @PostMapping
    public SavePayload save(Authentication authentication, @RequestBody SavePayload payload) {
        return saveService.save(authentication.getName(), payload);
    }

    @GetMapping
    public SavePayload load(Authentication authentication) {
        return saveService.load(authentication.getName());
    }
}
