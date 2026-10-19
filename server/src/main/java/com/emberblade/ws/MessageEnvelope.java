package com.emberblade.ws;

public record MessageEnvelope(String type, long seq, Object payload) {
}
