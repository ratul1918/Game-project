package com.thelighthouse.server.run;
import jakarta.validation.Valid; import java.util.UUID; import org.springframework.security.core.Authentication; import org.springframework.web.bind.annotation.*;
@RestController public class RunController {
 private final RunService service; public RunController(RunService service){this.service=service;}
 @PostMapping("/runs/start") public RunResponse start(Authentication auth){return service.start(auth.getName());}
 @PostMapping("/runs/{id}/end") public RunResponse end(Authentication auth,@PathVariable UUID id,@Valid @RequestBody RunRequest request){return service.end(auth.getName(),id,request);}
}
