package com.thelighthouse.server.save;

import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;

public interface SaveRepository extends JpaRepository<Save, Long> {
    Optional<Save> findByPlayerUsername(String username);
}
