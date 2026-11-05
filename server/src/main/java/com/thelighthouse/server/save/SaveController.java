package com.thelighthouse.server.save;

import jakarta.validation.Valid;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

@RestController
public class SaveController {
    private final SaveService service;
    public SaveController(SaveService service){this.service=service;}
    @GetMapping("/save") public SaveResponse get(Authentication auth){return service.get(auth.getName());}
    @PutMapping("/save") public SaveResponse put(Authentication auth,@Valid @RequestBody SaveRequest request){return service.put(auth.getName(),request);}
}
