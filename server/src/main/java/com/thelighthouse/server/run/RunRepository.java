package com.thelighthouse.server.run;
import java.util.*; import org.springframework.data.jpa.repository.JpaRepository;
public interface RunRepository extends JpaRepository<Run,UUID> {}
