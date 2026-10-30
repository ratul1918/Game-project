package com.thelighthouse.server.ws;

import com.fasterxml.jackson.databind.JsonNode;

public record SocketEnvelope(String type, long seq, JsonNode payload) {}
