package com.thelighthouse.server.auth;

import com.thelighthouse.server.player.Player;
import com.thelighthouse.server.player.PlayerRepository;
import com.thelighthouse.server.security.JwtService;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import org.springframework.http.HttpStatus;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;

@RestController
@RequestMapping("/auth")
public class AuthController {
    private final PlayerRepository players; private final PasswordEncoder passwords; private final JwtService jwt;
    public AuthController(PlayerRepository players, PasswordEncoder passwords, JwtService jwt) { this.players=players;this.passwords=passwords;this.jwt=jwt; }
    @PostMapping("/register") @ResponseStatus(HttpStatus.CREATED)
    public AuthResponse register(@Valid @RequestBody Credentials request) {
        String username=request.username().trim();if(players.existsByUsername(username))throw new ResponseStatusException(HttpStatus.CONFLICT,"Username already exists");
        players.save(new Player(username,passwords.encode(request.password())));return new AuthResponse(jwt.issue(username));
    }
    @PostMapping("/login")
    public AuthResponse login(@Valid @RequestBody Credentials request) {
        Player player=players.findByUsername(request.username().trim()).orElseThrow(()->new ResponseStatusException(HttpStatus.UNAUTHORIZED,"Invalid credentials"));
        if(!passwords.matches(request.password(),player.getPasswordHash()))throw new ResponseStatusException(HttpStatus.UNAUTHORIZED,"Invalid credentials");return new AuthResponse(jwt.issue(player.getUsername()));
    }
    public record Credentials(@NotBlank @Size(max=64) String username,@NotBlank @Size(min=8,max=128) String password) {}
    public record AuthResponse(String token) {}
}
