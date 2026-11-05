package com.thelighthouse.server.run;
import jakarta.validation.constraints.*;
public record RunRequest(@NotBlank @Size(max=32) String outcome,@Min(0) int deaths,@Min(0) @Max(864000) int durationSeconds) {}
