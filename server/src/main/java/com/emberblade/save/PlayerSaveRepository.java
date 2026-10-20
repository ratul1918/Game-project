package com.emberblade.save;

import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PlayerSaveRepository extends JpaRepository<PlayerSave, Long> {
    Optional<PlayerSave> findByUsername(String username);
}
