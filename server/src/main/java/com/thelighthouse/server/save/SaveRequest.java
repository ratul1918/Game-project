package com.thelighthouse.server.save;

import jakarta.validation.constraints.*;

public record SaveRequest(
    @NotBlank @Size(max=128) String act,
    @NotBlank @Size(max=128) String checkpoint,
    @NotBlank @Size(max=131072) String itemsJson,
    @NotBlank @Size(max=131072) String pagesJson,
    @NotBlank @Size(max=131072) String flagsJson,
    @NotBlank @Size(max=131072) String optionsJson,
    @Min(0) int version) {}
