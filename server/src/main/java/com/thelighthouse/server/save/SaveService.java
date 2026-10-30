package com.thelighthouse.server.save;

import com.thelighthouse.server.player.PlayerRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.http.HttpStatus;

@Service
public class SaveService {
    private final SaveRepository saves; private final PlayerRepository players;
    public SaveService(SaveRepository saves, PlayerRepository players){this.saves=saves;this.players=players;}
    @Transactional(readOnly=true) public SaveResponse get(String username) {
        return saves.findByPlayerUsername(username).map(SaveResponse::from)
            .orElseThrow(()->new ResponseStatusException(HttpStatus.NOT_FOUND,"Save not found"));
    }
    @Transactional public SaveResponse put(String username, SaveRequest request) {
        var player=players.findByUsername(username).orElseThrow(()->new ResponseStatusException(HttpStatus.UNAUTHORIZED,"Unknown player"));
        var existing=saves.findByPlayerUsername(username);
        if(existing.isPresent()) {
            Save save=existing.get();
            if(request.version()!=save.getVersion()) throw new SaveConflictException(SaveResponse.from(save));
            save.update(request, save.getVersion()+1); return SaveResponse.from(save);
        }
        if(request.version()!=0) throw new SaveConflictException(null);
        return SaveResponse.from(saves.save(new Save(player,request)));
    }
}
