package com.thelighthouse.server.run;
import com.thelighthouse.server.player.PlayerRepository; import java.util.UUID;
import org.springframework.http.HttpStatus; import org.springframework.stereotype.Service; import org.springframework.transaction.annotation.Transactional; import org.springframework.web.server.ResponseStatusException;
@Service public class RunService {
 private final RunRepository runs; private final PlayerRepository players;
 public RunService(RunRepository runs,PlayerRepository players){this.runs=runs;this.players=players;}
 @Transactional public RunResponse start(String username){var p=players.findByUsername(username).orElseThrow(()->new ResponseStatusException(HttpStatus.UNAUTHORIZED,"Unknown player"));return RunResponse.from(runs.save(new Run(p)));}
 @Transactional public RunResponse end(String username,UUID id,RunRequest request){var r=runs.findById(id).orElseThrow(()->new ResponseStatusException(HttpStatus.NOT_FOUND,"Run not found"));if(!r.getPlayer().getUsername().equals(username))throw new ResponseStatusException(HttpStatus.NOT_FOUND,"Run not found");if(r.getEndedAt()!=null)throw new ResponseStatusException(HttpStatus.CONFLICT,"Run already ended");r.end(request.outcome(),request.deaths(),request.durationSeconds());return RunResponse.from(r);}
}
