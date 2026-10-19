package com.emberblade.auth;

import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

@Service
public class AuthService {
    private final AppUserRepository users;
    private final PasswordEncoder passwordEncoder;
    private final JwtService jwtService;

    public AuthService(AppUserRepository users, PasswordEncoder passwordEncoder, JwtService jwtService) {
        this.users = users;
        this.passwordEncoder = passwordEncoder;
        this.jwtService = jwtService;
    }

    public void register(String username, String password) {
        if (username == null || username.isBlank() || password == null || password.length() < 8) {
            throw new IllegalArgumentException("Username is required and password must be at least 8 characters");
        }
        if (users.existsByUsername(username)) {
            throw new IllegalArgumentException("Username is already registered");
        }
        users.save(new AppUser(username, passwordEncoder.encode(password)));
    }

    public String login(String username, String password) {
        AppUser user = users.findByUsername(username)
                .orElseThrow(() -> new IllegalArgumentException("Invalid username or password"));
        if (!passwordEncoder.matches(password, user.getPasswordHash())) {
            throw new IllegalArgumentException("Invalid username or password");
        }
        return jwtService.issue(user.getUsername());
    }
}
