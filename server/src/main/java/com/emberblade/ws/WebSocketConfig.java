package com.emberblade.ws;

import com.emberblade.auth.JwtService;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.socket.config.annotation.EnableWebSocket;
import org.springframework.web.socket.config.annotation.WebSocketConfigurer;
import org.springframework.web.socket.config.annotation.WebSocketHandlerRegistry;

@Configuration
@EnableWebSocket
public class WebSocketConfig implements WebSocketConfigurer {
    private final JwtService jwtService;
    private final Broadcaster broadcaster;

    public WebSocketConfig(JwtService jwtService, Broadcaster broadcaster) {
        this.jwtService = jwtService;
        this.broadcaster = broadcaster;
    }

    @Override
    public void registerWebSocketHandlers(WebSocketHandlerRegistry registry) {
        registry.addHandler(new EmberWebSocketHandler(broadcaster), "/ws")
                .addInterceptors(new TokenHandshakeInterceptor(jwtService))
                .setAllowedOriginPatterns("*");
    }
}
